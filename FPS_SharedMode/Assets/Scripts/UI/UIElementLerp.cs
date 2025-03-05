using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIElementLerp : MonoBehaviour
{

    private RectTransform rect;
    [SerializeField] private RectTransform parent;
    [SerializeField] private float m_duration = 1.0f;
    [SerializeField] private float m_delayedCall = 0.0f;
    [SerializeField] private Ease m_ease = Ease.OutBack;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        DOVirtual.DelayedCall(m_delayedCall, () =>
        {
            rect.DOAnchorPos(Vector2.zero, m_duration).SetEase(m_ease);
        });
    }
}
