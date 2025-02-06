using System.Collections.Generic;
using UnityEngine;

public interface IBalloons
{
    public List<GameObject> Balloons { get; }
    public List<GameObject> DestroyedBalloons { get; }
    public int MaxBalloons { get; }
    public float BalloonHeightIncrease { get; }
    public float BalloonRespawnTime { get; }

    void DestroyRandomBalloon();
    void RespawnBalloons();
}
