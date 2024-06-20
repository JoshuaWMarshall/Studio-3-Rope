    using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class RopeManagerJosh : MonoBehaviour
{
   
    

    // Node is a class like : (add public and stuff, i'm lazy)
    //public Vector2 position;
    //public Vector2 previousPosition;
    private List<Node> nodes = new List<Node>();
    //public List<Constraint> constraints = new List<Constraint>();
    class Node
    {
        
        public float mass; // if you wanted to have mass
        public bool isFixed; // bool fixed; // if true, don't do integration, it's position is locked.
    }

    public void Integrate()
    {
        //Vector2 newPosition = position + (position - previousPosition);
        //previousPosition = position;
        //position = newPosition; 
    }

    class Constraint
    {
        int node1;
        int node2;
        float compensate1;
        float compensate2;
        float desiredDistance;
    }

    

    //public int AddNode(Vector2 position, float mass, bool isFixed)
    //{
        // The addNode returns the index number of the node it added(so 0, then 1, then 2, etc).So you could make a square like this:
        // int corner1 = addNode(new Vector2(0, 0), 1, false);
        // int corner2 = addNode(new Vector2(1, 0), 1, false);
        // int corner3 = addNode(new Vector2(1, 1), 1, false);
        // int corner4 = addNode(new Vector2(0, 1), 1, false);
    //}

    public void AddConstraint(int node1, int node2, float desiredDistance, float compensate1 = 0.5f, float compensate2 = 0.5f)
    {
        // addConstraint(corner1, corner2,1.0f);
        // addConstraint(corner2, corner3,1.0f);
        // addConstraint(corner3, corner4,1.0f);
        // addConstraint(corner4, corner1,1.0f);
    }



   

    // Also handy would be an addConstraint that doesn't take a distance, it calculates it manually by getting the distance between the two nodes (makes diagonals easier).
    private void FixedUpdate()
    {
    
	// Loop over nodes

    // Add gravity to each(with addForce)

    // Integrate each node

    // Loop 10 times(or more / less)

    // Loop over constraints

    // Do fixed distance constraint between the constraint's node1 and node2

    // Like: constraintfixeddist(ref nodeList[constraint.node1].state.pos, ref nodeList[constraint.node2].state.pos, constraint.desiredDistance, constraint.compensate1, constraint.compensate2);
    // Loop over nodes
    // Apply ground check, move nodes to ground if they go below.
    // Loop over nodes
    // Copy node positions into game objects if you have visuals.
    }
    

    // Depending on how you want it to look, you could put a gameobject in the Node class (so it is like we did today, sprites or meshes on the nodes), or in the Constraint class (for things like pipes). If they are on the constraints, you move the gameobject to the mid point between the two nodes it uses, and rotate it to match the vector between them.Maybe scale it too to match the distance.


}
