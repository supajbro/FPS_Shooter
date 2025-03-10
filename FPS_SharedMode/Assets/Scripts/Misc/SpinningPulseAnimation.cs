using UnityEngine;

public class SpinningPulseAnimation : MonoBehaviour
{

    public float baseScale = 1f;    // Base scale size
    public float scaleAmplitude = 0.2f; // How much the scale changes
    public float scaleSpeed = 2f;   // Speed of scaling

    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        float scaleFactor = baseScale + Mathf.Sin(Time.time * scaleSpeed) * scaleAmplitude;
        transform.localScale = initialScale * scaleFactor;
    }

}
