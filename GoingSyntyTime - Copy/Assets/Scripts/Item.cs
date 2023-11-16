using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create New Item", fileName = "NewItem")]
public class Item : ScriptableObject
{
    
    public string itemName;         
    public GameObject itemPrefab;   
    public Vector3 defaultRotation; 
    public Vector3 positionOffset;  // The new position offset variable

    [TextArea(3, 10)] // Makes the tooltip field a larger input box in the inspector
    public string tooltipText;
    [TextArea]
    public string failedCraftPrompt;  // Add this line to include the field in your Item class
}