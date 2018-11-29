using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;


public interface IPowerUp
{
    void OnPickedUp(Player.PlayerMovement player);
}
