using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static PlayerMovement;

public class Movement : NetworkBehaviour, IPlayerController, IBalloons
{

    #region - States -
    public enum PlayerStates
    {
        Idle = 0,
        Walk,
        Run,
        Jump
    }
    [SerializeField] protected PlayerStates m_currentState;
    [SerializeField] protected PlayerStates m_previousState;
    public void SetCurrentState(PlayerStates state)
    {
        m_previousState = m_currentState;
        m_currentState = state;
    }
    #endregion

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

    public virtual void IdleUpdate(ref Vector3 moveInput, ref Vector3 move)
    {
        if (moveInput.magnitude > 0f && IsGrounded)
        {
            SetCurrentState(PlayerStates.Walk);
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
            SetCurrentState(PlayerStates.Idle);
        }

        transform.position += move;
    }

    public virtual void JumpUpdate(ref Vector3 move)
    {
        if (IsGrounded)
        {
            SetCurrentState(PlayerStates.Idle);
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

    public void RespawnBalloons()
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
}
