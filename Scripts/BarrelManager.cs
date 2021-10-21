using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BarrelManager : MonoBehaviour
{
    public static List<Barrel> barrels = new List<Barrel>();

    #if UNITY_EDITOR
    void OnDrawGizmos() {
        // print(barrels.Count);
        foreach (Barrel barrel in barrels)
        {
            Vector3 managerPosition = transform.position;
            Vector3 barrelPosition = barrel.transform.position;
            float halfHeight = (managerPosition.y - barrelPosition.y) / 2;
            Vector3 offset = Vector3.up * halfHeight;

            Handles.DrawBezier(
                managerPosition, 
                barrelPosition, 
                managerPosition - offset,
                barrelPosition + offset,
                barrel.type.color,
                EditorGUIUtility.whiteTexture,
                1f);
            // Handles.DrawAAPolyLine(transform.position, barrel.transform.position);
        }
    }
    #endif
}
