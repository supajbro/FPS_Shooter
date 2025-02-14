using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotPooler : NetworkBehaviour
{

    [SerializeField] private NetworkObject m_botPrefab;
    [SerializeField] private int m_botSpawnCount = 5;
    [SerializeField] private List<BotPath> m_botPaths = new();
    [SerializeField] private List<BotMovement> m_bots = new(); 

    #region - Getters -
    public List<BotPath> BotPaths => m_botPaths;
    public List<BotMovement> Bots => m_bots;
    #endregion

    private void Awake()
    {
        void AssignPathIndices()
        {
            m_botPaths = FindObjectsOfType<BotPath>().ToList();

            // Find the starting path (InitPathPoint == true)
            BotPath startPath = m_botPaths.FirstOrDefault(path => path.InitPathPoint);
            if (startPath == null)
            {
                Debug.LogError("No starting path found. Make sure the initial path you want the bots to start at is true.");
                return;
            }

            // Clear and rebuild list in correct order
            List<BotPath> orderedPaths = new List<BotPath>();
            BotPath currentPath = startPath;
            int index = 0;

            while (currentPath != null)
            {
                // Assign index and add to the ordered list
                currentPath.SetPathIndex(index);
                orderedPaths.Add(currentPath);
                Debug.Log($"Assigned Index {index} to {currentPath.name}");

                // Move to the next path
                currentPath = currentPath.NextPath;
                index++;
            }

            // Replace m_botPaths with the correctly ordered list
            m_botPaths = orderedPaths;
        }
        AssignPathIndices();

        // Grab all of the bots that are idle
        m_bots = FindObjectsOfType<BotMovement>().ToList();
    }

    public override void Spawned()
    {
        StartCoroutine(SpawnBotsDelay());
    }

    private IEnumerator SpawnBotsDelay()
    {
        for (int i = 0; i < m_botSpawnCount; i++)
        {
            SpawnBot();
            yield return new WaitForSeconds(5f); // Wait for 5 seconds before spawning the next bot
        }
    }

    private void SpawnBot()
    {
        // Spawn the bullet at the spawn point with the correct rotation
        NetworkObject bot = Runner.Spawn(
            m_botPrefab,
            m_botPaths[0].transform.position,
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
