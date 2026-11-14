
using UnityEngine;

public class BoostFruit : MonoBehaviour
{
    [Header("Q Panel")]
    public GameObject boostPrompt;

    [Header("Panel Position")]
    public Vector2 panelOffset = new Vector2(0f, 100f);

    private bool boostCollected = false;

    private Transform player;
    private RectTransform promptRect;
    private RectTransform canvasRect;
    private Canvas canvas;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (boostPrompt != null)
        {
            boostPrompt.SetActive(false);

            promptRect = boostPrompt.GetComponent<RectTransform>();
            canvas = boostPrompt.GetComponentInParent<Canvas>();

            if (canvas != null)
            {
                canvasRect = canvas.GetComponent<RectTransform>();
            }
        }
    }

    private void Update()
    {
        if (boostCollected && player != null)
        {
            FollowPlayer();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !boostCollected)
        {
            boostCollected = true;

            // Store the player's position.
            player = other.transform;

            // Hide the fruit.
            gameObject.SetActive(false);

            // Show the Q panel.
            if (boostPrompt != null)
            {
                boostPrompt.SetActive(true);
            }

            Debug.Log("Boost Fruit Collected! Press Q to use it.");
        }
    }

    private void FollowPlayer()
    {
        if (mainCamera == null ||
            canvas == null ||
            canvasRect == null ||
            promptRect == null)
        {
            return;
        }

        // Convert the player's world position into screen position.
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(
            player.position
        );

        // Convert screen position into Canvas position.
        Vector2 canvasPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvas.worldCamera,
            out canvasPosition
        );

        // Put the Q panel above the player.
        promptRect.anchoredPosition = canvasPosition + panelOffset;
    }
}
