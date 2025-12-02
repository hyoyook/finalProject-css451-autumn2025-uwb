using UnityEngine;

public partial class TexturePlacement : MonoBehaviour
{
    // Planar mapping using local XZ -> UV (0..1, 0..1)
    private Vector2[] GeneratePlanarUVs(Vector3[] v)
    {
        Vector2[] uv = new Vector2[v.Length];

        if (v.Length == 0)
            return uv;

        float minX = v[0].x, maxX = v[0].x;
        float minZ = v[0].z, maxZ = v[0].z;

        // find bounds in XZ
        for (int i = 1; i < v.Length; i++)
        {
            if (v[i].x < minX) minX = v[i].x;
            if (v[i].x > maxX) maxX = v[i].x;
            if (v[i].z < minZ) minZ = v[i].z;
            if (v[i].z > maxZ) maxZ = v[i].z;
        }

        float dx = Mathf.Max(0.0001f, maxX - minX);
        float dz = Mathf.Max(0.0001f, maxZ - minZ);

        for (int i = 0; i < v.Length; i++)
        {
            float u = (v[i].x - minX) / dx;
            float w = (v[i].z - minZ) / dz;
            uv[i] = new Vector2(u, w);
        }

        return uv;
    }
}
