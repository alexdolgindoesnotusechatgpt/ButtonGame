using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Sprite pressedButtonSprite;

    private void Start()
    {
        // Play Title Music immediately
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTitleMusic();
        }
    }

    private void Awake()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSfx();

                if (pressedButtonSprite != null)
                {
                    Image btnImage = playButton.GetComponent<Image>();
                    if (btnImage != null) btnImage.sprite = pressedButtonSprite;
                }

                SceneManager.LoadScene("GameScene");
            });
        }
    }
}
