using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressSlider : MonoBehaviour
{
    public static ProgressSlider instance;
    private UIManager uiManager;

    private bool isFull;

    void Awake()
    {
        instance = this;
        uiManager = FindAnyObjectByType<UIManager>();
    }

    public TextMeshProUGUI textValue;

    void Start()
    {
        isFull = false;
    }

    void Update()
    {
        textValue.text = GetComponent<Slider>().value.ToString("F0") + "%";

        // NOTE: Do NOT call ShowGameCompleted or GameComplete here.
        // isFull is set inside AnimateSlider once the coroutine finishes,
        // and GameComplete is called there exactly once. Calling anything
        // from Update() means it fires every frame after the slider fills,
        // which caused all remaining collectibles to be wiped on the next
        // RestartGame() because IsGameOver was set prematurely.
    }

    public void addProgress(int progress)
    {
        StartCoroutine(AnimateSlider(progress));
    }

    private IEnumerator AnimateSlider(int progress)
    {
        Slider slider = GetComponent<Slider>();
        float startValue = slider.value;
        float targetValue = Mathf.Min(slider.value + progress, slider.maxValue);
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            yield return null;
        }

        slider.value = targetValue;

        // Only trigger game complete ONCE, right here, when the animation finishes.
        if (!isFull && slider.value >= slider.maxValue)
        {
            isFull = true;
            GameManager.Instance?.GameComplete();
        }
    }

    // Called by GameManager.RestartGame() to reset state for a new round.
    public void ResetSlider()
    {
        StopAllCoroutines();
        isFull = false;
        GetComponent<Slider>().value = 0f;
    }
}