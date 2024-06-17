using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class RopeState
{
    public Vector2 position;
    public Vector2 previousPosition;

    public RopeState(Vector2 position)
    {
        this.position = position;
        this.previousPosition = position;
    }

    public void AddForce(Vector2 force)
    {
        Vector2 acceleration = force; // Assuming mass is 1 for simplicity
        Vector2 newPosition = position + (position - previousPosition) + acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime;
        previousPosition = position;
        position = newPosition;
    }

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
    public float distance;
    public int AddNode(Vector2 position, float mass, bool isFixed)
    {
        Node newNode = new Node(position, mass, isFixed);
        nodes.Add(newNode);
        GameObject visual = Instantiate(nodeSomehting);
        visual.transform.position = position;
        SpriteRenderer rend = visual.GetComponent<SpriteRenderer>();
        nodeVis.Add(visual);
        return nodes.Count - 1;
    }

    private void Start()
    {
        for (int i = 0; i < 20; i++)
        {
            if (i == 0 || i == 19)
            {
                Vector2 temp = new Vector2(0 + i , 0 );
                AddNode(temp, 1, true);
            }
            else
            {
                Vector2 temp = new Vector2(0 + i , 0 );
                AddNode(temp, 1, false);
            }

            if (i != 0)
            {
                AddConstraint(i-1, i);
            }
        }
    }

    public void AddConstraint(int node1, int node2, float desiredDistance, float compensate1 = 0.5f, float compensate2 = 0.5f)
    {
        Constraint newConstraint = new Constraint(node1, node2, desiredDistance, compensate1, compensate2);
        constraints.Add(newConstraint);
    }

    public void AddConstraint(int node1, int node2)
    {
        AddConstraint(node1, node2, distance);
    }

    void FixedUpdate()
    {
        // Apply gravity and integrate nodes
        foreach (var node in nodes)
        {
            if (!node.isFixed)
            {
                // Apply gravity
                node.state.AddForce(new Vector2(0, -9.81f) * node.mass);
            }
            else
            {
                node.state.position = node.state.previousPosition;
            }
            node.state.Integrate();
        }

        // Satisfy constraints
        for (int i = 0; i < 10; i++)
        {
            foreach (var constraint in constraints)
            {
                var node1 = nodes[constraint.node1];
                var node2 = nodes[constraint.node2];
                ConstraintFixedDist(ref node1.state.position, ref node2.state.position, constraint.desiredDistance, constraint.compensate1, constraint.compensate2);
            }
        }

        // Apply ground check
        foreach (var node in nodes)
        {
            if (node.state.position.y < -4.5f)
            {
                node.state.position.y = -4.4f;
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
        }
    }
    private void ConstraintFixedDist(ref Vector2 pos1, ref Vector2 pos2, float desiredDistance, float compensate1,
        float compensate2)
    {
        Vector2 delta = pos2 - pos1;
        float currentDistance = delta.magnitude;
        float difference = (currentDistance - desiredDistance) / currentDistance;
        Vector2 correction = delta * difference;

        pos1 += correction * compensate1;
        pos2 -= correction * compensate2;
    }
}

public class Constraint
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


