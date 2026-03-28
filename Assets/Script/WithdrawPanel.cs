using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WithdrawPanel : MonoBehaviour
{
    [Header("Panels")]
    public GameObject withdrawRoot;
    public GameObject withdrawalStatusPanel;
    public GameObject activationPopup;
    public GameObject vipPopup;

    [Header("Texts")]
    public TextMeshProUGUI totalBalanceText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI queueText;
    public TextMeshProUGUI toastText;

    [Header("Buttons")]
    public Button option200kButton;
    public Button option500kButton;
    public Button option1mButton;
    public Button payActivationButton;
    public Button upgradeVipButton;

    [Header("Progress")]
    public Slider queueProgressBar;

    private UI gameUi;

    private bool isActivated;
    private bool isVIP;

    private const int Option200k = 200000;

    private void Start()
    {
        gameUi = FindObjectOfType<UI>();

        if (option200kButton != null) option200kButton.onClick.AddListener(OnClick200kOption);
        if (option500kButton != null) option500kButton.onClick.AddListener(() => ShowToast("Minimal saldo belum cukup untuk opsi ini."));
        if (option1mButton != null) option1mButton.onClick.AddListener(() => ShowToast("Lanjut main untuk membuka opsi ini."));

        if (payActivationButton != null) payActivationButton.onClick.AddListener(OnPayActivation);
        if (upgradeVipButton != null) upgradeVipButton.onClick.AddListener(OnUpgradeVip);

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnBalanceChanged += UpdateBalance;
            UpdateBalance(CurrencyManager.Instance.CurrentBalance);
        }

        SetPanelState(withdraw: true, status: false, activation: false, vip: false);
        if (queueProgressBar != null)
        {
            queueProgressBar.value = 0f;
        }
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnBalanceChanged -= UpdateBalance;
        }
    }

    private void UpdateBalance(int balance)
    {
        if (totalBalanceText != null)
        {
            totalBalanceText.text = $"Total Saldo\n{balance:N0} IDR";
        }
    }

    private void OnClick200kOption()
    {
        if (CurrencyManager.Instance == null)
        {
            ShowToast("Sistem saldo belum siap.");
            return;
        }

        if (CurrencyManager.Instance.CurrentBalance < Option200k)
        {
            ShowToast("Saldo tidak cukup, butuh 200.000 IDR untuk tarik dana.");
            return;
        }

        SetPanelState(withdraw: false, status: true, activation: true, vip: false);
        if (statusText != null)
        {
            statusText.text = "Status penarikan: Menunggu aktivasi";
        }
    }

    private void OnPayActivation()
    {
        InvokePayment(10000, () =>
        {
            isActivated = true;
            if (activationPopup != null)
            {
                activationPopup.SetActive(false);
            }

            if (statusText != null)
            {
                statusText.text = "Status penarikan: Dalam antrean";
            }

            if (queueText != null)
            {
                queueText.text = "Posisi antrean: 8564/8565";
            }

            StartCoroutine(QueueFlowRoutine());
        });
    }

    private IEnumerator QueueFlowRoutine()
    {
        const float duration = 5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (queueProgressBar != null)
            {
                queueProgressBar.value = Mathf.Clamp01(elapsed / duration);
            }
            yield return null;
        }

        if (!isVIP && vipPopup != null)
        {
            vipPopup.SetActive(true);
        }
    }

    private void OnUpgradeVip()
    {
        InvokePayment(25000, () =>
        {
            isVIP = true;
            if (vipPopup != null)
            {
                vipPopup.SetActive(false);
            }

            if (statusText != null)
            {
                statusText.text = "Status penarikan: VIP diproses";
            }

            if (queueText != null)
            {
                queueText.text = "Dana virtual berhasil dikirim!";
            }

            if (queueProgressBar != null)
            {
                queueProgressBar.value = 1f;
            }

            gameUi?.ShowToast("VIP aktif. Penarikan virtual dipercepat.");
        });
    }

    // Placeholder: connect your payment SDK/gateway here.
    public void InvokePayment(int amount, Action onSuccess)
    {
        Debug.Log($"InvokePayment called with amount: {amount}");
        onSuccess?.Invoke();
    }

    private void ShowToast(string message)
    {
        if (toastText != null)
        {
            StopAllCoroutines();
            StartCoroutine(ToastRoutine(message));
        }

        gameUi?.ShowToast(message);
    }

    private IEnumerator ToastRoutine(string message)
    {
        toastText.text = message;
        toastText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        toastText.gameObject.SetActive(false);
    }

    private void SetPanelState(bool withdraw, bool status, bool activation, bool vip)
    {
        if (withdrawRoot != null) withdrawRoot.SetActive(withdraw);
        if (withdrawalStatusPanel != null) withdrawalStatusPanel.SetActive(status);
        if (activationPopup != null) activationPopup.SetActive(activation);
        if (vipPopup != null) vipPopup.SetActive(vip);
    }
}
