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

        Vector3 nextPos = Vector3.zero;
        if (m_nextPath != null)
        {
            nextPos = m_nextPath.transform.position;
            nextPos.y = 0;
        }

        // If bot is close to the path
        if (Vector3.Distance(botPos, pos) < m_minPathDistance)
        {
            return true;
        }

        // If bot is closer to next path
        if (Vector3.Distance(botPos, nextPos) < Vector3.Distance(botPos, pos))
        {
            return true;
        }

        return false;
    }

    public bool NearPathPoint(BotMovement bot)
    {
        Vector3 botPos = bot.transform.position;
        botPos.y = 0;

        Vector3 pos = transform.position;
        pos.y = 0;

        if (Vector3.Distance(botPos, pos) < m_minPathDistance * 2)
        {
            return true;
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
