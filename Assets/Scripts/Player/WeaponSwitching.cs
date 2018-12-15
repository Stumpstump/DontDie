﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    [SerializeField]  float mouseWheelCoolDown;

    private float mouseWheelElapsedTime;
    private int selectedWeapon;

    private void Awake()
    {
        SelectWeapon();
    }

    private void Update()
    {
        
        int LastSelectedWeapon = selectedWeapon;
        mouseWheelElapsedTime += Time.deltaTime;

        if(Input.GetAxisRaw("Mouse ScrollWheel") != 0 && mouseWheelElapsedTime >= mouseWheelCoolDown)
        {
            mouseWheelElapsedTime = 0f;

            float AmountToChange = Input.GetAxisRaw("Mouse ScrollWheel");

            if (selectedWeapon <= 0 && AmountToChange < 0f)
            {                
                selectedWeapon = transform.childCount - 1;

            }

            else if (selectedWeapon >= transform.childCount - 1 && AmountToChange > 0f)
            {
                selectedWeapon = 0;
            }

            else
            {
                selectedWeapon += AmountToChange > 0f ? 1 : -1;
                
            }
        }

        else
        {
            int currentKeyCode = 49;
            int currentLoop = 0;

            foreach(var weapon in transform)
            {
                if(Input.GetKey((KeyCode)currentKeyCode))
                {
                    selectedWeapon = currentLoop;
                }

                ++currentLoop;
                ++currentKeyCode;
            }          
        }


        if (LastSelectedWeapon != selectedWeapon)
            SelectWeapon();
    }

    public GameObject GetActiveWeapon()
    {
        return transform.GetChild(selectedWeapon).gameObject;
    }

    public void SelectWeapon()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == selectedWeapon);
        }
    }
}