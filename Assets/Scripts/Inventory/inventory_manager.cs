using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inventory_manager : MonoBehaviour
{
    public GameObject inventory_menu;
    private bool is_inventory_open = false;

    public item_slot[] item_slots;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        // Check if the 'I' key is pressed to toggle the inventory
        if (Input.GetButtonDown("Inventory") && is_inventory_open)
        {
            Time.timeScale = 1f; // Start the game
            Debug.Log("Inventory closed");
            inventory_menu.SetActive(false);
            is_inventory_open = false;
        }
        else if (Input.GetButtonDown("Inventory") && !is_inventory_open)
        {
            Time.timeScale = 0; //Stop the game
            Debug.Log("Inventory open");
            inventory_menu.SetActive(true);
            is_inventory_open = true;
        }

    }

    public void AddItemToInventory(string item_name, Sprite sprite)
    {
        for (int i = 0; i < item_slots.Length; i++)
        {
            if (item_slots[i].is_full == false)
            {
                item_slots[i].add_item(item_name, sprite);
                Debug.Log("Item added to slot: " + item_name);
                return;
            }
        }

        Debug.Log("Item added to inventory: " + item_name);
        return;
    }

    public void DeselectAllSlots()
    {
        for(int i = 0; i< item_slots.Length; i++)
        {
            item_slots[i].selected_grid.SetActive(false);
            item_slots[i].is_selected = false;
        }
    }
}
