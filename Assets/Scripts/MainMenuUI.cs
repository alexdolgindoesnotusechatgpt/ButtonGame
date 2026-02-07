using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Sprite pressedButtonSprite; // The sprite to show when clicked

    private void Awake()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(() =>
            {
                // Switch the sprite immediately
                if (pressedButtonSprite != null)
                {
                    Image btnImage = playButton.GetComponent<Image>();
                    if (btnImage != null)
                    {
                        btnImage.sprite = pressedButtonSprite;
                    }
                }

                // Load the game scene
                SceneManager.LoadScene("GameScene");
            });
        }
    }
}
