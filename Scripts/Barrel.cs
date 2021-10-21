using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class Barrel : MonoBehaviour
{
    public float damage;
    public float radius;
    public Color color;

    // Setting material color correctly
    MaterialPropertyBlock mpb;
    // Caching is much more efficient
    static readonly int shPropColor = Shader.PropertyToID("_Color");
    public MaterialPropertyBlock Mpb 
    {
        get {
            if (mpb == null) mpb = new MaterialPropertyBlock();
            return mpb;
        }
    }

    void ApplyColor() {
        MeshRenderer rnd = GetComponent<MeshRenderer>();
        Mpb.SetColor(shPropColor, color);
        rnd.SetPropertyBlock(Mpb);
    }

    void OnEnable() => BarrelManager.barrels.Add(this);
    void OnDisable() => BarrelManager.barrels.Remove(this);
    void OnValidate() {
        ApplyColor();
    } 

    void OnDrawGizmosSelected() {
        Handles.color = color;
        Handles.DrawWireDisc(transform.position, transform.up, radius);
        Handles.color = Color.white;
    }
}
