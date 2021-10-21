using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class Barrel : MonoBehaviour
{
    static readonly int shPropColor = Shader.PropertyToID("_Color");

    public BarrelType type;

    // Setting material color correctly
    // Caching is much more efficient
    MaterialPropertyBlock mpb;
    public MaterialPropertyBlock Mpb 
    {
        get {
            if (mpb == null) mpb = new MaterialPropertyBlock();
            return mpb;
        }
    }


    void OnEnable() => BarrelManager.barrels.Add(this);
    void OnDisable() => BarrelManager.barrels.Remove(this);
    void OnValidate() => ApplyColor();
    void OnDrawGizmosSelected() {
        Handles.color = type.color;
        Handles.DrawWireDisc(transform.position, transform.up, type.radius);
        Handles.color = Color.white;
    }


    void ApplyColor() {
        MeshRenderer rnd = GetComponent<MeshRenderer>();
        Mpb.SetColor(shPropColor,type.color);
        rnd.SetPropertyBlock(Mpb);
    }
}
