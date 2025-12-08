using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodePrimitive : MonoBehaviour
{
    private Color MyColor = new Color(0.1f, 0.1f, 0.2f, 1.0f);
    public Vector3 Pivot;

    // Cached initial local transform values (used for shader matrix calculation)
    // These are cached on Start so that modifying world transform doesn't affect the calculation
    private Vector3 cachedLocalPosition;
    private Quaternion cachedLocalRotation;
    private Vector3 cachedLocalScale;
    private bool isInitialized = false;

    // Use this for initialization @
    void Start()
    {
        CacheLocalTransform();
    }

    void Update()
    {
    }

    /// <summary>
    /// Cache the initial local transform values so they're not affected by world transform changes
    /// </summary>
    private void CacheLocalTransform()
    {
        if (!isInitialized)
        {
            cachedLocalPosition = transform.localPosition;
            cachedLocalRotation = transform.localRotation;
            cachedLocalScale = transform.localScale;
            isInitialized = true;
        }
    }

    public void LoadShaderMatrix(ref Matrix4x4 nodeMatrix)
    {
        // Ensure we have cached values (in case this is called before Start)
        CacheLocalTransform();

        Matrix4x4 p = Matrix4x4.TRS(Pivot, Quaternion.identity, Vector3.one);
        Matrix4x4 invp = Matrix4x4.TRS(-Pivot, Quaternion.identity, Vector3.one);
        // Use cached local transform values instead of current transform
        // This prevents the world transform update from affecting the calculation
        Matrix4x4 trs = Matrix4x4.TRS(cachedLocalPosition, cachedLocalRotation, cachedLocalScale);
        Matrix4x4 m = nodeMatrix * p * trs * invp;


        // Update the actual Unity transform to match the shader matrix
        // This makes the outline/gizmo position match the rendered position
        ApplyMatrixToTransform(m);

        // Set matrix on ALL materials, not just the first one
        Renderer renderer = GetComponent<Renderer>();
        Material[] mats = renderer.materials;
        foreach (Material mat in mats)
        {
            mat.SetMatrix("MyXformMat", m);
        }
        // GetComponent<Renderer>().material.SetColor("MyColor", MyColor);

    }

    /// <summary>
    /// Extracts position, rotation, and scale from a Matrix4x4 and applies to this transform.
    /// This syncs the Unity transform with the shader matrix so outlines match rendered geometry.
    /// </summary>
    private void ApplyMatrixToTransform(Matrix4x4 m)
    {
        // Extract position from the matrix (last column)
        Vector3 position = m.GetColumn(3);
        
        // Extract scale from the matrix (length of each axis column)
        Vector3 scale = new Vector3(
            m.GetColumn(0).magnitude,
            m.GetColumn(1).magnitude,
            m.GetColumn(2).magnitude
        );
        
        // Extract rotation from the matrix (normalize the columns to remove scale)
        Matrix4x4 rotationMatrix = m;
        rotationMatrix.SetColumn(0, m.GetColumn(0).normalized);
        rotationMatrix.SetColumn(1, m.GetColumn(1).normalized);
        rotationMatrix.SetColumn(2, m.GetColumn(2).normalized);
        rotationMatrix.SetColumn(3, new Vector4(0, 0, 0, 1));
        Quaternion rotation = Quaternion.LookRotation(rotationMatrix.GetColumn(2), rotationMatrix.GetColumn(1));
        
        // Apply to transform (world space, not local)
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;
    }
}
