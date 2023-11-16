using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Inventory/CraftingRecipe")]
public class CraftingRecipe : ScriptableObject
{
    public List<Item> inputList1 = new List<Item>();
    public List<Item> inputList2 = new List<Item>();
    public List<Item> outputItems = new List<Item>();
    public string popupInfo;  
    [Header("Crafting Sound Effect")]
    public AudioClip craftingSoundEffect; // The new field to store the audio clip
   
}