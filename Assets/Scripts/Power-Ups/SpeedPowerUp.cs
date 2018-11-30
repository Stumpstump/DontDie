using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpeedPowerUp : MonoBehaviour
{
    public int SpeedValue;
    public float Duration;

    private float elapsedTime;
    private Player.PlayerMovement player = null;
    
    void OnPickedUp()
    {
        //Play a fancy sound and some kind of particle system
        this.gameObject.SetActive(false);
    }
   
    void UpdatePickUp(object source, EventArgs e)
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime >= Duration)
        {
            player.ChangeSpeedFactor(-SpeedValue);
            player.PowerUps -= UpdatePickUp;
            GameObject.Destroy(this.gameObject);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if(collider.GetComponent<Player.PlayerMovement>() != null)
        {
            player = collider.GetComponent<Player.PlayerMovement>();
            player.PowerUps += this.UpdatePickUp;
            player.ChangeSpeedFactor(SpeedValue);

            OnPickedUp();
        }
    }


}
