using UnityEngine;

public static class VectorExtension
{
    public static Vector3 Add(this Vector3 one, Vector3 two)
    {
        return new Vector3(one.x + two.x, one.y, one.z + two.z);
    }
}