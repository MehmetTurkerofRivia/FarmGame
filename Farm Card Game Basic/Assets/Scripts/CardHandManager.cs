using UnityEngine;
using TMPro;

public class CardHandManager : MonoBehaviour
{
    public static CardHandManager Instance { get; private set; }

    private const int UpkeepTurnInterval = 5;
    private const int UpkeepBaseExpense = 2;
    private const int UpkeepBaseGrowthTurnInterval = 10;
    private const int UpkeepCostPerWorker = 3;
    private const int WorkerIncomePerTurn = 2;
    private static readonly Color UpkeepCountdownStartColor = Color.white;
    private static readonly Color UpkeepCountdownEndColor = new Color32(128, 0, 32, 255);

    [SerializeField] private CardsMovement fieldCardPrefab;
    [SerializeField] private CardsMovement workerCardPrefab;
    [SerializeField] private CardsMovement bucketCardPrefab;
    [SerializeField] private Transform[] cardSlots = new Transform[4];
    [SerializeField] private int reloadCost = 5;
    [SerializeField] private int handSize = 4;
    [SerializeField] private WifeRequestPanelManager wifeRequestPanelManager;
    [SerializeField] private Silo[] silos;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private Transform backgroundCanvasRoot;
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject[] gameObjectsToHide;

    [Header("Statistics")]
    [SerializeField] private TMP_Text roundCountText;
    [SerializeField] private TMP_Text upkeepCountdownText;
    [SerializeField] private TMP_Text upkeepCostText;
    [SerializeField] private BlobEffect upkeepCostBlobEffect;

    private int remainingCards;
    private int turnsSinceLastUpkeep;
    private int turnCount;
    private int lastKnownWorkerCount = -1;
    private bool isGameEnded;

