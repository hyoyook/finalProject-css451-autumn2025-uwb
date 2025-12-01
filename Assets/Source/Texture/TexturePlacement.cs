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

    private Mesh mMesh;             // mesh of the plane
    private Vector2[] baseUV;       // store the initial UV coords of the mesh

    private bool[] isClothVertex;   // track which vertex belongs to the cloth mesh (table->element 0)

    private void Awake()
    {
        // duplicate mesh, so only the cloth texture changes
        mMesh = Instantiate(GetComponent<MeshFilter>().mesh);
        GetComponent<MeshFilter>().mesh = mMesh;       
        Debug.Log($"[TexturePlacement] Mesh duplicated. Vertex count = {mMesh.vertexCount}, UV count = {mMesh.uv.Length}");


        baseUV = (Vector2[])mMesh.uv.Clone();

        BuildClothMask();
    }

    private void LateUpdate()
    {
        Vector2[] newUV = new Vector2[baseUV.Length];

        Vector2 translation = new Vector2(UV_Translate_X, UV_Translate_Y);
        Vector2 scale = new Vector2(UV_Scale_X, UV_Scale_Y);

        Matrix3x3 M = Matrix3x3Helpers.CreateTRS(translation, UV_Rotation, scale);

        for (int i = 0; i < baseUV.Length; i++)
        {
            if (isClothVertex[i])
            {
                newUV[i] = M * baseUV[i]; // apply transform
            }
            else
            {
                newUV[i] = baseUV[i];     // leave the wooden part untouched
            }
        }

        mMesh.uv = newUV; // write back to the mesh
    }

    // pool table has 1 mesh, so we are building a mask, only for cloth submesh
    // ChatGPT: "Unity, UV transform application for one of the mesh materials"
    private void BuildClothMask()
    {
        isClothVertex = new bool[mMesh.vertexCount];

        // get all triangle for submesh 0 (element 0)
        int[] tris = mMesh.GetTriangles(0);

        Debug.Log($"[TexturePlacement] Submesh 0 triangle index count = {tris.Length}");

        // for every triangle index of the cloth mesh, mark the vertex
        for (int i = 0; i < tris.Length; i++)
        {
            int v = tris[i];
            isClothVertex[v] = true;
        }
       

    }
}
