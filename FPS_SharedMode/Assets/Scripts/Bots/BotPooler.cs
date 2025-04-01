using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotPooler : NetworkBehaviour
{

    [SerializeField] private NetworkObject m_botPrefab;
    [SerializeField] private int m_botSpawnCount = 5;
    [SerializeField] private List<BotMovement> m_bots = new(); 

    #region - Getters -
    public List<BotMovement> Bots => m_bots;
    #endregion

    private void Awake()
    {
        // Grab all of the bots that are idle
        m_bots = FindObjectsOfType<BotMovement>().ToList();
    }

    public override void Spawned()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        StartCoroutine(SpawnBotsDelay());
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
    }

    private IEnumerator SpawnBotsDelay()
    {
        for (int i = 0; i < m_botSpawnCount; i++)
        {
            yield return new WaitForSeconds(10f); // Wait for 5 seconds before spawning the next bot
            SpawnBot();
        }
    }

    private void SpawnBot()
    {
        var rand = Random.Range(0, GameManager.instance.spawnPoints.Count);

        // Spawn the bullet at the spawn point with the correct rotation
        NetworkObject bot = Runner.Spawn(
            m_botPrefab,
            GameManager.instance.spawnPoints[rand].transform.position,
            transform.rotation,
            Object.InputAuthority,
            (runner, obj) =>
            {
                var _bot = obj.GetComponent<BotMovement>();
                _bot.m_controller.enabled = false;
                _bot.SetPath(0);
                _bot.m_controller.enabled = true;
            });
        m_bots.Add(bot.GetComponent<BotMovement>());
    }

}
