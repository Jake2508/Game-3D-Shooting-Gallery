using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverMessage;
    [SerializeField] private TMP_Text gameOverScoreText;

    [Header("Pause UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Instability")]
    [SerializeField] private float instabilityPerMiss = 0.2f; // 5 misses to game over
    [SerializeField] private float instabilityPerHit = 0.05f; // recovery on hit
    
    public float Instability { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }


    public static event Action<bool> OnPauseStateChanged;
    public static event Action<float> OnInstabilityChanged;
    public static event Action<bool> OnGameOver; // true won - false fails


    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable() => Character.OnPausePressed += TogglePause;
    private void OnDisable() => Character.OnPausePressed -= TogglePause;
    private void TogglePause() => SetPaused(!IsPaused);
    public void TriggerWin() => HandleGameOver(true); // Called by score manager when all targets are hit

    private void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if(pausePanel != null)
            pausePanel.SetActive(paused);

        ApplyCursorState();
        OnPauseStateChanged?.Invoke(paused);
    }

    /// <summary>
    /// Show or Hide Mouse cursor on pause events
    /// </summary>
    private void ApplyCursorState()
    {
        bool uiVisible = IsPaused || IsGameOver;

        Cursor.lockState = uiVisible ? CursorLockMode.None : CursorLockMode.Confined;
        Cursor.visible = uiVisible;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            ApplyCursorState(); // resync OS cursor the instant focus returns, instead of waiting on the next pause toggle
    }

    public void OnTargetMissed() => AdjustInstability(instabilityPerMiss);
    public void OnTargetHit() => AdjustInstability(-instabilityPerHit);

    private void AdjustInstability(float delta)
    {
        if (IsGameOver) 
            return; // Disable on gameover

        Instability = Mathf.Clamp01(Instability + delta);
        OnInstabilityChanged?.Invoke(Instability);
        
        Debug.Log($"Instability: {Instability}");
        if (Instability >= 1f) HandleGameOver(false);
    }

    private void HandleGameOver(bool won)
    {
        if (IsGameOver)
            return;

        IsGameOver = true;
        Time.timeScale = 0f;
        ApplyCursorState();

        if(gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverMessage != null)
            gameOverMessage.text = won ? "Booth Survived!" : "Booth Collapsed!";

        if (gameOverScoreText != null && ScoreManager.Instance != null)
            gameOverScoreText.text = $"Score: {ScoreManager.Instance.totalScore}";

        OnGameOver?.Invoke(won);
    }

    public void RestartRun()
    {
        Time.timeScale = 1f; // must reset before reload or the new scene loads frozen
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
