using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Receivable : MonoBehaviour
{
    public bool canStack = false;
    [SerializeField] private GameObject Prefab;
    [SerializeField] private float _UniqueItemId;

    public float UniqueItemId
    {
        get
        {
            return _UniqueItemId;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
            other.GetComponent<Player.Inventory>().AddItem(this.gameObject, this.Prefab);
            GameObject.Destroy(this.gameObject);
    }

}
