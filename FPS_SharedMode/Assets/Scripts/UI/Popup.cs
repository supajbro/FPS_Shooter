using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{

    public static Popup Instance;

    [Header("Popup")]
    [SerializeField] private CanvasGroup m_canvasGroup;
    [SerializeField] private RectTransform m_popupRect;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI m_popupTitle;
    [SerializeField] private TMP_InputField m_inputField;
    [SerializeField] private Button m_buttonYes;
    [SerializeField] private TextMeshProUGUI m_buttonYesText;
    [SerializeField] private Button m_buttonNo;
    [SerializeField] private TextMeshProUGUI m_buttonNoText;

    public TMP_InputField InputField => m_inputField;

    private const float Duration = 0.5f;

    private void Awake()
    {
        Instance = this;
    }

    public void Display(bool displayInputField, Action buttonYes, Action buttonNo, string popupTitle, string buttonYesString, string buttonNoString)
    {
        m_popupRect.transform.DOScale(1, Duration).SetEase(Ease.OutBack);
        m_canvasGroup.interactable = true;
        m_canvasGroup.blocksRaycasts = true;

        m_inputField.gameObject.SetActive(displayInputField);

        m_popupTitle.text = popupTitle;
        m_buttonYesText.text = buttonYesString;
        m_buttonNoText.text = buttonNoString;

        // Ensure previous listeners are cleared to prevent duplicate calls
        m_buttonYes.onClick.RemoveAllListeners();
        m_buttonNo.onClick.RemoveAllListeners();

        m_buttonYes.onClick.AddListener(() => buttonYes?.Invoke());
        m_buttonNo.onClick.AddListener(() => buttonNo?.Invoke());
    }

    public void Close()
    {
        m_popupRect.transform.DOScale(0, Duration).SetEase(Ease.InBack);
        m_canvasGroup.interactable = false;
        m_canvasGroup.blocksRaycasts = false;
    }

}
