using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static PlayerMovement;
using DG.Tweening;

public class Movement : NetworkBehaviour, IPlayerController, IBalloons
{

    #region - States -
    public enum MovementStates
    {
        Idle = 0,
        Walk,
        Run,
        Jump
    }
    [SerializeField] protected MovementStates m_currentState;
    [SerializeField] protected MovementStates m_previousState;
    public void SetCurrentState(MovementStates state)
    {
        m_previousState = m_currentState;
        m_currentState = state;
    }
    #endregion

    [Header("Main Components")]
    public CharacterController m_controller;
    [Networked] public bool Boss { get; set; }
    [Networked] public bool HasWon { get; set; }
    [Networked] public bool GameOver { get; set; }
    [Networked] public string PlayerName { get; set; }

    [Header("Movement")]
    [SerializeField] protected float m_speed = 35f;
    [SerializeField] protected Vector3 m_velocity;
    [SerializeField] private LayerMask m_groundLayer;
    protected float m_moveVelocity = 0f;

    [Header("Jumping")]
    [SerializeField] protected float m_jumpForce = 2f;
    [SerializeField] protected float m_maxJumpForce = 2f;
    [SerializeField] protected float m_initialMaxJumpForce = 2f;

    [Header("Movement Values")]
    [SerializeField] protected float m_speedIncreaseScale = 0.5f;
    [SerializeField] protected float m_minMoveVelocity = 0.25f;
    [SerializeField] protected float m_maxMoveVelocity = 1f;
    protected float m_timeOffGround = 0.0f;
    protected bool m_canMove = true;
    public bool CanMove => m_canMove;

    [Header("Jump Values")]
    protected bool m_jumpPressed;
    protected bool m_canJump = true;

    [Header("Respawn Values")]
    protected bool m_respawning = false;
    protected float m_respawnTimer = 0f;
    protected float m_maxRespawnTime = 3f;

    protected Vector3 m_lastMoveOnGround;
    protected float m_speedInAirScaler = 1.0f;

    public float Speed => m_speed;
    public Vector3 Velocity => m_velocity;
    public float MoveVelocity => m_moveVelocity;
    public float JumpForce => m_jumpForce;
    public float MaxJumpForce => m_maxJumpForce;
    public float InitialMaxJumpForce => m_initialMaxJumpForce;

    [Header("Knockback Values")]
    [SerializeField] protected float KnockbackPwr = 10.0f;
    [SerializeField] protected float m_knockbackTime = 1.0f;
    protected bool m_knockback = false;

    public bool IsGrounded => UpdateGroundCheck();

    [Header("Balloons")]
    [SerializeField] protected List<GameObject> m_balloons;
    [SerializeField] protected List<GameObject> m_destroyedBallons = new();
    [SerializeField] protected int m_maxBalloons = 3;
    [SerializeField] protected float m_ballonHeightIncrease = 1f;
    [SerializeField] protected float m_balloonRespawnTime = 10f;

    public List<GameObject> Balloons => m_balloons;
    public List<GameObject> DestroyedBalloons => m_destroyedBallons;
    public int MaxBalloons => m_maxBalloons;
    public float BalloonHeightIncrease => m_ballonHeightIncrease;
    public float BalloonRespawnTime => m_balloonRespawnTime;

    [Networked] public int ActiveBallons { get; set; }

    public virtual void Update(){}

    public override void Render()
    {
        BossUpdate();
    }

