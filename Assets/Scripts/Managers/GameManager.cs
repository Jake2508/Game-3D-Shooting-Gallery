using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Pause UI")]
    [SerializeField] private GameObject pausePanel;

    public bool IsPaused { get; private set; }

    public static event Action<bool> OnPauseStateChanged;


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
        Cursor.lockState = IsPaused ? CursorLockMode.None : CursorLockMode.Confined;
        Cursor.visible = IsPaused;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            ApplyCursorState(); // resync OS cursor the instant focus returns, instead of waiting on the next pause toggle
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
