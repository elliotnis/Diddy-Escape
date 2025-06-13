using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inventory_manager: MonoBehaviour
{
    public GameObject inventory_menu;
    private bool is_inventory_open = false;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Inventory Manager Update called");

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
}
