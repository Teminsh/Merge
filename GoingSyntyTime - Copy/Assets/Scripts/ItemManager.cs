using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private GameObject GoodCraftAttemptVFX;
    [SerializeField] private GameObject BadCraftAttemptVFX;
    
    [SerializeField] private float VFXDisplayDuration = 3.0f;
    [System.Serializable]
    public class ItemInstantiatedEvent : UnityEvent<Transform> { }

    // Create an instance of the event
    public ItemInstantiatedEvent OnItemInstantiated;
    
    [Header("Audio")]
    public AudioSource audioSource; 
    public AudioClip mouseClickSFX;
    public AudioClip mergeFailedSFX;

    [Header("Grid Positions")]
    public List<Transform> gridPositions = new List<Transform>();

    [Header("Player's Inventory")]
    public List<Item> playerInventory = new List<Item>();

    [Header("All Available Items")]
    public List<Item> allItems = new List<Item>();

    [Header("Crafting Recipes")]
    public List<CraftingRecipe> craftingRecipes = new List<CraftingRecipe>();

    private GridPositionController firstSelectedItem = null;
    private GridPositionController secondSelectedItem = null;

    [Header("Discovery Popup")]
    public GameObject discoveryPopup;    
    public TMPro.TextMeshProUGUI popupText; 

    private GameObject currentVFXInstance;

    [Header("Crafting Delay")]
    public float craftingDelay = 1.0f;

    [Header("Crafting Rewards")]
    public List<CraftingReward> craftingRewards;  
    private int successfulCraftCount = 0;  

    [Header("Crafting Reward Popup")]
    public GameObject craftingRewardPopup; 
    public TMPro.TextMeshProUGUI craftingRewardPopupText; 
    
    [Header("Discovery Popup Animation Settings")]
    public float offset = 350f;
    public float duration = 1.5f; 
    private Tween hidePopupTween;

    [Header("Win/Lose Conditions")]
    public List<Item> winItems = new List<Item>();
    public List<Item> loseItems = new List<Item>();

    public void ShowGoodCraftAttemptVFX()
    {
        HideActiveVFX();

        Vector3 targetPosition = GoodCraftAttemptVFX.transform.position;
        GoodCraftAttemptVFX.transform.position = targetPosition + Vector3.up * 21.0f; // Start position above the target
        GoodCraftAttemptVFX.SetActive(true);
        GoodCraftAttemptVFX.transform.DOMove(targetPosition, 1f).SetEase(Ease.OutQuart); // Tween to the target position with a smooth deceleration
        HideVFXAfterDelay(GoodCraftAttemptVFX, VFXDisplayDuration);
    }

    public void ShowBadCraftAttemptVFX()
    {
        HideActiveVFX();

        Vector3 targetPosition = BadCraftAttemptVFX.transform.position;
        BadCraftAttemptVFX.transform.position = targetPosition + Vector3.up * 5.0f; // Start position above the target
        BadCraftAttemptVFX.SetActive(true);
        BadCraftAttemptVFX.transform.DOMove(targetPosition, 0.5f).SetEase(Ease.OutQuart); // Tween to the target position with a smooth deceleration
        HideVFXAfterDelay(BadCraftAttemptVFX, VFXDisplayDuration);
    }

    private void HideActiveVFX()
    {
        if (GoodCraftAttemptVFX.activeSelf)
        {
            GoodCraftAttemptVFX.SetActive(false);
            GoodCraftAttemptVFX.transform.localScale = new Vector3(1f, GoodCraftAttemptVFX.transform.localScale.y, GoodCraftAttemptVFX.transform.localScale.z); // Resetting the scale for the next time it is activated
        }
        if (BadCraftAttemptVFX.activeSelf)
        {
            BadCraftAttemptVFX.SetActive(false);
            BadCraftAttemptVFX.transform.localScale = new Vector3(1f, BadCraftAttemptVFX.transform.localScale.y, BadCraftAttemptVFX.transform.localScale.z); // Resetting the scale for the next time it is activated
        }
    }

    private void HideVFXAfterDelay(GameObject VFXObject, float delay)
    {
        DOVirtual.DelayedCall(delay, () =>
        {
            VFXObject.transform.DOScaleX(0f, 0.5f).OnComplete(() =>
            {
                VFXObject.SetActive(false);
                VFXObject.transform.localScale = new Vector3(1f, VFXObject.transform.localScale.y, VFXObject.transform.localScale.z); // Resetting the scale for the next time it is activated
            });
        });
    }
    private IEnumerator CheckForWinCondition(Item item)
    {
        if (winItems.Contains(item))
        {
            yield return new WaitForSeconds(5.0f);
            // Trigger Win Condition
            Debug.Log("You won!");
        }
    }

    private IEnumerator CheckForLoseCondition(Item item)
    {
        if (loseItems.Contains(item))
        {
            yield return new WaitForSeconds(5.0f);
            // Trigger Lose Condition
            Debug.Log("You lost!");
        }
    }
    
    public bool IsCraftingInProgress()
    {
        return firstSelectedItem != null && secondSelectedItem != null;
    }
    
    public void ShowPopup(string info)
    {
        
        HideBothPopups();
        discoveryPopup.transform.DOKill();

        if (hidePopupTween != null)
            hidePopupTween.Kill();

        discoveryPopup.transform.localScale = Vector3.one; 

        float x = discoveryPopup.transform.position.x;
        float y = discoveryPopup.transform.position.y;
        float z = discoveryPopup.transform.position.z;

        discoveryPopup.transform.position = new Vector3(x, y + offset, z); 

        popupText.text = info;

        discoveryPopup.transform.DOMoveY(y, duration)
            .SetEase(Ease.OutBack)
            .OnStart(() => 
            {
                discoveryPopup.SetActive(true);
                popupText.rectTransform.localScale = new Vector3(0, 0, 1);

                DOVirtual.DelayedCall(0.75f, () => {
                    popupText.rectTransform.DOScale(1, 0.5f)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() => HidePopupAfterDelay(3.5f)); 
                });
            });
        
    }

    public void ShowCraftingRewardPopup(string rewardMessage)
    {
        
        Debug.Log("Showing reward popup with message: " + rewardMessage);
        craftingRewardPopup.transform.DOKill();

        craftingRewardPopup.transform.localScale = Vector3.one; 

        float x = craftingRewardPopup.transform.position.x;
        float y = craftingRewardPopup.transform.position.y;
        float z = craftingRewardPopup.transform.position.z;

        craftingRewardPopup.transform.position = new Vector3(x, y + offset, z); 

        craftingRewardPopupText.text = rewardMessage;
        craftingRewardPopup.transform.localScale = new Vector3(0.59f, 0.59f, 1); 

        craftingRewardPopup.transform.DOMoveY(y, duration)
            .SetEase(Ease.OutBack)
            .OnStart(() => 
            {
                craftingRewardPopup.SetActive(true);
                craftingRewardPopupText.rectTransform.localScale = new Vector3(0, 0, 1);

                DOVirtual.DelayedCall(0.75f, () => {
                    craftingRewardPopupText.rectTransform.DOScale(1, 0.5f)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() => HidePopupAfterDelay(3.5f, true, true)); 
                });
            });

        
    }

    public void HidePopupAfterDelay(float delay, bool isCraftingReward = false, bool hideBoth = false)
    {
        if (hidePopupTween != null)
            hidePopupTween.Kill();

        hidePopupTween = DOVirtual.DelayedCall(delay, () => 
        {
            if (hideBoth)
            {
                HideBothPopups();
            }
            else
            {
                GameObject popupToHide = isCraftingReward ? craftingRewardPopup : discoveryPopup;
                popupToHide.transform.DOScale(Vector3.zero, 0.5f) 
                    .SetEase(Ease.InOutQuad)
                    .OnComplete(() => 
                    {
                        popupToHide.SetActive(false);
                        popupToHide.transform.localScale = new Vector3(0.59f, 0.59f, 1); 
                    }); 

            }
        });
    }

    public void HideBothPopups()
    {
        float craftingRewardOriginalY = craftingRewardPopup.transform.position.y - offset;

        craftingRewardPopup.transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => craftingRewardPopup.SetActive(false));

        discoveryPopup.transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => discoveryPopup.SetActive(false));
    }
    public void ClosePopup()
    {
        discoveryPopup.SetActive(false);
        craftingRewardPopup.SetActive(false);
    }
    private void Awake()
    {
        successfulCraftCount = 0;

        foreach (var craftingReward in craftingRewards)
        {
            craftingReward.isAwarded = false;
        }
        for (int i = 0; i < playerInventory.Count && i < gridPositions.Count; i++)
        {
            InstantiateItem(playerInventory[i], gridPositions[i]);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && discoveryPopup.activeInHierarchy)
        {
            ClosePopup();
        }
    }
    private void InstantiateItem(Item item, Transform gridPosition)
    {
        if (item && item.itemPrefab)
        {
    
            StartCoroutine(CheckForWinCondition(item));
            StartCoroutine(CheckForLoseCondition(item));
            
            // Instantiate the item with a scale of zero
            GameObject instantiatedItem = Instantiate(item.itemPrefab, gridPosition.position + item.positionOffset, Quaternion.Euler(item.defaultRotation), gridPosition);
            

            // Ensure that the GameObject is active for the DOTween animation to work
            instantiatedItem.SetActive(true);


            gridPosition.GetComponent<GridPositionController>().containedItem = item;
            OnItemInstantiated.Invoke(gridPosition);
        }
    }

    public void CheckForCrafting(GridPositionController selectedGrid)
    {
        audioSource.PlayOneShot(mouseClickSFX);
        if (firstSelectedItem == null)
        {
            firstSelectedItem = selectedGrid;
            firstSelectedItem.SetAnimationState(ItemAnimationState.Selected); 

            return;
        }
        else if (secondSelectedItem == null)
        {
            
            secondSelectedItem = selectedGrid;
            secondSelectedItem.SetAnimationState(ItemAnimationState.Selected);
            
            StartCoroutine(PerformCrafting());
        }
    }
    
    private IEnumerator PerformCrafting()
    {
        yield return new WaitForSeconds(craftingDelay); 

        bool successfulCraft = false;

        foreach (CraftingRecipe recipe in craftingRecipes)
        {
            if ((recipe.inputList1.Contains(firstSelectedItem.containedItem) && recipe.inputList2.Contains(secondSelectedItem.containedItem)) ||
                (recipe.inputList1.Contains(secondSelectedItem.containedItem) && recipe.inputList2.Contains(firstSelectedItem.containedItem)))
            {
                successfulCraft = true;
                ShowGoodCraftAttemptVFX();

                ShowPopup(recipe.popupInfo);
                successfulCraftCount++;

                Destroy(firstSelectedItem.gameObject.transform.GetChild(0).gameObject);
                Destroy(secondSelectedItem.gameObject.transform.GetChild(0).gameObject);
                firstSelectedItem.containedItem = null;
                secondSelectedItem.containedItem = null;
                firstSelectedItem.DestroyVFX();
                secondSelectedItem.DestroyVFX();

                foreach (var outputItem in recipe.outputItems)
                {
                    Transform availableGridPosition = GetFirstAvailableGridPosition();
                    if (availableGridPosition != null)
                    {
                        audioSource.PlayOneShot(recipe.craftingSoundEffect);
                        InstantiateItem(outputItem, availableGridPosition);
                    }
                    else
                    {
                        Debug.LogWarning("No available grid position for output item");
                    }
                }

                DOVirtual.DelayedCall(1.5f, () => CheckForCraftingReward());
                break;
            }
        }
        if (!successfulCraft)
        {
            ShowBadCraftAttemptVFX();
            audioSource.PlayOneShot(mergeFailedSFX);
            if (firstSelectedItem)
            {
                
                firstSelectedItem.SetAnimationState(ItemAnimationState.Idle);
                secondSelectedItem.SetAnimationState(ItemAnimationState.Idle);
                StartCoroutine(firstSelectedItem.RotateUnsuccessfulMatch());
                StartCoroutine(secondSelectedItem.RotateUnsuccessfulMatch());
                firstSelectedItem.DestroyVFX();
                secondSelectedItem.DestroyVFX();
                // Get a random failed craft prompt from one of the two items
                Item randomItem = UnityEngine.Random.Range(0, 2) == 0 ? firstSelectedItem.containedItem : secondSelectedItem.containedItem;
                if(randomItem != null)
                {
                    ShowPopup(randomItem.failedCraftPrompt);
                }
                
            }
        }

        if (currentVFXInstance)
        {
            Destroy(currentVFXInstance);
        }

        ResetGridPosition(firstSelectedItem);
        ResetGridPosition(secondSelectedItem);

        firstSelectedItem = null;
        secondSelectedItem = null;
    }

    private void CheckForCraftingReward()
    {
        foreach (var craftingReward in craftingRewards)
        {
            Debug.Log($"Checking reward: {craftingReward.rewardMessage}, successfulCraftCount: {successfulCraftCount}, successfulCraftsRequired: {craftingReward.successfulCraftsRequired}, isAwarded: {craftingReward.isAwarded}");
            
            if (successfulCraftCount >= craftingReward.successfulCraftsRequired && !craftingReward.isAwarded)
            {
                ShowCraftingRewardPopup(craftingReward.rewardMessage);
                AddItemToInventory(craftingReward.rewardItem);
                craftingReward.isAwarded = true;
                break;
            }
        }
    }

    public void AddItemToInventory(Item item)
    {
        Transform availableGridPosition = GetFirstAvailableGridPosition();
        if (availableGridPosition != null)
        {
            InstantiateItem(item, availableGridPosition);
        }
        else
        {
            // Handle the case when there's no available grid position. Maybe show a message or do something else.
        }
    }

    private Transform GetFirstAvailableGridPosition()
    {
        for (int i = 0; i < gridPositions.Count; i++)
        {
            if (gridPositions[i].GetComponent<GridPositionController>().containedItem == null)
            {
                return gridPositions[i];
            }
        }
        return null;
    }
    private void ResetGridPosition(GridPositionController gridPos)
    {
        if (gridPos)
        {
            gridPos.transform.localScale = gridPos.defaultScale;
            gridPos.isEnlarged = false;
        }
    }
}