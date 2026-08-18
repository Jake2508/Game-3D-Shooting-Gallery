using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Run Length")]
    [SerializeField] private int totalTargets = 40; // Total Targets before run ends

    // Target Statistics
    public int totalScore { get; private set; }
    public int targetsHit { get; private set; }
    public int targetsMissed { get; private set; }
    public int targetsResolved => targetsHit + targetsMissed; // computed, not settable
    public int TotalTargets => totalTargets;

    public static event Action<int> OnScoreChanged;
    public static event Action<int, int> OnProgressChanged; // Total Targets Complete

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetQuota(int quota) => totalTargets = quota;

    public void RegisterHit(int points)
    {
        // Early exit if GameOver is true
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        totalScore += points;
        targetsHit++;
        OnScoreChanged?.Invoke(totalScore);
        CheckTargetQuota();
    }

    public void RegisterMiss()
    {
        // Early exit if GameOver is true
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        targetsMissed++;
        CheckTargetQuota();
    }

    private void CheckTargetQuota()
    {
        OnProgressChanged?.Invoke(targetsResolved, totalTargets);
        if (targetsResolved >= totalTargets && GameManager.Instance != null)
            GameManager.Instance.TriggerWin();
    }
}