using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class plus1_1 : MonoBehaviour
{
    [Header("Base Settings")]
    public float lifetime = 120f; // Base lifetime in frames
    public float startSpeed = 30f; // Initial horizontal speed
    public float arcHeight = 30f; // Maximum height of the arc
    public float minScalePercentage = 0.2f; // Minimum scale as a percentage (e.g., 0.2 = 20%)

    [Header("Randomization Settings")]
    [Range(0f, 1f)] public float lifetimeVariance = 0.25f; // Variance as a percentage (e.g., 0.25 = ±25%)
    [Range(0f, 1f)] public float startSpeedVariance = 0.25f; // Variance for start speed
    [Range(0f, 1f)] public float arcHeightVariance = 0.25f; // Variance for arc height

    private float lifetimer; // Randomized lifetime
    private float timeElapsed; // Tracks time since spawn
    private Vector2 initialPosition; // Starting position
    private float randomizedStartSpeed; // Randomized horizontal speed
    private float randomizedArcHeight; // Randomized arc height
    private Vector3 initialScale; // Initial object scale
    private float directionMultiplier; // Left (-1) or Right (1)

    void Start()
    {
        // Randomize lifetime based on editor-set variance
        float lifetimeMin = lifetime * (1f - lifetimeVariance);
        float lifetimeMax = lifetime * (1f + lifetimeVariance);
        lifetimer = Random.Range(lifetimeMin, lifetimeMax);

        // Randomize start speed based on editor-set variance
        float startSpeedMin = startSpeed * (1f - startSpeedVariance);
        float startSpeedMax = startSpeed * (1f + startSpeedVariance);
        randomizedStartSpeed = Random.Range(startSpeedMin, startSpeedMax);

        // Randomize arc height based on editor-set variance
        float arcHeightMin = arcHeight * (1f - arcHeightVariance);
        float arcHeightMax = arcHeight * (1f + arcHeightVariance);
        randomizedArcHeight = Random.Range(arcHeightMin, arcHeightMax);

        // Save the initial position and scale
        initialPosition = transform.position;
        initialScale = transform.localScale;

        // Randomize direction (-1 for left, 1 for right)
        directionMultiplier = Random.Range(-1f, 1f) > 0 ? 1f : -1f;
    }

    void FixedUpdate()
    {
        if (!pauser1.paused)
        {
            // Track elapsed time
            timeElapsed += Time.fixedDeltaTime;

            // Calculate the fraction of lifetime completed
            float progress = timeElapsed / lifetimer;

            // Destroy the object when its lifetime is up
            if (progress >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            // Calculate horizontal and vertical position
            float horizontalOffset = randomizedStartSpeed * directionMultiplier * progress; // Horizontal movement
            float verticalOffset = randomizedArcHeight * Mathf.Sin(Mathf.PI * progress); // Single arc

            // Update position
            transform.position = initialPosition + new Vector2(horizontalOffset, verticalOffset);

            // Gradually reduce scale but not below the minimum
            float scaleProgress = Mathf.Lerp(1f, minScalePercentage, progress); // Scale factor
            transform.localScale = initialScale * scaleProgress; // Apply scale
        }
    }
}
