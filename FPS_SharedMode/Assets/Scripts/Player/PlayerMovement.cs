using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : Movement, IHealth
{

    [Header("Main Components")]
    private Camera m_camera;
    private CamFOV m_camFOV;
    private PlayerParticles m_particles;

    [Header("Player Body Parts")]
    [SerializeField] private MeshRenderer m_playerMesh;
    [SerializeField] private GameObject m_playerHead;
    [SerializeField] private GameObject m_playerSpine;

    [Header("Camera & Movement")]
    [SerializeField] private Transform m_camPos;

    [Header("Weapon Components")]
    [SerializeField] private PlayerWeapon m_weapon;
    [SerializeField] private GameObject m_weaponModel;
    [SerializeField] private Animator m_weaponAnim;

    public PlayerWeapon Weapon => m_weapon;

    [Header("Knockback Values")]
    [SerializeField] private float KnockbackPwr = 10.0f;
    private bool m_knockback = false;
    [SerializeField] private float m_knockbackTime = 1.0f;

    #region - Init Properties -
    private void Awake()
    {
        m_controller = GetComponent<CharacterController>();
        m_particles = GetComponent<PlayerParticles>();
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
        UpdateVelocity(move);
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
    #endregion

    #region - Player Controller States -
    public override void IdleUpdate(ref Vector3 moveInput, ref Vector3 move)
    {
        base.IdleUpdate(ref moveInput, ref move);
        m_weaponAnim.SetInteger("Gun", 0);
    }

    public override void WalkUpdate(Vector3 move)
    {
        base.WalkUpdate(move);
        m_weaponAnim.SetInteger("Gun", 1);
    }

    public override void JumpUpdate(ref Vector3 move)
    {
        base.JumpUpdate(ref move);
        m_weaponAnim.SetInteger("Gun", 2);
    }
    #endregion

    #region - Knockback -
    Vector3 knockbackDirection = Vector3.zero;
    float initKnockbackDot = 0f;
    private void KnockbackLogic(ref Vector3 move)
    {
        m_knockbackTime = Mathf.Max(m_knockbackTime - Runner.DeltaTime, 0.0f);

        if (!m_knockback || m_knockbackTime <= 0.0f || IsGrounded)
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
        if (IsGrounded)
        {
            return;
        }

        m_knockbackForwardDir = transform.forward;
        m_setInitKnockbackDir = true;
        m_knockback = true;
        m_knockbackTime = 1.0f;
        m_camFOV.InitFOVScale(m_camera.fieldOfView + 2.5f, true);
        m_particles.PlayParticle(m_particles.KnockbackParticle);
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

    #region - RPC Calls -

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
