using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitScriptableObject data;
    private float currentHealth;

    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private bool isMoving = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = data.maxHealth;

        // Set the Rigidbody2D to Kinematic if it’s not already set.
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void Update()
{
    if (isMoving)
    {
        Vector2 currentPos = rb.position;
        Vector2 direction = (targetPosition - currentPos).normalized;
        float distance = Vector2.Distance(currentPos, targetPosition);

        Debug.Log($"Moving towards: {targetPosition}, Distance: {distance}");

        // Move the unit towards the target position
        if (distance > 0.1f)
        {
            // Using velocity for movement (can still be influenced by collisions)
            rb.linearVelocity = direction * data.movementSpeed;
        }
        else
        {
            // Stop movement when the target is reached
            isMoving = false;
            rb.linearVelocity = Vector2.zero; // Stop the velocity
            rb.position = targetPosition; // Set final position
            Debug.Log("Arrived at target position");
        }
    }
}

    public void MoveTo(Vector2 targetPos)
    {
        Debug.Log($"MoveTo called with target: {targetPos}");
        
        targetPosition = targetPos;
        isMoving = true;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
