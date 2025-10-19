using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyLogger : MonoBehaviour
{
    private void OnDestroy()
    {
        Debug.Log($"[DestroyLogger] {gameObject.name} が Destroy されました。StackTrace:\n{System.Environment.StackTrace}");
    }
}
