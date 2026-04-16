using UnityEngine;

/// <summary>
/// Attach to an empty GameObject prefab named "ObstacleEffect".
/// Spawns a rocky debris burst — no prefab particle setup needed.
///
/// SETUP:
/// 1. Create empty GameObject → name "ObstacleEffect"
/// 2. Attach this script
/// 3. Save as prefab in Prefabs folder
/// 4. Drag prefab into ObstacleHit.cs → Hit Effect Prefab field
/// </summary>
public class ObstacleEffect : MonoBehaviour
{
    [Header("Burst")]
    public int burstCount = 25;
    public float spreadRadius = 0.5f;
    public float lifetime = 0.8f;
    public float startSpeed = 5f;
    public float startSize = 0.2f;

    [Header("Colors — match your obstacle sprite")]
    public Color colorA = new Color(0.45f, 0.35f, 0.25f, 1f); // dark brown/rock
    public Color colorB = new Color(0.65f, 0.55f, 0.40f, 1f); // mid stone
    public Color colorC = new Color(0.85f, 0.80f, 0.70f, 1f); // light dust

    private ParticleSystem ps;

    void Awake() => BuildParticleSystem();

    void Start()
    {
        ps.Play();
        Destroy(gameObject, lifetime + 0.5f);
    }

    void BuildParticleSystem()
    {
        ps = gameObject.AddComponent<ParticleSystem>();

        // ── Main ─────────────────────────────────────────────────
        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.5f, lifetime);
        main.startSpeed = new ParticleSystem.MinMaxCurve(startSpeed * 0.5f, startSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(startSize, startSize * 2.5f);
        main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 1.2f; // chunks fall with gravity

        // ── Emission — single burst ───────────────────────────────
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(
            new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)burstCount) }
        );

        // ── Shape — hemisphere so debris flies upward/outward ────
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = spreadRadius;

        // ── Rotation over lifetime — tumbling rocks ───────────────
        var rotOverLife = ps.rotationOverLifetime;
        rotOverLife.enabled = true;
        rotOverLife.z = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);

        // ── Size over lifetime — shrink to dust ───────────────────
        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.6f, 0.8f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // ── Color over lifetime — fade to dust ───────────────────
        var colorOverLife = ps.colorOverLifetime;
        colorOverLife.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(colorB, 0f),
                new GradientColorKey(colorC, 0.5f),
                new GradientColorKey(colorC, 1f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0f, 1f),
            }
        );
        colorOverLife.color = new ParticleSystem.MinMaxGradient(grad);

        // ── Renderer ─────────────────────────────────────────────
        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = new Material(Shader.Find("Particles/Standard Unlit"));
        rend.sortingLayerName = "Default"; // match your sprite sorting layer
        rend.sortingOrder = 10;
        rend.renderMode = ParticleSystemRenderMode.Billboard;
    }
}
