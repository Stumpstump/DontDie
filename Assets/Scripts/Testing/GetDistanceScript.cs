using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetDistanceScript : MonoBehaviour
{
    [SerializeField] private GameObject objectOne;
    [SerializeField] private GameObject objectTwo;

    private void FixedUpdate()
    {
        Debug.Log(Vector3.Distance(new Vector3(objectOne.transform.position.x ,0, objectOne.transform.position.z), new Vector3(objectTwo.transform.position.x, 0, objectTwo.transform.position.z)));
    }
}
