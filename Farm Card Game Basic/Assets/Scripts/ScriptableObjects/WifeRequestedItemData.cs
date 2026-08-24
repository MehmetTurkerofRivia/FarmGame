using UnityEngine;

[CreateAssetMenu(fileName = "WifeRequestedItem", menuName = "Farm Card Game/Wife Requested Item")]
public class WifeRequestedItemData : ScriptableObject
{
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private int price = 5;

    public Sprite ItemSprite => itemSprite;
    public int Price => Mathf.Max(0, price);
}
