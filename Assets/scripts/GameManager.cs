using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Speed Settings")]
    public float startSpeed = 5f;
    public float maxSpeed = 20f;
    public float speedIncreaseRate = 0.5f;

    [Header("Score")]
    public float scoreMultiplier = 1f;

    [Header("Level")]
    [Tooltip("1 = Level 1, 2 = Level 2")]
    public int currentLevel = 1;

    // Runtime state
    public float CurrentSpeed { get; private set; }
    public float Score { get; private set; }
    public bool IsGameOver { get; set; }

    private float highScore = 0f;

    private UIManager uiManager;
    private PlayerController player;
    private PlayerHealth playerHealth;
    private GroundSpawner groundSpawner;
    private CollectibleSpawner collectibleSpawner;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        highScore = PlayerPrefs.GetFloat("HighScore", 0f);
    }

    void Start()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        player = FindFirstObjectByType<PlayerController>();
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        groundSpawner = FindFirstObjectByType<GroundSpawner>();
        collectibleSpawner = FindFirstObjectByType<CollectibleSpawner>();

        // Read level chosen from main menu — overrides Inspector value
        currentLevel = MainMenuManager.SelectedLevel;

        //StartGame();
    }

    void Update()
    {
        if (IsGameOver)
            return;

        CurrentSpeed = Mathf.Min(CurrentSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);
        Score += CurrentSpeed * scoreMultiplier * Time.deltaTime;

        uiManager?.UpdateScore(Score);
    }

    public void AddScore(float amount)
    {
        if (IsGameOver)
            return;
        Score = Mathf.Max(0f, Score + amount);
        uiManager?.UpdateScore(Score);
    }

    public void StartGame()
    {
        IsGameOver = false;
        CurrentSpeed = startSpeed;
        Score = 0f;

        uiManager?.ShowGame();
        uiManager?.UpdateScore(0f);
        uiManager?.UpdateHighScore(highScore);

        // Start correct level logic
        if (currentLevel == 2)
        {
            Level2Manager.Instance?.StartLevel2();
            Level3Manager.Instance?.StopLevel3();
        }
        else if (currentLevel == 3)
        {
            Level3Manager.Instance?.StartLevel3();
            Level2Manager.Instance?.StopLevel2();
        }
        else
        {
            Level2Manager.Instance?.StopLevel2();
            Level3Manager.Instance?.StopLevel3();
        }

        // Level 1 collectibles only spawn in Level 1
        if (collectibleSpawner != null)
            collectibleSpawner.gameObject.SetActive(currentLevel == 1);
    }

    public void GameOver()
    {
        IsGameOver = true;

        Level2Manager.Instance?.StopLevel2();
        Level3Manager.Instance?.StopLevel3();

        if (Score > highScore)
        {
            highScore = Score;
            PlayerPrefs.SetFloat("HighScore", highScore);
            PlayerPrefs.Save();
        }

        uiManager?.ShowGameOver(Score, highScore);
    }

    public void GameComplete()
    {
        IsGameOver = true;

        Level2Manager.Instance?.StopLevel2();
        Level3Manager.Instance?.StopLevel3();

        if (Score > highScore)
        {
            highScore = Score;
            PlayerPrefs.SetFloat("HighScore", highScore);
            PlayerPrefs.Save();
        }

        uiManager?.ShowGameCompleted(Score, highScore);
    }

    public void RestartGame()
    {
        if (player != null)
        {
            player.transform.position = new Vector3(-4.3f, 0f, 0f);
            player.ResetPlayer();

            var anim = player.GetComponent<Animator>();
            if (anim != null)
                anim.Play("CharacterRun");
        }

        // Re-read level in case it changed
        currentLevel = MainMenuManager.SelectedLevel;

        playerHealth?.ResetHealth();
        groundSpawner?.ResetSpawner();
        collectibleSpawner?.ResetSpawner();

        // Destroy any leftover Level 2 / 3 collectibles
        foreach (var item in FindObjectsByType<Level2CollectibleItem>(FindObjectsSortMode.None))
            Destroy(item.gameObject);
        foreach (var item in FindObjectsByType<Level3CollectibleItem>(FindObjectsSortMode.None))
            Destroy(item.gameObject);

        // Fully reset Level 2 / 3 spawner state so they start fresh
        Level2CollectibleSpawner.Instance?.ResetSpawn();
        Level3CollectibleSpawner.Instance?.ResetSpawn();
        Level2Manager.Instance?.StopLevel2();
        Level3Manager.Instance?.StopLevel3();

        if (uiManager != null)
        {
            uiManager.gameCompletePanel.SetActive(false);
            uiManager.gameOverPanel.SetActive(false);
        }

        if (ProgressSlider.instance != null)
            ProgressSlider.instance.GetComponent<Slider>().value = 0f;

        StartGame();
    }

    public void GoHome()
    {
        Level2Manager.Instance?.StopLevel2();
        Level3Manager.Instance?.StopLevel3();
        SceneManager.LoadScene("Menu");
    }
}
