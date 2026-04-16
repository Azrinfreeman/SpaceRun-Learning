using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows an instruction panel when the game scene loads.
/// The game is frozen (Time.timeScale = 0) while instructions show.
/// Player can either tap/click to dismiss early, or wait for the countdown.
///
/// SETUP:
/// 1. Create a UI Panel in your Canvas, name it "InstructionPanel"
/// 2. Add instruction text, icons, whatever you like inside it
/// 3. Optionally add a countdown TextMeshPro text and a "TAP TO START" button
/// 4. Attach this script to an empty GameObject named "InstructionPanel"
/// 5. Assign references in the Inspector
/// 6. Make sure GameManager.StartGame() does NOT auto-call on Start —
///    see note below about GameManager tweak
/// </summary>
public class InstructionPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject instructionPanel;

    [Tooltip("Optional — shows countdown before auto-dismiss")]
    public TextMeshProUGUI countdownText;

    [Tooltip("Optional — 'TAP TO START' button to skip countdown")]
    public Button tapToStartButton;

    [Header("Timing")]
    [Tooltip("Seconds to show the panel before auto-starting")]
    public float displayDuration = 4f;

    [Header("Countdown Text Format")]
    public string countdownPrefix = "Starting in ";
    public string countdownSuffix = "...";

    // ── private ────────────────────────────────────────────────────
    private float timer = 0f;
    private bool isDismissed = false;

    // ── Unity lifecycle ────────────────────────────────────────────
    void Start()
    {
        // Freeze the game while instructions show
        Time.timeScale = 0f;

        instructionPanel.SetActive(true);

        if (tapToStartButton != null)
            tapToStartButton.onClick.AddListener(Dismiss);

        timer = displayDuration;
    }

    void Update()
    {
        if (isDismissed)
            return;

        // Use unscaled delta so timer works even when timeScale = 0
        timer -= Time.unscaledDeltaTime;

        // Update countdown text
        if (countdownText != null)
            countdownText.text =
                countdownPrefix
                + Mathf.CeilToInt(Mathf.Max(timer, 0f)).ToString()
                + countdownSuffix;

        // Tap / click anywhere to dismiss early
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            Dismiss();

        // Auto-dismiss when timer hits 0
        if (timer <= 0f)
            Dismiss();
    }

    // ── Dismiss ────────────────────────────────────────────────────

    public void Dismiss()
    {
        if (isDismissed)
            return;
        isDismissed = true;

        instructionPanel.SetActive(false);

        // Unfreeze the game
        Time.timeScale = 1f;

        // Tell GameManager to officially start
        GameManager.Instance?.StartGame();
    }

    // ── Cleanup on scene unload ────────────────────────────────────
    void OnDestroy()
    {
        // Safety — always restore timescale if this object is destroyed
        Time.timeScale = 1f;
    }
}
