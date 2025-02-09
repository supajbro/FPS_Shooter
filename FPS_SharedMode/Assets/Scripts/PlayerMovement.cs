using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour, IHealth, IPlayerController, IBalloons
{

    #region - States -
    public enum PlayerStates
    {
        Idle = 0,
        Walk,
        Run,
        Jump
    }
    [SerializeField] private PlayerStates m_currentState;
    [SerializeField] private PlayerStates m_previousState;
    public void SetCurrentState(PlayerStates state)
    {
        m_previousState = m_currentState;
        m_currentState = state;
    }
    #endregion

    [Header("Main Components")]
    private CharacterController m_controller;
    private Camera m_camera;
    private CamFOV m_camFOV;

    [Header("Player Body Parts")]
    [SerializeField] private MeshRenderer m_playerMesh;
    [SerializeField] private GameObject m_playerHead;
    [SerializeField] private GameObject m_playerSpine;

    [Header("Camera & Movement")]
    [SerializeField] private Transform m_camPos;
    [SerializeField] private LayerMask m_groundLayer;

    [Header("Weapon Components")]
    [SerializeField] private PlayerWeapon m_weapon;
    [SerializeField] private GameObject m_weaponModel;
    [SerializeField] private Animator m_weaponAnim;

    public PlayerWeapon Weapon => m_weapon;

    [Header("Movement")]
    [SerializeField] private float m_speed = 35f;
    [SerializeField] private Vector3 m_velocity;
    private float m_moveVelocity = 0f;

    [Header("Jumping")]
    [SerializeField] private float m_jumpForce = 2f;
    [SerializeField] private float m_maxJumpForce = 2f;
    [SerializeField] private float m_initialMaxJumpForce = 2f;

    #region Player Controller Properties
    public float Speed => m_speed;
    public Vector3 Velocity => m_velocity;
    public float MoveVelocity => m_moveVelocity;
    public float JumpForce => m_jumpForce;
    public float MaxJumpForce => m_maxJumpForce;
    public float InitialMaxJumpForce => m_initialMaxJumpForce;
    #endregion

    [Header("Movement Values")]
    [SerializeField] private float m_speedIncreaseScale = 0.5f;
    [SerializeField] private float m_minMoveVelocity = 0.25f;
    [SerializeField] private float m_maxMoveVelocity = 1f;
    private float m_timeOffGround = 0.0f;
    private bool m_canMove = true;

    [Header("Jump Values")]
    private bool m_jumpPressed;
    private bool m_canJump = true;

    [Header("Respawn Values")]
    private bool m_respawning = false;
    private float m_respawnTimer = 0f;
    private float m_maxRespawnTime = 3f;

    [Header("Knockback Values")]
    [SerializeField] private float KnockbackPwr = 10.0f;
    private bool m_knockback = false;
    [SerializeField] private float m_knockbackTime = 1.0f;

    public bool IsGrounded => UpdateGroundCheck();

    #region - Init Properties -
    private void Awake()
    {
        m_controller = GetComponent<CharacterController>();
    }

    public override void Spawned()
    {
        if (!HasStateAuthority) return;

        InitLocalPlayer();
        ConfigurePlayerVisuals();
        InitMovement();
        InitCamera();
        ConfigureCursor();
        InitPlayerStats();
    }

    private void InitLocalPlayer()
    {
        GameManager.instance.SetLocalPlayer(this);
        SetCurrentState(PlayerStates.Idle);
    }

    private void ConfigurePlayerVisuals()
    {
        var skinnedMeshRenderer = m_playerHead.GetComponent<SkinnedMeshRenderer>();
        skinnedMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
    }

    private void InitMovement()
    {
        m_controller.enabled = false;
        m_controller.enabled = true;
        m_canMove = true;
    }

    private void InitCamera()
    {
        m_camera = Camera.main;
        m_camera.GetComponent<FirstPersonCamera>().SetTarget(m_camPos);
        m_camFOV = m_camera.GetComponent<CamFOV>();
    }

    private void ConfigureCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void InitPlayerStats()
    {
        m_currentHealth = m_maxHealth;
        ActiveBallons = m_balloons.Count;
        SetJumpHeight();
    }
    #endregion

    #region - Update Properties -
    private void Update()
    {
        // Jump input check
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_jumpPressed = true;
        }

        // DEBUG
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DestroyRandomBalloon();
        }

        RespawnBalloons();
    }

    private Vector3 m_lastMoveOnGround;
    private float m_speedInAirScaler = 1.0f;
    public override void FixedUpdateNetwork()
    {
        UpdateMoveVelocity();
        Quaternion camRotY = Quaternion.Euler(0, m_camera.transform.rotation.eulerAngles.y, 0);
        Vector3 moveInput = GetMoveInput();

        Vector3 move = camRotY * moveInput * Runner.DeltaTime * m_speed;

        HandleGroundState(ref move);
        ProcessJump();
        ApplyKnockback(ref move);
        RotatePlayer(camRotY);
        RotateWeapon();

        UpdatePlayerState(ref moveInput, ref move);
        UpdateVelocity();
        Respawn();
    }

    private void UpdateMoveVelocity()
    {
        m_moveVelocity = Mathf.Clamp(m_moveVelocity + Runner.DeltaTime * m_speedIncreaseScale, m_minMoveVelocity, m_maxMoveVelocity);
    }

    private Vector3 GetMoveInput()
    {
        if (!m_canMove || m_knockback) return Vector3.zero;

        Vector3 moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (moveInput.magnitude == 0f)
        {
            m_moveVelocity = 0f;
        }

        return moveInput * m_moveVelocity;
    }

    private void HandleGroundState(ref Vector3 move)
    {
        if (UpdateGroundCheck())
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

            if (m_timeOffGround > 0.5f)
            {
                m_canJump = false;
            }
        }
    }

    private void ProcessJump()
    {
        if (m_jumpPressed && m_canJump)
        {
            m_velocity.y = 0.0f;
            m_jumpPressed = false;
            m_jumpForce = m_maxJumpForce;
        }

        if (!UpdateGroundCheck())
        {
            SetCurrentState(PlayerStates.Jump);
        }
    }

    private void ApplyKnockback(ref Vector3 move)
    {
        KnockbackLogic(ref move);
    }

    private void RotatePlayer(Quaternion camRotY)
    {
        transform.rotation = camRotY;
    }

    private void RotateWeapon()
    {
        Quaternion weaponRot = Quaternion.Euler(m_camera.transform.rotation.eulerAngles.x, 0, 0);
        m_playerSpine.transform.localRotation = weaponRot;
    }

    private void UpdatePlayerState(ref Vector3 moveInput, ref Vector3 move)
    {
        switch (m_currentState)
        {
            case PlayerStates.Idle:
                IdleUpdate(ref moveInput, ref move);
                break;
            case PlayerStates.Walk:
                WalkUpdate(move);
                break;
            case PlayerStates.Jump:
                JumpUpdate(ref move);
                break;
        }
    }

    private void UpdateVelocity()
    {
        m_velocity.y += m_jumpForce;
        if (UpdateGroundCheck() && m_jumpForce != m_maxJumpForce)
        {
            m_velocity.y = 0f;
        }
    }

    private bool UpdateGroundCheck()
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
    #endregion

    #region - Player Controller States -
    private void IdleUpdate(ref Vector3 moveInput, ref Vector3 move)
    {
        if(moveInput.magnitude > 0f && UpdateGroundCheck())
        {
            SetCurrentState(PlayerStates.Walk);
            return;
        }

        moveInput = Vector3.zero;
        move = Vector3.zero;
        m_velocity = Vector3.zero;

        m_weaponAnim.SetInteger("Gun", 0);
    }

    private void WalkUpdate(Vector3 move)
    {
        if(move.magnitude <= 0.0f && UpdateGroundCheck())
        {
            SetCurrentState(PlayerStates.Idle);
        }

        transform.position += move;
        m_controller.Move(move + m_velocity * Runner.DeltaTime);
        m_weaponAnim.SetInteger("Gun", 1);
    }

    private void JumpUpdate(ref Vector3 move)
    {
        if (UpdateGroundCheck())
        {
            SetCurrentState(PlayerStates.Idle);
            return;
        }

        // Restrict the movement of the player when in the air
        if (move.magnitude > 0.75f)
        {
            m_speedInAirScaler = (m_speedInAirScaler > 0.75f) ? m_speedInAirScaler - Runner.DeltaTime : 0.75f;
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
            fallForce = (ActiveBallons > 0) ? 2.5f : fallForce * 1.5f;
        }

        m_jumpForce -= Runner.DeltaTime * fallForce;
        m_jumpForce = Mathf.Clamp(m_jumpForce, -m_maxJumpForce, m_maxJumpForce);

        m_controller.Move(move + m_velocity * Runner.DeltaTime);

        m_weaponAnim.SetInteger("Gun", 2);
    }
    #endregion

    #region - Knockback -
    Vector3 knockbackDirection = Vector3.zero;
    float initKnockbackDot = 0f;
    private void KnockbackLogic(ref Vector3 move)
    {
        m_knockbackTime = Mathf.Max(m_knockbackTime - Runner.DeltaTime, 0.0f);

        if (!m_knockback || m_knockbackTime <= 0.0f || UpdateGroundCheck())
        {
            m_knockback = false;
            return;
        }

        // First frame set knockback dir and dot product
        if (m_setInitKnockbackDir)
        {
            m_setInitKnockbackDir = false;
            knockbackDirection = -m_knockbackForwardDir;
            initKnockbackDot = Vector3.Dot(move.normalized, knockbackDirection);
        }

        // Apply knockback force
        float knockbackSpeed = KnockbackPwr;
        move = (initKnockbackDot > 0) ? move + (knockbackDirection * knockbackSpeed * Runner.DeltaTime) : knockbackDirection * knockbackSpeed * Runner.DeltaTime;
        m_lastMoveOnGround = move;
    }


    Vector3 m_knockbackForwardDir = Vector3.zero;
    private bool m_setInitKnockbackDir = false;
    public void KnockPlayerBack()
    {
        if (UpdateGroundCheck())
        {
            return;
        }

        m_knockbackForwardDir = transform.forward;
        m_setInitKnockbackDir = true;
        m_knockback = true;
        m_knockbackTime = 1.0f;
        m_camFOV.InitFOVScale(m_camera.fieldOfView + 2.5f, true);
    }
    #endregion

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
            var pos = GameManager.instance.spawnPoints[randSpawnPos].position;
            transform.position = pos;
            m_controller.enabled = true;
            m_velocity = Vector3.zero;
            m_currentHealth = m_maxHealth;
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

    #region - Health Properties -
    [Header("Health Values")]
    [SerializeField] private float m_currentHealth = 100f;
    [SerializeField] private float m_maxHealth = 100f;

    public float MaxHealth => m_maxHealth;
    public float Health => m_currentHealth;

    public void TakeDamage(float amount)
    {
        RPC_TakeDamage(amount);
    }

    public void Heal(float amount)
    {
        m_currentHealth += amount;
    }
    #endregion

    #region - Balloon Properties -
    [Header("Balloons")]
    [SerializeField] private List<GameObject> m_balloons;
    [SerializeField] private List<GameObject> m_destroyedBallons = new();
    [SerializeField] private int m_maxBalloons = 3;
    [SerializeField] private float m_ballonHeightIncrease = 1f;
    [SerializeField] private float m_balloonRespawnTime = 10f;

    public List<GameObject> Balloons => m_balloons;
    public List<GameObject> DestroyedBalloons => m_destroyedBallons;
    public int MaxBalloons => m_maxBalloons;
    public float BalloonHeightIncrease => m_ballonHeightIncrease;
    public float BalloonRespawnTime => m_balloonRespawnTime;

    [Networked] public int ActiveBallons { get; set; }

    public void DestroyRandomBalloon()
    {
        int randBalloon = Random.Range(0, ActiveBallons);
        RPC_DestroyBalloon(m_balloons[randBalloon].GetComponent<NetworkBehaviour>());
    }

    public void RespawnBalloons()
    {
        if(ActiveBallons >= MaxBalloons)
        {
            return;
        }

        m_balloonRespawnTime -= Time.deltaTime;

        if(m_balloonRespawnTime <= 0.0f)
        {
            var balloon = m_destroyedBallons[0];
            RPC_RespawnBalloon(balloon.GetComponent<NetworkBehaviour>());
            m_balloonRespawnTime = 10.0f;
        }
    }
    #endregion

    #region - RPC Calls -
    //[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_DestroyBalloon(NetworkBehaviour balloon)
    {
        var balloonObject = Runner.TryGetNetworkedBehaviourId(balloon);
        if (balloonObject != null)
        {
            m_balloons.Remove(balloon.gameObject);
            m_destroyedBallons.Add(balloon.gameObject);
            balloon.GetComponent<MeshRenderer>().enabled = false;
        }

        SetJumpHeight();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_RespawnBalloon(NetworkBehaviour balloon)
    {
        var balloonObject = Runner.TryGetNetworkedBehaviourId(balloon);
        if (balloonObject != null)
        {
            m_balloons.Add(balloon.gameObject);
            balloon.GetComponent<MeshRenderer>().enabled = true;
            m_destroyedBallons.Remove(balloon.gameObject);
        }

        SetJumpHeight();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_TakeDamage(float damage)
    {
        if (!HasStateAuthority)
        {
            return;
        }

        m_currentHealth = Mathf.Clamp(m_currentHealth - damage, 0, m_currentHealth);

        if (m_currentHealth <= 0)
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
        var randSpawnPos = Random.Range(0, GameManager.instance.spawnPoints.Count);
        var pos = GameManager.instance.spawnPoints[randSpawnPos].position;
        transform.position = pos;
        m_controller.enabled = true;
        m_canMove = true;
        SetCurrentState(PlayerStates.Idle);

        foreach (var balloon in m_destroyedBallons)
        {
            m_balloons.Add(balloon);
            balloon.GetComponent<MeshRenderer>().enabled = true;
            m_balloonRespawnTime = 10.0f;
        }
        m_destroyedBallons.Clear();
        SetJumpHeight();
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

        m_currentHealth = 0;
        m_canMove = false;
        Debug.Log("Player: " + this.name + " has died");

        if (m_currentHealth <= 0)
        {
            //m_respawning = true;
            RPC_ChangeMesh(false);
        }
    }
    #endregion
}
