using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LaptopDimmer : MonoBehaviour
{
    [Header("Connections")]
    [SerializeField] private SimpleBattery battery;

    [Header("Settings")]
    [Tooltip("Brightness when battery is full (0 to 1)")]
    [SerializeField] private float maxBrightness = 1.0f;

    [Tooltip("How much the screen flickers (0.5 = dips to 50% brightness)")]
    [SerializeField] private float flickerStrength = 0.3f;

    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (battery == null || canvasGroup == null) return;

        // 1. Get Battery Percentage
        float pct = battery.CurrentCharge / battery.GetMaxCharge();

        float targetAlpha = maxBrightness;

        // 2. Dimming Logic (Start dimming at 50%)
        if (pct <= 0.5f)
        {
            // Map 0.0-0.5 battery to 0.0-1.0 brightness
            float dimFactor = pct / 0.5f;
            targetAlpha = Mathf.Lerp(0f, maxBrightness, dimFactor);
        }

        // 3. Flicker Logic (Start flickering at 30%)
        if (pct < 0.3f && pct > 0f)
        {
            float noise = Mathf.PerlinNoise(Time.time * 15f, 0f); // Fast flickering
            if (noise > 0.6f)
            {
                targetAlpha -= (flickerStrength * noise);
            }
        }

        // 4. Hard Cutoff at 0
        if (pct <= 0) targetAlpha = 0f;

        // Apply to the UI
        canvasGroup.alpha = Mathf.Clamp01(targetAlpha);
    }
}