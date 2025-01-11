using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{

    public GameObject playerPrefab;
    public Vector3 spawnPos = new Vector3(0, 1, 0);

    public void PlayerJoined(PlayerRef player)
    {
        if(player == Runner.LocalPlayer)
        {
            Runner.Spawn(playerPrefab, spawnPos, Quaternion.identity);
        }
    }

}
