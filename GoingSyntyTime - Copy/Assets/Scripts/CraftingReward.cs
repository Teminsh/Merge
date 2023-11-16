using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Inventory/CraftingReward")]
public class CraftingReward : ScriptableObject
{
    public int successfulCraftsRequired;
    public Item rewardItem;
    public string rewardMessage; // Add this line to store the reward message
    public bool isAwarded = false;
}