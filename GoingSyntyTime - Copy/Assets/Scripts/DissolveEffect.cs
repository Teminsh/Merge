using UnityEngine;

public class DissolveEffect : MonoBehaviour
{
    private Renderer rend;
    public float dissolveSpeed = 0.5f;
    private float dissolveValue = 1.0f;

    private static readonly int DissolveID = Shader.PropertyToID("#Dissolve");

    private void Awake()
    {
        rend = GetComponent<Renderer>();

        // Initialize dissolve value at 1
        foreach (var material in rend.materials)
        {
            material.SetFloat(DissolveID, dissolveValue);
        }
    }

    private void Update()
    {
        if (dissolveValue > 0)
        {
            // Decrease the dissolve value over time
            dissolveValue -= dissolveSpeed * Time.deltaTime;
            dissolveValue = Mathf.Clamp(dissolveValue, 0, 1);

            foreach (var material in rend.materials)
            {
                material.SetFloat(DissolveID, dissolveValue);
            }
        }
    }
}