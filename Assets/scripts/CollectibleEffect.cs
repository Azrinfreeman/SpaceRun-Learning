using UnityEngine;

/// <summary>
/// Attach this to an empty GameObject prefab named "CollectEffect".
/// It creates and plays a burst particle effect entirely in code —
/// no particle prefab setup needed.
///
/// SETUP:
/// 1. Create an empty GameObject → name it "CollectEffect"
/// 2. Attach this script to it
/// 3. Save it as a prefab in your Prefabs folder
/// 4. Drag that prefab into CollectibleItem → Collect Effect Prefab field
/// </summary>
public class CollectibleEffect : MonoBehaviour
{
    [Header("Burst Shape")]
    [Tooltip("How many particles burst on collect")]
    public int burstCount = 20;
    public float spreadRadius = 0.8f;

    [Header("Particle Life")]
    public float lifetime = 0.6f;
    public float startSpeed = 4f;
    public float startSize = 0.15f;

    [Header("Colors")]
    [Tooltip("Pick colors that match your collectible sprite")]
    public Color colorA = new Color(1f, 0.85f, 0f, 1f); // gold
    public Color colorB = new Color(1f, 1f, 1f, 1f); // white flash
    public Color colorC = new Color(1f, 0.5f, 0f, 1f); // orange

    // ── private ────────────────────────────────────────────────────
    private ParticleSystem ps;

    void Awake()
    {
        BuildParticleSystem();
    }

    void Start()
    {
        ps.Play();
        // Auto-destroy after particles finish
        Destroy(gameObject, lifetime + 0.5f);
    }

    // ── Build the particle system entirely in code ─────────────────

    void BuildParticleSystem()
    {
        ps = gameObject.AddComponent<ParticleSystem>();

        // ── Main module ──────────────────────────────────────────
        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = lifetime;
        main.startSpeed = startSpeed;
        main.startSize = startSize;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.3f; // slight droop so they arc

        // Randomise color between colorA / colorB / colorC
        main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);

        // ── Emission — one single burst, then done ───────────────
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0; // no continuous emission
        emission.SetBursts(
            new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)burstCount) }
        );

        // ── Shape — sphere so particles fly in all directions ────
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = spreadRadius;

        // ── Size over lifetime — shrink to nothing ───────────────
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // ── Color over lifetime — fade out at end ────────────────
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(colorA, 0f),
                new GradientColorKey(colorB, 0.3f),
                new GradientColorKey(colorC, 1f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.6f),
                new GradientAlphaKey(0f, 1f), // fade out
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        // ── Renderer — use built-in particle material ────────────
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.sortingLayerName = "Default"; // match your player layer
        renderer.sortingOrder = 10; // render on top of everything
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }
}
