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
            player.SetCanMove(false);
            player.RPC_Win();
            StartCoroutine(RespawnDelay(player));
        }
    }

    private IEnumerator RespawnDelay(Movement player)
    {
        yield return new WaitForSeconds(3.0f);

        foreach (var _player in GameManager.instance.GetAllPlayers())
        {
            _player.RPC_Respawn();
        }
        //player.RPC_Respawn();
    }

}
