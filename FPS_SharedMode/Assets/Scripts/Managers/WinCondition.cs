using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCondition : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            var player = other.gameObject.GetComponent<Movement>();

            if (player.GameOver)
            {
                return;
            }

            player.SetCanMove(false);
            player.RPC_Win();
            StartCoroutine(RespawnDelay());

            foreach (var _player in GameManager.instance.GetAllPlayers())
            {
                if (_player is PlayerMovement player2)
                {
                    Debug.Log($"[Respawn] Player respawned {_player.PlayerName}");
                    player2.RPC_RespawnPlayer();
                    //player2.HasWon = false;
                    //player2.GameOver = false;
                }
                else if (_player is BotMovement bot)
                {
                    Debug.Log($"[Respawn] Bot respawned {_player.PlayerName}");
                    bot.RPC_Respawn();
                    //bot.HasWon = false;
                    //bot.GameOver = false;
                }
            }
        }
    }

    private IEnumerator RespawnDelay()
    {
        foreach (var _player in GameManager.instance.GetAllPlayers())
        {
            Debug.Log($"[Respawn] Stopped movement {_player.PlayerName}");
            _player.RPC_StopMovement();
        }

        yield return new WaitForSeconds(3.0f);

        foreach (var _player in GameManager.instance.GetAllPlayers())
        {
            if (_player is PlayerMovement player)
            {
                Debug.Log($"[Respawn] Player respawned {_player.PlayerName}");
                //player.RPC_RespawnPlayer();
                player.HasWon = false;
                player.GameOver = false;
            }
            else if (_player is BotMovement bot)
            {
                Debug.Log($"[Respawn] Bot respawned {_player.PlayerName}");
                //bot.RPC_Respawn();
                bot.HasWon = false;
                bot.GameOver = false;
            }
        }
    }

}
