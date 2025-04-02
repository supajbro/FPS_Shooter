using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{

    [SerializeField] private List<Image> m_balloons;
    private int m_activeBalloons = 0;
    private int m_previousActiveBalloons = -1;

    [Header("Network Values")]
    [SerializeField] private TextMeshProUGUI m_networkText;

    [Header("Animation Values")]
    [SerializeField] private float m_initPos = -75f;
    [SerializeField] private float m_moveToPos = 250f;

    public float InitPos => m_initPos;
    public float MoveToPos => m_moveToPos;

    private void Update()
    {
        m_networkText.text = "Session: " + GameManager.instance.MainMenu.CurrentSessionName;

        ActiveBalloonsUpdate();
    }

    private void ActiveBalloonsUpdate()
    {
        if (m_balloons.Count <= 0)
        {
            return;
        }

        if (GameManager.instance.GetLocalPlayer() == null)
        {
            return;
        }

        if (GameManager.instance.GetLocalPlayer())
        {
            m_activeBalloons = GameManager.instance.GetLocalPlayer().ActiveBallons;
        }

        // Iterate over balloons and set positions based on active balloons count
        for (int i = 0; i < m_balloons.Count; i++)
        {
            float target = (i < m_activeBalloons) ? m_initPos : m_moveToPos;
            MoveBalloon(i, target, Ease.OutBack);
        }
    }

    private void MoveBalloon(int index, float yPos, Ease ease)
    {
        m_balloons[index].transform.DOLocalMoveY(yPos, 1.0f).SetEase(ease);
    }

}
