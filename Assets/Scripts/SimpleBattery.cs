using UnityEngine;
using UnityEngine.Events;

public class SimpleBattery : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How many seconds the battery lasts from full to empty")]
    [SerializeField] public float maxSeconds = 5.0f;

    [Header("Visuals")]
    [Tooltip("The SpriteRenderer that displays the battery")]
    [SerializeField] private SpriteRenderer batteryRenderer;

    [Tooltip("Order them from Empty (Element 0) to Full (Element 5)")]
    [SerializeField] private Sprite[] batteryLevels;

    [Header("Events")]
    public UnityEvent OnPowerDepleted;
    public UnityEvent OnPowerRestored;

    // Public property so other scripts can check charge
    public float CurrentCharge { get; private set; }

    private bool isDepleted = false;

    void Start()
    {
        CurrentCharge = maxSeconds;
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

    public void AddCharge(float amount)
    {
        CurrentCharge += amount;
    }

    void UpdateVisuals()
    {
        if (batteryRenderer == null || batteryLevels.Length == 0) return;

        // 1. Calculate percentage (0.0 to 1.0)
        float pct = CurrentCharge / maxSeconds;

        // 2. Convert percentage to an Array Index
        // Example: If pct is 0.5 and we have 6 sprites -> Index 3
        int index = Mathf.FloorToInt(pct * batteryLevels.Length);

        // 3. Safety Clamp
        // If pct is 1.0, the math above gives us index 6 (which is out of bounds for an array of 6), so we clamp it.
        index = Mathf.Clamp(index, 0, batteryLevels.Length - 1);

        // 4. Swap the Sprite
        batteryRenderer.sprite = batteryLevels[index];
    }

    public float GetMaxCharge()
    {
        return maxSeconds;
    }
}