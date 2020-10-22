using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeneralUtilities
{
    public static float Normalization(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }

    public static void LogMonoBehaviour(GameObject gameObject)
    {
        Debug.LogWarning("Logging " + gameObject.name);
    }
}
