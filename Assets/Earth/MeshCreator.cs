using UnityEngine;
using LazySquirrelLabs.SphereGenerator.Generators;

public class MeshCreator : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        SphereGenerator generator = new UVSphereGenerator(0.5f * GameManager.worldScale, 100);  
        Mesh mesh = generator.Generate();
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
    }
}
