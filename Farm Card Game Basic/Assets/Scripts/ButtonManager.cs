using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] private CardHandManager cardHandManager;
    [SerializeField] private WifeRequestPanelManager wifeRequestPanelManager;
    [SerializeField] private Silo silo;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnEndTurnButtonClicked();
        }
    }

    public void OnReloadButtonClicked()
    {
        if (cardHandManager == null)
        {
            Debug.LogWarning("ButtonManager: CardHandManager is not assigned.");
            return;
        }

        if (!cardHandManager.TryReloadHand())
        {
            Debug.LogWarning("ButtonManager: Not enough gold to reload hand.");
        }
    }

    public void OnEndTurnButtonClicked()
    {
        if (cardHandManager == null)
        {
            Debug.LogWarning("ButtonManager: CardHandManager is not assigned.");
            return;
        }

        if (cardHandManager.IsGameEnded)
        {
            return;
        }

        cardHandManager.EndTurn();
    }

    public void OnBuyRequestedItemButtonClicked()
    {
        if (wifeRequestPanelManager == null)
        {
            Debug.LogWarning("ButtonManager: WifeRequestPanelManager is not assigned.");
            return;
        }

        if (!wifeRequestPanelManager.TryBuyCurrentItem())
        {
            Debug.LogWarning("ButtonManager: Not enough gold to buy requested item.");
        }
    }

    public void OnGiveRequestedItemButtonClicked()
    {
        if (wifeRequestPanelManager == null)
        {
            Debug.LogWarning("ButtonManager: WifeRequestPanelManager is not assigned.");
            return;
        }

        if (!wifeRequestPanelManager.GiveCurrentItem())
        {
            Debug.LogWarning("ButtonManager: Could not give requested item (maybe game over).");
        }
    }

    public void OnUpgradeSiloButtonClicked()
    {
        if (silo == null)
        {
            Debug.LogWarning("ButtonManager: Silo is not assigned.");
            return;
        }

        if (!silo.TryUpgradeCapacity())
        {
            Debug.LogWarning("ButtonManager: Silo upgrade failed (not enough gold).");
        }
    }
}
