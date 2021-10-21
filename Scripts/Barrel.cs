using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    public float damage;
    public float radius;
    public Color color;

    void OnEnable() => BarrelManager.barrels.Add(this);
    void OnDisable() => BarrelManager.barrels.Remove(this);
    void OnValidate() => radius = Mathf.Abs(radius);

    void OnDrawGizmosSelected() {
        Handles.color = color;
        Handles.DrawWireDisc(transform.position, transform.up, radius);
        Handles.color = Color.white;
    }
}
