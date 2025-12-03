using System.Collections.Generic;   // List<>
using UnityEngine;

public class BallPlacement : MonoBehaviour
{
    public Transform[] balls = new Transform[15];

    // triangle setting
    public Vector3 apexPos = new Vector3(0f, 2.147f, -2f);  // hardcoded apex pos
    public float diameter = 0.3f;
    //public float gap      = 0.01f;
    
    
    void Start()
    {
        RackBalls();
    }

    private void RackBalls() 
    {
        Debug.Log("[BallPlacement] RackBalls called, balls.Length = " + balls.Length);
        List<Vector3> slots = ComputeBallPos();
        RandomizeBallPos(slots);

    }


    private List<Vector3> ComputeBallPos()
    {
        Debug.Log("[BallPlacement] ComputeBallPos called");
        // math the 15 positions in a triangle
        List<Vector3> slots = new List<Vector3>(15);

        Vector3 right = Vector3.right;
        Vector3 forward = Vector3.back; // apex faces the user

        float rowDepth = Mathf.Sqrt(3f / 4f) * diameter; // gaps between rows in Z

        // 1, 2, 3, 4, 5 balls 
        for (int row = 1; row < 6; row++)
        {
            // row == # balls in the row

            // z pos
            Vector3 rowOffset = forward * (row * rowDepth);

            // center the row around the apex x
            float rowWidth = row * diameter;
            float startX = apexPos.x - rowWidth * 0.5f;

            for (int i = 0; i < row; i++)
            {
                float x = startX + i * diameter;
                float y = apexPos.y;
                float z = apexPos.z + rowOffset.z;

                slots.Add(new Vector3(x, y, z));
            }
        }
        Debug.Log("[BallPlacement] slots.Count = " + slots.Count);
        return slots;
    }

    // https://www.geeksforgeeks.org/dsa/shuffle-a-given-array-using-fisher-yates-shuffle-algorithm/
    private void RandomizeBallPos(List<Vector3> slots)
    {
        Debug.Log("[BallPlacement] RandomizeBallPos called");

        int n = balls.Length;
        Transform[] shuffled = new Transform[15];   // create copy of the balls to shuffle
        balls.CopyTo(shuffled, 0);

        for (int i = n - 1; i > 0; i--)
        {
            // pick random index between 0 and i+1
            int j = Random.Range(0, i + 1);         
            // swap s[i] with s[j] (random index)
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        // assign to positions
        for (int i = 0; i < 15; i++)
        {
            shuffled[i].position = slots[i];
            shuffled[i].rotation = Quaternion.identity; // no rotation
            Debug.Log($"[BallPlacement] Ball {shuffled[i].name} -> {slots[i]}");
        }
        
    }

}
