using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI m_ammoCount;
    [SerializeField] private TextMeshProUGUI m_ballonCount;

    private void Update()
    {
        if (GameManager.instance.GetLocalPlayer())
        {
            m_ammoCount.text = $"{GameManager.instance.GetLocalPlayer().Weapon.AmmoCount}/{GameManager.instance.GetLocalPlayer().Weapon.MaxAmmoCount}";
            m_ballonCount.text = $"Balloons: {GameManager.instance.GetLocalPlayer().ActiveBallons}";
        }
    }

}
