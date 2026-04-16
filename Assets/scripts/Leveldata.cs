using UnityEngine;

/// <summary>
/// Defines a single question/answer set used by the level system.
/// Create these as ScriptableObject assets in your project.
///
/// HOW TO CREATE:
/// Right-click in Project window → Create → StarExplorer → Question
/// </summary>
[CreateAssetMenu(fileName = "Question", menuName = "StarExplorer/Question")]
public class QuestionData : ScriptableObject
{
    [Tooltip("The question audio that plays before answer choices appear")]
    public AudioClip questionSound;

    [Tooltip("The correct answer audio clip")]
    public AudioClip correctAnswerSound;

    [Tooltip("Wrong answer audio clips (used in Level 2 and 3)")]
    public AudioClip[] wrongAnswerSounds;

    [Tooltip("Optional sprite to show on the correct collectible")]
    public Sprite correctSprite;

    [Tooltip("Optional sprites for wrong collectibles")]
    public Sprite[] wrongSprites;

    [Tooltip("Score value for collecting the correct answer")]
    public float correctScoreValue = 10f;

    [Tooltip("Score penalty for collecting a wrong answer")]
    public float wrongScorePenalty = 5f;
}
