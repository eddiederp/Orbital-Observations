using System;
using System.Globalization;
using System.Collections.Generic;
using System.Numerics;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms;
using Vector3 = UnityEngine.Vector3;
using Unity.AI.Navigation.LowLevel;
using System.Linq;

public struct DoubleVector3
{
    public double x;
    public double y;
    public double z;

    public DoubleVector3 (double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static DoubleVector3 operator *(DoubleVector3 a, double d)
    {
        return new DoubleVector3(
            a.x * d,
            a.y * d,
            a.z * d  
        );
    }
    
    public static DoubleVector3 operator +(DoubleVector3 a, DoubleVector3 b)
    {
        return new DoubleVector3(
            a.x + b.x,
            a.y + b.y,
            a.z + b.z  
        );
    }

    public static DoubleVector3 operator -(DoubleVector3 a, DoubleVector3 b)
    {
        return new DoubleVector3(
            a.x - b.x,
            a.y - b.y,
            a.z - b.z  
        );
    }
}

public class SGP4Solver
{
    const double KE = 0.743669161e-1; // something to do with earth radii idfk
    const double KMPER = 6378.135;
    //parameters for sgp4 density function
    const double QOMS2T = 1.88027916E-9;
    const double S = 1.01222928; //can change based on satellite perigee
    //zonal harmonics
    const double AE = 1.0; //earth radii
    const double J2 = 1.08262668e-3;
    const double J3 = -0.253881E-5;
    const double K2 = 5.413080E-4;
    const double K4 = 0.62098875E-6;
    //solver params
    const int iterations = 10;
    const double tolerance = 1e-12;
    string refEpoch = "";
    ComputeShader sgp4ComputeSolver;
    ComputeBuffer satelliteDataBuffer;
    ComputeBuffer satellitePositionsBuffer;

    public SGP4Solver(ComputeShader computeSolver)
    {
        sgp4ComputeSolver = computeSolver;
    }

    public List<Vector3> GetAllSatellitePositions(List<SatelliteOrbitComputeData> data, float time)
    {
        List<Vector3> positions = new List<Vector3>();

        int satelliteDataSize = sizeof(float) * 8;
        satelliteDataBuffer = new ComputeBuffer(data.Count, satelliteDataSize);
        satelliteDataBuffer.SetData(data);

        int satellitePositionSize = sizeof(float) * 3;
        satellitePositionsBuffer = new ComputeBuffer(data.Count, satellitePositionSize);

        sgp4ComputeSolver.SetBuffer(0, "satelliteData", satelliteDataBuffer);
        sgp4ComputeSolver.SetBuffer(0, "finalPositions", satellitePositionsBuffer);
        sgp4ComputeSolver.SetFloat("t", (float)(DateTime.UtcNow.AddMinutes(time) - DateTime.UnixEpoch).TotalMinutes);
        sgp4ComputeSolver.Dispatch(0, Mathf.CeilToInt(data.Count / 64f), 1, 1);
        
        Vector3[] temePositions = new Vector3[data.Count];
        satellitePositionsBuffer.GetData(temePositions);

        foreach (Vector3 temePosition in temePositions)
        {
            positions.Append(SatelliteCoordinateConverter.TEMEToECEF(temePosition * (float)KMPER, DateTime.UtcNow));
        }

        return positions;
    }
        
