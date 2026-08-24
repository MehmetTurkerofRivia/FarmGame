using UnityEngine;

public class WorkerWander : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.1f;
    [SerializeField] private Vector2 pauseDurationRange = new Vector2(0.35f, 1.1f);
    [SerializeField] private float targetReachDistance = 0.03f;

    private Vector3 moveAreaCenter;
    private Vector2 moveAreaSize = new Vector2(1f, 0.6f);
    private Vector3 targetPosition;
    private float pauseTimer;
    private bool hasMoveArea;

    public void SetMoveArea(Vector3 center, Vector2 size)
    {
        moveAreaCenter = center;
        moveAreaSize = new Vector2(Mathf.Max(0.2f, size.x), Mathf.Max(0.2f, size.y));
        hasMoveArea = true;
        PickNewTarget(true);
    }

    private void Start()
    {
        if (!hasMoveArea)
        {
            SetMoveArea(transform.position, moveAreaSize);
        }
    }

    private void Update()
    {
        if (!hasMoveArea)
        {
            return;
        }

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) <= targetReachDistance)
        {
            pauseTimer = Random.Range(pauseDurationRange.x, pauseDurationRange.y);
            PickNewTarget(false);
        }
    }

    private void PickNewTarget(bool snapImmediately)
    {
        Vector2 halfArea = moveAreaSize * 0.5f;
        Vector3 nextTarget = moveAreaCenter + new Vector3(
            Random.Range(-halfArea.x, halfArea.x),
            Random.Range(-halfArea.y, halfArea.y),
            transform.position.z - moveAreaCenter.z);

        nextTarget.z = transform.position.z;
        targetPosition = nextTarget;

        if (snapImmediately)
        {
            transform.position = targetPosition;
        }
    }
}
