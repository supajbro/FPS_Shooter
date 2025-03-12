using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotMovement : Movement
{

    [Header("Main Components")]
    [SerializeField] private BotWeapon m_weapon;
    private BotPooler m_botPooler;

    [Header("Bot Path")]
    private BotPath m_currentPath;
    private int m_currentPathIndex = -1;

    [Header("Bot Body Parts")]
    [SerializeField] private MeshRenderer m_botMesh;
    [SerializeField] private GameObject m_botHead;
    [SerializeField] private GameObject m_botSpine;

    #region - Init Properties -
    private void Awake()
    {
        m_controller = GetComponent<CharacterController>();
        m_botPooler = FindObjectOfType<BotPooler>();
        //m_particles = GetComponent<PlayerParticles>();
    }

    public void SetPath(int pathIndex)
    {
        if (pathIndex < 0 || pathIndex >= m_botPooler.BotPaths.Count)
        {
            Debug.LogWarning($"SetPath: pathIndex {pathIndex} is out of bounds");
            return;
        }

        m_currentPath = m_botPooler.BotPaths[pathIndex];
        m_currentPathIndex = pathIndex;
    }

    public override void Spawned()
    {
        InitLocalPlayer();
        ConfigureBotVisuals();
        InitMovement();
        InitBotStats();
    }

    private void InitLocalPlayer()
    {
        SetCurrentState(MovementStates.Idle);
    }

    private void ConfigureBotVisuals()
    {
        var skinnedMeshRenderer = m_botHead.GetComponent<SkinnedMeshRenderer>();
        skinnedMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
    }

    private void InitMovement()
    {
        m_controller.enabled = false;
        m_controller.enabled = true;
        m_canMove = true;
    }

    private void InitBotStats()
    {
        //m_currentHealth = m_maxHealth;
        ActiveBallons = m_balloons.Count;
        SetJumpHeight();
    }
    #endregion

    #region - Update Properties -
    public override void BossUpdate()
    {
        Boss = false;
        return;
    }

    public override void Update()
    {
        base.Update();
        // Jump input check
        if (m_currentPath && m_currentPath.NextPath)
        {
            Vector3 toNextPath = (m_currentPath.NextPath.transform.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, toNextPath);
            bool atCurrentPoint = m_currentPath.NearPathPoint(this);
            if (dotProduct >= 0.75f && m_currentPath.NextPath.transform.position.y > m_currentPath.transform.position.y && atCurrentPoint && !m_jumpPressed)
            {
                Debug.Log($"{m_currentPath.NextPath.transform.position.y} / {m_currentPath.transform.position.y}");
                m_jumpPressed = true;
            }
        }
        else
        {
            movementScale = 0.0f;
        }

        RespawnBalloons();
    }

    public override void FixedUpdateNetwork()
    {
        UpdateMoveVelocity();

        if (m_currentPath && m_currentPath.ReachedPathPoint(this) && m_currentPathIndex <= m_botPooler.BotPaths.Count)
        {
            SetPath(m_currentPathIndex + 1);
        }

        Vector3 target = (m_weapon.ShootPressed && m_weapon.Target) ? m_weapon.Target.transform.position : m_currentPath.transform.position;
        Vector3 targetDirection = (target - transform.position).normalized;
        Vector3 flatDirection = new Vector3(targetDirection.x, 0, targetDirection.z);
        Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
        Quaternion camRotY = Quaternion.Slerp(transform.rotation, targetRotation, Runner.DeltaTime * 20);

        Vector3 moveInput = GetMoveInput();

        Vector3 move = camRotY * moveInput * Runner.DeltaTime * m_speed;

        HandleGroundState(ref move);
        ProcessJump();
        ApplyKnockback(ref move);
        RotateBot(camRotY);
        //RotateWeapon();

        UpdateBotState(ref moveInput, ref move);
        UpdateVelocity(move);
        //Respawn();
    }

    private void UpdateMoveVelocity()
    {
        m_moveVelocity = Mathf.Clamp(m_moveVelocity + Runner.DeltaTime * m_speedIncreaseScale, m_minMoveVelocity, m_maxMoveVelocity);
    }

    private float movementScale = 0.25f;
    const float MinScale = 0.25f;
    const float MaxScale = 0.75f;
    private Vector3 GetMoveInput()
    {
        if (!m_canMove || m_knockback) return Vector3.zero;

        if(m_currentState == MovementStates.Walk)
        {
            movementScale = (movementScale < MaxScale) ? movementScale + Runner.DeltaTime : MaxScale;
        }
        else
        {
            movementScale = (movementScale > MinScale) ? movementScale - Runner.DeltaTime : MinScale;
        }

        Vector3 moveInput = new Vector3(movementScale, 0, movementScale);
        if (moveInput.magnitude == 0f)
        {
            m_moveVelocity = 0f;
        }

        if (m_weapon.Target != null)
        {
            moveInput = Vector3.zero;
            m_moveVelocity = 0.0f;
        }

        return moveInput * m_moveVelocity;
    }

    private void ApplyKnockback(ref Vector3 move)
    {
        KnockbackUpdate(ref move);
    }

    private void RotateBot(Quaternion camRotY)
    {
        transform.rotation = camRotY;
    }

    private void UpdateBotState(ref Vector3 moveInput, ref Vector3 move)
    {
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

    public override void RPC_Respawn()
    {
        SetPath(0);
        m_velocity.y = 0f;
        m_controller.enabled = false;
        m_canMove = false;
        Vector3 spawnPos = m_currentPath.transform.position;
        transform.position = spawnPos;
        m_controller.enabled = true;
        m_canMove = true;
        base.RPC_Respawn();
    }

}
