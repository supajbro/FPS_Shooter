using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{

    private CharacterController m_controller;
    [SerializeField] private MeshRenderer m_playerMesh;
    [SerializeField] private GameObject m_playerHead;
    [SerializeField] private GameObject m_playerSpine;
    [SerializeField] private Transform m_camPos;
    [SerializeField] private PlayerWeapon m_weapon;
    public PlayerWeapon Weapon { get { return m_weapon; } }

    private Camera m_camera;
    public Camera Cam { get { return m_camera; } }

    [SerializeField] private LayerMask m_groundLayer;

    [Header("Values")]
    [SerializeField] private float m_speed = 2f;
    public Vector3 m_velocity;
    private float m_moveVelocity = 0f;
    [SerializeField] private float m_speedIncreaseScale = 0.75f;
    [SerializeField] private float m_minMoveVelocity = 0.25f;
    [SerializeField] private float m_maxMoveVelocity = 1f;
    private float m_timeOffGround = 0.0f;

    [Header("Jump Values")]
    [SerializeField] private float m_jumpForce;
    public void SetJumpForce(float value) {  m_jumpForce = value; }
    [SerializeField] private float m_maxJumpForce;
    [SerializeField] private float m_initialMaxJumpForce = 2f;
    private bool m_jumpPressed;
    private bool m_canJump = true;

    [Header("Health Values")]
    [SerializeField] private float m_maxHealth = 100f;
    public float MaxHealth { get { return m_maxHealth; } }
    [Networked] public float CurrentHealth { get; set; }

    [Header("Respawn Values")]
    private bool m_respawning = false;
    private float m_respawnTimer = 0f;
    private float m_maxRespawnTime = 3f;

    [Header("Knockback Values")]
    [SerializeField] private float KnockbackPwr = 10.0f;
    private bool m_knockback = false;
    private float m_knockbackTime = 1.0f;

    [Header("Balloons")]
    [SerializeField] private List<GameObject> m_balloons;
    [SerializeField] private float m_ballonHeightIncrease = 1f;
    public List<GameObject> Ballons { get { return m_balloons; } }
    [Networked] public int ActiveBallons { get; set; }

    public bool isGrounded => IsGrounded();
    private bool m_canMove = true;

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

            m_playerHead.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

            // Allow player to be set in init position
            m_controller.enabled = false;
            m_controller.enabled = true;
            m_canMove = true;

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

        if (Input.GetKeyDown(KeyCode.Q))
        {
            DestroyRandomBalloon();
        }
    }

    Vector3 m_lastMoveOnGround;
    float m_speedInAirScaler = 1.0f;
    public override void FixedUpdateNetwork()
    {
        // Set the movement velocity
        m_moveVelocity = Mathf.Clamp(m_moveVelocity + Runner.DeltaTime * m_speedIncreaseScale, m_minMoveVelocity, m_maxMoveVelocity);

        Quaternion camRotY = Quaternion.identity;
        Vector3 moveInput = Vector3.zero;

        if (m_canMove && !m_knockback)
        {
            camRotY = Quaternion.Euler(0, m_camera.transform.rotation.eulerAngles.y, 0);
            moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (moveInput.magnitude == 0f)
            {
                m_moveVelocity = 0f;
            }
            moveInput *= m_moveVelocity;
        }

        Vector3 move = camRotY * moveInput * Runner.DeltaTime * m_speed;

        if (IsGrounded())
        {
            m_lastMoveOnGround = move;
            m_speedInAirScaler = 1.0f;
            m_canJump = true;
            m_timeOffGround = 0.0f;
        }
        else
        {
            move = m_lastMoveOnGround;
            m_timeOffGround += Runner.DeltaTime;

            if(m_timeOffGround > 0.5f)
            {
                m_canJump = false;
            }
        }

        // Initialise the jump
        if (m_jumpPressed && m_canJump)
        {
            m_velocity.y = 0.0f;
            m_jumpPressed = false;
            m_jumpForce = m_maxJumpForce;
        }

        // Update the jump force when the player is off the ground
        if (!IsGrounded())
        {
            // Restrict the movement of the player when in the air
            if (move.magnitude > 0.5f)
            {
                m_speedInAirScaler = (m_speedInAirScaler > 0.5f) ? m_speedInAirScaler - Runner.DeltaTime : 0.5f;
                move = move.normalized * m_speedInAirScaler;
            }

            // Change the velocity the player is falling if they are about to fall down and have balloons attached
            const float InitialFallVelocity = 7.5f;
            const float LowestFallVelocity = 3.5f;
            const float MiddleFallVelocity = 5.5f;
            const float HighestFallVelocity = 10f;

            float fallForce = InitialFallVelocity;

            if (m_jumpForce > 1.0f)
            {
                switch (ActiveBallons)
                {
                    case 3:
                        fallForce = LowestFallVelocity;
                        break;
                    case 2:
                        fallForce = MiddleFallVelocity;
                        break;
                    case 1:
                        fallForce = HighestFallVelocity;
                        break;
                }
            }
            else if (m_jumpForce < 1.0f) // If player starts falling, change fall velocity dependant if player has balloons
            {
                fallForce = (ActiveBallons > 0) ? 1.5f : fallForce * 1.5f;
            }

            m_jumpForce -= Runner.DeltaTime * fallForce;
            m_jumpForce = Mathf.Clamp(m_jumpForce, -m_maxJumpForce, m_maxJumpForce);
        }

        m_knockbackTime -= Runner.DeltaTime;
        if (m_knockback && m_knockbackTime > 0.0f && !IsGrounded())
        {
            Vector3 knockbackDirection = -m_knockbackForwardDir; // Move backwards relative to the player's forward direction
            float knockbackSpeed = KnockbackPwr; // Adjust this value for desired knockback speed
            move += knockbackDirection * knockbackSpeed * Runner.DeltaTime;
            m_lastMoveOnGround = move;
        }
        else if(m_knockbackTime <= 0.0f || IsGrounded())
        {
            m_knockback = false;
        }

        gameObject.transform.rotation = camRotY;
        transform.position += move;

        // Rotate the weapon
        Quaternion weaponRot = Quaternion.Euler(m_camera.transform.rotation.eulerAngles.x, 0, 0);
        m_playerSpine.transform.localRotation = weaponRot;
        //m_weapon.transform.rotation = camRotX;

        // Update the y velocity and reset it to 0 if player is grounded
        m_velocity.y += m_jumpForce;
        if(IsGrounded() && m_jumpForce != m_maxJumpForce)
        {
            m_velocity.y = 0f;
        }

        m_controller.Move(move + m_velocity * Runner.DeltaTime);

        Respawn();
    }

    Vector3 m_knockbackForwardDir = Vector3.zero;
    public void KnockPlayerBack()
    {
        if (IsGrounded())
        {
            return;
        }

        m_knockbackForwardDir = transform.forward;
        m_knockback = true;
        m_knockbackTime = 1.0f;
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

    private void DestroyRandomBalloon()
    {
        int randBalloon = Random.Range(0, ActiveBallons);
        RPC_DestroyBalloon(m_balloons[randBalloon].GetComponent<NetworkBehaviour>());
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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Respawn()
    {
        m_controller.enabled = false;
        m_canMove = false;
        m_velocity.y = 0;
        var randSpawnPos = Random.Range(0, GameManager.instance.spawnPoints.Count);
        transform.position = GameManager.instance.spawnPoints[randSpawnPos].position;
        m_controller.enabled = true;
        m_canMove = true;
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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerDie()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        CurrentHealth = 0;
        m_canMove = false;
        Debug.Log("Player: " + this.name + " has died");

        if (CurrentHealth <= 0)
        {
            //m_respawning = true;
            RPC_ChangeMesh(false);
        }
    }

}
