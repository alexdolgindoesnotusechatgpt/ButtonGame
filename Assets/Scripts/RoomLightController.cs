using UnityEngine;
using UnityEngine.Rendering.Universal; // Required for 2D Lights

public class LightController2D : MonoBehaviour
{
    [Header("Connections")]
    [SerializeField] private SimpleBattery battery;
    [SerializeField] private Light2D globalLight;

    [Header("Settings")]
    [Tooltip("The brightness when the battery is healthy (> 50%)")]
    [SerializeField] private float stableIntensity = 1.0f;

    [Tooltip("How much the light dips during a flicker (0.5 = dips to 50% brightness)")]
    [SerializeField] private float flickerStrength = 0.5f;

    [Tooltip("How fast the light flickers")]
    [SerializeField] private float flickerSpeed = 20f;

    void Start()
    {
        // Auto-grab light if not assigned
        if (globalLight == null) globalLight = GetComponent<Light2D>();
    }

    void Update()
    {
        if (battery == null || globalLight == null) return;

        // 1. Get Battery % (0.0 to 1.0)
        float pct = battery.CurrentCharge / battery.GetMaxCharge();

        float finalIntensity = stableIntensity;

        // 2. Dimming Logic (Only happens below 50%)
        if (pct <= 0.5f)
        {
            // Normalize the 0.0-0.5 range into a 0.0-1.0 range
            // Example: Battery 0.25 (25%) becomes 0.5 (50% brightness)
            float dimFactor = pct / 0.5f;
            finalIntensity = Mathf.Lerp(0f, stableIntensity, dimFactor);
        }

        // 3. Flicker Logic (Only happens below 30%)
        if (pct < 0.3f && pct > 0f)
        {
            // Use Perlin Noise for "organic" flickering (like a failing generator)
            // Time.time * speed creates movement through the noise
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);

            // If noise is high, dip the intensity
            if (noise > 0.5f)
            {
                // The lower the battery, the more aggressive the flicker can get
                finalIntensity -= (flickerStrength * noise);
            }
        }

        // 4. Hard clamp to 0 if dead
        if (pct <= 0) finalIntensity = 0;

        // Apply
        globalLight.intensity = Mathf.Clamp(finalIntensity, 0f, stableIntensity);
    }
}