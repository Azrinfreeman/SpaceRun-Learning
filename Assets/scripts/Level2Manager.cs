using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

public class Level2Manager : MonoBehaviour
{
    public static Level2Manager Instance { get; private set; }

    [Header("Letter Prefabs & Sounds")]
    public LetterEntry[] letters;

    [Header("Question Settings")]
    public int soundRepeatCount = 3;
    public float repeatInterval = 1.2f;
    public float initialDelay = 0.5f;

    [Header("Wrong Collectibles Per Question")]
    public int wrongLetterCount = 2;

    [Header("Score")]
    public float correctScoreValue = 15f;
    public float wrongScorePenalty = 5f;

    [Header("UI (optional)")]
    public TextMeshProUGUI feedbackText;
    public float feedbackDuration = 1f;

    // ── Runtime state ──────────────────────────────────────────────
    public LetterEntry CurrentQuestion { get; private set; }
    public bool WaitingForCollect { get; private set; } = false;
    public bool IsActive { get; private set; } = false;

    private AudioSource audioSource;
    private List<int> usedIndices = new List<int>();
    private bool questionAnswered = false;
    private Coroutine soundCoroutine = null;

    // Track which index is the current question so we compare by index, not reference
    private int currentQuestionIndex = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    // ── Public API ─────────────────────────────────────────────────

    public void StartLevel2()
    {
        IsActive = true;
        questionAnswered = false;
        currentQuestionIndex = -1;
        usedIndices.Clear();

        LoadNextQuestion();
    }

    public void OnCollected(GameObject collectedPrefabRef, bool isCorrect)
    {
        if (!IsActive || CurrentQuestion == null)
            return;
        if (questionAnswered)
            return;

        if (isCorrect)
        {
            questionAnswered = true;
            WaitingForCollect = false;

            if (soundCoroutine != null)
            {
                StopCoroutine(soundCoroutine);
                soundCoroutine = null;
            }

            GameManager.Instance?.AddScore(correctScoreValue);
            ProgressSlider.instance?.addProgress(5);
            ShowFeedback("Correct!", true);

            Level2CollectibleSpawner.Instance?.ResetSpawn();

            Invoke(nameof(LoadNextQuestion), feedbackDuration + 0.3f);
        }
        else
        {
            GameManager.Instance?.AddScore(-wrongScorePenalty);
            ShowFeedback("Wrong!", false);

            Level2CollectibleSpawner.Instance?.ResetSpawn();

            // Load a NEW random question, not the same one, after wrong answer
            if (soundCoroutine == null)
            {
                Invoke(nameof(LoadNewRandomQuestion), feedbackDuration + 0.3f);
            }
        }
    }

    /// <summary>
    /// Called when a collectible scrolls off screen without being collected.
    /// If it was the last one in the batch, loads a new question.
    /// </summary>
    public void OnCollectibleMissed(GameObject missed)
    {
        if (!IsActive)
            return;

        var remaining = FindObjectsByType<Level2CollectibleItem>(FindObjectsSortMode.None);

        int count = 0;
        foreach (var item in remaining)
            if (item.gameObject != missed)
                count++;

        if (count == 0)
        {
            Level2CollectibleSpawner.Instance?.ResetSpawn();
            if (soundCoroutine != null)
            {
                StopCoroutine(soundCoroutine);
                soundCoroutine = null;
            }
            questionAnswered = false;
            CancelInvoke();
            Invoke(nameof(LoadNewRandomQuestion), 0.5f);
        }
    }

    public GameObject GetCorrectPrefab()
    {
        return CurrentQuestion?.prefab;
    }

