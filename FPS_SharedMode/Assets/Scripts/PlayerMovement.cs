using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{

    private CharacterController m_controller;
    private Camera m_camera;
    private PlayerWeapon m_weapon;
    [SerializeField] private Transform m_weaponRotator;

    private Vector3 m_velocity;
    private bool m_jumpPressed;

    [Header("Values")]
    [SerializeField] private float m_speed = 2f;
    [SerializeField] private float m_jumpForce = 5f;
    [SerializeField] private float m_gravity = -9.81f;

    private void Awake()
    {
        m_controller = GetComponent<CharacterController>();
        m_weapon = GetComponent<PlayerWeapon>();
    }

    public override void Spawned()
    {
        // Is local player
        if (HasStateAuthority)
        {
            m_camera = Camera.main;
            m_camera.GetComponent<FirstPersonCamera>().SetTarget(transform);
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

        if (!m_controller.isGrounded)
        {
            m_velocity = new Vector3(0, -1, 0);
        }

        Quaternion camRotY = Quaternion.Euler(0, m_camera.transform.rotation.eulerAngles.y, 0);
        Vector3 moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 move = camRotY * moveInput * Runner.DeltaTime * m_speed;

        gameObject.transform.rotation = camRotY;
        transform.position += move;

        Quaternion camRotX = Quaternion.Euler(m_camera.transform.rotation.eulerAngles.x, m_camera.transform.rotation.eulerAngles.y, 0);
        Debug.Log("CAM: " + camRotX);
        m_weaponRotator.rotation = camRotX;

        m_velocity.y += m_gravity * Runner.DeltaTime;
        if(m_jumpPressed && m_controller.isGrounded)
        {
            m_velocity.y += m_jumpForce;
        }

        m_controller.Move(move + m_velocity * Runner.DeltaTime);

        m_jumpPressed = false;
    }

}
