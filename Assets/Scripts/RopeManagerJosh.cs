using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoshNode
{
    public Vector2 position;
    public Vector2 previousPosition;
    public float mass;
    public bool isFixed;

    public JoshNode(Vector2 position, float mass, bool isFixed)
    {
        this.position = position;
        this.previousPosition = position; // Initialize previousPosition to current position
        this.mass = mass;
        this.isFixed = isFixed;
    }

    public void AddForce(Vector2 force)
    {
        Vector2 acceleration = force / mass; // F = ma, so a = F/m
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


public class RopeManagerJosh : MonoBehaviour
{
    public List<JoshNode> nodes = new List<JoshNode>();
    public float segmentLength;
    public int segmentCount;
    private LineRenderer line;

    void Start()
    {
        // Initialize the segments
        for (int i = 0; i < segmentCount; i++)
        {
          //  bool isFixed = (i == 0 || i == segmentCount - 1); // Fix the first and last nodes
          if (i == 0 || i == segmentCount - 1)
          {
              nodes.Add(new JoshNode(new Vector2(i * segmentLength, 0), 1, true));
          }
          else
          {
              nodes.Add(new JoshNode(new Vector2(i * segmentLength, 0), 1, false));
          }
            
        }

        // Initialize the LineRenderer
        line = gameObject.GetComponent<LineRenderer>();
        line.positionCount = nodes.Count;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.material = new Material(Shader.Find("Sprites/Default"));
    }

    void FixedUpdate()
    {
        Simulate(Time.fixedDeltaTime);
    }

    public void Simulate(float deltaTime)
    {
        // Apply gravity and integrate nodes
        foreach (var node in nodes)
        {
            if (!node.isFixed)
            {
                // Apply gravity
                node.AddForce(Vector2.down * 9.81f * node.mass);
            }
            else
            {
                node.position = node.previousPosition;
            }
            node.Integrate();
        }

        // Apply ground check
        foreach (var node in nodes)
        {
            if (node.position.y < -3.5f)
            {
                node.position.y = -3.5f;
            }
        }

        // Apply constraints to keep segments at a fixed distance
        for (int i = 0; i < segmentCount; i++)  // Iterating multiple times for stability
        {
            ApplyConstraints();
        }

        // Update visuals after simulation
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            line.SetPosition(i, nodes[i].position);
        }
    }

    private void ApplyConstraints()
    {
        // Apply distance constraints between nodes
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            JoshNode nodeA = nodes[i];
            JoshNode nodeB = nodes[i + 1];

            Vector2 delta = nodeA.position - nodeB.position;
            float distance = delta.magnitude;
            float error = distance - segmentLength;
            Vector2 correction = delta * error;

            if (!nodeA.isFixed)
            {
                nodeA.position -= correction;
            }
            if (!nodeB.isFixed)
            {
                nodeB.position += correction;
            }
        }
    }
}