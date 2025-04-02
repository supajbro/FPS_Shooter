using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{

    [Header("Main Components")]
    [SerializeField] private float m_mouseSensitivity = 10f;
    [SerializeField] private Transform m_shootDirection;
    [SerializeField, Range(0f, 1f)] private float m_smoothFactor = 0.5f;
    private Transform m_target;

    private float m_verticalRot;
    private float m_horizontalRot;

    public Transform ShootDirection => m_shootDirection;

    public void SetTarget(Transform target)
    {
        m_target = target;
    }

    private void LateUpdate()
    {
        if (m_target == null)
        {
            return;
        }

        m_mouseSensitivity = GameManager.instance.sensitivity.value;

        transform.position = m_target.position;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Update vertical and horizontal rotation
        m_verticalRot -= mouseY * m_mouseSensitivity;
        m_verticalRot = Mathf.Clamp(m_verticalRot, -70f, 70);

        m_horizontalRot += mouseX * m_mouseSensitivity;

        // Smooth the rotation using Lerp
        float smoothedVerticalRot = Mathf.LerpAngle(transform.eulerAngles.x, m_verticalRot, m_smoothFactor);
        float smoothedHorizontalRot = Mathf.LerpAngle(transform.eulerAngles.y, m_horizontalRot, m_smoothFactor);

        // Apply the smoothed rotation
        transform.rotation = Quaternion.Euler(smoothedVerticalRot, smoothedHorizontalRot, 0f);
    }


}
