using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class XformRenderer : MonoBehaviour {
    public Material mat451;
    Mesh _mesh;
    MaterialPropertyBlock _mpb;
    static readonly int ID_Model = Shader.PropertyToID("_Model");
    static readonly int ID_VP    = Shader.PropertyToID("_ViewProj");

    void Awake() {
        _mesh = GetComponent<MeshFilter>().sharedMesh;
        _mpb = new MaterialPropertyBlock();
        // Disable the MeshRenderer if one exists
        var mr = GetComponent<MeshRenderer>();
        if (mr) mr.enabled = false;
    }

    void LateUpdate() {
        var vp = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix;
        var model = transform.localToWorldMatrix; // or your scene-node world * local

        _mpb.Clear();
        _mpb.SetMatrix(ID_Model, model);
        _mpb.SetMatrix(ID_VP, vp);

        Graphics.DrawMesh(_mesh, Matrix4x4.identity, mat451, 0, Camera.main, 0, _mpb,
                          castShadows:false, receiveShadows:false);
    }
}
