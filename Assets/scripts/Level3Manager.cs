using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

/// <summary>
/// Level 3 — same logic as Level 2 but:
/// - Uses WORD prefabs instead of letter prefabs
/// - Always spawns exactly 3 choices (1 correct + 2 wrong)
/// - Has its own separate word entry list
/// </summary>
public class Level3Manager : MonoBehaviour
{
    public static Level3Manager Instance { get; private set; }

    [Header("Word Prefabs & Sounds")]
    [Tooltip("Each entry = one word prefab + its spoken audio clip")]
    public WordEntry[] words;

    [Header("Question Settings")]
    public int soundRepeatCount = 3;
    public float repeatInterval = 1.2f;
    public float initialDelay = 0.5f;

    [Header("Cluster Size (fixed at 3 for Level 3)")]
    [HideInInspector]
    public int wrongWordCount = 2; // always 2 wrong + 1 correct = 3 total

    [Header("Score")]
    public float correctScoreValue = 20f;
    public float wrongScorePenalty = 8f;

    [Header("UI (optional)")]
    public TextMeshProUGUI feedbackText;
    public float feedbackDuration = 1f;

    // ── Runtime state ──────────────────────────────────────────────
    public WordEntry CurrentQuestion { get; private set; }
    public bool WaitingForCollect { get; private set; } = false;
    public bool IsActive { get; private set; } = false;

    private AudioSource audioSource;
    private List<int> usedIndices = new List<int>();
    private bool questionAnswered = false;
    private int currentQuestionIndex = -1;
    private Coroutine soundCoroutine = null;

    // ── Unity lifecycle ────────────────────────────────────────────

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

