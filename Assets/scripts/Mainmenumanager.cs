using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main menu manager.
/// Auto-sets button labels on Start — no manual text editing needed.
///
/// SETUP:
/// 1. Attach to empty GameObject named "MainMenuManager"
/// 2. Assign Level1Button, Level2Button, ExitButton in Inspector
/// 3. Wire each button OnClick():
///    Level1Button → LoadLevel1()
///    Level2Button → LoadLevel2()
///    ExitButton   → ExitGame()
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Button exitButton;

    [Header("Button Labels (auto-set on Start)")]
    public string level1Label = "Level 1";
    public string level2Label = "Level 2";
    public string level3Label = "Level 3";
    public string exitLabel = "Exit";

    [Header("Scene")]
    [Tooltip("Must match exactly in File → Build Settings")]
    public string gameSceneName = "GameScene";

    // ── Static level selection ─────────────────────────────────────
    public static int SelectedLevel = 1;

    // ── Unity lifecycle ────────────────────────────────────────────
    void Start()
    {
        SetButtonLabel(level1Button, level1Label);
        SetButtonLabel(level2Button, level2Label);
        SetButtonLabel(level3Button, level3Label);
        SetButtonLabel(exitButton, exitLabel);
    }

    // ── Button methods ─────────────────────────────────────────────

    public void LoadLevel1()
    {
        SelectedLevel = 1;
        SceneManager.LoadScene(gameSceneName);
    }

    public void LoadLevel2()
    {
        SelectedLevel = 2;
        SceneManager.LoadScene(gameSceneName);
    }

    public void LoadLevel3()
    {
        SelectedLevel = 3;
        SceneManager.LoadScene(gameSceneName);
    }

    public void ExitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    // ── Helper ─────────────────────────────────────────────────────

    void SetButtonLabel(Button btn, string label)
    {
        if (btn == null)
            return;

        // Try TextMeshPro first, fall back to legacy Text
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = label;
            return;
        }

        var legacy = btn.GetComponentInChildren<Text>();
        if (legacy != null)
            legacy.text = label;
    }
}
