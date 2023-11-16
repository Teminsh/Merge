using UnityEngine;

public class RotateSkybox : MonoBehaviour
{
    public float rotationSpeed = 1.0f;
    private Material skyboxMaterial;
    private float rotation;

    void Start()
    {
        // Get the current skybox material
        skyboxMaterial = RenderSettings.skybox;
    }

    void Update()
    {
        // Increment the rotation value and set it to the "_Rotation" property of the skybox material
        rotation += rotationSpeed * Time.deltaTime;
        rotation %= 360f; // Keep the rotation value between 0 and 360
        skyboxMaterial.SetFloat("_Rotation", rotation);
    }
}