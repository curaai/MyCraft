using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ItemSlot : MonoBehaviour
{
    // * Item class can be replace int type of property ItemID 
    public BlockType ItemID;
    public Image Icon;
}