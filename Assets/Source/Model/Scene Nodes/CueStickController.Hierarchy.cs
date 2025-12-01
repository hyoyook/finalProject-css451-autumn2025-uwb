using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
///  EXPERIMENTAL Partial class for CueStickController - Hierarchy node selection and manipulation
/// 
/// Automatically discovers all SceneNodes in the hierarchy and allows
/// different keybinds to control different nodes.
/// 
/// Controls:
/// - No modifier: Root node moves (orbit around ball)
/// - CTRL held: Selected child node rotates (pitch adjustment)
/// - SPACE held: Deepest node translates (draw/strike)
/// </summary>
public partial class CueStickController
{
    [Header("Hierarchy Control")]
    [Tooltip("Show hierarchy debug info")]
    public bool ShowHierarchyDebug = false;

    // Flattened list of all SceneNodes in hierarchy (populated at Start)
    private List<SceneNode> allNodes = new List<SceneNode>();
    
    // Node name to index mapping for quick lookup
    private Dictionary<string, int> nodeNameToIndex = new Dictionary<string, int>();
    
    // Currently selected node index for manipulation
    private int selectedNodeIndex = 0;
    
    // Cached reference to the deepest node (for draw/strike)
    private SceneNode deepestNode = null;

    /// <summary>
    /// Initialize hierarchy system - call from Start()
    /// </summary>
    private void HierarchyStart()
    {
        if (CueHierarchy == null)
            return;
            
        // Discover all nodes in hierarchy
        DiscoverNodes(CueHierarchy, 0);
        
        // Find the deepest node (last in flattened list)
        if (allNodes.Count > 0)
        {
            deepestNode = allNodes[allNodes.Count - 1];
        }
        
        if (ShowHierarchyDebug)
        {
            Debug.Log($"=== Discovered {allNodes.Count} SceneNodes ===");
            for (int i = 0; i < allNodes.Count; i++)
            {
                Debug.Log($"  [{i}] {allNodes[i].name}");
            }
            Debug.Log($"Deepest node: {(deepestNode != null ? deepestNode.name : "null")}");
        }
    }

    /// <summary>
    /// Recursively discover all SceneNodes in the hierarchy
    /// </summary>
    private void DiscoverNodes(SceneNode node, int depth)
    {
        if (node == null)
            return;
            
        // Add to flattened list
        allNodes.Add(node);
        nodeNameToIndex[node.name] = allNodes.Count - 1;
        
        // Recurse to children
        if (node.ChildrenList != null)
        {
            foreach (SceneNode child in node.ChildrenList)
            {
                if (child != null)
                {
                    DiscoverNodes(child, depth + 1);
                }
            }
        }
    }

    /// <summary>
    /// Get a node by its depth in the hierarchy (0 = root, 1 = first child, etc.)
    /// </summary>
    public SceneNode GetNodeByDepth(int depth)
    {
        if (depth >= 0 && depth < allNodes.Count)
        {
            return allNodes[depth];
        }
        return null;
    }

    /// <summary>
    /// Get a node by name
    /// </summary>
    public SceneNode GetNodeByName(string name)
    {
        if (nodeNameToIndex.TryGetValue(name, out int index))
        {
            return allNodes[index];
        }
        return null;
    }

    /// <summary>
    /// Get the deepest node in the hierarchy (typically the Hand)
    /// </summary>
    public SceneNode GetDeepestNode()
    {
        return deepestNode;
    }

    /// <summary>
    /// Get all nodes as a list (useful for UI dropdowns)
    /// </summary>
    public List<SceneNode> GetAllNodes()
    {
        return allNodes;
    }

    /// <summary>
    /// Get node names for UI dropdown
    /// </summary>
    public string[] GetNodeNames()
    {
        string[] names = new string[allNodes.Count];
        for (int i = 0; i < allNodes.Count; i++)
        {
            names[i] = allNodes[i].name;
        }
        return names;
    }

    /// <summary>
    /// Select a node by index for manipulation
    /// </summary>
    public void SelectNode(int index)
    {
        if (index >= 0 && index < allNodes.Count)
        {
            selectedNodeIndex = index;
            if (ShowHierarchyDebug)
            {
                Debug.Log($"Selected node: {allNodes[selectedNodeIndex].name}");
            }
        }
    }

    /// <summary>
    /// Get the currently selected node
    /// </summary>
    public SceneNode GetSelectedNode()
    {
        if (selectedNodeIndex >= 0 && selectedNodeIndex < allNodes.Count)
        {
            return allNodes[selectedNodeIndex];
        }
        return null;
    }

    /// <summary>
    /// Cycle through nodes with number keys (for testing)
    /// </summary>
    private void UpdateNodeSelection()
    {
        if (Keyboard.current == null)
            return;
            
        // Number keys 1-9 select nodes
        for (int i = 0; i < Mathf.Min(9, allNodes.Count); i++)
        {
            Key key = (Key)((int)Key.Digit1 + i);
            if (Keyboard.current[key].wasPressedThisFrame)
            {
                SelectNode(i);
            }
        }
        
        // Debug print with F2
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            Debug.Log($"=== Hierarchy State ===");
            for (int i = 0; i < allNodes.Count; i++)
            {
                string marker = (i == selectedNodeIndex) ? " <-- SELECTED" : "";
                Debug.Log($"  [{i}] {allNodes[i].name} - Pos: {allNodes[i].transform.localPosition}{marker}");
            }
        }
    }

    /// <summary>
    /// Get the node index count
    /// </summary>
    public int NodeCount => allNodes.Count;
    
    /// <summary>
    /// Get the selected node index
    /// </summary>
    public int SelectedNodeIndex => selectedNodeIndex;
}
