
using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    [Header("Jelly Dash")]
    [SerializeField] private Animator jellyDashAnimator;
    [SerializeField] private SpriteRenderer jellyDashSpriteRenderer;

    [Header("Space Waves")]
    [SerializeField] private GameObject spaceWaveVisual;
    [SerializeField] private Animator spaceWaveAnimator;

    [Header("Space Waves Trail")]
    [SerializeField] private TrailRenderer spaceWaveTrail;
    [SerializeField] private SpriteRenderer trailSpriteRenderer;

    [Header("Trail Settings")]
    [SerializeField] private float trailWidth = 0.5f;
    [SerializeField] private float trailLifetime = 100000f;

    private bool isInSpaceWave;

    private void Awake()
    {
        // Make sure gameplay starts unpaused.
        Time.timeScale = 1f;

        EnterJellyDash();
    }

    public void EnterSpaceWave()
    {
        isInSpaceWave = true;

        // Hide Jelly Dash.
        if (jellyDashAnimator != null)
            jellyDashAnimator.enabled = false;

        if (jellyDashSpriteRenderer != null)
            jellyDashSpriteRenderer.enabled = false;

        // Show Space Waves.
        if (spaceWaveVisual != null)
            spaceWaveVisual.SetActive(true);

        if (spaceWaveAnimator != null)
        {
            spaceWaveAnimator.enabled = true;
            spaceWaveAnimator.Rebind();
            spaceWaveAnimator.Update(0f);
        }

        StartFreshTrail();
    }

    public void EnterJellyDash()
    {
        isInSpaceWave = false;

        // Stop and clear Space Waves trail.
        if (spaceWaveTrail != null)
        {
            spaceWaveTrail.emitting = false;
            spaceWaveTrail.Clear();
        }

        // Hide Space Waves.
        if (spaceWaveVisual != null)
            spaceWaveVisual.SetActive(false);

        // Restore Jelly Dash.
        if (jellyDashAnimator != null)
            jellyDashAnimator.enabled = true;

        if (jellyDashSpriteRenderer != null)
            jellyDashSpriteRenderer.enabled = true;
    }

    public void OnPlayerRespawn()
    {
        if (!isInSpaceWave)
            return;

        StartFreshTrail();
    }

    private void StartFreshTrail()
    {
        if (spaceWaveTrail == null)
            return;

        // Remove the previous trail completely.
        spaceWaveTrail.emitting = false;
        spaceWaveTrail.Clear();

        // Keep the trail long and consistent in thickness.
        spaceWaveTrail.time = trailLifetime;
        spaceWaveTrail.widthMultiplier = trailWidth;

        // Hide the original PNG Sprite Renderer.
        if (trailSpriteRenderer != null)
            trailSpriteRenderer.enabled = false;

        // Start recording the player's movement.
        spaceWaveTrail.emitting = true;
    }
}