    /// <summary>
    /// Returns N wrong letter entries by INDEX comparison, not prefab reference.
    /// This prevents the bug where two entries with similar references slip through.
    /// </summary>
    public List<LetterEntry> GetWrongLetters(int count)
    {
        var wrong = new List<LetterEntry>();
        var pool = new List<LetterEntry>();

        for (int i = 0; i < letters.Length; i++)
        {
            // Skip the current question by index, not by prefab reference
            if (i == currentQuestionIndex)
                continue;

            // Skip null entries
            if (letters[i].prefab == null)
                continue;

            pool.Add(letters[i]);
        }

        // Shuffle pool
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        int take = Mathf.Min(count, pool.Count);

        for (int i = 0; i < take; i++)
        {
            wrong.Add(pool[i]);
        }

        // Warn if we could not fill the requested count
        if (wrong.Count < count)
        {
            Debug.LogWarning(
                $"[Level2Manager] Only {wrong.Count} wrong letters available, needed {count}. Add more entries to the Letters array."
            );
        }

        return wrong;
    }

    public void StopLevel2()
    {
        IsActive = false;

        if (soundCoroutine != null)
        {
            StopCoroutine(soundCoroutine);
            soundCoroutine = null;
        }

        CancelInvoke();
    }

    // ── Private ────────────────────────────────────────────────────

    /// <summary>
    /// Picks a different random question after a wrong answer.
    /// </summary>
    void LoadNewRandomQuestion()
    {
        if (letters == null || letters.Length == 0)
            return;

        questionAnswered = false;
        WaitingForCollect = false;

        // Pick any index different from the current one
        int idx;
        int safety = 0;

        do
        {
            idx = Random.Range(0, letters.Length);
            safety++;
        } while (idx == currentQuestionIndex && letters.Length > 1 && safety < 100);

        currentQuestionIndex = idx;
        CurrentQuestion = letters[idx];

        if (CurrentQuestion.prefab == null)
        {
            Debug.LogError($"[Level2Manager] Letter entry [{idx}] has no prefab assigned!");
            return;
        }

        Debug.Log(
            $"[Level2Manager] New random question after wrong: entry[{idx}] prefab={CurrentQuestion.prefab.name}"
        );

        if (soundCoroutine != null)
        {
            StopCoroutine(soundCoroutine);
            soundCoroutine = null;
        }

        soundCoroutine = StartCoroutine(PlayQuestionSoundRepeat(initialDelay));
    }

    void LoadNextQuestion()
    {
        if (letters == null || letters.Length == 0)
        {
            Debug.LogError("[Level2Manager] No letters assigned in Inspector!");
            return;
        }

        questionAnswered = false;
        WaitingForCollect = false;

        if (usedIndices.Count >= letters.Length)
        {
            usedIndices.Clear();
        }

        // Pick unused index
        int idx;
        int safety = 0;

        do
        {
            idx = Random.Range(0, letters.Length);
            safety++;
        } while (usedIndices.Contains(idx) && safety < 100);

        usedIndices.Add(idx);
        currentQuestionIndex = idx;
        CurrentQuestion = letters[idx];

        // Guard — make sure the picked entry is valid
        if (CurrentQuestion.prefab == null)
        {
            Debug.LogError($"[Level2Manager] Letter entry [{idx}] has no prefab assigned!");
            return;
        }

        if (CurrentQuestion.sound == null)
        {
            Debug.LogWarning($"[Level2Manager] Letter entry [{idx}] has no sound assigned!");
        }

        Debug.Log($"[Level2Manager] Question: entry[{idx}] prefab={CurrentQuestion.prefab.name}");

        if (soundCoroutine != null)
        {
            StopCoroutine(soundCoroutine);
            soundCoroutine = null;
        }

        soundCoroutine = StartCoroutine(PlayQuestionSoundRepeat(initialDelay));
    }

    IEnumerator PlayQuestionSoundRepeat(float startDelay)
    {
        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < soundRepeatCount; i++)
        {
            if (CurrentQuestion?.sound != null)
            {
                audioSource.PlayOneShot(CurrentQuestion.sound);
            }

            float clipLen = CurrentQuestion?.sound != null ? CurrentQuestion.sound.length : 0.5f;
            yield return new WaitForSeconds(clipLen + repeatInterval);
        }

        soundCoroutine = null;
        WaitingForCollect = true;

