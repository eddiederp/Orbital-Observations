using UnityEngine;

public class EarthMenuRotateScript : MonoBehaviour
{
    public GameObject earth;
    public float rotation;
    public float rotationSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        rotation += rotationSpeed * Time.deltaTime;
        earth.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.up);
    }
}
