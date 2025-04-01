using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnZone : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Movement>(out Movement controller))
        {
            controller.SetDead();
            //if (controller is PlayerMovement player)
            //{
            //    player.RPC_RespawnPlayer();
            //}
            //else if (controller is BotMovement bot)
            //{
            //    Debug.Log($"[Respawn] Bot respawned {bot.PlayerName}");
            //    bot.RPC_Respawn();
            //}
        }

        if(other.gameObject.TryGetComponent<PlayerMovement>(out PlayerMovement _player) == GameManager.instance.GetLocalPlayer())
        {
            GameManager.instance.PlayTransition();
        }
    }

}
