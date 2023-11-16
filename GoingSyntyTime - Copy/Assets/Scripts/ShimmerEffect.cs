using UnityEngine;

public class ShimmerEffect : MonoBehaviour
{
    public Color startColor = Color.white;
    public Color endColor = Color.yellow;
    public float duration = 1.0f;

    private SkinnedMeshRenderer skinnedMeshRenderer;
    private float lerpTime;

    void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
    }

    void Update()
    {
        lerpTime = (Mathf.Sin(Time.time / duration * (2 * Mathf.PI)) + 1) / 2;
        skinnedMeshRenderer.material.SetColor("_TintColor", Color.Lerp(startColor, endColor, lerpTime));
    }
}