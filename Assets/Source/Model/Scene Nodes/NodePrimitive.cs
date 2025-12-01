using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodePrimitive : MonoBehaviour
{
    public Color MyColor = new Color(0.1f, 0.1f, 0.2f, 1.0f);
    public Vector3 Pivot;

    [Tooltip("If true, uses standard Unity transforms instead of custom 451Shader. Allows any shader/material to work.")]
    public bool UseStandardTransform = false;

    // Use this for initialization @
    void Start()
    {
    }

    void Update()
    {
    }


    public void LoadShaderMatrix(ref Matrix4x4 nodeMatrix)
    {
        Matrix4x4 p = Matrix4x4.TRS(Pivot, Quaternion.identity, Vector3.one);
        Matrix4x4 invp = Matrix4x4.TRS(-Pivot, Quaternion.identity, Vector3.one);
        Matrix4x4 trs = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
        Matrix4x4 m = nodeMatrix * p * trs * invp;

        if (UseStandardTransform)
        {
            // Apply transform directly to GameObject - works with ANY shader
            ApplyMatrixToTransform(m);
        }
        else
        {
            // Original 451Shader approach
            GetComponent<Renderer>().material.SetMatrix("MyXformMat", m);
            GetComponent<Renderer>().material.SetColor("MyColor", MyColor);
        }
    }

    /// <summary>
    /// Applies a Matrix4x4 directly to this GameObject's transform.
    /// This allows any standard shader (URP Lit, Standard, etc.) to work with SceneNodes.
    /// </summary>
    private void ApplyMatrixToTransform(Matrix4x4 matrix)
    {
        // Extract position from the matrix (4th column)
        Vector3 position = matrix.GetColumn(3);

        // Extract rotation from the matrix
        Quaternion rotation = matrix.rotation;

        // Extract scale from the matrix
        Vector3 scale = new Vector3(
            matrix.GetColumn(0).magnitude,
            matrix.GetColumn(1).magnitude,
            matrix.GetColumn(2).magnitude
        );

        // Apply to transform (world space)
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;
    }
}