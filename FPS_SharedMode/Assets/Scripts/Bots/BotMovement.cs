using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotMovement : Movement
{

    [Header("Main Components")]
    public CharacterController m_controller;
    private BotPooler m_botPooler;

    [Header("Bot Path")]
    private BotPath m_currentPath;
    private int m_currentPathIndex = -1;

    [Header("Bot Body Parts")]
    [SerializeField] private MeshRenderer m_botMesh;
    [SerializeField] private GameObject m_botHead;
    [SerializeField] private GameObject m_botSpine;

    [Header("Knockback Values")]
    [SerializeField] private float KnockbackPwr = 10.0f;
    private bool m_knockback = false;
    [SerializeField] private float m_knockbackTime = 1.0f;

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
        SetCurrentState(PlayerStates.Idle);
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
    private void Update()
    {
        // TODO: Rework to not take in input (check if bot path has different y axis
        // Jump input check
        if (m_currentPath && m_currentPath.NextPath)
        {
            if (m_currentPath.NextPath.transform.position.y > m_currentPath.transform.position.y && !m_jumpPressed)
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

        if (m_currentPath && m_currentPath.ReachedPathPoint(this))
        {
            SetPath(m_currentPathIndex + 1);
            Debug.Log("PATH: " + m_currentPathIndex);
        }

        Vector3 targetDirection = (m_currentPath.transform.position - transform.position).normalized;
        Vector3 flatDirection = new Vector3(targetDirection.x, 0, targetDirection.z);
        Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
        Quaternion camRotY = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 25);


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

        if(m_currentState == PlayerStates.Walk)
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

        return moveInput * m_moveVelocity;
    }

    private void HandleGroundState(ref Vector3 move)
    {
        if (IsGrounded)
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
            m_velocity.y = (m_velocity.y < 0.0f) ? 0.0f : m_velocity.y;
            m_jumpPressed = false;
            m_jumpForce = m_maxJumpForce;
        }

        if (!IsGrounded)
        {
            SetCurrentState(PlayerStates.Jump);
        }
    }

    private void ApplyKnockback(ref Vector3 move)
    {
        KnockbackLogic(ref move);
    }

    private void RotateBot(Quaternion camRotY)
    {
        transform.rotation = camRotY;
    }

    private void UpdateBotState(ref Vector3 moveInput, ref Vector3 move)
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

    private void UpdateVelocity(Vector3 move)
    {
        m_velocity.y += m_jumpForce;
        if (IsGrounded && m_jumpForce != m_maxJumpForce)
        {
            m_velocity.y = 0f;
        }
        m_controller.Move(move + m_velocity * Runner.DeltaTime);
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
        //m_particles.PlayParticle(m_particles.KnockbackParticle);
    }
    #endregion

}