        Level2CollectibleSpawner.Instance?.TriggerSpawn();
    }

    void ReplayQuestionSound()
    {
        if (soundCoroutine != null)
            return;

        soundCoroutine = StartCoroutine(ReplaySoundOnly());
    }

    IEnumerator ReplaySoundOnly()
    {
        // Destroy leftover collectibles first
        foreach (var item in FindObjectsByType<Level2CollectibleItem>(FindObjectsSortMode.None))
        {
            Destroy(item.gameObject);
        }

        for (int i = 0; i < soundRepeatCount; i++)
        {
            if (CurrentQuestion?.sound != null)
            {
                audioSource.PlayOneShot(CurrentQuestion.sound);
            }

            float clipLen = CurrentQuestion?.sound != null ? CurrentQuestion.sound.length : 0.5f;
            yield return new WaitForSeconds(clipLen + repeatInterval);
        }

        soundCoroutine = null;
        WaitingForCollect = true;

        Level2CollectibleSpawner.Instance?.TriggerSpawn();
    }

    void ShowFeedback(string message, bool correct)
    {
        if (feedbackText == null)
            return;

        feedbackText.text = message;
        feedbackText.color = correct ? Color.green : Color.red;
        feedbackText.gameObject.SetActive(true);

        Invoke(nameof(HideFeedback), feedbackDuration);
    }

    void HideFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
    }

#if UNITY_EDITOR

    [ContextMenu("Auto Build Letters From Level2 Folders")]
    private void AutoBuildLettersFromLevel2Folders()
    {
        string prefabFolderKeyword = "/prefabs/collectible/level2/";
        string soundFolderKeyword = "/prefabs/collectible/level2/audio/";

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

        List<GameObject> prefabList = prefabGuids
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path =>
            {
                string fixedPath = path.Replace("\\", "/").ToLower();
                return fixedPath.Contains(prefabFolderKeyword)
                    && !fixedPath.Contains(soundFolderKeyword);
            })
            .OrderBy(path => System.IO.Path.GetFileNameWithoutExtension(path))
            .Select(path => AssetDatabase.LoadAssetAtPath<GameObject>(path))
            .Where(prefab => prefab != null)
            .ToList();

        string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" });

        List<AudioClip> soundList = audioGuids
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path =>
            {
                string fixedPath = path.Replace("\\", "/").ToLower();
                return fixedPath.Contains(soundFolderKeyword);
            })
            .OrderBy(path => System.IO.Path.GetFileNameWithoutExtension(path))
            .Select(path => AssetDatabase.LoadAssetAtPath<AudioClip>(path))
            .Where(clip => clip != null)
            .ToList();

        if (prefabList.Count == 0)
        {
            Debug.LogError(
                "[Level2Manager] No prefabs found inside 'Assets/Prefabs/Collectible/Level2'."
            );
            return;
        }

        if (soundList.Count == 0)
        {
            Debug.LogError(
                "[Level2Manager] No sounds found inside 'Assets/Prefabs/Collectible/Level2/Audio'."
            );
            return;
        }

        int entryCount = Mathf.Min(prefabList.Count, soundList.Count);

        if (prefabList.Count != soundList.Count)
        {
            Debug.LogWarning(
                $"[Level2Manager] Prefab count and sound count are different. "
                    + $"Prefabs: {prefabList.Count}, Sounds: {soundList.Count}. "
                    + $"Only {entryCount} entries will be created."
            );
        }

        letters = new LetterEntry[entryCount];

        for (int i = 0; i < entryCount; i++)
        {
            letters[i] = new LetterEntry { prefab = prefabList[i], sound = soundList[i] };

            Debug.Log(
                $"[Level2Manager] Element {i}: Prefab = {prefabList[i].name}, Sound = {soundList[i].name}"
            );
        }

        EditorUtility.SetDirty(this);

        Debug.Log($"[Level2Manager] Auto build complete. Created {entryCount} letter entries.");
    }

#endif
}

[System.Serializable]
public class LetterEntry
{
    [Tooltip("Your existing collectible prefab for this letter")]
    public GameObject prefab;

    [Tooltip("The spoken sound for this letter")]
    public AudioClip sound;
}
