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
    [SerializeField] private float dayScreenDuration = 2.0f;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float gracePeriodDuration = 2.0f;
    [SerializeField] private float gameOverScreenDuration = 4.0f;

    [Header("Scene References")]
    [SerializeField] private SimpleBattery battery;
    [SerializeField] private EmailTyper emailTyper;
    [SerializeField] private ChattyEmployee chattyEmployee;
    [SerializeField] private Camera mainCamera;
    
    [Header("Camera Positions")]
    [SerializeField] private Transform laptopFocus;
    [SerializeField] private Transform employeeFocus;
    [SerializeField] private float cameraPanSpeed = 2.0f;

    [Header("Camera Zoom Settings")]
    [SerializeField] private float normalZoomSize = 5.0f; 
    [SerializeField] private float laptopZoomSize = 3.5f; 
    [SerializeField] private float zoomSpeed = 2.0f;

    [Header("UI References")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text emailCountText;
    [SerializeField] private Image livesImage;
    [SerializeField] private Sprite[] lifeSprites;
    
    [Header("Screens")]
    [SerializeField] private GameObject day1Screen;
    [SerializeField] private GameObject day2Screen;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject firedScreen;

    private int currentLives;
    private int emailsSent;
    private float timeRemaining;
    private bool isGameActive = false;
    private bool isChattyEventActive = false;
    private float nextChattyEventTime;
    
    private float targetZoomSize;
    private Coroutine gracePeriodCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // --- MUSIC SWITCH FIX ---
        // We try to switch music here. 
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameMusic();
        }
        else
        {
            // If AudioManager is missing (started directly in GameScene for testing), creating a temporary one or logging error.
            Debug.LogWarning("AudioManager instance not found in Game Scene. Start from Main Menu to hear music.");
        }
        // ------------------------

        currentLives = maxLives;
        emailsSent = 0;
        timeRemaining = day1Duration;
        
        if (mainCamera != null)
        {
            targetZoomSize = normalZoomSize;
            mainCamera.orthographicSize = normalZoomSize;
        }

        UpdateLivesUI();
        UpdateEmailUI();
        UpdateTimerUI();

        if (battery != null)
        {
            battery.OnPowerDepleted.AddListener(OnBatteryDied);
            battery.OnPowerRestored.AddListener(OnBatteryRestored); 
        }
        
        if (emailTyper != null)
            emailTyper.OnEmailSent.AddListener(OnEmailCompleted);

        nextChattyEventTime = day1Duration - 60f;

        StartCoroutine(DayStartSequence());
    }

    void Update()
    {
        if (mainCamera != null)
        {
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoomSize, Time.deltaTime * zoomSpeed);
        }

        if (!isGameActive) return;

        if (!isChattyEventActive && Input.GetMouseButtonDown(1))
        {
            if (Mathf.Abs(targetZoomSize - laptopZoomSize) < 0.1f)
                targetZoomSize = normalZoomSize;
            else
                targetZoomSize = laptopZoomSize;
        }

        timeRemaining -= Time.deltaTime;
        UpdateTimerUI();

        if (timeRemaining <= 0)
        {
            EndDay();
            return;
        }

        if (!isChattyEventActive && timeRemaining <= nextChattyEventTime)
        {
            StartCoroutine(ChattyEventRoutine());
            nextChattyEventTime -= 60f; 
        }
    }

    // --- Start Day Logic ---

    public void StartDay1()
    {
        isGameActive = true;
        ShowScreen(null);
        
        if (emailTyper != null) emailTyper.enabled = true;

        targetZoomSize = laptopZoomSize;
    }

    public void GoToMainMenu()
    {
        // Switch back to title music before loading menu
        if (AudioManager.Instance != null) AudioManager.Instance.PlayTitleMusic();
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- Gameplay Logic ---

    private void OnBatteryDied()
    {
        if (!isGameActive) return;
        if (gracePeriodCoroutine != null) StopCoroutine(gracePeriodCoroutine);
        gracePeriodCoroutine = StartCoroutine(GracePeriodRoutine());
    }

    private void OnBatteryRestored()
    {
        if (gracePeriodCoroutine != null)
        {
            StopCoroutine(gracePeriodCoroutine);
            gracePeriodCoroutine = null;
        }
    }

    private IEnumerator GracePeriodRoutine()
    {
        yield return new WaitForSeconds(gracePeriodDuration);

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
        if (emailTyper != null) emailTyper.enabled = false;
        targetZoomSize = normalZoomSize;

        if (emailsSent < minEmailsToSurvive)
        {
            StartCoroutine(TransitionToMainMenu(firedScreen));
        }
        else
        {
            ShowScreen(day2Screen);
        }
    }

    private void GameOver()
    {
        isGameActive = false;
        if (emailTyper != null) emailTyper.enabled = false;
        targetZoomSize = normalZoomSize;
        StartCoroutine(TransitionToMainMenu(gameOverScreen));
    }

    private IEnumerator TransitionToMainMenu(GameObject screenToShow)
    {
        ShowScreen(screenToShow);
        yield return new WaitForSeconds(gameOverScreenDuration);
        GoToMainMenu();
    }

    // --- Coroutines ---

    private IEnumerator DayStartSequence()
    {
        ShowScreen(day1Screen);
        
        CanvasGroup group = day1Screen.GetComponent<CanvasGroup>();
        if (group == null) group = day1Screen.AddComponent<CanvasGroup>();
        group.alpha = 1f;

        yield return new WaitForSeconds(dayScreenDuration);

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        StartDay1();
        group.alpha = 1f;
    }

    private IEnumerator ChattyEventRoutine()
    {
        isChattyEventActive = true;
        targetZoomSize = normalZoomSize; // Zoom out for event

        if (emailTyper != null) emailTyper.enabled = false;

        yield return MoveCamera(employeeFocus.position);

        if (chattyEmployee != null) chattyEmployee.StartChattering();

        yield return new WaitForSeconds(15f);

        if (chattyEmployee != null) chattyEmployee.StopChattering();

        yield return MoveCamera(laptopFocus.position);

        if (isGameActive && emailTyper != null) emailTyper.enabled = true;

        targetZoomSize = laptopZoomSize; // Zoom back in after event
        isChattyEventActive = false;
    }

    private IEnumerator MoveCamera(Vector3 targetPos)
    {
        if (mainCamera == null) yield break;

        float t = 0;
        Vector3 startPos = mainCamera.transform.position;
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