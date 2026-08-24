using UnityEngine;

public interface ICardDropProcessor
{
    bool TryHandleDrop(Vector3 worldPosition);
}
