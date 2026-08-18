using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private Transform[] spawnSlots; // empty child Transforms marking each pop-up position

    [Header("Target Pool")]
    [SerializeField] private Target targetPrefab;

    [Header("Levels")]
    [SerializeField] private LevelConfig[] levels;
    [SerializeField] private int currentLevelIndex = 0;

    private Target[] pool;
    private bool[] slotOccupied;
    private int targetsSpawnedThisLevel;
    private LevelConfig ActiveLevel => levels[currentLevelIndex];

    private void Start()
    {
        if (!ValidateSetup())
            return; // bail out rather than throw deeper in with a less obvious error

        BuildPool();

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.SetQuota(ActiveLevel.totalTargets);

        StartCoroutine(RunLevel());
    }

    // Inspector fields are easy to get wrong (empty arrays, off-by-one level index);
    // report the exact problem instead of letting ActiveLevel throw IndexOutOfRange.
    private bool ValidateSetup()
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("Spawner: Levels array is empty - add at least one LevelConfig.", this);
            return false;
        }

        if (currentLevelIndex < 0 || currentLevelIndex >= levels.Length)
        {
            Debug.LogError($"Spawner: Current Level Index is {currentLevelIndex}, but Levels only has {levels.Length} entry/entries (valid: 0-{levels.Length - 1}). Levels are zero-indexed, so \"level 1\" is index 0.", this);
            return false;
        }

        if (spawnSlots == null || spawnSlots.Length == 0)
        {
            Debug.LogError("Spawner: No spawn slots assigned - nothing can pop up.", this);
            return false;
        }

        if (targetPrefab == null)
        {
            Debug.LogError("Spawner: Target Prefab is not assigned.", this);
            return false;
        }

        return true;
    }

    private void BuildPool()
    {
        pool = new Target[spawnSlots.Length];
        slotOccupied = new bool[spawnSlots.Length];

        for (int i = 0; i < spawnSlots.Length; i++)
        {
            Target t = Instantiate(targetPrefab, transform);
            t.gameObject.SetActive(false);
            pool[i] = t;
        }
    }

    private IEnumerator RunLevel()
    {
        while (targetsSpawnedThisLevel < ActiveLevel.totalTargets)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                yield break; // stop spawning once the run has ended (collapse or quota-clear)

            if (CountActive() < ActiveLevel.simultaneousTargets)
                TrySpawnOne();

            float wait = Random.Range(ActiveLevel.spawnIntervalMin, ActiveLevel.spawnIntervalMax);
            yield return new WaitForSeconds(wait);
        }
    }

    private int CountActive()
    {
        int count = 0;
        foreach (bool occupied in slotOccupied)
            if (occupied) count++;
        return count;
    }

    private void TrySpawnOne()
    {
        List<int> freeSlots = new List<int>();
        for (int i = 0; i < slotOccupied.Length; i++)
            if (!slotOccupied[i]) freeSlots.Add(i);

        if (freeSlots.Count == 0) return;

        Target target = GetFreePooledTarget();
        if (target == null) return;

        int slotIndex = freeSlots[Random.Range(0, freeSlots.Count)];
        slotOccupied[slotIndex] = true;
        targetsSpawnedThisLevel++;

        target.transform.SetParent(spawnSlots[slotIndex], worldPositionStays: false);
        target.Spawn(ActiveLevel.targetVisibleTime);
        StartCoroutine(FreeSlotWhenDone(slotIndex, target));
    }

    private Target GetFreePooledTarget()
    {
        foreach (Target t in pool)
            if (!t.gameObject.activeSelf) return t;
        return null;
    }

    private IEnumerator FreeSlotWhenDone(int slotIndex, Target target)
    {
        yield return new WaitUntil(() => !target.gameObject.activeSelf);
        yield return new WaitForSeconds(ActiveLevel.slotCooldown);
        slotOccupied[slotIndex] = false;
    }
}