    public virtual void BossUpdate()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        Boss = Runner.IsSharedModeMasterClient;
    }

    public virtual void IdleUpdate(ref Vector3 moveInput, ref Vector3 move)
    {
        if (moveInput.magnitude > 0f && IsGrounded)
        {
            SetCurrentState(MovementStates.Walk);
            return;
        }

        moveInput = Vector3.zero;
        move = Vector3.zero;
        m_velocity = Vector3.zero;
    }

    public virtual void WalkUpdate(Vector3 move)
    {
        if (move.magnitude <= 0.0f && IsGrounded)
        {
            SetCurrentState(MovementStates.Idle);
        }

        transform.position += move;
    }

    public virtual void JumpUpdate(ref Vector3 move)
    {
        if (IsGrounded)
        {
            SetCurrentState(MovementStates.Idle);
            //m_particles.GroundStompParticle.PlayParticle(new Vector3(transform.position.x, transform.position.y - 0.9f, transform.position.z), null);
            return;
        }

        m_moveVelocity = 0.5f;

        // Restrict the movement of the player when in the air
        if (move.magnitude > 0.75f)
        {
            m_speedInAirScaler = (m_speedInAirScaler > 0.75f) ? m_speedInAirScaler - Runner.DeltaTime : 0.75f;
            move = move.normalized * m_speedInAirScaler;
        }

        // Change the velocity the player is falling if they are about to fall down and have balloons attached
        const float InitialFallVelocity = 7.5f;
        const float LowestFallVelocity = 3.5f; // For 3 balloons
        const float MiddleFallVelocity = 6f; // For 2 balloons
        const float HighestFallVelocity = 10f; // For 1 balloons

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
    }

    public void SetCanMove(bool check)
    {
        m_canMove = check;
    }

    public virtual void UpdateVelocity(Vector3 move)
    {
        m_velocity.y += m_jumpForce;
        if (IsGrounded && m_jumpForce != m_maxJumpForce)
        {
            m_velocity.y = 0f;
        }
        m_controller.Move(move + m_velocity * Runner.DeltaTime);
    }

    public virtual void HandleGroundState(ref Vector3 move)
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

    public virtual void ProcessJump()
    {
        if (m_jumpPressed && m_canJump)
        {
            m_velocity.y = (m_velocity.y < 0.0f) ? 0.0f : m_velocity.y;

            // If player did a koyote jump, give extra upward velocity
            if (!IsGrounded)
            {
                m_velocity.y = 5f;
            }

            m_jumpPressed = false;
            m_jumpForce = m_maxJumpForce;
        }

        if (!IsGrounded)
        {
            SetCurrentState(MovementStates.Jump);
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

    public void DestroyRandomBalloon()
    {
        int randBalloon = Random.Range(0, ActiveBallons);
        RPC_DestroyBalloon(m_balloons[randBalloon].GetComponent<NetworkBehaviour>());
    }

    public virtual void RespawnBalloons()
    {
        if (ActiveBallons >= MaxBalloons)
        {
            return;
        }

        m_balloonRespawnTime -= Time.deltaTime;

        if (m_balloonRespawnTime <= 0.0f)
        {
            var balloon = m_destroyedBallons[0];
            RPC_RespawnBalloon(balloon.GetComponent<NetworkBehaviour>());
            m_balloonRespawnTime = 10.0f;
        }
    }

    #region - Knockback -
    Vector3 knockbackDirection = Vector3.zero;
    float initKnockbackDot = 0f;
    public virtual void KnockbackUpdate(ref Vector3 move)
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

        Debug.DrawLine(transform.position, knockbackDirection * 10, Color.blue);

        // Apply knockback force
        float knockbackSpeed = KnockbackPwr;
        move = (initKnockbackDot > 0) ? move + (knockbackDirection * knockbackSpeed * Runner.DeltaTime) : knockbackDirection * knockbackSpeed * Runner.DeltaTime;
        m_lastMoveOnGround = move;
    }


    Vector3 m_knockbackForwardDir = Vector3.zero;
    private bool m_setInitKnockbackDir = false;
    public virtual void InitKnockback()
    {
        if (IsGrounded)
        {
            return;
        }

        m_knockbackForwardDir = transform.forward;
        m_setInitKnockbackDir = true;
        m_knockback = true;
        m_knockbackTime = 1.0f;
    }
    #endregion

    public void Respawn()
    {
        if (!m_respawning)
        {
            return;
        }

        m_respawnTimer += Runner.DeltaTime;
        if (m_respawnTimer >= m_maxRespawnTime)
        {
            m_controller.enabled = false;
            var randSpawnPos = Random.Range(0, GameManager.instance.spawnPoints.Count);
            var pos = GameManager.instance.spawnPoints[randSpawnPos].position;
            transform.position = pos;
            m_controller.enabled = true;
            m_velocity = Vector3.zero;
            //m_currentHealth = m_maxHealth;
            //RPC_ChangeMesh(true);
            m_respawning = false;
            m_respawnTimer = 0;
        }
    }

    #region - RPC Calls -
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_DestroyBalloon(NetworkBehaviour balloon)
    {
        var balloonObject = Runner.TryGetNetworkedBehaviourId(balloon);
        if (balloonObject != null)
        {
            if (m_destroyedBallons.Contains(balloon.gameObject))
            {
                return;
            }

            m_balloons.Remove(balloon.gameObject);
            m_destroyedBallons.Add(balloon.gameObject);
            balloon.GetComponent<MeshRenderer>().enabled = false;
            balloon.GetComponent<Balloon>().PopBalloon();
        }

        SetJumpHeight();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_RespawnBalloon(NetworkBehaviour balloon)
    {
        var balloonObject = Runner.TryGetNetworkedBehaviourId(balloon);
        if (balloonObject != null)
        {
            if (m_balloons.Contains(balloon.gameObject))
            {
                return;
            }

            m_balloons.Add(balloon.gameObject);
            balloon.gameObject.transform.localScale = Vector3.zero;
            balloon.GetComponent<MeshRenderer>().enabled = true;
            balloon.gameObject.transform.DOScale(Vector3.one, 1.0f).SetEase(Ease.OutBack);
            m_destroyedBallons.Remove(balloon.gameObject);
        }

        SetJumpHeight();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public virtual void RPC_StopMovement()
    {
        m_controller.enabled = false;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public virtual void RPC_Respawn()
    {
        SetCurrentState(MovementStates.Idle);

        foreach (var balloon in m_destroyedBallons)
        {
            if (m_balloons.Contains(balloon))
            {
                return;
            }

            m_balloons.Add(balloon);
            balloon.GetComponent<MeshRenderer>().enabled = true;
            m_balloonRespawnTime = 10.0f;
        }
        m_destroyedBallons.Clear();
        SetJumpHeight();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public virtual void RPC_Win()
    {
        HasWon = true;

        foreach (var player in GameManager.instance.GetAllPlayers())
        {
            player.GameOver = true;
        }
    }
    #endregion
}
