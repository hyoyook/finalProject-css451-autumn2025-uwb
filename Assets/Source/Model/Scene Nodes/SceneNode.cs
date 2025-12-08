using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneNode : MonoBehaviour
{

    protected Matrix4x4 mCombinedParentXform;

    public Vector3 NodeOrigin = Vector3.zero;
    public List<NodePrimitive> PrimitiveList;
    public List<SceneNode> ChildrenList;

    // Use this for initialization
    protected void Start()
    {
        InitializeSceneNode();
        // Debug.Log("PrimitiveList:" + PrimitiveList.Count); @
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void InitializeSceneNode()
    {
        mCombinedParentXform = Matrix4x4.identity;
    }

    // This must be called _BEFORE_ each draw!! 
    public void CompositeXform(ref Matrix4x4 parentXform)
    {
        // NodeOrigin represents where this node is positioned relative to parent
        // The local transform (position, rotation, scale) is applied around the local origin
        Matrix4x4 orgT = Matrix4x4.Translate(NodeOrigin);
        Matrix4x4 trs = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);

        // Order: parent -> move to NodeOrigin position -> apply local transform
        // This positions the node at NodeOrigin in parent space, then applies local transforms
        mCombinedParentXform = parentXform * orgT * trs;

        // propagate to all children
        if (ChildrenList != null)
        {
            foreach (SceneNode child in ChildrenList)
            {
                if (child != null)
                {
                    // Debug.Log("CompositeXform called on child: " + child.gameObject.name);
                    child.CompositeXform(ref mCombinedParentXform);
                }
                else
                {
                    // Debug.LogError("Child is null in ChildrenList of: " + gameObject.name);
                }
            }
        }
        else
        {
            // Debug.LogError("ChildrenList is null for: " + gameObject.name);
        }

        // disseminate to primitives
        if (PrimitiveList != null)
        {
            foreach (NodePrimitive p in PrimitiveList)
            {
                if (p != null)
                {
                    // Debug.Log($"LoadShaderMatrix called on: {gameObject.name}");
                    p.LoadShaderMatrix(ref mCombinedParentXform);
                }
                else
                {
                    Debug.LogError("Primitive is null in PrimitiveList of: " + gameObject.name);
                }

            }
        }
        else
        {
            Debug.LogError("PrimitiveList is null for: " + gameObject.name);
        }
    }
}