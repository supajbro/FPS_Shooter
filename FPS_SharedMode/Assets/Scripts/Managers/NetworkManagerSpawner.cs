using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerSpawner : SimulationBehaviour, IPlayerJoined
{

    public GameObject botPoolerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        Runner.Spawn(botPoolerPrefab, new Vector3(0,0,0), Quaternion.identity);
    }
}
