using UnityEngine;


public class npcMovement : MonoBehaviour
{
    [Header("NPC Respawning")]
    public string obstacleTag = "Damage";
    public Transform[] checkPoints;

    public npcProfile profile;
    public float targetY = 0f;
    private Rigidbody2D rb;
    private bool holding;
    private float nextDecisionTime;
    private float direction;

    private bool isActive = false; //NPC is not active

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }
   
   public void activateNPC()
   {
        isActive = true;
        nextDecisionTime = Time.time;

        Debug.Log("NPC activated by transition!");
   }

   public void deactivateNPC()
   {
        isActive = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
   }

    void Update()
    {
        if (!isActive || profile == null) return;

        if (Time.time > nextDecisionTime)
        {
            holding = Decide();
            nextDecisionTime = Time.time + profile.decisionInterval;
        }

        if (holding)
        {
            direction = 1f; //up
        }
        else
        {
            direction = -1f; //down
        }

        rb.linearVelocity = new Vector2(profile.speed, profile.speed * direction);
    }

    public bool Decide()
    {
        if (profile == null || rb == null) return false;
        
        Vector2 start = rb.position;

        float step = profile.speed * profile.stepTime;
        float castRadius = profile.radius + profile.safetyMargin;
        int depth = Mathf.Clamp(profile.depth,1,10);

        int bestMask = 0;
        float bestSurvived = -1;
        float bestScore = float.NegativeInfinity;

        int combos = 1 << depth;

        for (int mask = 0; mask < combos; mask++)
        {
            Vector2 pos = start;
            int survived = 0;
            float yError = 0f;
            int switches = 0;
            bool prev = holding;
 
            for (int i = 0; i < depth; i++)
            {
                bool up = ((mask >> i) & 1) == 1;

                Vector2 delta;

                if (up)
                {
                    delta = new Vector2(step, step);
                }
                else
                {
                    delta = new Vector2(step, -step);
                }
 
                RaycastHit2D hit = Physics2D.CircleCast(pos, castRadius, delta.normalized, delta.magnitude, profile.obstacleMask);

                if (hit.collider != null)
                {
                    Debug.Log("NPC detected obstacle: " + hit.collider.name);
                    break; 
                }
               
 
                pos += delta;
                survived++;

                yError += Mathf.Abs(pos.y - targetY);

                if (up != prev) switches++;
                prev = up;
            }
 
            float score = survived * 1000f - yError * profile.yWeight - switches * profile.switchPenalty;
 
            if (score > bestScore && survived == bestSurvived || survived > bestSurvived)
            {
                bestSurvived = survived;
                bestScore = score;
                bestMask = mask;
            }
        }

            // Occasionally choose a random path instead of the best path
            if (Random.value < profile.recklessness)
            {
                int randomMask = Random.Range(0, combos);

                return (randomMask & 1) == 1;
            }
        return (bestMask & 1) == 1;
    }
 
    void OnDrawGizmosSelected()
    {
        if (profile == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, profile.radius + profile.safetyMargin);
        Gizmos.DrawLine(new Vector3(-100, targetY), new Vector3(100, targetY));
    }

    private void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag(obstacleTag))
    {
        RespawnAtNearestCheckpoint();
    }
}

private void RespawnAtNearestCheckpoint()
{
    if (checkPoints == null || checkPoints.Length == 0)
    {
        Debug.LogWarning("NPC respawn failed: No checkpoints assigned!");
        return;
    }

    Transform nearestCheckpoint = null;
    float nearestDistance = float.PositiveInfinity;

    foreach (Transform checkPoint in checkPoints)
    {
        if (checkPoint == null)
            continue;

        float distance = Vector2.Distance(
            transform.position,
            checkPoint.position
        );

        if (distance < nearestDistance)
        {
            nearestDistance = distance;
            nearestCheckpoint = checkPoint;
        }
    }

    if (nearestCheckpoint == null)
        return;

    // Stop the NPC and move it to the nearest checkpoint
    rb.linearVelocity = Vector2.zero;
    rb.position = nearestCheckpoint.position;

    Debug.Log("NPC respawned at: " + nearestCheckpoint.name);
}
    
}