    public bool IsGameEnded => isGameEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        DealNewHand();
        UpdateUpkeepCountdownUI();
        UpdateUpkeepCostUI();
        lastKnownWorkerCount = FarmPlot.GetTotalWorkerCount();
    }

    private void Update()
    {
        int currentWorkerCount = FarmPlot.GetTotalWorkerCount();

        if (currentWorkerCount == lastKnownWorkerCount)
        {
            return;
        }

        lastKnownWorkerCount = currentWorkerCount;
        UpdateUpkeepCostUI();
    }

    public void NotifyCardUsed()
    {
        remainingCards = Mathf.Max(0, remainingCards - 1);
    }

    public void EndTurn()
    {
        if (isGameEnded)
        {
            return;
        }

        if (wifeRequestPanelManager != null && wifeRequestPanelManager.IsGameOver)
        {
            return;
        }

        turnCount++;
        UpdateRoundCountUI();

        int totalWorkers = FarmPlot.GetTotalWorkerCount();
        FarmPlot.DecreaseWaterForAllPlots();

        int incomeGold = totalWorkers * WorkerIncomePerTurn;
        bool didApplyUpkeepCost = false;
        int upkeepCost = 0;
        
        turnsSinceLastUpkeep++;
        if (turnsSinceLastUpkeep >= UpkeepTurnInterval)
        {
            turnsSinceLastUpkeep = 0;
            didApplyUpkeepCost = true;
        }

        UpdateUpkeepCountdownUI();

        GoldManager goldManager = GoldManager.Instance;

        if (goldManager != null)
        {
            goldManager.AddGold(incomeGold);

            if (didApplyUpkeepCost)
            {
                upkeepCost = GetUpkeepCostForState(totalWorkers, turnCount);
            }

            if (upkeepCost > 0)
            {
                goldManager.ConsumeGold(upkeepCost);
            }

            if (didApplyUpkeepCost)
            {
                upkeepCostBlobEffect?.PlayPulse();
            }

            if (goldManager.CurrentGold <= 0)
            {
                GameOver();
                return;
            }
        }

        UpdateUpkeepCostUI();

        wifeRequestPanelManager?.HandleEndTurn();

        if (wifeRequestPanelManager != null && wifeRequestPanelManager.IsGameOver)
        {
            GameOver();
            return;
        }

        DealNewHand();
    }

    private void GameOver()
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;

        if (loseScreen != null)
            loseScreen.SetActive(true);

        if (backgroundCanvasRoot != null)
            backgroundCanvasRoot.localScale = Vector3.one;
        
        if (mainCanvas != null)
            mainCanvas.SetActive(false);
        
        if (gameObjectsToHide != null)
        {
            foreach (GameObject obj in gameObjectsToHide)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }

    public void RestartGame()
    {
        isGameEnded = false;

        if (loseScreen != null)
            loseScreen.SetActive(false);

        if (backgroundCanvasRoot != null)
            backgroundCanvasRoot.localScale = Vector3.zero;
        
        if (mainCanvas != null)
            mainCanvas.SetActive(true);
        
        if (gameObjectsToHide != null)
        {
            foreach (GameObject obj in gameObjectsToHide)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }
        
        FarmPlot.ResetAllPlots();
        GoldManager.Instance?.ResetGold();
        wifeRequestPanelManager?.ResetState();
        turnsSinceLastUpkeep = 0;
        turnCount = 0;
        
        if (silos != null)
        {
            foreach (Silo silo in silos)
            {
                if (silo != null)
                    silo.ResetStorage();
            }
        }

        UpdateRoundCountUI();
        UpdateUpkeepCountdownUI();
        UpdateUpkeepCostUI();
        lastKnownWorkerCount = FarmPlot.GetTotalWorkerCount();
        DealNewHand();
    }

    public bool TryReloadHand()
    {
        if (isGameEnded)
        {
            return false;
        }

        if (!GoldManager.Instance || !GoldManager.Instance.TrySpendGold(reloadCost))
        {
            return false;
        }
        DealNewHand();
        return true;
    }

    public int GetReloadCost()
    {
        return reloadCost;
    }

    private void DealNewHand()
    {
        ClearSlots();

        CardsMovement[] availablePrefabs = GetAvailableCardPrefabs();

        if (availablePrefabs.Length == 0)
        {
            remainingCards = 0;
            return;
        }

        int slotCount = GetSlotCount();
        remainingCards = slotCount;

        for (int index = 0; index < slotCount; index++)
        {
            CardsMovement selectedPrefab = availablePrefabs[Random.Range(0, availablePrefabs.Length)];

            if (selectedPrefab == null)
            {
                remainingCards--;
                continue;
            }

            Transform slot = GetSlotTransform(index);
            Vector3 spawnPosition = slot != null ? slot.position : transform.position;
            Transform parent = slot != null ? slot : transform;
            CardsMovement spawnedCard = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity, parent);
            spawnedCard.SetStartPosition(spawnPosition);
        }
    }

    private CardsMovement[] GetAvailableCardPrefabs()
    {
        int count = 0;

        if (fieldCardPrefab != null)
        {
            count++;
        }

        if (workerCardPrefab != null)
        {
            count++;
        }

        if (bucketCardPrefab != null)
        {
            count++;
        }

        CardsMovement[] availablePrefabs = new CardsMovement[count];
        int index = 0;

        if (fieldCardPrefab != null)
        {
            availablePrefabs[index++] = fieldCardPrefab;
        }

        if (workerCardPrefab != null)
        {
            availablePrefabs[index++] = workerCardPrefab;
        }

        if (bucketCardPrefab != null)
        {
            availablePrefabs[index++] = bucketCardPrefab;
        }

        return availablePrefabs;
    }

    private void ClearSlots()
    {
        Transform[] slots = cardSlots;

        for (int index = 0; index < slots.Length; index++)
        {
            Transform slot = slots[index];

            if (slot == null)
            {
                continue;
            }

            for (int childIndex = slot.childCount - 1; childIndex >= 0; childIndex--)
            {
                Destroy(slot.GetChild(childIndex).gameObject);
            }
        }

        for (int childIndex = transform.childCount - 1; childIndex >= 0; childIndex--)
        {
            Destroy(transform.GetChild(childIndex).gameObject);
        }
    }

    private int GetSlotCount()
    {
        return Mathf.Max(1, handSize);
    }

    private Transform GetSlotTransform(int index)
    {
        if (cardSlots != null && index < cardSlots.Length)
        {
            return cardSlots[index];
        }

        return null;
    }

    private void UpdateRoundCountUI()
    {
        if (roundCountText != null)
        {
            roundCountText.text = turnCount.ToString();
        }
    }

    private void UpdateUpkeepCountdownUI()
    {
        if (upkeepCountdownText == null)
        {
            return;
        }

        int turnsUntilUpkeep = UpkeepTurnInterval - turnsSinceLastUpkeep;
        if (turnsUntilUpkeep <= 0)
        {
            turnsUntilUpkeep = UpkeepTurnInterval;
        }

        if (turnsUntilUpkeep <= 2)
        {
            upkeepCountdownText.text = $"!{turnsUntilUpkeep}";
        }
        else
        {
            upkeepCountdownText.text = turnsUntilUpkeep.ToString();
        }

        float urgency = Mathf.InverseLerp(UpkeepTurnInterval, 1f, turnsUntilUpkeep);
        upkeepCountdownText.color = Color.Lerp(UpkeepCountdownStartColor, UpkeepCountdownEndColor, urgency);
    }

    private void UpdateUpkeepCostUI()
    {
        if (upkeepCostText == null)
        {
            return;
        }

        int totalWorkers = FarmPlot.GetTotalWorkerCount();
        upkeepCostText.text = $"upkeep cost : {GetUpkeepCostForState(totalWorkers, turnCount)}";
    }

    private int GetUpkeepCostForState(int totalWorkers, int currentTurnCount)
    {
        int baseGrowthSteps = currentTurnCount / UpkeepBaseGrowthTurnInterval;
        int baseUpkeep = UpkeepBaseExpense << baseGrowthSteps;
        return baseUpkeep + (totalWorkers * UpkeepCostPerWorker);
    }
}
