using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;



public class SpeedField : MonoBehaviour
{    
    public bool ExtendSpeedDuration;

    [Tooltip("Only used if Extend Speed Duration is set to true")]
    public float Duration;

    public int SpeedValue = 0;

    //We use this class of Player Data so that the concept of this field is working if we ever come to multiplayer
    class PlayerData
    {
        public PlayerData(Player.PlayerController player)
        {
            Player = player;
            elapsedTimeSinceExit = 0f;
            isExitTimeActive = false;
        }

        public void Update(float Duration, int SpeedValue)
        {
            //Check if the Player still has a movement speed buff
            if(isExitTimeActive)
            {
                //If the elapsed time is bigger than the duration remove the buff
                elapsedTimeSinceExit += Time.deltaTime;                
                if(elapsedTimeSinceExit >= Duration)
                {
                    isExitTimeActive = false;
                    Player.ChangeSpeedFactor(-SpeedValue);
                }
            }
        }

        public void PlayerEntered(int SpeedValue)
        {
            //Buff the Players movement speed
            if(!isExitTimeActive)
                Player.ChangeSpeedFactor(SpeedValue);
            else
                isExitTimeActive = false;
        }

        public void PlayerLeft(int SpeedValue, bool ExtendSpeedDuration)
        {
            //If the movement speed shouldn't extend remove the buff
            if (!ExtendSpeedDuration)
                Player.ChangeSpeedFactor(-SpeedValue);
            else
            {
                elapsedTimeSinceExit = 0f;
                isExitTimeActive = true;
            }
        }

        public Player.PlayerController Player;
        public float elapsedTimeSinceExit;

        public bool isExitTimeActive;
    }

    List<PlayerData> Players = new List<PlayerData>();

    void Update()
    {
        for (int i = 0; i < Players.Count; i++)
        {
            Players[i].Update(Duration, SpeedValue);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        //Check if the collider is a player
        if(collider.GetComponent<Player.PlayerController>() != null)
        {
            //Check if the Player is already in the list
            //If he is in the list set his elapsed time since exit to zero and return, otherwise add him to the list
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Player == collider.GetComponent<Player.PlayerController>())
                {
                    Players[i].PlayerEntered(SpeedValue);
                    return;
                }
            }

            PlayerData newPlayer = new PlayerData(collider.GetComponent<Player.PlayerController>());
            newPlayer.PlayerEntered(SpeedValue);
            Players.Add(newPlayer);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if(collider.GetComponent<Player.PlayerController>() != null)
        {
            for(int i = 0; i < Players.Count; i++)
            { 
                if(Players[i].Player == collider.GetComponent<Player.PlayerController>())
                {
                    Players[i].PlayerLeft(SpeedValue, ExtendSpeedDuration);
                    return;
                }
            }
        }
    }
}


