using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField]
    private bool debugTimer = false;

    [SerializeField]
    private float debugMinutes = 0.1f;

    [Header("UI")]
    public TMP_Text whiteTimerText;
    public TMP_Text blackTimerText;

    private BoardManager boardManager;
    private GameUIManager uiManager;

    private float whiteTime;
    private float blackTime;

    private bool gameEnded = false;
    public bool isPaused = false;

    private void Start()
    {
        boardManager =
            FindFirstObjectByType<BoardManager>();

        uiManager =
            FindFirstObjectByType<GameUIManager>();

        float minutes =
            debugTimer
            ? debugMinutes
            : GameSettings.matchTimeMinutes;

        float startTime =
            minutes * 60f;

        whiteTime = startTime;
        blackTime = startTime;

        UpdateTimerUI();

        Debug.Log(
            "MATCH TIME = "
            + minutes
            + " MINUTES");
    }

    private void Update()
    {
        if (boardManager == null)
            return;

        if (gameEnded)
            return;

        if (uiManager != null && uiManager.IsGameFinished)
        {
            gameEnded = true;
            return;
        }

        if (isPaused)
            return;

        if (boardManager.isWhiteTurn)
        {
            whiteTime -= Time.deltaTime;
        }
        else
        {
            blackTime -= Time.deltaTime;
        }

        whiteTime =
            Mathf.Max(
                0,
                whiteTime);

        blackTime =
            Mathf.Max(
                0,
                blackTime);

        UpdateTimerUI();

        CheckTimeOut();
    }

    private void CheckTimeOut()
    {
        if (whiteTime <= 0)
        {
            gameEnded = true;

            Debug.Log(
                "WHITE TIME OUT");

            GameUIManager ui =
                FindFirstObjectByType<GameUIManager>();

            if (ui != null)
            {
                ui.ShowPlayer2Win("WAKTU HABIS!", "Sisi putih kehabisan waktu");
            }

            return;
        }

        if (blackTime <= 0)
        {
            gameEnded = true;

            Debug.Log(
                "BLACK TIME OUT");

            GameUIManager ui =
                FindFirstObjectByType<GameUIManager>();

            if (ui != null)
            {
                ui.ShowPlayer1Win("WAKTU HABIS!", "Sisi hitam kehabisan waktu");
            }

            return;
        }
    }

    private void UpdateTimerUI()
    {
        if (whiteTimerText != null)
        {
            whiteTimerText.text =
                FormatTime(
                    whiteTime);
        }

        if (blackTimerText != null)
        {
            blackTimerText.text =
                FormatTime(
                    blackTime);
        }
    }

    private string FormatTime(
        float time)
    {
        int minutes =
            Mathf.FloorToInt(
                time / 60);

        int seconds =
            Mathf.FloorToInt(
                time % 60);

        return string.Format(
            "{0:00}:{1:00}",
            minutes,
            seconds);
    }
}