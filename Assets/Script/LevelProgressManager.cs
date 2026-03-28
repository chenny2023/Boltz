using UnityEngine;

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager Instance { get; private set; }

    private const string LevelKey = "currentLevelIndex";
    private const int MaxLevel = 20;

    public UI gameUi;
    public int CurrentLevelIndex { get; private set; }

    private GameState gameState;
    private bool completionLocked;
    private string lastCompletedStateHash = string.Empty;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureManagerInScene()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject go = new GameObject("LevelProgressManager");
        go.AddComponent<LevelProgressManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentLevelIndex = Mathf.Clamp(PlayerPrefs.GetInt(LevelKey, 1), 1, MaxLevel);
    }

    private void Start()
    {
        gameState = FindObjectOfType<GameState>();
        gameUi = FindObjectOfType<UI>();
        PushLevelToUi();
    }

    public void OnPlayerMoveResolved()
    {
        if (gameState == null)
        {
            gameState = FindObjectOfType<GameState>();
            if (gameState == null)
            {
                return;
            }
        }

        var currentState = gameState.getState();
        bool isGoal = gameState.IsGoaLstate(currentState);

        if (!isGoal)
        {
            completionLocked = false;
            return;
        }

        if (completionLocked)
        {
            return;
        }

        completionLocked = true;

        string solvedStateHash = gameState.SerializeState(currentState);
        if (lastCompletedStateHash == solvedStateHash)
        {
            return;
        }

        int reward = CurrencyManager.Instance != null
            ? CurrencyManager.Instance.AddReward(CurrentLevelIndex)
            : 0;

        if (gameUi == null)
        {
            gameUi = FindObjectOfType<UI>();
        }

        if (gameUi != null)
        {
            gameUi.SetText($"Level {CurrentLevelIndex} selesai! +{reward:N0} IDR");
            gameUi.ShowToast($"Hadiah level: +{reward:N0} IDR");
        }

        lastCompletedStateHash = solvedStateHash;

        CurrentLevelIndex = Mathf.Min(CurrentLevelIndex + 1, MaxLevel);
        PlayerPrefs.SetInt(LevelKey, CurrentLevelIndex);
        PlayerPrefs.Save();
        PushLevelToUi();
    }

    private void PushLevelToUi()
    {
        if (gameUi == null)
        {
            gameUi = FindObjectOfType<UI>();
        }

        if (gameUi != null)
        {
            gameUi.SetLevel(CurrentLevelIndex);
        }
    }
}
