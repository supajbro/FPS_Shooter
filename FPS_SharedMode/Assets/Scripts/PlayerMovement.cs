using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{

    private CharacterController m_controller;
    [SerializeField] private Transform m_camPos;
    [SerializeField] private PlayerWeapon m_weapon;

    private Camera m_camera;
    public Camera Cam { get { return m_camera; } }

    [SerializeField] private LayerMask m_groundLayer;

    [Header("Values")]
    [SerializeField] private float m_speed = 2f;
    private Vector3 m_velocity;

    [Header("Jump Values")]
    [SerializeField] private float m_jumpForce = 5f;
    [SerializeField] private float m_maxJumpForce = 5f;
    private bool m_jumpPressed;

    public bool isGrounded => IsGrounded();

    private void Awake()
    {
        m_controller = GetComponent<CharacterController>();
    }

    public override void Spawned()
    {
        // Is local player
        if (HasStateAuthority)
        {
            m_camera = Camera.main;
            m_camera.GetComponent<FirstPersonCamera>().SetTarget(m_camPos);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_jumpPressed = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // FixedUpdateNetwork is only executed on the StateAuthority

        Quaternion camRotY = Quaternion.Euler(0, m_camera.transform.rotation.eulerAngles.y, 0);
        Vector3 moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 move = camRotY * moveInput * Runner.DeltaTime * m_speed;

        gameObject.transform.rotation = camRotY;
        transform.position += move;

        Quaternion camRotX = Quaternion.Euler(m_camera.transform.rotation.eulerAngles.x, m_camera.transform.rotation.eulerAngles.y, 0);
        m_weapon.transform.rotation = camRotX;

        // Initialise the jump
        if (m_jumpPressed && IsGrounded())
        {
            m_jumpPressed = false;
            m_jumpForce = m_maxJumpForce;
        }

        // Update the jump force when the player is off the ground
        if (!IsGrounded())
        {
            m_jumpForce -= Runner.DeltaTime * 7.5f;
            m_jumpForce = Mathf.Clamp(m_jumpForce, -m_maxJumpForce, m_maxJumpForce);
        }

        // Update the y velocity and reset it to 0 if player is grounded
        m_velocity.y += m_jumpForce;
        if(IsGrounded() && m_jumpForce != m_maxJumpForce)
        {
            m_velocity.y = 0f;
        }

        m_controller.Move(move + m_velocity * Runner.DeltaTime);
    }

    private bool IsGrounded()
    {
        const float groundCheckDistance = 1.5f;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, m_groundLayer))
        {
            Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.blue);
            return true;
        }

        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.red);
        return false;
    }

}
