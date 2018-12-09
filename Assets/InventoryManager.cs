using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public StandardGunScript GetActiveWeapon()
    {
        return GetComponentInChildren<WeaponSwitching>().GetActiveWeapon().GetComponent<StandardGunScript>();
    }
}
