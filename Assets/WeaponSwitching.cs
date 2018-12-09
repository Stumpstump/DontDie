using System.Collections;
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

            Debug.Log(selectedWeapon);
        }

        else
        {
            if (Input.GetKey(KeyCode.Alpha1))
                selectedWeapon = 0;

            else if (Input.GetKey(KeyCode.Alpha2) && transform.childCount > 1)
                selectedWeapon = 1;

            else if (Input.GetKey(KeyCode.Alpha3) && transform.childCount > 2)
                selectedWeapon = 2;
        }


        if (LastSelectedWeapon != selectedWeapon)
            SelectWeapon();
    }

    public GameObject GetActiveWeapon()
    {
        return transform.GetChild(selectedWeapon).gameObject;
    }

    private void SelectWeapon()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == selectedWeapon);
        }
    }
}
