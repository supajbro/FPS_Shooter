using UnityEngine;
public interface IPlayerController
{
    float Speed { get; }
    Vector3 Velocity { get; }
    float MoveVelocity {get;}

    float JumpForce { get; }
    float MaxJumpForce { get; }
    float InitialMaxJumpForce { get; }

    void IdleUpdate(ref Vector3 moveInput, ref Vector3 move) { }
    void WalkUpdate(Vector3 move) { }
    void JumpUpdate(ref Vector3 move) { }
}
