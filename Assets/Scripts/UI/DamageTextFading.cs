using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DamageTextFading : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(DestroyOnAnimationFinished());
    }

    private IEnumerator DestroyOnAnimationFinished()
    {
        yield return new WaitForSeconds(this.GetComponent<Animation>().clip.length);
        Destroy(this.gameObject);
    }

}