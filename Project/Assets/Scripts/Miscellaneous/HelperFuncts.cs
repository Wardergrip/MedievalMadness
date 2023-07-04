using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class HelperFuncts
{
    public static Vector3 GetRandomPositionInBox(this BoxCollider box)
    {
        return new Vector3
        (
            UnityEngine.Random.Range(box.bounds.min.x, box.bounds.max.x),
            UnityEngine.Random.Range(box.bounds.min.y, box.bounds.max.y),
            UnityEngine.Random.Range(box.bounds.min.z, box.bounds.max.z)
        );
    }

    public static bool IsPositionInBox(this BoxCollider box, Vector3 position, bool checkY = true)
    {
        bool insideX = box.bounds.min.x <= position.x && position.x <= box.bounds.max.x;
        bool insideY = true;
        if (checkY) insideY = box.bounds.min.y <= position.y && position.y <= box.bounds.max.y;
        bool insideZ = box.bounds.min.z <= position.z && position.z <= box.bounds.max.z;

        return insideX && insideY && insideZ;
    }

    // This can't be made an extension method because it does not require a parameter
    public static Quaternion GetRandomYOrientation()
    {
        Vector3 euler = new Vector3();
        euler.y = UnityEngine.Random.rotation.eulerAngles.y;
        Quaternion output = new Quaternion();
        output.eulerAngles = euler;
        return output;
    }
}
