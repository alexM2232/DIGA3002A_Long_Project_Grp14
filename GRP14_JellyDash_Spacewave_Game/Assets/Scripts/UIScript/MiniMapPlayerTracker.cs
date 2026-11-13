
using UnityEngine;

public class MiniMapPlayerTracker : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Race Positions")]
    [SerializeField] private Transform raceStartPoint;
    [SerializeField] private Transform raceFinishPoint;

    [Header("Mini Map UI")]
    [SerializeField] private RectTransform trackArea;
    [SerializeField] private RectTransform playerHead;

    private void Update()
    {
        if (player == null ||
            raceStartPoint == null ||
            raceFinishPoint == null ||
            trackArea == null ||
            playerHead == null)
        {
            return;
        }

        Vector2 start = raceStartPoint.position;
        Vector2 finish = raceFinishPoint.position;
        Vector2 playerPosition = player.position;

        Vector2 raceDirection = finish - start;

        if (raceDirection.sqrMagnitude < 0.001f)
            return;

        float progress = Vector2.Dot(
            playerPosition - start,
            raceDirection
        ) / raceDirection.sqrMagnitude;

        progress = Mathf.Clamp01(progress);

        Rect trackRect = trackArea.rect;

        float markerHalfWidth = playerHead.rect.width * 0.5f;

        float leftPosition = trackRect.xMin + markerHalfWidth;
        float rightPosition = trackRect.xMax - markerHalfWidth;

        if (rightPosition < leftPosition)
        {
            leftPosition = trackRect.center.x;
            rightPosition = trackRect.center.x;
        }

        Vector2 newPosition = playerHead.anchoredPosition;

        newPosition.x = Mathf.Lerp(
            leftPosition,
            rightPosition,
            progress
        );

        newPosition.y = 0f;

        playerHead.anchoredPosition = newPosition;
    }
}
