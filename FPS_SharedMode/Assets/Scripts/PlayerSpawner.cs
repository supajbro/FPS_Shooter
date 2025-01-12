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
            var randSpawnPos = Random.Range(0, GameManager.instance.spawnPoints.Count);
            var spawnedPlayer = Runner.Spawn(playerPrefab, GameManager.instance.spawnPoints[randSpawnPos].position, Quaternion.identity);
            spawnedPlayer.name += Random.Range(0, 10000);
        }
    }

}
