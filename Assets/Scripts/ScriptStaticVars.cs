using UnityEngine;

public class ScriptStaticVars : MonoBehaviour
{
    public static float horizontalSize;
    public static float horizontalSizePlusOne;

    static ScriptStaticVars()
    {
        horizontalSize = Camera.main.orthographicSize * Camera.main.aspect;
        horizontalSizePlusOne = horizontalSize + 1;
    }
}