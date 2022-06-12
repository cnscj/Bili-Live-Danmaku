using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationRound : MonoBehaviour
{
    public Vector3 point;
    public Vector3 axis;
    public float angle;

    void Update()
    {
        transform.RotateAround(point, axis, angle);
    }
}
