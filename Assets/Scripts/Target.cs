using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Pop Animation")]
    [SerializeField] private float riseHeight = 0.5f;
    [SerializeField] private float riseDuration = 0.25f;
    [SerializeField] private float sinkDuration = 0.2f;

    [Header("Idle Sway")]
    [SerializeField] private float swayAmount = 0.15f;
    [SerializeField] private float swaySpeed = 2f;

    [Header("Scoring")]
    [SerializeField] private int basePoints = 100;
    [SerializeField] private int minPoints = 10;

    [Header("Feedback (placeholder)")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip missClip;

    private Vector3 hiddenLocalPos;
    private Vector3 visibleLocalPos = Vector3.zero; // slot origin, since Target is parented directly to its slot
    private Color originalColor;
    private float spawnTime;
    private float visibleTime;
    private bool isActive;
    private Coroutine lifecycleRoutine;

    private void Awake()
    {
        if (targetRenderer != null)
            originalColor = targetRenderer.material.color;
    }

    // Called by the Spawner once this pooled target is parented to a slot
    public void Spawn(float visibleDuration)
    {
        visibleTime = visibleDuration;
        hiddenLocalPos = new Vector3(0f, -riseHeight, 0f);
        transform.localPosition = hiddenLocalPos;

        gameObject.SetActive(true);
        isActive = true;
        spawnTime = Time.time;

        if (lifecycleRoutine != null) StopCoroutine(lifecycleRoutine);
        lifecycleRoutine = StartCoroutine(LifecycleSequence());
    }

    private IEnumerator LifecycleSequence()
    {
        yield return StartCoroutine(MoveOverTime(hiddenLocalPos, visibleLocalPos, riseDuration));

        float elapsed = 0f;
        while (elapsed < visibleTime && isActive)
        {
            elapsed += Time.deltaTime;
            ApplySway();
            yield return null;
        }

        if (isActive)
            yield return StartCoroutine(HandleMissed());
    }

    private void ApplySway()
    {
        float offset = (Mathf.PerlinNoise(Time.time * swaySpeed, 0f) - 0.5f) * 2f * swayAmount;
        transform.localPosition = visibleLocalPos + new Vector3(offset, 0f, 0f);
    }

    public void HandleHit()
    {
        if (!isActive) return;
        isActive = false;
        if (lifecycleRoutine != null) StopCoroutine(lifecycleRoutine);

        float timeAlive = Time.time - spawnTime;
        float speedRatio = Mathf.Clamp01(timeAlive / visibleTime);
        int points = Mathf.Max(minPoints, Mathf.RoundToInt(Mathf.Lerp(basePoints, minPoints, speedRatio)));

        Debug.Log($"Target hit — time: {timeAlive:F2}s, points: {points}");

        if (audioSource != null && hitClip != null)
            audioSource.PlayOneShot(hitClip);

        if (ScoreManager.Instance != null) ScoreManager.Instance.RegisterHit(points);
        if (GameManager.Instance != null) GameManager.Instance.OnTargetHit();

        gameObject.SetActive(false); // placeholder — swap for a knockback/spin animation later
    }

    private IEnumerator HandleMissed()
    {
        isActive = false;
        Debug.Log("Target missed / timed out.");

        if (audioSource != null && missClip != null)
            audioSource.PlayOneShot(missClip);

        if (ScoreManager.Instance != null) ScoreManager.Instance.RegisterMiss();
        if (GameManager.Instance != null) GameManager.Instance.OnTargetMissed();

        yield return StartCoroutine(FlashRed());
        yield return StartCoroutine(MoveOverTime(transform.localPosition, hiddenLocalPos, sinkDuration));

        gameObject.SetActive(false);
    }

    private IEnumerator FlashRed()
    {
        if (targetRenderer == null) yield break;
        targetRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        targetRenderer.material.color = originalColor;
    }

    private IEnumerator MoveOverTime(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        transform.localPosition = to;
    }
}