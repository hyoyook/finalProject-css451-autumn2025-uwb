using System.Collections.Generic;
using UnityEngine;

public class BallPlacement : MonoBehaviour
{
    // This array is sorted 1-15 in the Inspector!
    // balls[0] = 1-Ball, balls[7] = 8-Ball, etc.
    public Transform[] balls = new Transform[15];

    public Vector3 apexPos = new Vector3(0f, 2.147f, -2f);
    public float diameter = 0.3f;

    void Start()
    {
        RackBalls();
    }

    private void RackBalls()
    {
        List<Vector3> slots = ComputeBallPos();
        Rack8BallStandard(slots); // Changed from RandomizeBallPos
    }

    private List<Vector3> ComputeBallPos()
    {
        List<Vector3> slots = new List<Vector3>(15);
        Vector3 right = Vector3.right;
        Vector3 forward = Vector3.back; 

        float rowDepth = Mathf.Sqrt(3f / 4f) * diameter; 

        // 1, 2, 3, 4, 5 balls per row
        for (int row = 1; row < 6; row++)
        {
            Vector3 rowOffset = forward * ((row - 1) * rowDepth); // Fixed math slightly (row-1) for apex at 0,0
            
            float rowWidth = (row - 1) * diameter; // Fixed width calc to match standard tight rack
            float startX = apexPos.x - rowWidth * 0.5f;

            for (int i = 0; i < row; i++)
            {
                float x = startX + i * diameter;
                float y = apexPos.y;
                float z = apexPos.z + rowOffset.z;

                slots.Add(new Vector3(x, y, z));
            }
        }
        return slots;
    }

    private void Rack8BallStandard(List<Vector3> slots)
    {
        // 1. Create a temporary array to hold our organized rack
        Transform[] rackedBalls = new Transform[15];

        // 2. Identify our balls by index (Assuming balls[] is sorted 1-15)
        Transform ball1 = balls[0];
        Transform ball8 = balls[7];

        List<Transform> solids = new List<Transform>(); // 2,3,4,5,6,7
        List<Transform> stripes = new List<Transform>(); // 9,10,11,12,13,14,15

        // Sort inputs into lists, skipping 1 and 8
        for (int i = 0; i < balls.Length; i++)
        {
            if (i == 0 || i == 7) continue; // Skip 1 and 8

            if (i < 7) solids.Add(balls[i]);      // Balls 2-7
            else stripes.Add(balls[i]);           // Balls 9-15
        }

        // 3. Shuffle the sub-lists (Solids and Stripes)
        ShuffleList(solids);
        ShuffleList(stripes);

        // --- PLACEMENT RULES ---

        // RULE A: Apex is the 1-Ball
        rackedBalls[0] = ball1;

        // RULE B: Center of the rack (Index 4) is the 8-Ball
        // (Row 3, middle spot is index 4)
        rackedBalls[4] = ball8;

        // RULE C: Bottom Corners (Indices 10 and 14) must be different suits
        // We take the first available from our shuffled lists
        bool flipCoin = Random.value > 0.5f;
        
        if (flipCoin)
        {
            rackedBalls[10] = solids[0];
            rackedBalls[14] = stripes[0];
        }
        else
        {
            rackedBalls[10] = stripes[0];
            rackedBalls[14] = solids[0];
        }

        // Remove the two used corner balls so we don't duplicate them
        solids.RemoveAt(0);
        stripes.RemoveAt(0);

        // 4. Fill the remaining spots
        // Combine leftovers into one pool and shuffle them again
        List<Transform> leftovers = new List<Transform>();
        leftovers.AddRange(solids);
        leftovers.AddRange(stripes);
        ShuffleList(leftovers);

        int leftoverIndex = 0;
        for (int i = 0; i < 15; i++)
        {
            // If we haven't placed a ball here yet (it's null), fill it
            if (rackedBalls[i] == null)
            {
                rackedBalls[i] = leftovers[leftoverIndex];
                leftoverIndex++;
            }
        }

        // 5. Apply positions AND RESET PHYSICS
        // Reset physics so balls doesn't move after being reracked
        for (int i = 0; i < 15; i++)
        {
            if (rackedBalls[i] != null)
            {
                // Move the ball
                rackedBalls[i].position = slots[i];
                rackedBalls[i].rotation = Quaternion.identity;

                // CRITICAL: Stop the ball's physics so it sits still
                Rigidbody rb = rackedBalls[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.Sleep(); // Optional: puts it to sleep until hit
                }
            }
        }
    }

    // Helper function to shuffle a generic list
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)    
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}