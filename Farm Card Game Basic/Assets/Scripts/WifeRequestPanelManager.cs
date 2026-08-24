using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WifeRequestPanelManager : MonoBehaviour
{
    [Header("Requested Item")]
    [SerializeField] private WifeRequestedItemData[] requestableItems;
    [SerializeField] private Image requestedItemImage;
    [SerializeField] private TMP_Text requestedItemPriceText;

    [Header("Wife Happiness")]
    [SerializeField] [Range(0, 100)] private int startingHappiness = 100;
    [SerializeField] [Range(1, 100)] private int happinessLossPerMissedTurn = 5;
    [SerializeField] private Image wifeMoodImage;
    [SerializeField] private Sprite[] wifeMoodSprites = new Sprite[5];
    [SerializeField] private TMP_Text happinessValueText;

    [Header("Statistics")]
    [SerializeField] private TMP_Text giftCountText;

    private WifeRequestedItemData currentRequestedItem;
    private int currentHappiness;
    private bool boughtItemThisTurn;
    private int giftCount;
    
    private bool isGameOver;

    public bool IsGameOver => isGameOver;

    private void Start()
    {
        ResetState();
    }

    public void OnBuyButtonClicked()
    {
        TryBuyCurrentItem();
    }

    public bool TryBuyCurrentItem()
    {
        if (isGameOver)
        {
            return false;
        }

        if (currentRequestedItem == null)
        {
            SelectRandomRequestedItem();
            return false;
        }

        GoldManager goldManager = GoldManager.Instance;
        if (goldManager == null)
        {
            Debug.LogWarning("WifeRequestPanelManager: GoldManager not found.");
            return false;
        }

        if (!goldManager.TrySpendGold(currentRequestedItem.Price))
        {
            return false;
        }

        boughtItemThisTurn = true;
        currentHappiness = Mathf.Min(100, currentHappiness + 30);
        UpdateHappinessUI();
        SelectRandomRequestedItem();
        return true;
    }

    // Give current requested item directly (does not consume gold)
    public bool GiveCurrentItem()
    {
        if (isGameOver)
        {
            return false;
        }

        if (currentRequestedItem == null)
        {
            SelectRandomRequestedItem();
            return false;
        }

        // Mark as fulfilled this turn and increase happiness by 50 (clamped to 100)
        boughtItemThisTurn = true;
        currentHappiness = Mathf.Min(100, currentHappiness + 50);
        giftCount++;
        UpdateHappinessUI();
        UpdateGiftCountUI();
        SelectRandomRequestedItem();
        return true;
    }

    public void HandleEndTurn()
    {
        if (isGameOver)
        {
            return;
        }

        if (!boughtItemThisTurn)
        {
            currentHappiness = Mathf.Max(0, currentHappiness - happinessLossPerMissedTurn);
            UpdateHappinessUI();

            if (currentHappiness <= 0)
            {
                TriggerGameOver();
            }
        }

        boughtItemThisTurn = false;
    }

    public void ResetState()
    {
        isGameOver = false;
        Time.timeScale = 1f;


        currentHappiness = Mathf.Clamp(startingHappiness, 0, 100);
        boughtItemThisTurn = false;
        giftCount = 0;
        SelectRandomRequestedItem();
        UpdateHappinessUI();
        UpdateGiftCountUI();
    }

    private void TriggerGameOver()
    {
        isGameOver = true;
    }

    private void SelectRandomRequestedItem()
    {
        if (requestableItems == null || requestableItems.Length == 0)
        {
            currentRequestedItem = null;
            UpdateRequestedItemUI();
            return;
        }

        currentRequestedItem = requestableItems[Random.Range(0, requestableItems.Length)];
        UpdateRequestedItemUI();
    }

    private void UpdateRequestedItemUI()
    {
        if (requestedItemImage != null)
        {
            requestedItemImage.sprite = currentRequestedItem != null ? currentRequestedItem.ItemSprite : null;
            requestedItemImage.enabled = currentRequestedItem != null && currentRequestedItem.ItemSprite != null;
        }

        if (requestedItemPriceText != null)
        {
            requestedItemPriceText.text = currentRequestedItem != null ? currentRequestedItem.Price.ToString() : "-";
        }
    }

    private void UpdateHappinessUI()
    {
        if (happinessValueText != null)
        {
            happinessValueText.text = currentHappiness.ToString();
        }

        if (wifeMoodImage != null)
        {
            Sprite moodSprite = GetMoodSpriteByHappiness();
            wifeMoodImage.sprite = moodSprite;
            wifeMoodImage.enabled = moodSprite != null;
        }
    }

    private Sprite GetMoodSpriteByHappiness()
    {
        if (wifeMoodSprites == null || wifeMoodSprites.Length == 0)
        {
            return null;
        }

        int maxIndex = wifeMoodSprites.Length - 1;
        int sadnessTier = Mathf.Clamp((100 - currentHappiness) / 20, 0, maxIndex);
        return wifeMoodSprites[sadnessTier];
    }

    private void UpdateGiftCountUI()
    {
        if (giftCountText != null)
        {
            giftCountText.text = giftCount.ToString();
        }
    }
}
