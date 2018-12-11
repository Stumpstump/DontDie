using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace Player
{
    public class Inventory : MonoBehaviour
    {
        private WeaponSwitching Weapons;
        private Dictionary<string, List<GameObject>> Items = new Dictionary<string, List<GameObject>>();
        private List<GameObject> EquipedWeapons = new List<GameObject>();


        private void Awake()
        {
            Weapons = this.GetComponentInChildren<WeaponSwitching>();


        }

        private void Update()
        {
        }

        //Returns false if an item with the same unique id already exists
        public bool AddItem(GameObject source, GameObject item)
        {
            if(Items.ContainsKey(item.tag))
            {
               GameObject searchObject = Items[item.tag].Find(x => x.gameObject.name == item.name);

                if (searchObject == null || source.GetComponent<Receivable>().canStack)// || source.GetComponent<Receivable>().canStack)
                {
                    Items[item.tag].Add(item);
                }

                else
                    return false;
            }

            else
            {
                Items.Add(item.tag, new List<GameObject>());
                Items[item.tag].Add(item);
            }

            if (item.tag == "Weapon")
            {
                if (Weapons.transform.childCount < 9)
                {
                    var newWeapon = Instantiate(item, Weapons.transform);
                    Weapons.SelectWeapon();

                }
            }

            return true;           
        }

        public bool DeleteItem(GameObject item)
        {
            if(Items.ContainsKey(item.tag))
            {
                GameObject searchedObject = Items[item.tag].Find(x => x.GetComponent<Receivable>().UniqueItemId == item.GetComponent<Receivable>().UniqueItemId);

                if(searchedObject == null)
                {
                    if (item.tag == "Weapon")
                    {
                        if (item.GetComponent<StandardGunScript>())
                        {
                            for (int i = 0; i < Weapons.transform.childCount; i++)
                            {
                                if (item.GetComponent<Receivable>().UniqueItemId == Weapons.transform.GetChild(i).GetComponent<Receivable>().UniqueItemId)
                                {
                                    GameObject.Destroy(Weapons.transform.GetChild(i));
                                    break;
                                }
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
            
        }
    }
}

