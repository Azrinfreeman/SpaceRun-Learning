using System.Collections;
using UnityEngine;

/// <summary>
/// A persistent singleton that plays the collectible-specific sound
/// AFTER the collectible GameObject is already destroyed.
///
/// Without this, destroying the collectible would cancel any
/// AudioSource on it before the sound finishes playing.
///
/// SETUP:
/// 1. Create an empty GameObject named "CollectibleSoundPlayer"
/// 2. Attach this script to it
/// 3. That's it — CollectibleItem calls CollectibleSoundPlayer.Play() automatically
/// </summary>
public class CollectibleSoundPlayer : MonoBehaviour
{
    public static CollectibleSoundPlayer Instance { get; private set; }

    private AudioSource audioSource;

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
        audioSource.spatialBlend = 0f; // full 2D sound
    }

    /// <summary>
    /// Call this from CollectibleItem before the GameObject is destroyed.
    /// delay = sfx clip length + any extra gap you want between sounds.
    /// </summary>
    public static void Play(AudioClip clip, float delay)
    {
        if (Instance == null || clip == null)
            return;
        Instance.StartCoroutine(Instance.PlayAfterDelay(clip, delay));
    }

    IEnumerator PlayAfterDelay(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.PlayOneShot(clip);
    }
}

/// <summary>
/// NOTE ON sfxScript.GetClipLength():
/// Your sfxScript needs a public method to return the clip length
/// so CollectibleItem knows how long to wait before playing the
/// collectible sound. Add this to your sfxScript if not already there:
///
///     public float GetClipLength()
///     {
///         var src = GetComponent<AudioSource>();
///         if (src != null && src.clip != null) return src.clip.length;
///         return 0f;
///     }
/// ///
/// If you don't want to modify sfxScript, just set soundDelay in
/// CollectibleItem Inspector to a fixed value (e.g. 0.8f) that
/// covers the length of your default SFX.
/// </summary>
