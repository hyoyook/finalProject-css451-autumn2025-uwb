using UnityEngine;

public partial class TexturePlacement : MonoBehaviour
{
    // translation
    public float UV_Translate_X = 0f;
    public float UV_Translate_Y = 0f;

    // rotation
    public float UV_Rotation = 0f;   // degrees

    // scale
    public float UV_Scale_X = 1f;
    public float UV_Scale_Y = 1f;

    private Mesh mMesh;             // mesh of the plane
    private Vector2[] baseUV;       // store the initial UV coords of the mesh

    private void Awake()
    {
        mMesh = GetComponent<MeshFilter>().mesh;

        // cache original UVs from the table
        baseUV = mMesh.uv;

        if (baseUV == null || baseUV.Length == 0)
        {
            Debug.LogError("[TexturePlacement] Mesh has no UVs to work with on " + name + ". Generating planner UVs");

            // generate UVs
            Vector3[] v = mMesh.vertices;
            Vector2[] planarUV = GeneratePlanarUVs(v);

            // assign to mesh
            mMesh.uv = planarUV;
            baseUV = (Vector2[])planarUV.Clone();
        }
        else
        {
            baseUV = (Vector2[])baseUV.Clone();
        }
    }

    private void LateUpdate()
    {
        if (mMesh == null || baseUV == null)
        {
            return;
        }

        // TRS 
        Vector2 translation = new Vector2(UV_Translate_X, UV_Translate_Y);
        Vector2 scale       = new Vector2(UV_Scale_X, UV_Scale_Y);

        // transform 
        Matrix3x3 M = Matrix3x3Helpers.CreateTRS(translation, UV_Rotation, scale);

        
        Vector2[] newUV = new Vector2[baseUV.Length];
        for (int i = 0; i < baseUV.Length; i++)
        {
            newUV[i] = M * baseUV[i];
        }

        mMesh.uv = newUV;   // write back to the mesh
    }

    public void ResetToBaseUV()
    {
        if (mMesh == null || baseUV == null)
        { 
            return; 
        }

        mMesh.uv = (Vector2[])baseUV.Clone();

        UV_Translate_X = 0f;
        UV_Translate_Y = 0f;
        UV_Rotation = 0f;
        UV_Scale_X = 1f;
        UV_Scale_Y = 1f;
    }
}
