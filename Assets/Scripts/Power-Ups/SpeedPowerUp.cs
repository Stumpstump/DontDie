using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpeedPowerUp : MonoBehaviour, IPowerUp
{
    public float SpeedValue;
    public float Duration;

    public void OnPickedUp(Player.PlayerMovement player)
    {
        player.SetNewSpeedPowerUp(this);
        GameObject.Destroy(this.gameObject);
    }
}
