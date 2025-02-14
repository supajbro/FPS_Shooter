using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotPath : MonoBehaviour
{

    [SerializeField] private float m_minPathDistance = 1.0f;
    [SerializeField] private BotPath m_nextPath;
    
    public bool ReachedPathPoint(BotMovement bot)
    {
        if (Vector3.Distance(bot.transform.position, transform.position) < m_minPathDistance)
        {
            return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.0f);

        if (m_nextPath)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, m_nextPath.transform.position);
        }
    }

}
