using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : Movement, IHealth
{

    [Header("Main Components")]
    private Camera m_camera;
    private CamFOV m_camFOV;
    private FirstPersonCamera m_firstPersonCamera;
    private PlayerParticles m_particles;

    [Header("Player Body Parts")]
    [SerializeField] private MeshRenderer m_playerMesh;
    [SerializeField] private GameObject m_playerHead;
    [SerializeField] private GameObject m_playerSpine;
    [SerializeField] private List<GameObject> m_playerRopes;

    [Header("Camera & Movement")]
    [SerializeField] private Transform m_camPos;

    [Header("Weapon Components")]
    [SerializeField] private PlayerWeapon m_weapon;

    [Header("Animations")]
    [SerializeField] private Animator m_weaponAnim;
    [SerializeField] private Animator m_walkAnim;
    [SerializeField] private Animator m_playerAnim;

    public PlayerWeapon Weapon => m_weapon;

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
        SpawnBotPooler();
        ConfigurePlayerVisuals();
        InitMovement();
        InitCamera();
        ConfigureCursor();
        InitPlayerStats();
    }

    private void InitLocalPlayer()
    {
        GameManager.instance.SetLocalPlayer(this);
        SetCurrentState(MovementStates.Idle);
        Boss = Runner.IsSharedModeMasterClient;
        GameManager.instance.MainMenu.CloseMainMenu();
        GameManager.instance.OpenPlayerScreen();
        PlayerName = GameManager.instance.MainMenu.PlayerName;
        gameObject.name = PlayerName;
    }

    private void SpawnBotPooler()
    {
        if (!Boss)
        {
            return;
        }

        GameManager.instance.SpawnBotPooler(Runner);
    }

    private void ConfigurePlayerVisuals()
    {
        var skinnedMeshRenderer = m_playerHead.GetComponent<SkinnedMeshRenderer>();
        skinnedMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

        foreach (var rope in m_playerRopes)
        {
            rope.SetActive(false);
        }
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
        m_firstPersonCamera = m_camera.GetComponent<FirstPersonCamera>();
        m_weapon.Init(m_firstPersonCamera.ShootDirection);
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
    public override void Update()
    {
        base.Update();
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
        if (Input.GetKeyDown(KeyCode.E))
        {
            RPC_Win();
            RPC_Respawn();
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
        KnockbackUpdate(ref move);
    }

    public override void InitKnockback()
    {
        if (IsGrounded)
        {
            return;
        }

        base.InitKnockback();

        m_camFOV.InitFOVScale(m_camera.fieldOfView + 2.5f, true);
        m_particles.PlayParticle(m_particles.KnockbackParticle);
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
        if (!m_canMove)
        {
            return;
        }

        switch (m_currentState)
        {
            case MovementStates.Idle:
                IdleUpdate(ref moveInput, ref move);
                break;
            case MovementStates.Walk:
                WalkUpdate(move);
                break;
            case MovementStates.Jump:
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
        RPC_ChangeWalkAnim(0);
        RPC_ChangePlayerAnim(0);
    }

    public override void WalkUpdate(Vector3 move)
    {
        base.WalkUpdate(move);
        m_weaponAnim.SetInteger("Gun", 1);
        RPC_ChangeWalkAnim(1);
        RPC_ChangePlayerAnim(1);
    }

    public override void JumpUpdate(ref Vector3 move)
    {
        base.JumpUpdate(ref move);
        m_weaponAnim.SetInteger("Gun", 2);
        RPC_ChangePlayerAnim(3);
    }
    #endregion

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

    public override void RPC_Win()
    {
        base.RPC_Win();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RespawnPlayer()
    {
        m_velocity = Vector3.zero;
        m_controller.enabled = false;
        m_canMove = false;

        // Update position
        Debug.Log("[Pos] Old pos: " + transform.position);
        int randSpawnPos = Random.Range(0, GameManager.instance.spawnPoints.Count);
        Vector3 pos = GameManager.instance.spawnPoints[randSpawnPos].position;
        transform.position = pos;
        Debug.Log("[Pos] New pos: " + transform.position);

        m_controller.enabled = true;
        m_canMove = true;

        RPC_Respawn();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ChangeWalkAnim(int index)
    {
        m_walkAnim.SetInteger("Walk", index);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ChangePlayerAnim(int index)
    {
        m_playerAnim.SetInteger("Player", index);
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
