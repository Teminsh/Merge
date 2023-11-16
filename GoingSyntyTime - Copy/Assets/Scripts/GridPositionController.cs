using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class GridPositionController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Vector3 defaultScale;
    public bool isEnlarged = false;
    public bool isSelected = false;
    public Item containedItem;
    public GameObject vfxPrefab; // Assign this in the editor
    public Vector3 vfxPositionOffset = Vector3.zero; 
    public Vector3 vfxScaleOffset = Vector3.one; // Offset for the scale of the VFX
    public Vector3 vfxRotationOffset = Vector3.zero; // Offset for the rotation of the VFX

    private ItemManager itemManager;
    private Animator itemAnimator;

    public Transform tooltipTransform; // Assign this in the editor to your 3D Text object
    private TextMeshPro tooltipTextComponent;
    public Vector3 tooltipOffset = new Vector3(0, 0.5f, 0); // Add this variable to adjust the tooltip's position
    public static Transform currentlyShownTooltip;

    public ParticleSystem tooltipParticleSystem;
    private Vector3 originalRotation; 

    private void Start()
    {
        itemAnimator = GetComponentInChildren<Animator>();
        itemManager = FindObjectOfType<ItemManager>();
        tooltipTextComponent = tooltipTransform.GetComponent<TextMeshPro>();
        tooltipTransform.gameObject.SetActive(false);
        tooltipTransform.localScale = Vector3.zero;
        originalRotation = transform.eulerAngles;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (containedItem)
        {
            if (currentlyShownTooltip && currentlyShownTooltip != tooltipTransform)
            {
                currentlyShownTooltip.DOKill();
                currentlyShownTooltip.DOScale(Vector3.zero, 0.3f)
                    .OnComplete(() =>
                    {
                        currentlyShownTooltip.gameObject.SetActive(false);
                        ShowCurrentTooltip();
                    });
            }
            else
            {
                ShowCurrentTooltip();
            }
        }
        if (!isSelected)
        {
            SetRotationState(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (containedItem && currentlyShownTooltip == tooltipTransform)
        {
            HideCurrentTooltip();
        }
        if (!isSelected)
        {
            SetRotationState(false);
        }
    }
    
    private void SetRotationState(bool isHovered)
    {
        if (isHovered && !isSelected)
        {
            transform.DOKill();
            transform.DORotate(new Vector3(originalRotation.x, originalRotation.y + 360, originalRotation.z), 3f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
        }
        else if (!isSelected)
        {
            transform.DOKill();
            transform.DORotate(originalRotation, 1f).SetEase(Ease.OutQuad);
        }
    }


    public void ShowCurrentTooltip()
    {
        tooltipTransform.position = this.transform.position + tooltipOffset; 
        tooltipTextComponent.text = containedItem.tooltipText;

        tooltipTransform.gameObject.SetActive(true);
        tooltipTransform.DOKill(); 
        tooltipTransform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.2f).SetEase(Ease.OutBack);

        tooltipParticleSystem.transform.localScale = Vector3.zero; // Set initial scale to zero
        tooltipParticleSystem.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack); // Tween to the desired scale

        currentlyShownTooltip = tooltipTransform;  
    }

    private void HideCurrentTooltip()
    {
        tooltipTransform.DOKill();
        tooltipTransform.DOScale(Vector3.zero, 0.3f)
            .OnComplete(() => tooltipTransform.gameObject.SetActive(true));

        tooltipParticleSystem.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutBack); // Tween to zero scale

        currentlyShownTooltip = null;
    }
    
    public IEnumerator RotateUnsuccessfulMatch()
    {
        Transform vfxTransform = null; // Store the transform of the VFX

        // Decouple the VFX from the item

        Vector3 initialRotation = transform.eulerAngles;

        // Use DOPunchRotation to create a shaking effect
        transform.DOPunchRotation(new Vector3(0, 0, 15), duration: 0.3f, vibrato: 10, elasticity: 0.3f);


        // Wait for the duration of the shaking animation plus a little extra time before resetting the rotation
        yield return new WaitForSeconds(0.6f);
        
        // Smoothly interpolate the rotation back to its initial state
        transform.DORotate(initialRotation, 0.3f).SetEase(Ease.OutQuad);

        DestroyVFX();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(itemManager.IsCraftingInProgress() || isEnlarged)
        {
            return;
        }
        if (containedItem) // Check if there's an item in the grid position
        {
            if (isEnlarged)
            {
                SetAnimationState(ItemAnimationState.Idle);
                
                // Destroy the VFX when the item is deselected
                DestroyVFX();

                transform.localScale = defaultScale;
                isEnlarged = false;
            }
            else
            {
                SpawnVFX();
                transform.DOScale(defaultScale * 1.1f, 0.3f).SetLoops(2, LoopType.Yoyo);
                isEnlarged = true;

                // Set the animation state to selected when the item is selected
                SetAnimationState(ItemAnimationState.Selected);
            }

            itemManager.CheckForCrafting(this);
        }
        if (containedItem && !isEnlarged)
        {
            isSelected = !isSelected;

            if (isSelected)
            {
                SetRotationState(false);
            }
            else
            {
                SetRotationState(true);
            }
        }
    }
    public GameObject currentVFXInstance;  // Add this line to hold a reference to the VFX instance

    private void SpawnVFX()
    {
        if (vfxPrefab)
        {
            if(currentVFXInstance)
                Destroy(currentVFXInstance);

            currentVFXInstance = Instantiate(vfxPrefab, transform.position + vfxPositionOffset, Quaternion.identity, transform);
            currentVFXInstance.transform.localScale += vfxScaleOffset; // Apply the scale offset
            currentVFXInstance.transform.Rotate(vfxRotationOffset); // Apply the rotation offset
        }
    }


    public void SetAnimationState(ItemAnimationState state)
    {
        if(!itemAnimator) 
        {
            itemAnimator = GetComponentInChildren<Animator>();
        }

        if(itemAnimator)
        {
            switch (state)
            {
                case ItemAnimationState.Idle:
                    itemAnimator.SetTrigger("Idle");
                    break;
                case ItemAnimationState.Selected:
                    itemAnimator.SetTrigger("Selected");
                    break;
            }
        }
    }

    public void DestroyVFX()
    {
        var vfx = transform.GetComponentInChildren<ParticleSystem>();
        if (vfx)
        {
            vfx.Stop(); // Stop emitting new particles

            Destroy(vfx.gameObject, vfx.main.startLifetime.constantMax); // Destroy the game object after a delay
        }
    }
}

public enum ItemAnimationState
{
    Idle,
    Selected
}
