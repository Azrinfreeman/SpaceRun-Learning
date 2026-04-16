using TMPro;
using UnityEngine;

/// <summary>
/// Auto-sets any TextMeshProUGUI in the game scene to show
/// which level is currently active. Updates immediately on Start.
///
/// SETUP:
/// 1. Create a TextMeshProUGUI in your game scene Canvas
/// 2. Attach this script to the same GameObject as the TMP text
///    OR assign the target text in the Inspector
/// </summary>
public class LevelDisplayUI : MonoBehaviour
{
    [Tooltip("Leave empty to use the TMP on this same GameObject")]
    public TextMeshProUGUI targetText;

    [Tooltip("Format string — {0} is replaced with the level number")]
    public string format = "Level {0}";

    void Start()
    {
        if (targetText == null)
            targetText = GetComponent<TextMeshProUGUI>();

        UpdateText();
    }

    void UpdateText()
    {
        if (targetText == null)
            return;
        int level = MainMenuManager.SelectedLevel;
        targetText.text = string.Format(format, level);
    }
}
