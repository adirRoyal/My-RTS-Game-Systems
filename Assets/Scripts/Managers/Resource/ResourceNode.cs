using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    public ResourceType resourceType; // Type of resource this node provides
    public int amount = 100;          // Current amount of resource left in this node

    /// <summary>
    /// Attempts to gather a specified amount from this resource node.
    /// Returns the actual amount taken (could be less if not enough left).
    /// </summary>
    public int Gather(int gatherAmount)
    {
        // Calculate how much we can actually take (don't go below zero)
        int taken = Mathf.Min(gatherAmount, amount);
        amount -= taken;

        // If depleted, destroy this resource node game object
        if (amount <= 0)
        {
            // Optional: add destruction effects/animation here
            Destroy(gameObject);
        }
        return taken;
    }
}
