using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnZone : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Movement>(out Movement controller))
        {
            controller.RPC_Respawn();
        }
    }

}
