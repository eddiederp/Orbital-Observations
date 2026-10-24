using UnityEngine;
using System;

public static class SatelliteCoordinateConverter
{
    private const double TWO_PI = 2.0 * Math.PI;


    // ============================================================
    // TEME -> ECEF
    // ============================================================
    //
    // Input:
    //     TEME position in kilometers
    //
    //     utc:
    //         Exact UTC time at which the SGP4 position was generated
    //
    // Output:
    //     Earth-fixed XYZ position in kilometers
    //
    // X:
    //     Approximately points toward the Greenwich meridian
    //
    // Y:
    //     Approximately points 90 degrees east of Greenwich
    //
    // Z:
    //     Points toward the North Pole
    //
    public static DoubleVector3 TEMEToECEF(
        DoubleVector3 temePosition,
        DateTime utc)
    {
        utc = utc.ToUniversalTime();

        // Calculate Julian Date
        double jd = JulianDate(utc);

        // Calculate Greenwich Mean Sidereal Time
        double gmst = GreenwichMeanSiderealTime(jd);

        double cosGMST = Math.Cos(gmst);
        double sinGMST = Math.Sin(gmst);

        //
        // Rotate TEME around the Earth's Z axis.
        //
        double xECEF =
            cosGMST * temePosition.x +
            sinGMST * temePosition.y;

        double yECEF =
            -sinGMST * temePosition.x +
            cosGMST * temePosition.y;

        double zECEF =
            temePosition.z;

        return new DoubleVector3(
            xECEF,
            yECEF,
            zECEF
        );
    }


    // ============================================================
    // JULIAN DATE
    // ============================================================

    private static double JulianDate(
        DateTime utc)
    {
        utc = utc.ToUniversalTime();

        int year = utc.Year;
        int month = utc.Month;

        double day =
            utc.Day
            + utc.Hour / 24.0
            + utc.Minute / 1440.0
            + utc.Second / 86400.0
            + utc.Millisecond / 86400000.0;

        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }

        int A =
            year / 100;

        int B =
            2 - A + A / 4;

        double jd =
            Math.Floor(
                365.25 * (year + 4716)
            )
            +
            Math.Floor(
                30.6001 * (month + 1)
            )
            +
            day
            +
            B
            -
            1524.5;

        return jd;
    }


    // ============================================================
    // GREENWICH MEAN SIDEREAL TIME
    // ============================================================
    //
    // Input:
    //     Julian Date
    //
    // Output:
    //     GMST in radians
    //
    private static double GreenwichMeanSiderealTime(
        double jd)
    {
        //
        // Julian centuries since J2000.0
        //
        double T =
            (jd - 2451545.0) / 36525.0;

        //
        // GMST in seconds.
        //
        double gmstSeconds =
            67310.54841
            +
            (876600.0 * 3600.0
             + 8640184.812866) * T
            +
            0.093104 * T * T
            -
            6.2e-6 * T * T * T;

        //
        // Reduce to one 24-hour day.
        //
        gmstSeconds %= 86400.0;

        if (gmstSeconds < 0.0)
        {
            gmstSeconds += 86400.0;
        }

        //
        // Convert sidereal seconds to radians.
        //
        double gmstRadians =
            gmstSeconds *
            (TWO_PI / 86400.0);

        return gmstRadians;
    }

    public static Vector3 TEMEToECEF(
        Vector3 temePosition,
        DateTime utc)
    {
        utc = utc.ToUniversalTime();

        // Calculate Julian Date
        float jd = JulianDateF(utc);

        // Calculate Greenwich Mean Sidereal Time
        float gmst = GreenwichMeanSiderealTime(jd);

        float cosGMST = Mathf.Cos(gmst);
        float sinGMST = Mathf.Sin(gmst);

        //
        // Rotate TEME around the Earth's Z axis.
        //
        float xECEF =
            cosGMST * temePosition.x +
            sinGMST * temePosition.y;

        float yECEF =
            -sinGMST * temePosition.x +
            cosGMST * temePosition.y;

        float zECEF =
            temePosition.z;

        return new Vector3(
            xECEF,
            yECEF,
            zECEF
        );
    }


    // ============================================================
    // JULIAN DATE
    // ============================================================

    private static float JulianDateF(
        DateTime utc)
    {
        utc = utc.ToUniversalTime();

        int year = utc.Year;
        int month = utc.Month;

        float day =
            utc.Day
            + utc.Hour / 24.0f
            + utc.Minute / 1440.0f
            + utc.Second / 86400.0f
            + utc.Millisecond / 86400000.0f;

        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }

        int A =
            year / 100;

        int B =
            2 - A + A / 4;

        float jd =
            Mathf.Floor(
                365.25f * (year + 4716)
            )
            +
            Mathf.Floor(
                30.6001f * (month + 1)
            )
            +
            day
            +
            B
            -
            1524.5f;

        return jd;
    }


    // ============================================================
    // GREENWICH MEAN SIDEREAL TIME
    // ============================================================
    //
    // Input:
    //     Julian Date
    //
    // Output:
    //     GMST in radians
    //
    private static float GreenwichMeanSiderealTime(
        float jd)
    {
        //
        // Julian centuries since J2000.0
        //
        float T =
            (jd - 2451545.0f) / 36525.0f;

        //
        // GMST in seconds.
        //
        float gmstSeconds =
            67310.54841f
            +
            (876600.0f * 3600.0f
             + 8640184.812866f) * T
            +
            0.093104f * T * T
            -
            6.2e-6f * T * T * T;

        //
        // Reduce to one 24-hour day.
        //
        gmstSeconds %= 86400.0f;

        if (gmstSeconds < 0.0)
        {
            gmstSeconds += 86400.0f;
        }

        //
        // Convert sidereal seconds to radians.
        //
        float gmstRadians =
            gmstSeconds *
            (Mathf.PI / 43200.0f);

        return gmstRadians;
    }
}