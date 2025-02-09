using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFOV : MonoBehaviour
{

    private Camera m_cam;

    [Header("FOV Values")]
    [SerializeField] private float m_minFOV = 75.0f;
    [SerializeField] private float m_maxFOV = 85.0f;
    [SerializeField] private float m_scaleDuration = 1.0f;
    [SerializeField] private AnimationCurve m_scaleCurve;

    private bool m_scaleFOV = false;
    private float m_initValue = 0.0f;
    private float m_toScaleValue = 0.0f;
    private float m_scaleElapsedTime = 0.0f;

    private void Awake()
    {
        m_cam = GetComponent<Camera>();
    }

    private void Update()
    {
        ScaleFOVUpdate();
    }

    private void ScaleFOVUpdate()
    {
        if (!m_scaleFOV) 
        { 
            return; 
        }

        m_scaleElapsedTime += Time.deltaTime;
        if (m_scaleElapsedTime >= m_scaleDuration)
        {
            m_scaleFOV = false;
            m_scaleElapsedTime = 0.0f;
            return;
        }

        float progress = m_scaleElapsedTime / m_scaleDuration;
        m_cam.fieldOfView = Mathf.LerpUnclamped(m_initValue, m_toScaleValue, m_scaleCurve.Evaluate(progress));
    }

    public void InitFOVScale(float scaleTo, bool forceScale)
    {
        if (m_scaleFOV && !forceScale)
        {
            return;
        }
        m_initValue = m_cam.fieldOfView;
        m_scaleFOV = true;
        m_toScaleValue = scaleTo;
    }

}
