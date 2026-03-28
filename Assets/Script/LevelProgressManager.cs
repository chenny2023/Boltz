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

        bool isGoal = gameState.IsGoaLstate(gameState.getState());

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
