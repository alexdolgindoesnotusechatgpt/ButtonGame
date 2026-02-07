using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
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

    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] typeSoundEffects; // Array for variety
    [SerializeField] private AudioClip errorSoundEffect;
    [SerializeField] private AudioClip emailSentSoundEffect;
    private AudioSource audioSource;

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
        audioSource = GetComponent<AudioSource>();
        emailDisplay.alignment = TextAlignmentOptions.TopLeft;

        emailContent = CleanText(emailContent);

        if (sendButton != null)
        {
            sendButton.interactable = false;
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }

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
                    PlayTypingSound(); 
                }
            }
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
                CheckButtonState();
                PlayTypingSound();
            }
            else
            {
                PlayErrorSound();
            }
        }
    }

    void PlayTypingSound()
    {
        if (audioSource != null && typeSoundEffects != null && typeSoundEffects.Length > 0)
        {
            // Pick a random typing sound for variety
            AudioClip clip = typeSoundEffects[Random.Range(0, typeSoundEffects.Length)];
            // Randomize pitch slightly for realism
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip);
        }
    }

    void PlayErrorSound()
    {
        if (audioSource != null && errorSoundEffect != null)
        {
            audioSource.pitch = 1.0f; // Reset pitch for error
            audioSource.PlayOneShot(errorSoundEffect);
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

            // Play generic button click or specific email sent sound
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSfx();
            else if (audioSource != null && emailSentSoundEffect != null) audioSource.PlayOneShot(emailSentSoundEffect);

            if (sendButton != null) sendButton.interactable = false;
            if (cursorScript != null) cursorScript.SetCursorVisibility(false);
            OnEmailSent?.Invoke();
        }
    }

    public void SetNewEmail(string newText)
    {
        emailContent = CleanText(newText);
        currentTyped = "";
        isTypingComplete = false;
        isEmailSent = false;
        targetScrollY = 0;

        UpdateDisplay();
        
        if (cursorScript != null) cursorScript.SetCursorVisibility(true);
    }

    private string CleanText(string rawText)
    {
        return rawText.Replace("\r", "");
    }

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