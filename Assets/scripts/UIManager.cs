using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public GameObject gameCompletePanel; // fixed: was GameManager, should be GameObject
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalHighScoreText;
    public Button restartButton;
    private PlayerController playerController;

    void Start()
    {
        gameOverPanel.SetActive(false);
        gameCompletePanel.SetActive(false);
        restartButton.onClick.AddListener(OnRestartClicked);
        playerController = FindAnyObjectByType<PlayerController>();
    }

    public void ShowGame()
    {
        gameOverPanel.SetActive(false);
        gameCompletePanel.SetActive(false);
    }

    public void UpdateScore(float score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.FloorToInt(score).ToString();
    }

    public void UpdateHighScore(float hs)
    {
        if (highScoreText != null)
            highScoreText.text = "Best: " + Mathf.FloorToInt(hs).ToString();
    }

    public void ShowGameOver(float score, float highScore)
    {
        gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + Mathf.FloorToInt(score).ToString();

        if (finalHighScoreText != null)
            finalHighScoreText.text = "Best: " + Mathf.FloorToInt(highScore).ToString();
    }

    public void ShowGameCompleted(float score, float highscore)
    {
        GameManager.Instance.IsGameOver = true; // Ensure game is marked as over

        gameCompletePanel.SetActive(true);
        playerController.rb.linearVelocity = Vector2.zero;
        playerController.rb.gravityScale = 1f;
        playerController.animator.Play("Static");
        if (finalScoreText != null)
            finalScoreText.text = "Score: " + Mathf.FloorToInt(score).ToString();

        if (finalHighScoreText != null)
            finalHighScoreText.text = "Best: " + Mathf.FloorToInt(highscore).ToString();
    }

    void OnRestartClicked()
    {
        GameManager.Instance.RestartGame();
    }

    void OnReturnToMenuClicked() { }
}
