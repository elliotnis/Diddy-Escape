using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class item_slot : MonoBehaviour, IPointerClickHandler
{
    public string item_name;
    public int quantity;
    public Sprite item_icon;
    public bool is_full;

    public TMP_Text quantity_text;
    public Image item_icon_image;

    public GameObject selected_grid;
    public bool is_selected;

    public inventory_manager inventoryManager;

    private void Start()
    {
        inventoryManager = GameObject.Find("Inventory_Canvas").GetComponent<inventory_manager>();
    }
    public void add_item(string item_name, Sprite item_icon)
    {
        this.item_name = item_name;
        quantity++;
        
        this.item_icon = item_icon;
        is_full = true;

        quantity_text.text = quantity.ToString();
        quantity_text.enabled = true;
        item_icon_image.sprite = item_icon;
    }
 
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Left click on item slot: ");
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }

        
    }

    public void OnLeftClick()
    {
        inventoryManager.DeselectAllSlots();
        selected_grid.SetActive(true);
        is_selected = true;
    }
    public void OnRightClick()
    {
      
    }
}
