using UnityEngine;

public static class MathUtils
{
    // world size so that the object takes up `degrees` of the player's vision at `distance`
    public static float AngularToWorldSize(float degrees, float distance) => 2f * distance * Mathf.Tan(degrees * 0.5f * Mathf.Deg2Rad);
}
