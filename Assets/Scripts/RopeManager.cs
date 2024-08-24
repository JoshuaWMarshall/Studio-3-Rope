using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;



[Serializable]
public class RopeState
{
    // The current position of the rope.
    public Vector2 position;
    
    // The previous position of the rope.
    public Vector2 previousPosition;
    
    // Initializes a new instance of the RopeState class with the given position.
    /// <param name="position">The initial position of the rope.</param>
    public RopeState(Vector2 position)
    {
        this.position = position;
        this.previousPosition = position;
    }
    
    // Adds a force to the current position of the rope with a configurable damping factor.
    /// <param name="force">The force to be added.</param>
    /// <param name="dampingFactor">The damping factor to be applied.</param>
    public void AddForce(Vector2 force, float dampingFactor)
    {
        Vector2 acceleration = force; // Assuming mass is 1 for simplicity
        Vector2 newPosition = position + (position - previousPosition) * dampingFactor + acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime;
        previousPosition = position;
        position = newPosition;
    }
    
    // Integrates the current position of the rope.
    public void Integrate()
    {
        Vector2 newPosition = position + (position - previousPosition);
        previousPosition = position;
        position = newPosition;
    }
}

[Serializable]
public class Node
{
    public RopeState state;
    public float mass;
    public bool isFixed;
    public Node(Vector2 position, float mass, bool isFixed)
    {
        this.state = new RopeState(position);
        this.mass = mass;
        this.isFixed = isFixed;
    }
}
public class RopeManager : MonoBehaviour
{
    [SerializeField] private List<Node> nodes = new List<Node>();
    public List<Constraint> constraints = new List<Constraint>();
    public GameObject nodeSomehting;
    public List<GameObject> nodeVis = new List<GameObject>();
    private LineRenderer line;
    private Node selectedNode = null;
    public int AddNode(Vector2 position, float mass, bool isFixed)
    {
        Node newNode = new Node(position, mass, isFixed);
        nodes.Add(newNode);
        GameObject visual = Instantiate(nodeSomehting);
        visual.transform.position = position;
        SpriteRenderer rend = visual.GetComponent<SpriteRenderer>();
        nodeVis.Add(visual);
        line.positionCount = nodes.Count;
        return nodes.Count - 1;
        
    }

    private void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();
        line.positionCount = nodes.Count;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        for (int i = 0; i < 20; i++)
        {
            if (i == 0 || i == 19)
            {
                float random = Random.Range(1, 100);
                random = random / 1000;
                Vector2 temp = new Vector2(random + i , 0 );
                AddNode(temp, 1, true);
                line.SetPosition(i,temp);
            }
            else
            {
                float random = Random.Range(0, 100);
                random = random / 1000;
                Vector2 temp = new Vector2(random + i , random * i );
                AddNode(temp, 1, false);
                line.SetPosition(i,temp);
            }

            if (i != 0)
            {
                AddConstraint(i-1, i);
            }
        }
    }
    void Update()
    {
        HandleMouseInput();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            selectedNode = GetNodeAtPosition(mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            selectedNode = null;
        }

        if (selectedNode != null)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            selectedNode.state.position = mousePosition;
            selectedNode.state.previousPosition = mousePosition; // To avoid snapping back
        }
    }

    Node GetNodeAtPosition(Vector2 position)
    {
        foreach (var node in nodes)
        {
            if (Vector2.Distance(node.state.position, position) < 0.5f) // Adjust the threshold as needed
            {
                return node;
            }
        }
        return null;
    }

    public void AddConstraint(int node1, int node2, float desiredDistance, float compensate1 = 0.5f, float compensate2 = 0.5f)
    {
        Constraint newConstraint = new Constraint(node1, node2, desiredDistance, compensate1, compensate2);
        constraints.Add(newConstraint);
    }

    public void AddConstraint(int node1, int node2)
    {
        float distance = Vector2.Distance(nodes[node1].state.position, nodes[node2].state.position);
        AddConstraint(node1, node2, distance);
    }

    void FixedUpdate()
    {
        // Apply gravity and integrate nodes
        foreach (var node in nodes)
        {
            if (node == selectedNode)
            {
                continue; // Skip the selected node
            }

            if (!node.isFixed)
            {
                node.state.AddForce(Vector2.down * 9.81f * node.mass, 0.99f);
            }
            else
            {
                node.state.position = node.state.previousPosition;
            }
            node.state.Integrate();
        }

        // Apply ground check
        foreach (var node in nodes)
        {
            if (node.state.position.y < -3.5f)
            {
                node.state.position.y = -3.5f;
            }
        }
    
        // Satisfy constraints
        for (int i = 0; i < 20; i++)
        {
            foreach (var constraint in constraints)
            {
                var node1 = nodes[constraint.node1];
                var node2 = nodes[constraint.node2];
                ConstraintLengthMinDist(ref node1.state.position, ref node2.state.position, constraint.compensate1, constraint.compensate2, constraint.desiredDistance *0.9f);
                ConstraintMaxDist(ref node1.state.position, ref node2.state.position, constraint.compensate1, constraint.compensate2, constraint.desiredDistance * 1.1f);
              //  ConstraintFixedDist(ref node1.state.position, ref node2.state.position, constraint.desiredDistance, constraint.compensate1, constraint.compensate2);
            }
        }

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            Node node = nodes[i];
            nodeVis[i].transform.position = new Vector3(node.state.position.x, node.state.position.y);
            line.SetPosition(i,node.state.position);
        }
    }
    private void ConstraintLengthMinDist(ref Vector2 pos1, ref Vector2 pos2, float compensate1,
        float compensate2, float minDistance)
    {
        Vector2 delta = pos2 - pos1;
        float currentDistance = delta.magnitude;
        if (currentDistance > 0 && currentDistance < minDistance)
        {
            float difference = (currentDistance - minDistance) / currentDistance;
            Vector2 correction = delta * difference;
            pos1 += correction * compensate1;
            pos2 -= correction * compensate2;
        }
    }
    private void ConstraintMaxDist(ref Vector2 pos1, ref Vector2 pos2, float compensate1,
        float compensate2, float maxDistance)
    {
        Vector2 delta = pos2 - pos1;
        float currentDistance = delta.magnitude;
        if (currentDistance > maxDistance)
        {
            float difference = (currentDistance - maxDistance) / currentDistance;
            Vector2 correction = delta * difference;
            pos1 += correction * compensate1;
            pos2 -= correction * compensate2;
        }
    }
}

[Serializable]public class Constraint
{
    public int node1;
    public int node2;
    public float compensate1;
    public float compensate2;
    public float desiredDistance;

    public Constraint(int node1, int node2, float desiredDistance, float compensate1 = 0.5f, float compensate2 = 0.5f)
    {
        this.node1 = node1;
        this.node2 = node2;
        this.desiredDistance = desiredDistance;
        this.compensate1 = compensate1;
        this.compensate2 = compensate2;
    }
}


