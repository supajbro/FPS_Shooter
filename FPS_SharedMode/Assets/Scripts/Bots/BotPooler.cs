using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotPooler : MonoBehaviour
{

    [SerializeField] private List<BotPath> m_botPaths = new();
    [SerializeField] private List<BotMovement> m_bots = new();

    #region - Getters -
    public List<BotPath> BotPaths => m_botPaths;
    public List<BotMovement> Bots => m_bots;
    #endregion

    private void Awake()
    {
        // Grab the list and reverse it so the first one the bots should be at is the first in the list
        m_botPaths = FindObjectsOfType<BotPath>().ToList();
        m_botPaths.Reverse();

        // Grab all of the bots that are idle
        m_bots = FindObjectsOfType<BotMovement>().ToList();
    }

}
