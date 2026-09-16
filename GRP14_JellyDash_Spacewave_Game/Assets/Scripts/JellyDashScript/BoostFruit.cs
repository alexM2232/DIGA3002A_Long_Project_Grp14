using UnityEngine;

public class BoostFruit : MonoBehaviour
{
    [Header("Q Panel")]
    public GameObject boostPrompt;

    [Header("Player")]
    public Transform player;

    [Header("Panel Position")]
    public Vector3 panelOffset = new Vector3(0f, 1.5f, 0f);

    private bool boostCollected = false;

    private Canvas canvas;
    private RectTransform promptRect;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (boostPrompt != null)
        {
            boostPrompt.SetActive(false);

            promptRect = boostPrompt.GetComponent<RectTransform>();

            canvas = boostPrompt.GetComponentInParent<Canvas>();
        }
    }

    private void Update()
    {
        // Only move the Q panel after the fruit has been collected.
        if (boostCollected && boostPrompt != null && player != null)
        {
            FollowPlayer();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only the Player can collect the fruit.
        if (other.CompareTag("Player") && !boostCollected)
        {
            boostCollected = true;

            // Automatically find the player.
            player = other.transform;

            // Make the fruit disappear.
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
        if (mainCamera == null || canvas == null || promptRect == null)
        {
            return;
        }

        // Get the player's world position with an offset above them.
        Vector3 worldPosition = player.position + panelOffset;

        // Convert world position to screen position.
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        // Convert screen position to Canvas position.
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        Vector2 canvasPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvas.worldCamera,
            out canvasPosition
        );

        // Move the Q panel to the player's position.
        promptRect.localPosition = canvasPosition;
    }
}
