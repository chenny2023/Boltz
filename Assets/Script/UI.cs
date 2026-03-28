using System.Collections;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI toastText;
    public GameObject solveButton;
    public GameObject showPathButton;

    private Coroutine toastCoroutine;

    void Start()
    {
        if (Text == null)
        {
            Text = this.GetComponentInChildren<TextMeshProUGUI>();
        }
        SetShowButton(false);
        SetSolveButton(true);
        if (Text != null)
        {
            Text.text = "Klik bolt untuk edit nut";
        }
        UpdateBalanceText(CurrencyManager.Instance != null ? CurrencyManager.Instance.CurrentBalance : 0);
        SetLevel(PlayerPrefs.GetInt("currentLevelIndex", 1));

        if (toastText != null)
        {
            toastText.gameObject.SetActive(false);
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnBalanceChanged += UpdateBalanceText;
        }
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnBalanceChanged -= UpdateBalanceText;
        }
    }

    public void SetText(string text)
    {
        if (Text != null)
        {
            Text.text = text;
        }
    }

    public void SetSolveButton(bool active)
    {
        if (solveButton != null)
        {
            solveButton.SetActive(active);
        }
    }

    public void SetShowButton(bool active)
    {
        if (showPathButton != null)
        {
            showPathButton.SetActive(active);
        }
    }

    public void SetLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }
    }

    public void ShowToast(string message, float duration = 2f)
    {
        if (toastText == null)
        {
            return;
        }

        if (toastCoroutine != null)
        {
            StopCoroutine(toastCoroutine);
        }

        toastCoroutine = StartCoroutine(ToastRoutine(message, duration));
    }

    private IEnumerator ToastRoutine(string message, float duration)
    {
        toastText.text = message;
        toastText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        toastText.gameObject.SetActive(false);
    }

    private void UpdateBalanceText(int balance)
    {
        if (balanceText != null)
        {
            balanceText.text = $"Total Saldo: {balance:N0} IDR";
        }
    }
}
