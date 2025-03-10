using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingVisual : MonoBehaviour
{

    [SerializeField] private List<Image> m_loadingVisuals;
    private Image m_currentImage;
    private Image m_previousImage;
    private int m_currentIndex = 0;

    [SerializeField] private Color m_normalColor;
    [SerializeField] private Color m_highlightedColor;

    private float m_timeElapsed = 0.0f;
    private const float MaxTimeElapsed = 0.05f;

    private void OnEnable()
    {
        m_currentIndex = 0;
        SetCurrentLoadingVisual(m_currentIndex);
    }

    private void Update()
    {
        LoadingUpdate();
    }

    private void LoadingUpdate()
    {
        m_timeElapsed += Time.deltaTime;
        if(m_timeElapsed >= MaxTimeElapsed)
        {
            m_timeElapsed = 0.0f;
            m_currentIndex++;
            SetCurrentLoadingVisual(m_currentIndex);
        }
    }

    private void SetCurrentLoadingVisual(int index)
    {
        if(index < 0 ||  index >= m_loadingVisuals.Count)
        {
            index = 0;
            m_currentIndex = index;
        }

        if (m_currentImage != null)
        {
            m_previousImage = m_currentImage;
            m_previousImage.color = m_normalColor;
        }
        m_currentImage = m_loadingVisuals[index];
        m_currentImage.color = m_highlightedColor;
    }

}
