using UnityEngine;

public class sfxScript : MonoBehaviour
{
    [Header("SFX")]
    [Tooltip("Assign your  audio clip here (WAV or MP3)")]
    public AudioClip SFX;

    [Range(0f, 1f)]
    [Tooltip("Volume of the jump sound")]
    public float SFXVolume = 0.8f;

    [Tooltip("Pitch variation for a more natural feel — 0 = none")]
    [Range(0f, 0.3f)]
    public float pitchVariation = 0.1f;
    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public float GetClipLength()
    {
        var src = GetComponent<AudioSource>();
        if (src != null && src.clip != null)
            return src.clip.length;
        return 0f;
    }

    public void PlaySFX()
    {
        if (SFX == null)
            return;

        GameObject tempGO = new GameObject("TempAudio");
        AudioSource tempSource = tempGO.AddComponent<AudioSource>();

        tempSource.clip = SFX;
        tempSource.volume = SFXVolume;
        tempSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        tempSource.spatialBlend = 0f;
        tempSource.Play();

        Destroy(tempGO, SFX.length + 0.1f);
    }
}
