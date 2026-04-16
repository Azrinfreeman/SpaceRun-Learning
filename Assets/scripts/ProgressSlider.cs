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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isFull = false;
    }

    // Update is called once per frame
    void Update()
    {
        textValue.text = GetComponent<Slider>().value.ToString("F0") + "%";

        if (isFull)
        {
            uiManager.ShowGameCompleted(
                GameManager.Instance.Score,
                PlayerPrefs.GetFloat("HighScore", 0f)
            );
        }
    }

    public void addProgress(int progress)
    {
        StartCoroutine(AnimateSlider(progress));
    }

    private IEnumerator AnimateSlider(int progress)
    {
        Slider slider = GetComponent<Slider>();
        float startValue = slider.value;
        float targetValue = slider.value + progress;
        float duration = 0.5f; // seconds
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            yield return null;
        }

        slider.value = targetValue;
        if (slider.value == 100f)
        {
            isFull = true;
        } // ensure it lands exactly on target
    }
}
