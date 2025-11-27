using UnityEngine;

public class TexturePlacement : MonoBehaviour
{
    // translation
    public float UV_Translate_X = 0f;
    public float UV_Translate_Y = 0f;

    // rotation
    public float UV_Rotation = 0f;   // degrees

    // scale
    public float UV_Scale_X = 1f;
    public float UV_Scale_Y = 1f;

    private Mesh mMesh;         // mesh of the plane
    private Vector2[] baseUV;   // store the initial UV coords of the mesh

    private void Awake()
    {
        mMesh = GetComponent<MeshFilter>().mesh;    // store the current UVs
        baseUV = (Vector2[])mMesh.uv.Clone();       // so we don't overwrite the initial data
    }

    private void LateUpdate()
    {
        Vector2[] newUV = new Vector2[baseUV.Length];

        Vector2 translation = new Vector2(UV_Translate_X, UV_Translate_Y);
        Vector2 scale = new Vector2(UV_Scale_X, UV_Scale_Y);

        Matrix3x3 M = Matrix3x3Helpers.CreateTRS(translation, UV_Rotation, scale);

        for (int i = 0; i < baseUV.Length; i++)
        {
            newUV[i] = M * baseUV[i];
        }

        mMesh.uv = newUV;   // write back to the mesh
    }
}