    public void StartLevel3()
    {
        IsActive = true;
        questionAnswered = false;
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

            Level3CollectibleSpawner.Instance?.ResetSpawn();

            Invoke(nameof(LoadNextQuestion), feedbackDuration + 0.3f);
        }
        else
        {
            GameManager.Instance?.AddScore(-wrongScorePenalty);
            ShowFeedback("Wrong!", false);

            Level3CollectibleSpawner.Instance?.ResetSpawn();

            if (soundCoroutine == null)
            {
                Invoke(nameof(ReplayQuestionSound), feedbackDuration + 0.3f);
            }
        }
    }

    public GameObject GetCorrectPrefab()
    {
        return CurrentQuestion?.prefab;
    }

    public List<WordEntry> GetWrongWords(int count)
    {
        var wrong = new List<WordEntry>();
        var pool = new List<WordEntry>();

        if (words == null || words.Length == 0)
        {
            return wrong;
        }

        for (int i = 0; i < words.Length; i++)
        {
            // Compare by index instead of prefab reference.
            // This is safer if two entries somehow use similar prefab references.
            if (i == currentQuestionIndex)
                continue;

            if (words[i].prefab == null)
                continue;

            pool.Add(words[i]);
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

        if (wrong.Count < count)
        {
            Debug.LogWarning(
                $"[Level3Manager] Only {wrong.Count} wrong words available, needed {count}. Add more entries to the Words array."
            );
        }

        return wrong;
    }

    /// <summary>
    /// Called when a collectible scrolls off screen without being collected.
    /// If it was the last one in the batch, loads a new question.
    /// </summary>
    public void OnCollectibleMissed(GameObject missed)
    {
        if (!IsActive)
            return;

        // Check if any collectibles remain in the scene
        var remaining = FindObjectsByType<Level3CollectibleItem>(FindObjectsSortMode.None);

        // Count items that are not the one being destroyed
        int count = 0;

        foreach (var item in remaining)
        {
            if (item.gameObject != missed)
            {
                count++;
            }
        }

        if (count == 0)
        {
            Level3CollectibleSpawner.Instance?.ResetSpawn();

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

    public void StopLevel3()
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

    void LoadNewRandomQuestion()
    {
        if (words == null || words.Length == 0)
        {
            Debug.LogError("[Level3Manager] No words assigned in Inspector!");
            return;
        }

        questionAnswered = false;
        WaitingForCollect = false;

        int idx;
        int safety = 0;

        do
        {
            idx = Random.Range(0, words.Length);
            safety++;
        }
        while (idx == currentQuestionIndex && words.Length > 1 && safety < 100);

        currentQuestionIndex = idx;
        CurrentQuestion = words[idx];

        if (CurrentQuestion.prefab == null)
        {
            Debug.LogError($"[Level3Manager] Word entry [{idx}] has no prefab assigned!");
            return;
        }

        if (CurrentQuestion.sound == null)
        {
            Debug.LogWarning($"[Level3Manager] Word entry [{idx}] has no sound assigned!");
        }

        Debug.Log($"[Level3Manager] New random question: entry[{idx}] prefab={CurrentQuestion.prefab.name}");

        if (soundCoroutine != null)
        {
            StopCoroutine(soundCoroutine);
            soundCoroutine = null;
        }

        soundCoroutine = StartCoroutine(PlayQuestionSoundRepeat(initialDelay));
    }

    void LoadNextQuestion()
    {
        if (words == null || words.Length == 0)
        {
            Debug.LogError("[Level3Manager] No words assigned in Inspector!");
            return;
        }

        questionAnswered = false;
        WaitingForCollect = false;

        if (usedIndices.Count >= words.Length)
        {
            usedIndices.Clear();
        }

        int idx;
        int safety = 0;

        do
        {
            idx = Random.Range(0, words.Length);
            safety++;
        }
        while (usedIndices.Contains(idx) && safety < 100);

        usedIndices.Add(idx);
        currentQuestionIndex = idx;
        CurrentQuestion = words[idx];

        if (CurrentQuestion.prefab == null)
        {
            Debug.LogError($"[Level3Manager] Word entry [{idx}] has no prefab assigned!");
            return;
        }

        if (CurrentQuestion.sound == null)
        {
            Debug.LogWarning($"[Level3Manager] Word entry [{idx}] has no sound assigned!");
        }

        Debug.Log($"[Level3Manager] Question: entry[{idx}] prefab={CurrentQuestion.prefab.name}");

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

        Level3CollectibleSpawner.Instance?.TriggerSpawn();
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
        foreach (var item in FindObjectsByType<Level3CollectibleItem>(FindObjectsSortMode.None))
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

        Level3CollectibleSpawner.Instance?.TriggerSpawn();
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

    [ContextMenu("Auto Build Words From Level3 Folders")]
    private void AutoBuildWordsFromLevel3Folders()
    {
        string prefabFolderPath = "Assets/Prefabs/Collectible/Level3";
        string soundFolderPath = "Assets/Prefabs/Collectible/Level3/Part2/Sounds";

        if (!AssetDatabase.IsValidFolder(prefabFolderPath))
        {
            Debug.LogError($"[Level3Manager] Prefab folder does not exist: {prefabFolderPath}");
            return;
        }

        if (!AssetDatabase.IsValidFolder(soundFolderPath))
        {
            Debug.LogError($"[Level3Manager] Sound folder does not exist: {soundFolderPath}");
            return;
        }

        // Find all prefabs inside Assets/Prefabs/Collectible/Level3
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolderPath });

        List<GameObject> prefabList = prefabGuids
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path =>
            {
                string fixedPath = path.Replace("\\", "/").ToLower();

                // Include prefabs inside Level3,
                // but exclude anything inside Part2/Sounds or other subfolders you do not want.
                return fixedPath.Contains("assets/prefabs/collectible/level3/")
                       && !fixedPath.Contains("assets/prefabs/collectible/level3/part2/sounds/");
            })
            .OrderBy(path => System.IO.Path.GetFileNameWithoutExtension(path))
            .Select(path => AssetDatabase.LoadAssetAtPath<GameObject>(path))
            .Where(prefab => prefab != null)
            .ToList();

        // Find all audio clips inside Assets/Prefabs/Collectible/Level3/Part2/Sounds
        string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { soundFolderPath });

        List<AudioClip> soundList = audioGuids
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .OrderBy(path => System.IO.Path.GetFileNameWithoutExtension(path))
            .Select(path => AssetDatabase.LoadAssetAtPath<AudioClip>(path))
            .Where(clip => clip != null)
            .ToList();

        if (prefabList.Count == 0)
        {
            Debug.LogError("[Level3Manager] No prefabs found inside 'Assets/Prefabs/Collectible/Level3'.");
            return;
        }

        if (soundList.Count == 0)
        {
            Debug.LogError("[Level3Manager] No sounds found inside 'Assets/Prefabs/Collectible/Level3/Part2/Sounds'.");
            return;
        }

        int entryCount = Mathf.Min(prefabList.Count, soundList.Count);

        if (prefabList.Count != soundList.Count)
        {
            Debug.LogWarning(
                $"[Level3Manager] Prefab count and sound count are different. " +
                $"Prefabs: {prefabList.Count}, Sounds: {soundList.Count}. " +
                $"Only {entryCount} entries will be created."
            );
        }

        words = new WordEntry[entryCount];

        for (int i = 0; i < entryCount; i++)
        {
            words[i] = new WordEntry
            {
                prefab = prefabList[i],
                sound = soundList[i]
            };

            Debug.Log(
                $"[Level3Manager] Element {i}: Prefab = {prefabList[i].name}, Sound = {soundList[i].name}"
            );
        }

        EditorUtility.SetDirty(this);

        Debug.Log($"[Level3Manager] Auto build complete. Created {entryCount} word entries.");
    }

#endif
}

/// <summary>
/// One word entry — your word prefab + its spoken sound.
/// </summary>
[System.Serializable]
public class WordEntry
{
    [Tooltip("Your word collectible prefab")]
    public GameObject prefab;

    [Tooltip("The spoken audio clip for this word")]
    public AudioClip sound;
}