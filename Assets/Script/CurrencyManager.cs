using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    private const string BalanceKey = "currentBalance";
    private const string InitializedKey = "currencyInitialized";
    private const int InitialBalance = 5000;

    // Level index is 1-based in gameplay/UI.
    private readonly int[] levelRewards =
    {
        15000, // 1
        30000, // 2
        20000, 20000, 20000, // 3-5
        10000, 10000, 10000, 10000, 10000, // 6-10
        5000, 5000, 5000, 5000, 5000, // 11-15
        3000, 3000, 3000, 3000, 3000 // 16-20
    };

    public int CurrentBalance { get; private set; }
    public event Action<int> OnBalanceChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureManagerInScene()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject go = new GameObject("CurrencyManager");
        go.AddComponent<CurrencyManager>();
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
        LoadOrInitialize();
    }

    private void LoadOrInitialize()
    {
        bool initialized = PlayerPrefs.GetInt(InitializedKey, 0) == 1;
        if (!initialized)
        {
            CurrentBalance = InitialBalance;
            PlayerPrefs.SetInt(BalanceKey, CurrentBalance);
            PlayerPrefs.SetInt(InitializedKey, 1);
            PlayerPrefs.Save();
        }
        else
        {
            CurrentBalance = PlayerPrefs.GetInt(BalanceKey, InitialBalance);
        }

        NotifyBalanceChanged();
    }

    public int GetRewardByLevel(int levelIndex)
    {
        if (levelIndex < 1 || levelIndex > levelRewards.Length)
        {
            return 0;
        }

        return levelRewards[levelIndex - 1];
    }

    public int AddReward(int levelIndex)
    {
        int reward = GetRewardByLevel(levelIndex);
        if (reward <= 0)
        {
            return 0;
        }

        CurrentBalance += reward;
        SaveBalance();
        NotifyBalanceChanged();
        return reward;
    }

    public bool SpendBalance(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (CurrentBalance < amount)
        {
            return false;
        }

        CurrentBalance -= amount;
        SaveBalance();
        NotifyBalanceChanged();
        return true;
    }

    private void SaveBalance()
    {
        PlayerPrefs.SetInt(BalanceKey, CurrentBalance);
        PlayerPrefs.Save();
    }

    private void NotifyBalanceChanged()
    {
        OnBalanceChanged?.Invoke(CurrentBalance);
    }
}
