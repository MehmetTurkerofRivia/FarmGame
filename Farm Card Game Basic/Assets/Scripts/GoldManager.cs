using TMPro;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    [SerializeField] private int startingGold = 15;
    [SerializeField] private TMP_Text goldText;

    private int currentGold;

    public int CurrentGold => currentGold;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentGold = Mathf.Max(0, startingGold);
        UpdateGoldText();
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        currentGold += amount;
        UpdateGoldText();
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0)
            return true;

        if (currentGold < amount)
            return false;

        currentGold -= amount;
        UpdateGoldText();
        return true;
    }

    public void ConsumeGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentGold = Mathf.Max(0, currentGold - amount);
        UpdateGoldText();
    }

    public void ResetGold()
    {
        currentGold = Mathf.Max(0, startingGold);
        UpdateGoldText();
    }

    private void UpdateGoldText()
    {
        if (goldText != null)
        {
            goldText.text = currentGold.ToString();
        }
    }
}
