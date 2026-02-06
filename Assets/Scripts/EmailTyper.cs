using UnityEngine;
using TMPro;
using UnityEngine.Events;
using JetBrains.Annotations;

public class EmailTyper : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text emailDisplay;

    [Header("Game Settings")]
    [TextArea(5, 10)]
    [SerializeField] private string emailContent = "Subject: Hello World\n\nTo whom it may concern,\n\nI am writing to inform you...";

    [SerializeField] private Color typedColor = Color.green;
    [SerializeField] private Color remainingColor = Color.white;

    [Header("Scrolling Settings")]
    [SerializeField] private RectTransform textRect;
    [SerializeField] private float scrollSpeed = 8f;

    [Tooltip("The line being typed will always try to stay at this Y position inside the mask (e.g., -50).")]
    [SerializeField] private float preferredLineY = -50f;

    [Tooltip("The black object that covers the screen when power dies")]
    [SerializeField] private GameObject blackScreenOverlay;

    [Header("Events")]
    public UnityEvent OnEmailComplete;

    private string currentTyped = "";
    private bool isComplete = false;
    private float targetScrollY = 0f;

    void Start()
    {

        // Text must be Top-Aligned for the math to work easily
        emailDisplay.alignment = TextAlignmentOptions.TopLeft;

        // Initialize display
        emailDisplay.text = emailContent;
        UpdateDisplay();
    }

    void Update()
    {
        HandleInput();
        HandleScrolling();
    }

    void HandleInput()
    {
        if (isComplete) return;

        foreach (char c in Input.inputString)
        {
            if (c == '\b') // Backspace
            {
                if (currentTyped.Length > 0)
                {
                    currentTyped = currentTyped.Substring(0, currentTyped.Length - 1);
                    UpdateDisplay();
                }
            }
            else if (c == '\n' || c == '\r') // Enter
            {
                CheckInput('\n');
            }
            else
            {
                CheckInput(c);
            }
        }
    }

    void CheckInput(char inputChar)
    {
        if (currentTyped.Length < emailContent.Length)
        {
            char expectedChar = emailContent[currentTyped.Length];

            if (inputChar == expectedChar)
            {
                currentTyped += inputChar;
                UpdateDisplay();

                if (currentTyped.Length == emailContent.Length)
                {
                    isComplete = true;
                    OnEmailComplete?.Invoke();
                }
            }
        }
    }

    void HandleScrolling()
    {
        if (textRect == null) return;

        Vector3 newPos = textRect.anchoredPosition;
        // Smoothly move the text container to the target Y
        newPos.y = Mathf.Lerp(newPos.y, targetScrollY, Time.deltaTime * scrollSpeed);
        textRect.anchoredPosition = newPos;
    }

    void UpdateDisplay()
    {
        // 1. Update Text Colors
        string hexColor = ColorUtility.ToHtmlStringRGB(typedColor);
        string remainHex = ColorUtility.ToHtmlStringRGB(remainingColor);
        string remainingText = "";

        if (currentTyped.Length < emailContent.Length)
            remainingText = emailContent.Substring(currentTyped.Length);

        // Simple coloring: Typed vs Remaining
        emailDisplay.text = $"<color=#{hexColor}>{currentTyped}</color><color=#{remainHex}>{remainingText}</color>";

        // 2. Force Update to calculate positions of the new text
        emailDisplay.ForceMeshUpdate();

        // 3. Update Scroll Target
        UpdateScrollTarget();
    }

    void UpdateScrollTarget()
    {
        // Get the index of the character we just typed
        int charIndex = currentTyped.Length;

        // Safety check: if at end of string, clamp to last char
        if (charIndex >= emailDisplay.textInfo.characterCount)
            charIndex = emailDisplay.textInfo.characterCount - 1;

        if (charIndex < 0) return;

        // Get the line number of that character
        TMP_CharacterInfo charInfo = emailDisplay.textInfo.characterInfo[charIndex];
        int lineIndex = charInfo.lineNumber;

        if (lineIndex < emailDisplay.textInfo.lineInfo.Length)
        {
            TMP_LineInfo lineInfo = emailDisplay.textInfo.lineInfo[lineIndex];

            // "ascender" is the Y position of the TOP of the current line
            float currentLineTopY = lineInfo.ascender;

            // Calculate how much we need to move the content UP to get this line to 'preferredLineY'
            float neededY = preferredLineY - currentLineTopY;

            // Clamp so we don't scroll past the top
            if (neededY < 0) neededY = 0;

            targetScrollY = neededY;
        }
    }

    public void SetNewEmail(string newText)
    {
        emailContent = newText;
        currentTyped = "";
        isComplete = false;
        targetScrollY = 0;
        UpdateDisplay();
    }

    public void TurnOffScreen()
    {
        this.enabled = false;
        emailDisplay.enabled = false;
    }
    public void TurnOnScreen()
    {
        this.enabled = true;
        emailDisplay.enabled = true;
    }
}