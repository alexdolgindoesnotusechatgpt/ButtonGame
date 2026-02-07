using UnityEngine;
using TMPro;
using UnityEngine.UI; // Required for the Button
using UnityEngine.Events;

public class EmailTyper : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text emailDisplay;
    [Tooltip("The UI Button on the laptop screen that sends the email")]
    [SerializeField] private Button sendButton;

    [Header("Game Settings")]
    [TextArea(5, 10)]
    [SerializeField] private string emailContent = "Subject: Hello World...";
    [SerializeField] private Color typedColor = Color.green;
    [SerializeField] private Color remainingColor = Color.white;

    [Header("Scrolling Settings")]
    [SerializeField] private RectTransform textRect;
    [SerializeField] private float scrollSpeed = 8f;
    [SerializeField] private float preferredLineY = -50f;

    [Header("Visuals")]
    [SerializeField] private TypewriterCursor cursorScript; 

    [Header("Events")]
    public UnityEvent OnEmailSent;

    private string currentTyped = "";
    private bool isTypingComplete = false;
    private bool isEmailSent = false;
    private float targetScrollY = 0f;

    void Start()
    {
        emailDisplay.alignment = TextAlignmentOptions.TopLeft;

        // --- FIX ADDED HERE ---
        // We sanitize the starting text to remove invisible characters
        emailContent = CleanText(emailContent);
        // ----------------------

        if (sendButton != null)
        {
            sendButton.interactable = false;
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }

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
        if (isEmailSent) return;

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
            // Consolidate both Return and Enter keys to a standard newline character
            else if (c == '\n' || c == '\r')
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
                CheckButtonState(); // Check if we are done
            }
            else
            {
                // Optional Debug: Uncomment this if you get stuck again
                // Debug.Log($"Expected: {(int)expectedChar} | Typed: {(int)inputChar}");
            }
        }
    }

    void UpdateDisplay()
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(typedColor);
        string remainHex = ColorUtility.ToHtmlStringRGB(remainingColor);
        string remainingText = "";

        if (currentTyped.Length < emailContent.Length)
            remainingText = emailContent.Substring(currentTyped.Length);

        emailDisplay.text = $"<color=#{hexColor}>{currentTyped}</color><color=#{remainHex}>{remainingText}</color>";

        emailDisplay.ForceMeshUpdate();
        UpdateScrollTarget();
        CheckButtonState();

        if (cursorScript != null)
        {
            int nextIndex = currentTyped.Length;
            cursorScript.MoveToChar(nextIndex);
        }
    }

    void CheckButtonState()
    {
        isTypingComplete = (currentTyped.Length == emailContent.Length);

        if (sendButton != null)
        {
            sendButton.interactable = isTypingComplete && !isEmailSent;
        }
    }

    public void OnSendButtonClicked()
    {
        if (isTypingComplete && !isEmailSent)
        {
            isEmailSent = true;
            Debug.Log("Email Sent!");

            if (sendButton != null) sendButton.interactable = false;
            if (cursorScript != null) cursorScript.SetCursorVisibility(false);
            OnEmailSent?.Invoke();
        }
    }

    public void SetNewEmail(string newText)
    {
        // --- FIX ADDED HERE ---
        // Clean the new email text before we start typing it
        emailContent = CleanText(newText);
        // ----------------------

        currentTyped = "";
        isTypingComplete = false;
        isEmailSent = false;
        targetScrollY = 0;

        UpdateDisplay();
        
        if (cursorScript != null) cursorScript.SetCursorVisibility(true);
    }

    // --- HELPER FUNCTION TO FIX INVISIBLE CHARACTERS ---
    private string CleanText(string rawText)
    {
        // Replaces the invisible "Carriage Return" (\r) with nothing.
        // This ensures the Enter key (\n) matches perfectly.
        return rawText.Replace("\r", "");
    }

    // --- Scrolling Logic ---
    void HandleScrolling()
    {
        if (textRect == null) return;
        Vector3 newPos = textRect.anchoredPosition;
        newPos.y = Mathf.Lerp(newPos.y, targetScrollY, Time.deltaTime * scrollSpeed);
        textRect.anchoredPosition = newPos;
    }

    void UpdateScrollTarget()
    {
        int charIndex = currentTyped.Length;
        if (charIndex >= emailDisplay.textInfo.characterCount)
            charIndex = emailDisplay.textInfo.characterCount - 1;

        if (charIndex < 0) return;

        int lineIndex = emailDisplay.textInfo.characterInfo[charIndex].lineNumber;

        if (lineIndex < emailDisplay.textInfo.lineInfo.Length)
        {
            float currentLineTopY = emailDisplay.textInfo.lineInfo[lineIndex].ascender;
            float neededY = preferredLineY - currentLineTopY;
            if (neededY < 0) neededY = 0;
            targetScrollY = neededY;
        }
    }

    void TurnOffScreen()
    {
        this.enabled = false;

    }
}