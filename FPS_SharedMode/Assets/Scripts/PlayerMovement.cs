using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{

    private CharacterController m_controller;
    [SerializeField] private MeshRenderer m_playerMesh;
    [SerializeField] private Transform m_camPos;
    [SerializeField] private PlayerWeapon m_weapon;
    public PlayerWeapon Weapon { get { return m_weapon; } }

    private Camera m_camera;
    public Camera Cam { get { return m_camera; } }

    [SerializeField] private LayerMask m_groundLayer;

    [Header("Values")]
    [SerializeField] private float m_speed = 2f;
    private Vector3 m_velocity;
    private float m_moveVelocity = 0f;
    [SerializeField] private float m_speedIncreaseScale = 0.75f;
    [SerializeField] private float m_minMoveVelocity = 0.25f;
    [SerializeField] private float m_maxMoveVelocity = 1f;

    [Header("Jump Values")]
    [SerializeField] private float m_jumpForce;
    [SerializeField] private float m_maxJumpForce;
    [SerializeField] private float m_initialMaxJumpForce = 2f;
    private bool m_jumpPressed;

    [Header("Health Values")]
    [SerializeField] private float m_maxHealth = 100f;
    public float MaxHealth { get { return m_maxHealth; } }
    [Networked] public float CurrentHealth { get; set; }

    [Header("Respawn Values")]
    private bool m_respawning = false;
    private float m_respawnTimer = 0f;
    private float m_maxRespawnTime = 3f;

    [Header("Balloons")]
    [SerializeField] private List<GameObject> m_balloons;
    [SerializeField] private float m_ballonHeightIncrease = 1f;
    public List<GameObject> Ballons { get { return m_balloons; } }
    [Networked] public int ActiveBallons { get; set; }

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
            GameManager.instance.SetLocalPlayer(this);

            // Allow player to be set in init position
            m_controller.enabled = false;
            m_controller.enabled = true;

            m_camera = Camera.main;
            m_camera.GetComponent<FirstPersonCamera>().SetTarget(m_camPos);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            CurrentHealth = m_maxHealth;
            ActiveBallons = m_balloons.Count;

            SetJumpHeight();

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
        // Set the movement velocity
        m_moveVelocity = Mathf.Clamp(m_moveVelocity + Runner.DeltaTime * m_speedIncreaseScale, m_minMoveVelocity, m_maxMoveVelocity);

        Quaternion camRotY = Quaternion.Euler(0, m_camera.transform.rotation.eulerAngles.y, 0);
        Vector3 moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (moveInput.magnitude == 0f)
        {
            m_moveVelocity = 0f;
            Debug.Log("MOVE: " + moveInput.magnitude);
        }
        moveInput *= m_moveVelocity;

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
            // Change the velocity the player is falling if they are about to fall down and have balloons attached
            float fallForce = 7.5f;
            if(m_jumpForce < 1.0f && ActiveBallons > 0)
            {
                fallForce = 1.5f;
            }

            m_jumpForce -= Runner.DeltaTime * fallForce;
            m_jumpForce = Mathf.Clamp(m_jumpForce, -m_maxJumpForce, m_maxJumpForce);
        }

        // Update the y velocity and reset it to 0 if player is grounded
        m_velocity.y += m_jumpForce;
        if(IsGrounded() && m_jumpForce != m_maxJumpForce)
        {
            m_velocity.y = 0f;
        }

        m_controller.Move(move + m_velocity * Runner.DeltaTime);

        Respawn();
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

    public void Respawn()
    {
        if (!m_respawning)
        {
            return;
        }

        m_respawnTimer += Runner.DeltaTime;
        if(m_respawnTimer >= m_maxRespawnTime)
        {
            m_controller.enabled = false;
            var randSpawnPos = Random.Range(0, GameManager.instance.spawnPoints.Count);
            transform.position = GameManager.instance.spawnPoints[randSpawnPos].position;
            m_controller.enabled = true;
            CurrentHealth = m_maxHealth;
            RPC_ChangeMesh(true);
            m_respawning = false;
            m_respawnTimer = 0;
        }
    }

    // Set the jump height by how many balloons you have
    public void SetJumpHeight()
    {
        m_maxJumpForce = m_initialMaxJumpForce;
        ActiveBallons = m_balloons.Count;

        for (int i = 0; i < ActiveBallons; i++)
        {
            m_maxJumpForce += m_ballonHeightIncrease;
        }
    }

    //[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_DestroyBalloon(NetworkBehaviour balloon)
    {
        var balloonObject = Runner.TryGetNetworkedBehaviourId(balloon);
        if (balloonObject != null)
        {
            m_balloons.Remove(balloon.gameObject);
            balloon.GetComponent<MeshRenderer>().enabled = false;
        }

        SetJumpHeight();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(float damage)
    {
        if (!HasStateAuthority)
        {
            return;
        }

        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, CurrentHealth);

        if (CurrentHealth <= 0)
        {
            m_respawning = true;
            RPC_ChangeMesh(false);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ChangeMesh(bool on)
    {
        if (on)
        {
            m_playerMesh.enabled = true;
            return;
        }
        m_playerMesh.enabled = false;
    }

}
