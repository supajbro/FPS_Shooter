using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnZone : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Movement>(out Movement controller))
        {
            if (controller is PlayerMovement player)
            {
                player.RPC_RespawnPlayer();
            }
            else if (controller is BotMovement bot)
            {
                Debug.Log($"[Respawn] Bot respawned {bot.PlayerName}");
                bot.RPC_Respawn();
            }
        }
    }

}
