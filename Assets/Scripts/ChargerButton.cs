using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ChargerButton : MonoBehaviour
{
    [Header("Connections")]
    [SerializeField] private SimpleBattery targetBattery; // Drag the Battery Object here

    [Header("Charging Settings")]
    [Tooltip("How fast to charge. 2.0 = charges 2x faster than it drains.")]
    [SerializeField] private float chargeMultiplier = 3.0f;

    [Header("Button Visuals")]
    [SerializeField] private Sprite unpressedSprite;
    [SerializeField] private Sprite pressedSprite;

    private bool isHolding = false;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSprite(false);
    }

    void Update()
    {
        // If holding AND the battery exists
        if (isHolding && targetBattery != null)
        {
            // Send charge to the battery
            // We multiply by Time.deltaTime here so the battery gets seconds-worth of charge
            targetBattery.AddCharge(Time.deltaTime * chargeMultiplier);
        }
    }

    // --- Visual Swapping ---
    void UpdateSprite(bool isPressed)
    {
        if (spriteRenderer == null) return;

        if (isPressed && pressedSprite != null)
            spriteRenderer.sprite = pressedSprite;
        else if (!isPressed && unpressedSprite != null)
            spriteRenderer.sprite = unpressedSprite;
    }

    // --- Input Handling ---

    private void OnMouseDown()
    {
        isHolding = true;
        UpdateSprite(true);
        
        // Play Global Click Sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSfx();
        }
    }

    private void OnMouseUp()
    {
        isHolding = false;
        UpdateSprite(false);
    }

    private void OnMouseExit()
    {
        // If mouse slips off button, release it
        isHolding = false;
        UpdateSprite(false);
    }
}