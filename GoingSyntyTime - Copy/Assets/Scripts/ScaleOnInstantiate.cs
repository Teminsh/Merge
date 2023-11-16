using UnityEngine;
using DG.Tweening;

public class ScaleOnInstantiate : MonoBehaviour
{
    public float scaleDuration = 1.0f;

    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale; // Store the original scale
        transform.localScale = Vector3.zero;  // Set the initial scale to zero
    }

    void Start()
    {
        transform.DOScale(originalScale, scaleDuration); // Scale to the original scale using DOTween
    }
}