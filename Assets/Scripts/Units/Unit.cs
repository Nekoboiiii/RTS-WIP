using System.Data;
using Unity.VisualScripting;
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
    }

    private void Update()
    {
        if (isMoving)
        {
            Vector2 currentPos = rb.position;
            Vector2 direction = (targetPosition - currentPos).normalized;
            float distance = Vector2.Distance(currentPos, targetPosition);

            if (distance > 0.1f)
            {
                rb.MovePosition(currentPos + direction * data.movementSpeed * Time.deltaTime);
            }
            else
            {
                isMoving = false;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    public void MoveTo(Vector2 targetPos)
    {
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
