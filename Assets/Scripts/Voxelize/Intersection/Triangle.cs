using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle
{

    public Vector3 A { get { return a; } }
    public Vector3 B { get { return b; } }
    public Vector3 C { get { return c; } }

    public Vector3 AB { get { return ab; } }
    public Vector3 BC { get { return bc; } }
    public Vector3 CA { get { return ca; } }

    public Vector3 Normal { get { return normal; } }

    protected Vector3 a, b, c, normal;
    protected Vector3 ab, bc, ca;

    public Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.ab = b - a;
        this.bc = c - b;
        this.ca = a - c;

        var cross = Vector3.Cross(this.ab, this.bc);
        this.normal = cross / cross.magnitude;
    }

}

