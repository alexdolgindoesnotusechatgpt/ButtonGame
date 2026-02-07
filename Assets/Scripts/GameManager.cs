using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Limits")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float day1Duration = 180f; // 3 minutes
    [SerializeField] private int minEmailsToSurvive = 2;

    [Header("Transition Settings")]
    [Tooltip("How long the Day 1 screen stays fully visible before fading.")]
    [SerializeField] private float dayScreenDuration = 2.0f;
    [Tooltip("How long the fade out takes.")]
    [SerializeField] private float fadeDuration = 1.0f;
    [Tooltip("Grace period (in seconds) before losing a life when battery dies.")]
    [SerializeField] private float gracePeriodDuration = 2.0f;
    [Tooltip("How long to show Game Over / Fired screen before returning to Main Menu.")]
    [SerializeField] private float gameOverScreenDuration = 4.0f;

    [Header("Scene References")]
    [SerializeField] private SimpleBattery battery;
    [SerializeField] private EmailTyper emailTyper;
    [SerializeField] private ChattyEmployee chattyEmployee;
    [SerializeField] private Camera mainCamera;
    
    [Header("Camera Positions")]
    [Tooltip("Empty GameObject positioned where camera looks at laptop")]
    [SerializeField] private Transform laptopFocus;
    [Tooltip("Empty GameObject positioned where camera looks at employee")]
    [SerializeField] private Transform employeeFocus;
    [SerializeField] private float cameraPanSpeed = 2.0f;

    [Header("Camera Zoom Settings")]
    [SerializeField] private float normalZoomSize = 5.0f; // Standard orthographic size
    [SerializeField] private float laptopZoomSize = 3.5f; // Zoomed in orthographic size
    [SerializeField] private float zoomSpeed = 2.0f;

    [Header("UI References")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text emailCountText;
    
    [Tooltip("The UI Image used to display the lives.")]
    [SerializeField] private Image livesImage;

    [Tooltip("Order from 0 lives to 3 lives. Element 0 = Empty, Element 3 = Full.")]
    [SerializeField] private Sprite[] lifeSprites;
    
    [Header("Screens")]
    [SerializeField] private GameObject day1Screen; // Contains "Day 1" text
    [SerializeField] private GameObject day2Screen; // "You Survived"
    [SerializeField] private GameObject gameOverScreen; // "You Died" (0 Lives)
    [SerializeField] private GameObject firedScreen;  // "You're Fired" (< 2 Emails)

    private int currentLives;
    private int emailsSent;
    private float timeRemaining;
    private bool isGameActive = false;
    private bool isChattyEventActive = false;
    private float nextChattyEventTime;
    
    // Zoom state
    private float targetZoomSize;

    private Coroutine gracePeriodCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Setup Initial State
        currentLives = maxLives;
        emailsSent = 0;
        timeRemaining = day1Duration;
        
        // Initialize camera zoom
        if (mainCamera != null)
        {
            targetZoomSize = normalZoomSize;
            mainCamera.orthographicSize = normalZoomSize;
        }

        UpdateLivesUI();
        UpdateEmailUI();
        UpdateTimerUI();

        // Listen for Battery Death and Restoration
        if (battery != null)
        {
            battery.OnPowerDepleted.AddListener(OnBatteryDied);
            battery.OnPowerRestored.AddListener(OnBatteryRestored); 
        }
        
        // Listen for Email Sent
        if (emailTyper != null)
            emailTyper.OnEmailSent.AddListener(OnEmailCompleted);

        // Schedule first interruption (e.g., at 60 seconds remaining)
        nextChattyEventTime = day1Duration - 60f;

        // Start the Intro Sequence automatically
        StartCoroutine(DayStartSequence());
    }

    void Update()
    {
        // Handle Zooming every frame
        if (mainCamera != null)
        {
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoomSize, Time.deltaTime * zoomSpeed);
        }

        if (!isGameActive) return;

        // Timer Logic
        timeRemaining -= Time.deltaTime;
        UpdateTimerUI();

        if (timeRemaining <= 0)
        {
            EndDay();
            return;
        }

        // Chatty Event Logic (Triggers every 60 seconds)
        if (!isChattyEventActive && timeRemaining <= nextChattyEventTime)
        {
            StartCoroutine(ChattyEventRoutine());
            nextChattyEventTime -= 60f; 
        }
    }

    // --- Public methods for UI Buttons ---

    public void StartDay1()
    {
        isGameActive = true;
        ShowScreen(null); // Hide all overlay screens
        
        // Ensure inputs are active
        if (emailTyper != null) emailTyper.enabled = true;

        // Zoom in when day starts
        targetZoomSize = laptopZoomSize;
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- Gameplay Logic ---

    // Triggered when battery hits 0
    private void OnBatteryDied()
    {
        if (!isGameActive) return;

        // Start the grace period instead of immediately losing a life
        if (gracePeriodCoroutine != null) StopCoroutine(gracePeriodCoroutine);
        gracePeriodCoroutine = StartCoroutine(GracePeriodRoutine());
    }

    // Triggered usually by the ChargerButton adding charge back to the battery
    private void OnBatteryRestored()
    {
        // One of the conditions to survive the grace period: charge it back up!
        if (gracePeriodCoroutine != null)
        {
            StopCoroutine(gracePeriodCoroutine);
            gracePeriodCoroutine = null;
        }
    }

    private IEnumerator GracePeriodRoutine()
    {
        // Wait for player to panic and find the charger
        yield return new WaitForSeconds(gracePeriodDuration);

        // If we get here, the battery was not restored in time
        if (isGameActive)
        {
            LoseLife();
        }
        
        gracePeriodCoroutine = null;
    }

    private void LoseLife()
    {
        currentLives--;
        UpdateLivesUI();

        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            // Reset Battery so the game can continue
            // We fully recharge it instantly so the player can continue their emails
            // Note: EmailTyper is untouched, so progress remains.
            if (battery != null)
                battery.AddCharge(battery.maxSeconds);
        }
    }

    private void OnEmailCompleted()
    {
        emailsSent++;
        UpdateEmailUI();
    }

    private void EndDay()
    {
        isGameActive = false;
        
        // Disable gameplay
        if (emailTyper != null) emailTyper.enabled = false;

        // Zoom out
        targetZoomSize = normalZoomSize;

        if (emailsSent < minEmailsToSurvive)
        {
            StartCoroutine(TransitionToMainMenu(firedScreen));
        }
        else
        {
            // Success!
            ShowScreen(day2Screen);
        }
    }

    private void GameOver()
    {
        isGameActive = false;
        if (emailTyper != null) emailTyper.enabled = false;

        // Zoom out
        targetZoomSize = normalZoomSize;
        
        StartCoroutine(TransitionToMainMenu(gameOverScreen));
    }

    private IEnumerator TransitionToMainMenu(GameObject screenToShow)
    {
        ShowScreen(screenToShow);

        // Wait a few seconds for player to realize their failure
        yield return new WaitForSeconds(gameOverScreenDuration);

        GoToMainMenu();
    }

    // --- Coroutines ---

    private IEnumerator DayStartSequence()
    {
        // 1. Show the screen (Active = true)
        ShowScreen(day1Screen);
        
        // Helper: Ensure we have a CanvasGroup to fade
        CanvasGroup group = day1Screen.GetComponent<CanvasGroup>();
        if (group == null) group = day1Screen.AddComponent<CanvasGroup>();
        
        group.alpha = 1f;

        // 2. Wait for reading time
        yield return new WaitForSeconds(dayScreenDuration);

        // 3. Fade Out
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        // 4. Start Game (This will also setSetActive(false) on the screen)
        StartDay1();
        
        // Reset alpha in case we reuse this object later (optional)
        group.alpha = 1f;
    }

    private IEnumerator ChattyEventRoutine()
    {
        isChattyEventActive = true;
        
        // 1. Zoom OUT before moving
        targetZoomSize = normalZoomSize;

        // 2. Disable working on laptop so typing stops
        if (emailTyper != null) emailTyper.enabled = false;

        // 3. Pan to Employee
        yield return MoveCamera(employeeFocus.position);

        // 4. Start Chattering
        if (chattyEmployee != null) chattyEmployee.StartChattering();

        // 5. Wait for duration (e.g., 15 seconds)
        yield return new WaitForSeconds(15f);

        // 6. Stop Chattering
        if (chattyEmployee != null) chattyEmployee.StopChattering();

        // 7. Pan Back
        yield return MoveCamera(laptopFocus.position);

        // 8. Re-enable working
        if (isGameActive && emailTyper != null) emailTyper.enabled = true;

        // 9. Zoom back IN
        targetZoomSize = laptopZoomSize;

        isChattyEventActive = false;
    }

    private IEnumerator MoveCamera(Vector3 targetPos)
    {
        if (mainCamera == null) yield break;

        float t = 0;
        Vector3 startPos = mainCamera.transform.position;
        // Maintain the camera's original Z depth (critical for 2D)
        targetPos.z = startPos.z;

        while (t < 1.0f)
        {
            t += Time.deltaTime * cameraPanSpeed;
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        mainCamera.transform.position = targetPos;
    }

    // --- UI Helpers ---

    private void UpdateLivesUI()
    {
        if (livesImage == null || lifeSprites == null || lifeSprites.Length == 0) return;

        // Safely map currentLives (0-3) to the array index
        int index = Mathf.Clamp(currentLives, 0, lifeSprites.Length - 1);
        
        livesImage.sprite = lifeSprites[index];
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            float t = Mathf.Max(0, timeRemaining);
            int minutes = Mathf.FloorToInt(t / 60);
            int seconds = Mathf.FloorToInt(t % 60);
            timerText.text = $"{minutes}:{seconds:00}";
        }
    }

    private void UpdateEmailUI()
    {
        if (emailCountText != null)
            emailCountText.text = $"Emails: {emailsSent}";
    }

    private void ShowScreen(GameObject screenToShow)
    {
        if (day1Screen) day1Screen.SetActive(day1Screen == screenToShow);
        if (day2Screen) day2Screen.SetActive(day2Screen == screenToShow);
        if (gameOverScreen) gameOverScreen.SetActive(gameOverScreen == screenToShow);
        if (firedScreen) firedScreen.SetActive(firedScreen == screenToShow);
    }
}