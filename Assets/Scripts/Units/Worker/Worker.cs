using UnityEngine;

public class Worker : MonoBehaviour
{
    public float moveSpeed = 3f;
    public int inventory = 0;

    public ResourceNode targetResourceNode;
    public GameObject targetBuilding;

    void Update()
    {
        if (targetResourceNode != null && targetResourceNode.HasResources() && inventory == 0)
        {
            MoveToResource();
        }
        else if (inventory > 0 && targetBuilding != null)
        {
            MoveToBuilding();
        }
    }

    void MoveToResource()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetResourceNode.transform.position, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetResourceNode.transform.position) < 1f)
        {
            targetResourceNode.Harvest(1);
            inventory++;
        }
    }

    void MoveToBuilding()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetBuilding.transform.position, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetBuilding.transform.position) < 1f)
        {
            // Optional: Ressourcen ans Gebäude übergeben
            inventory = 0;
        }
    }

    // 👉 Diese Methoden brauchst du für Methode 3:
    public void SetTargetResourceNode(ResourceNode node)
    {
        targetResourceNode = node;
    }

    public void SetTargetBuilding(GameObject building)
    {
        targetBuilding = building;
    }
}