using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{

    [SerializeField] private Transform m_target;
    [SerializeField] private float m_mouseSensitivity = 10f;

    private float m_verticalRot;
    private float m_horizontalRot;

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

        transform.position = m_target.position;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Update vertical and horizontal rotation
        m_verticalRot -= mouseY * m_mouseSensitivity;
        m_verticalRot = Mathf.Clamp(m_verticalRot, -70f, 70);

        m_horizontalRot += mouseX * m_mouseSensitivity;

        // Smooth the rotation using Lerp
        float smoothFactor = 0.1f;  // Adjust this value for more or less smoothing
        float smoothedVerticalRot = Mathf.LerpAngle(transform.eulerAngles.x, m_verticalRot, smoothFactor);
        float smoothedHorizontalRot = Mathf.LerpAngle(transform.eulerAngles.y, m_horizontalRot, smoothFactor);

        // Apply the smoothed rotation
        transform.rotation = Quaternion.Euler(smoothedVerticalRot, smoothedHorizontalRot, 0f);
    }


}