    //Implementation based on description of SGP4 Algorithm from Space Track Report No. 3 (Hoots + Roerich, 1980)
    public (Vector3, Vector3) GetPositionAndVelocityFromOMM(SatelliteOMMData data, double time) //grab data from OMM for now bc of innacuracies with epoch single datatype with compute data struct
    {
        DateTime referenceTime;
        if (refEpoch == "")
        {
            referenceTime = DateTime.UtcNow;
        }
        else
        {
            referenceTime = DateTime.Parse(
            refEpoch,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal |
            DateTimeStyles.AdjustToUniversal
        );
        }

        DateTime epochTime = data.epoch;

        DateTime targetTime = referenceTime.AddMinutes(time);
        
        double deltaT = (targetTime - epochTime).TotalMinutes;
        double n0 = data.meanMotion * Math.PI * 2.0 / 1440.0f;
        double e0 = data.eccentricity;
        double i0 = data.inclination * Mathf.Deg2Rad;
        double M0 = data.meanAnomaly * Mathf.Deg2Rad;
        double omega0 = data.argumentOfPericenter * Mathf.Deg2Rad;
        double bigOmega0 = data.longitudeAscendingNode * Mathf.Deg2Rad;
        double bStar = data.BStar;

        //recover orbital elements - mean motion and semi major axis
        //first sma estimate using mean motion and grav param
        double a1 = Math.Pow(KE / n0, 2.0 / 3.0);
        //acount for change in orbit from uneven gravity from earth
        double delta1 = 1.5 * (K2 / Math.Pow(a1, 2.0)) * ((3.0 * Math.Pow(Math.Cos(i0), 2.0) - 1.0) / Math.Pow(1.0 - Math.Pow(e0, 2.0), 1.5));
        //recalculate sma estimate with change
        double a0 = a1 * (1.0 - (1.0 / 3.0 * delta1) - Math.Pow(delta1, 2.0) - (134.0 / 81.0 * Math.Pow(delta1, 3.0)));
        //recalculate orbit change with new sma estimate
        double delta0 = 1.5 * (K2 / Math.Pow(a0, 2.0)) * ((3.0 * Math.Pow(Math.Cos(i0), 2.0) - 1.0) / Math.Pow(1.0 - Math.Pow(e0, 2.0), 1.5));
        //final mean motion and sma estimates
        double nDPrime0 = n0 / (1.0 + delta0);
        double aDPrime0 = a0 / (1.0 - delta0);

        //calculate perigee & apply corrections for lower perigees
        double rp0 = aDPrime0 * (1.0 - e0); //perigee radius in earth radii
        double ph0 = (rp0 - AE) * KMPER; //perigee height in km
        double s = S;
        double qoms2t = QOMS2T;
        if (ph0 <= 156.0)
        {
            double Sstar = 0;
            Sstar = rp0 - S + AE;

            if (ph0 <= 98.0)
            {
                Sstar = 20.0 / KMPER + AE;
            }
            double QOMS2Tstar = Math.Pow(Math.Pow(QOMS2T, 0.25) + S - Sstar, 4.0);
            s = Sstar;
            qoms2t = QOMS2Tstar;
        }

        //calculating orbital constants
        double theta = Math.Cos(i0);
        double xi = 1.0 / (aDPrime0 - s);
        double beta0 = Math.Pow(1 - Math.Pow(e0, 2.0), 0.5);
        double eta = aDPrime0 * e0 * xi;

        //density function
        double C2 = qoms2t * Math.Pow(xi, 4.0) * nDPrime0 * Math.Pow(beta0, 2.0) * Math.Pow(1.0 - Math.Pow(eta, 2.0), -3.5) * (aDPrime0 * (1 + 3.0 / 2.0 * Math.Pow(eta, 2.0) + 4.0 * e0 * Math.Pow(eta, 3.0)) +
            3.0 / 2.0 * (K2 * xi / (1 - Math.Pow(eta, 2.0))) * (-0.5 + 1.5 * Math.Pow(theta, 2.0)) * (8.0 + 24.0 * Math.Pow(eta, 2.0) + 3.0 * Math.Pow(eta, 4.0)));
        double C1 = bStar * C2;
        double A30 = -J3 * Math.Pow(AE, 3.0);
        double C3 = qoms2t * Math.Pow(xi, 5.0) * A30 * nDPrime0 * AE * Math.Sin(i0) / (K2 * e0);
        double C4 = 2.0 * qoms2t * Math.Pow(xi, 4.0) * aDPrime0 * Math.Pow(beta0, 2.0) * Math.Pow(1.0 - Math.Pow(eta, 2.0), -3.5) * (2.0 * eta * (1 + e0 * eta) + 0.5 * e0 + 0.5 * Math.Pow(eta, 3.0) - 
            2.0 * K2 * xi / aDPrime0 * (1 - Math.Pow(eta, 2)) * (3.0 * (1.0 - 3.0 * Math.Pow(theta, 2.0)) * (1.0 + 1.5 * Math.Pow(eta, 2.0) - 2.0 * e0 * eta - 0.5 * e0 * Math.Pow(eta, 3.0)) + 
            0.75 * (1.0 - Math.Pow(theta, 2.0) * (2.0 * Math.Pow(eta, 2) - e0 * eta - e0 * Math.Pow(eta, 3))) * Math.Cos(2.0 * omega0)));
        double C5 = qoms2t * Math.Pow(xi, 4.0) * aDPrime0 * Math.Pow(beta0, 2.0) * Math.Pow(1.0 - Math.Pow(eta, 2.0), -3.5) * (1.0 + 11.0 / 4.0 * eta * (eta + e0) + e0 * Math.Pow(eta, 3.0));
        double D2 = 4.0 * aDPrime0  * xi * Math.Pow(C1, 2.0);
        double D3 = 4.0 / 3.0 * aDPrime0 * Math.Pow(xi, 2.0) * (17.0 * aDPrime0 +s) * Math.Pow(C1, 3.0);
        double D4 = 2.0 / 3.0 * aDPrime0 * Math.Pow(xi, 3.0) * (221.0 * aDPrime0 + 31.0 *s) * Math.Pow(C1, 4.0);
        
        //secular effects o atmospheric drag and gravitation
        double MDF = M0 + (1.0 + (3.0 * K2 * (-1.0 + 3.0 * Math.Pow(theta, 2.0)) / (2.0 * Math.Pow(aDPrime0, 2.0) * Math.Pow(beta0, 3.0))) + 
            3.0 * Math.Pow(K2, 2.0) * (13.0 - 78.0 * Math.Pow(theta, 2) + 137.0 * Math.Pow(theta, 4.0)) / (16.0 * Math.Pow(aDPrime0, 4.0) * Math.Pow(beta0, 7.0))) * nDPrime0 * (deltaT);
        double omegaDF = omega0 + (-3.0 * K2 * (1.0 - 5.0 * Math.Pow(theta, 2.0)) / (2.0 * Math.Pow(aDPrime0, 2.0) * Math.Pow(beta0, 4.0)) + 
            3.0 * Math.Pow(K2, 2.0) * (7.0 - 114.0 * Math.Pow(theta, 2) + 395.0 * Math.Pow(theta, 4.0)) / (16.0 * Math.Pow(aDPrime0, 4.0) * Math.Pow(beta0, 8.0)) +
            5.0 * K4 * (3.0 - 36.0 * Math.Pow(theta, 2.0) + 49.0 * Math.Pow(theta, 4.0)) / (4.0 * Math.Pow(aDPrime0, 4.0) * Math.Pow(beta0, 8.0))) * nDPrime0 * (deltaT);
        double bigOmegaDF = bigOmega0 + (-3.0 * K2 * theta / (Math.Pow(aDPrime0, 2.0) * Math.Pow(beta0, 4.0)) + 
            3.0 * Math.Pow(K2, 2.0) * (4.0 * theta - 19.0 * Math.Pow(theta, 3.0)) / (2.0 * Math.Pow(aDPrime0, 4.0) * Math.Pow(beta0, 8.0)) +
            5.0 * K4 * theta * (3.0 - 7.0 * Math.Pow(theta, 2)) / (2.0 * Math.Pow(aDPrime0, 4.0) * Math.Pow(beta0, 8.0))) * nDPrime0 * (deltaT);
        double dOmega = bStar * C3 * Math.Cos(omega0) * (deltaT);
        double dM = -2.0 / 3.0 * qoms2t * bStar * Math.Pow(xi, 4) * (AE / (e0 * eta)) * (Math.Pow(1.0 + eta * Math.Cos(MDF), 3.0) - Math.Pow(1.0 + eta * Math.Cos(M0), 3.0));
        double Mp = MDF;
        double omega = omegaDF;
        double e = e0 - bStar * C4 * deltaT;
        if (ph0 >= 220.0) //remove deltaM, deltaOmega, C5 terms for perigee height below 220 km
        {
            Mp = MDF + dOmega + dM;
            omega = omegaDF - dOmega - dM;
            e = e0 - bStar * C4 * deltaT - bStar * C5 * (Math.Sin(Mp) - Math.Sin(M0));
        }
        double bigOmega = bigOmegaDF - 11.5 * nDPrime0 * K2 * theta / (Math.Pow(aDPrime0, 2.0) * Math.Pow(beta0, 2.0)) * C1 * Math.Pow(deltaT, 2.0);
        
        double a;
        double bigL;
        if (ph0 < 220.0) //truncate a and bigL terms if perigee height below 220 km
        {
            a = aDPrime0 * Math.Pow(1.0 - C1 * (deltaT), 2.0);
            bigL = Mp + omega + bigOmega + nDPrime0 * (1.5 * C1 * Math.Pow(deltaT, 2.0));
        }
        else
        {
            a = aDPrime0 * Math.Pow(1.0 - C1 * (deltaT) - D2 * Math.Pow(deltaT, 2.0) - D3 * Math.Pow(deltaT, 3.0) - D4 * Math.Pow(deltaT, 4.0), 2.0);
            bigL = Mp + omega + bigOmega + nDPrime0 * (1.5 * C1 * Math.Pow(deltaT, 2.0) + (D2 + 2.0 * Math.Pow(C2, 2.0)) * Math.Pow(deltaT, 3.0) +
                0.25 * (3.0 * D3 + 12.0 * C1 * D2 + 10.0 * Math.Pow(C1, 3)) * Math.Pow(deltaT, 4.0) +
                0.2 * (3.0 * D4 + 12.0 * C1 * D3 + 6.0 * Math.Pow(D2, 2) + 30.0 * Math.Pow(C1, 2) * D2 + 15.0 * Math.Pow(C1, 4)) * Math.Pow(deltaT, 5.0));
        }
        double bigBeta = Math.Sqrt(1.0 - Math.Pow(e, 2.0));
        double n = KE / Math.Pow(a, 1.5);

        //long periodic terms
        double axN = e * Math.Cos(omega);
        double bigLL = A30 * Math.Sin(i0) / (8.0 * K2 * a * Math.Pow(bigBeta, 2.0)) * e * Math.Cos(omega) * (3.0 + 5.0 * theta) / (1.0 + theta);
        double ayNL = A30 * Math.Sin(i0) / (4.0 * K2 * a * Math.Pow(bigBeta, 2.0));
        double bigLT = bigL + bigLL;
        double ayN = e * Math.Sin(omega) + ayNL;
        double Ue = bigLT - bigOmega;
        
        //solve Kepler's equation for (E + omega)
        double Eomega = Ue;
        for (int i = 0; i < iterations; i++)
        {
            double deltaEomega = (Ue - ayN * Math.Cos(Eomega) + axN * Math.Sin(Eomega) - Eomega) / (-ayN * Math.Sin(Eomega) - axN * Math.Cos(Eomega) + 1.0);
            if (Math.Abs(deltaEomega) >= 0.95)
            {
                deltaEomega =
                    deltaEomega > 0.0
                    ? 0.95
                    : -0.95;
            }

            Eomega += deltaEomega;
            if (Math.Abs(deltaEomega) < tolerance) break;
        }

        //short-period periodics
        double eCosE = axN * Math.Cos(Eomega) + ayN * Math.Sin(Eomega);
        double eSinE = axN * Math.Sin(Eomega) - ayN * Math.Cos(Eomega);
        double eL = Math.Sqrt(Math.Pow(axN, 2.0) + Math.Pow(ayN, 2.0));
        double pL = a * (1.0 - Math.Pow(eL, 2.0));
        double r = a * (1.0 - eCosE);
        double rDot = KE * Math.Sqrt(a) / r * eSinE;
        double rfDot = KE * Math.Sqrt(pL) / r;
        double cosu = a / r * (Math.Cos(Eomega) - axN + ayN * eSinE / (1.0 + Math.Sqrt(1.0 - Math.Pow(eL, 2.0))));
        double sinu = a / r * (Math.Sin(Eomega) - ayN - axN * eSinE / (1.0 + Math.Sqrt(1.0 - Math.Pow(eL, 2.0))));
        double u = Math.Atan2(sinu, cosu);
        double deltar = K2 / (2.0 * pL) * (1.0 - Math.Pow(theta, 2.0)) * Math.Cos(2.0 * u);
        double deltau = -K2 / (4.0 * pL) * (7.0 * Math.Pow(theta, 2) - 1.0) * Math.Sin(2.0 * u);
        double deltaBigOmega = 3.0 * K2 * theta / (2.0 * Math.Pow(pL, 2.0)) * Math.Sin(2.0 * u);
        double deltai = 3.0 * K2 * theta / (2.0 * Math.Pow(pL, 2.0)) * Math.Sin(i0) * Math.Cos(2.0 * u);
        double deltarDot = -K2 * n / pL * (1.0 - Math.Pow(theta, 2.0)) * Math.Sin(2.0 * u);
        double deltarfDot = K2 * n / pL * ((1.0 - Math.Pow(theta, 2.0)) * Math.Cos(2.0 * u) - 1.5 * (1.0 - 3.0 * Math.Pow(theta, 2.0)));
        
        //osculating quantities
        double rk = r * (1.0 - 1.5 * K2 * Math.Sqrt(1.0 - Math.Pow(eL, 2.0)) / Math.Pow(pL, 2.0) * (3.0 * Math.Pow(theta, 2.0) - 1.0)) + deltar;
        double uk = u + deltau;
        double bigOmegak = bigOmega + deltaBigOmega;
        double ik = i0 + deltai;
        double rDotk = rDot + deltarDot;
        double rfDotk = rfDot + deltarfDot;

        //unit orientation vectors SPG4
        DoubleVector3 M = new DoubleVector3(
            -Math.Sin(bigOmegak) * Math.Cos(ik),
            Math.Cos(bigOmegak) * Math.Cos(ik),
            Math.Sin(ik)
        );
        DoubleVector3 N = new DoubleVector3(
            Math.Cos(bigOmegak),
            Math.Sin(bigOmegak),
            0f
        );
        DoubleVector3 U = M * Math.Sin(uk) + N * Math.Cos(uk);
        DoubleVector3 V = M * Math.Cos(uk) - N * Math.Sin(uk);

        //teme values
        DoubleVector3 temePosition = U * rk;
        DoubleVector3 temeVelocity = U * rDotk + V * rfDotk;

        //ece values
        DoubleVector3 ecefPosition = SatelliteCoordinateConverter.TEMEToECEF(temePosition * KMPER, targetTime);

        //final values
        Vector3 position = new Vector3(
            (float) ecefPosition.x, 
            (float) ecefPosition.z,
            (float) ecefPosition.y
        );
        Vector3 velocity = new Vector3(
            (float) ecefPosition.x, 
            (float) ecefPosition.z,
            (float) ecefPosition.y
        );
        
        return (position, velocity);
    }
}