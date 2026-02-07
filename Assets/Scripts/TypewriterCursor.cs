using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TypewriterCursor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private RectTransform cursorRect;
    [SerializeField] private Image cursorImage;

    [Header("Settings")]
    [SerializeField] private float blinkSpeed = 5f;
    [SerializeField] private float verticalOffset = -2f; // Adjust to move line up/down relative to text bottom

    private bool isActive = true;

    void Update()
    {
        HandleBlink();
    }

    void HandleBlink()
    {
        if (!isActive || cursorImage == null) return;

        float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);

        Color c = cursorImage.color;
        c.a = alpha;
        cursorImage.color = c;
    }

    public void MoveToChar(int charIndex)
    {
        if (targetText == null || cursorRect == null) return;

        targetText.ForceMeshUpdate();
        TMP_TextInfo textInfo = targetText.textInfo;

        if (charIndex >= textInfo.characterCount)
        {
            SetCursorVisibility(false);
            return;
        }

        SetCursorVisibility(true);

        TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];

        int lineIndex = charInfo.lineNumber;

        // This value is constant for the entire line, so 'a', 'g', 'y', and 'T' 
        float steadyY = textInfo.lineInfo[lineIndex].descender;

        // 3. Get X position (Standard alignment)
        float steadyX = charInfo.origin;

        // If the character is visible (normal letter), align with its visual left side
        if (charInfo.isVisible)
        {
            steadyX = charInfo.bottomLeft.x;
        }

        // 4. Apply Position
        cursorRect.localPosition = new Vector3(steadyX, steadyY + verticalOffset, 0);
    }

    public void SetCursorVisibility(bool visible)
    {
        if (cursorImage != null) cursorImage.enabled = visible;
        isActive = visible;
    }
}