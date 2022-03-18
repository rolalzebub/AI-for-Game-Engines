using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane
{

    public Vector3 Normal { get { return normal; } }
    public float Distance { get { return distance; } }

    protected Vector3 normal;
    protected float distance;

    public Plane(Vector3 normal, float distance)
    {
        this.normal = normal;
        this.distance = distance;
    }

}

