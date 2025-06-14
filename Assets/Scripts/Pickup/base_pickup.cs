using System;
using UnityEngine;


public class base_pickup : MonoBehaviour
{

    private float time;
    private float initial_y_position;
    private float oscillate_scale;

    public Sprite item_icon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        oscillate_scale = 0.1f;
        initial_y_position = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        transform.position = new Vector3(transform.position.x, initial_y_position + Mathf.Sin(Mathf.PI * time) * oscillate_scale, transform.position.z);

    }



    
}

