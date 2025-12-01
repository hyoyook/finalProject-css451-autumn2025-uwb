using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Utility script to help set up SceneNode hierarchies from existing GameObjects.
/// Attach this to any GameObject with Renderer components in children to automatically
/// create the SceneNode/NodePrimitive structure.
/// </summary>
[ExecuteInEditMode]
public class SceneNodeSetupHelper : MonoBehaviour
{
    [Header("Setup Options")]
    [Tooltip("Click to capture all current world positions for SceneNodes and NodePrimitives")]
    public bool CaptureAllPositions = false;
    
    [Tooltip("Click to auto-populate PrimitiveList from child renderers")]
    public bool AutoPopulatePrimitives = false;
    
    [Tooltip("Click to auto-populate ChildrenList from child SceneNodes")]
    public bool AutoPopulateChildren = false;
    
    [Tooltip("Show debug info about the hierarchy")]
    public bool DebugHierarchy = false;

    private void Update()
    {
        if (CaptureAllPositions)
        {
            CaptureAllPositions = false;
            DoCaptureAllPositions();
        }
        
        if (AutoPopulatePrimitives)
        {
            AutoPopulatePrimitives = false;
            DoAutoPopulatePrimitives();
        }
        
        if (AutoPopulateChildren)
        {
            AutoPopulateChildren = false;
            DoAutoPopulateChildren();
        }
        
        if (DebugHierarchy)
        {
            DebugHierarchy = false;
            DoDebugHierarchy();
        }
    }

    /// <summary>
    /// Capture all positions for SceneNodes and NodePrimitives in hierarchy
    /// </summary>
    private void DoCaptureAllPositions()
    {
        SceneNode node = GetComponent<SceneNode>();
        if (node != null)
        {
            node.CaptureInitialPosition();
            node.InitializeAllChildrenPositions();
            Debug.Log($"[SceneNodeSetupHelper] Captured positions for {name} and all children");
        }
        else
        {
            Debug.LogWarning($"[SceneNodeSetupHelper] No SceneNode component on {name}");
        }
    }

    /// <summary>
    /// Auto-populate the PrimitiveList with all NodePrimitive children (direct children only)
    /// </summary>
    private void DoAutoPopulatePrimitives()
    {
        SceneNode node = GetComponent<SceneNode>();
        if (node == null)
        {
            Debug.LogWarning($"[SceneNodeSetupHelper] No SceneNode component on {name}");
            return;
        }

        // Find the PrimitiveList child object
        Transform primitiveListParent = transform.Find("PrimitiveList");
        if (primitiveListParent == null)
        {
            // Look for any child with "Primitive" in name
            foreach (Transform child in transform)
            {
                if (child.name.ToLower().Contains("primitive"))
                {
                    primitiveListParent = child;
                    break;
                }
            }
        }

        if (primitiveListParent == null)
        {
            Debug.LogWarning($"[SceneNodeSetupHelper] No PrimitiveList child found on {name}");
            return;
        }

        node.PrimitiveList = new List<NodePrimitive>();
        foreach (Transform child in primitiveListParent)
        {
            NodePrimitive prim = child.GetComponent<NodePrimitive>();
            if (prim != null)
            {
                node.PrimitiveList.Add(prim);
            }
            else
            {
                // Auto-add NodePrimitive component if it has a Renderer
                Renderer rend = child.GetComponent<Renderer>();
                if (rend != null)
                {
                    prim = child.gameObject.AddComponent<NodePrimitive>();
                    prim.CaptureInitialPosition();
                    node.PrimitiveList.Add(prim);
                    Debug.Log($"[SceneNodeSetupHelper] Added NodePrimitive to {child.name}");
                }
            }
        }
        
        Debug.Log($"[SceneNodeSetupHelper] Populated {node.PrimitiveList.Count} primitives for {name}");
    }

    /// <summary>
    /// Auto-populate the ChildrenList with all SceneNode children
    /// </summary>
    private void DoAutoPopulateChildren()
    {
        SceneNode node = GetComponent<SceneNode>();
        if (node == null)
        {
            Debug.LogWarning($"[SceneNodeSetupHelper] No SceneNode component on {name}");
            return;
        }

        // Find the ChildrenList child object
        Transform childrenListParent = transform.Find("ChildrenList");
        if (childrenListParent == null)
        {
            // Look for any child with "Children" in name
            foreach (Transform child in transform)
            {
                if (child.name.ToLower().Contains("children"))
                {
                    childrenListParent = child;
                    break;
                }
            }
        }

        if (childrenListParent == null)
        {
            // No children list - that's okay for leaf nodes
            node.ChildrenList = new List<SceneNode>();
            Debug.Log($"[SceneNodeSetupHelper] No ChildrenList child found on {name}, using empty list");
            return;
        }

        node.ChildrenList = new List<SceneNode>();
        foreach (Transform child in childrenListParent)
        {
            SceneNode childNode = child.GetComponent<SceneNode>();
            if (childNode != null)
            {
                node.ChildrenList.Add(childNode);
            }
        }
        
        Debug.Log($"[SceneNodeSetupHelper] Populated {node.ChildrenList.Count} children for {name}");
    }

    /// <summary>
    /// Print debug info about the hierarchy
    /// </summary>
    private void DoDebugHierarchy()
    {
        SceneNode node = GetComponent<SceneNode>();
        if (node == null)
        {
            Debug.LogWarning($"[SceneNodeSetupHelper] No SceneNode component on {name}");
            return;
        }

        Debug.Log($"=== Hierarchy Debug for {name} ===");
        PrintNodeInfo(node, 0);
    }

    private void PrintNodeInfo(SceneNode node, int depth)
    {
        string indent = new string(' ', depth * 4);
        Debug.Log($"{indent}[SceneNode] {node.name}");
        Debug.Log($"{indent}  WorldPos: {node.transform.position}");
        Debug.Log($"{indent}  LocalPos: {node.transform.localPosition}");
        Debug.Log($"{indent}  NodeOrigin: {node.NodeOrigin}");

        if (node.PrimitiveList != null)
        {
            Debug.Log($"{indent}  Primitives ({node.PrimitiveList.Count}):");
            foreach (var prim in node.PrimitiveList)
            {
                if (prim != null)
                {
                    Debug.Log($"{indent}    - {prim.name}: Pivot={prim.Pivot}, WorldPos={prim.transform.position}");
                }
            }
        }

        if (node.ChildrenList != null)
        {
            Debug.Log($"{indent}  Children ({node.ChildrenList.Count}):");
            foreach (var child in node.ChildrenList)
            {
                if (child != null)
                {
                    PrintNodeInfo(child, depth + 1);
                }
            }
        }
    }
}
