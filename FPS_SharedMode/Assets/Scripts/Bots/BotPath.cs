using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotPath : MonoBehaviour
{

    [SerializeField] private float m_minPathDistance = 1.0f;
    [SerializeField] private BotPath m_nextPath;
    [SerializeField] private int m_pathIndex;
    [SerializeField] private bool m_initPathPoint = false;

    public BotPath NextPath => m_nextPath;
    public int PathIndex => m_pathIndex;
    public bool InitPathPoint => m_initPathPoint;

    public void SetPathIndex (int index)
    {
        m_pathIndex = index;
    }

    public bool ReachedPathPoint(BotMovement bot)
    {
        Vector3 botPos = bot.transform.position;
        botPos.y = 0;

        Vector3 pos = transform.position;
        pos.y = 0;

        if (Vector3.Distance(botPos, pos) < m_minPathDistance)
        {
            return true;
        }

        // Check if bot is closer to its next path
        if (m_nextPath)
        {
            float distToCurrentPath = Vector3.Distance(transform.position, bot.transform.position);
            float distToNextPath = Vector3.Distance(m_nextPath.transform.position, bot.transform.position);
            return distToNextPath < distToCurrentPath;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_minPathDistance);

        if (m_nextPath)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, m_nextPath.transform.position);
        }
    }

}
