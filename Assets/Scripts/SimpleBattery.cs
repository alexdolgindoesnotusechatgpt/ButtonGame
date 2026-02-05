using UnityEngine;
using UnityEngine.Events;

public class SimpleBattery : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How many seconds the battery lasts from full to empty")]
    [SerializeField] private float maxSeconds = 5.0f;

    [Header("Visuals")]
    [SerializeField] private Transform fillBarTransform; // Assign the Green Bar
    [SerializeField] private SpriteRenderer fillRenderer; // Assign Green Bar (for color)
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color criticalColor = Color.red;

    [Header("Events")]
    public UnityEvent OnPowerDepleted;
    public UnityEvent OnPowerRestored;

    // Public property so other scripts can check charge
    public float CurrentCharge { get; private set; }

    private bool isDepleted = false;
    private Vector3 originalScale;

    void Start()
    {
        CurrentCharge = maxSeconds;
        if (fillBarTransform != null) originalScale = fillBarTransform.localScale;
        UpdateVisuals();
    }

    void Update()
    {
        // 1. Always Drain
        CurrentCharge -= Time.deltaTime;

        // 2. Clamp
        CurrentCharge = Mathf.Clamp(CurrentCharge, 0, maxSeconds);

        // 3. Update Visuals
        UpdateVisuals();

        // 4. Check State
        if (CurrentCharge <= 0 && !isDepleted)
        {
            isDepleted = true;
            OnPowerDepleted?.Invoke();
        }
        else if (CurrentCharge > 0 && isDepleted)
        {
            isDepleted = false;
            OnPowerRestored?.Invoke();
        }
    }

    // Call this from the button!
    public void AddCharge(float amount)
    {
        CurrentCharge += amount;
    }

    void UpdateVisuals()
    {
        if (fillBarTransform == null) return;

        float pct = CurrentCharge / maxSeconds;

        // Scale Logic (Requires Pivot Left on Sprite)
        Vector3 newScale = originalScale;
        newScale.x = originalScale.x * pct;
        fillBarTransform.localScale = newScale;

        // Color Logic
        if (fillRenderer != null)
        {
            fillRenderer.color = (pct < 0.25f) ? criticalColor : normalColor;
        }
    }
}