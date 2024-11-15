using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundObjectMover : MonoBehaviour
{
    public float speed;             // Units per second to move in direction.
    public Vector3 dir;             // Normalised direction of movement.
    public float resetTime;         // Duration alive before the object is reset.

    private Vector3 startPos;       // Position of the object on start.

    void Start ()
    {
        startPos = transform.position;
        InvokeRepeating("ResetObject", 0, resetTime);
    }

    void Update ()
    {
        transform.position += dir * speed * Time.deltaTime;
    }

    // Resets the object's position.
    void ResetObject ()
    {
        transform.position = startPos;
    }
}