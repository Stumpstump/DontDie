using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DDS;
using System.Linq;

public class GameManager : MonoBehaviour
{
    private List<SpawnPosition> PlayerSpawnPositions = new List<SpawnPosition>();

    bool respawnRequested = false;
    GameObject playerToRespawn;

    private void Awake()
    {
        var SpawnPositions = GameObject.FindGameObjectsWithTag("PlayerRespawnNode");
        foreach (var spawnPosition in SpawnPositions)
            PlayerSpawnPositions.Add(spawnPosition.GetComponent<SpawnPosition>());
    }

    //Returns the respawn position of the player
    public void RequestPlayerRespawn(GameObject PlayerObject)
    {

        respawnRequested = true;
        playerToRespawn = PlayerObject;

        
    }

    void RespawnPlayer()
    {
        SpawnAbleObject player = new SpawnAbleObject();
        player.ObjectToSpawn = playerToRespawn;
        player.AdaptableSpawnHeight = 2f;
        player.ApplyLogicToChilds = false;
        List<SpawnPosition> PositionsSortedByDistance = PlayerSpawnPositions;
        PositionsSortedByDistance.Sort((e1, e2) => Vector3.Distance(e1.transform.position, player.ObjectToSpawn.transform.position).CompareTo(Vector3.Distance(e2.transform.position, player.ObjectToSpawn.transform.position)));

        for (int i = 0; i < PositionsSortedByDistance.Count; i++)
        {
            Vector3[] Position;
            if (PositionsSortedByDistance[i].GetPositions(player, 1, null, true, out Position))
            {
                playerToRespawn.transform.position = Position[0];
            }
        }
        respawnRequested = false;
    }

    private void LateUpdate()
    {
        if (respawnRequested)
            RespawnPlayer();
    }


}
