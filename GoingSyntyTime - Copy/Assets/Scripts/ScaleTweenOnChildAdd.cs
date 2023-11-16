using UnityEngine;
using DG.Tweening;

public class ScaleTweenOnChildAdd : MonoBehaviour
{
    private int childCount;

    private void Start()
    {
        childCount = transform.childCount;
    }

    private void Update()
    {
        if (transform.childCount > childCount)
        {
            for (int i = childCount; i < transform.childCount; i++)
            {
                Transform newChild = transform.GetChild(i);
                ApplyScaleTween(newChild);
            }
            childCount = transform.childCount;
        }
    }

    private void ApplyScaleTween(Transform newChild)
    {
        Vector3 originalScale = newChild.localScale;
        newChild.localScale = Vector3.zero;
        newChild.DOScale(originalScale, 0.3f).SetEase(Ease.OutBack);
    }
}