using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class Snapper : EditorWindow
{
    public enum GridType
    {
        Cartesion,
        Polar
    };

    [MenuItem("Tools/Snapper")]
    public static void OpenWindow() => GetWindow<Snapper>("Snapper");

    #region Main Data
    public float    gridScale = 1;
    public int      angle     = 30;
    public GridType gridType  = GridType.Polar;
    #endregion

    #region Serialized Attributes
    SerializedObject   so;
    SerializedProperty propGridScale;
    SerializedProperty propAngle;
    SerializedProperty propGridType;
    #endregion

    #region Enable and Disable
    void OnEnable()
    {
        // Setting up serialized properties (under and redo)
        so            = new SerializedObject(this);
        propGridScale = so.FindProperty("gridScale");
        propAngle     = so.FindProperty("angle");
        propGridType  = so.FindProperty("gridType");

        // Loading saved data
        gridScale = EditorPrefs.GetFloat("SNAPPER_TOOL_gridScale", 1);
        angle     = EditorPrefs.GetInt("SNAPPER_TOOL_angle", 25);
        gridType  = (GridType)EditorPrefs.GetInt("SNAPPER_TOOL_gridType", 0);

        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui   += DuringSceneGUI;
    }

    void OnDisable()
    {
        // Saving Data
        EditorPrefs.SetFloat("SNAPPER_TOOL_gridScale", gridScale);
        EditorPrefs.SetInt("SNAPPER_TOOL_angle", angle);
        EditorPrefs.SetInt("SNAPPER_TOOL_gridType", (int)gridType);

        Selection.selectionChanged -= Repaint;
    }
    #endregion

    #region Main GUI Methods
    void OnGUI()
    {
        // Save and Unsave
        so.Update();
        EditorGUILayout.PropertyField(propGridType);
        EditorGUILayout.PropertyField(propGridScale);
        if (gridType == GridType.Polar)
        {
            EditorGUILayout.PropertyField(propAngle);
            propAngle.intValue = Mathf.Max(4, propAngle.intValue);
        }
        so.ApplyModifiedProperties();

        using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
        {
            if (GUILayout.Button("Snap"))
            {
                SnapCoordsToGrid();
            }
        }
    }

    void DuringSceneGUI(SceneView sceneView)
    {
        if (Event.current.type != EventType.Repaint)
            return;
        Handles.zTest = CompareFunction.LessEqual;
        const float gridExtent = 16;

        if (gridType == GridType.Cartesion)
            DrawCartesionPlane(gridExtent);
        else if (gridType == GridType.Polar)
            DrawPolarPlane(gridExtent);
    }
    #endregion

    #region Draw Methods
    void DrawPolarPlane(float maxScale)
    {
        int rings = Mathf.RoundToInt(maxScale / gridScale);
        float outerRing = (rings - 1) * gridScale;
        for (int i = 1; i < rings; i++)
        {
            Handles.DrawWireDisc(Vector3.zero, Vector3.up, i * gridScale);
        }

        // Draw all the lines
        for (float currentAngle = 0; currentAngle < 360; currentAngle += angle)
        {
            Handles.DrawAAPolyLine(Vector3.zero, new Vector2(outerRing, currentAngle).GetCartesionCoords());
        }
    }

    void DrawCartesionPlane(float gridExtent)
    {
        int lines = Mathf.RoundToInt((gridExtent * 2) / gridScale);
        if (lines % 2 == 0)
            lines++;
        int halfLines = lines / 2;

        for (int i = 0; i < lines; i++)
        {
            int offset = i - halfLines;
            float xCoord = offset * gridScale;
            float zCoord0 = halfLines * gridScale;
            float zCoord1 = -halfLines * gridScale;

            // Vertical Lines
            Vector3 p0 = new Vector3(xCoord, 0, zCoord0);
            Vector3 p1 = new Vector3(xCoord, 0, zCoord1);
            Handles.DrawAAPolyLine(p0, p1);

            // Horizontal Lines
            p0 = new Vector3(zCoord0, 0, xCoord);
            p1 = new Vector3(zCoord1, 0, xCoord);
            Handles.DrawAAPolyLine(p0, p1);
        }
    }
    #endregion

    #region Snap Methods
    void SnapCoordsToGrid()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            Undo.RecordObject(go.transform, "snap");
            if (gridType == GridType.Cartesion)
                go.transform.position = go.transform.position.Round(gridScale);
            else if (gridType == GridType.Polar)
                go.transform.position = go.transform.position.RoundPolar(gridScale, angle);
        }
    }
    #endregion

}

public static class SnapUtils
{
    // Snaps the value x to the nearest val value
    public static float Snap(this float x, float val)
    {
        return Mathf.RoundToInt(x / val) * val;
    }

    #region Cartesian Coordinates
    public static Vector3 Round(this Vector3 v, float scale)
    {
        v.x = v.x.Snap(scale);
        v.y = v.y.Snap(scale);
        v.z = v.z.Snap(scale);
        return v;
    }
    #endregion

    #region Polar Coordinates

    // Gets the value "r" or scale from a vector3
    public static float GetScale(this Vector3 v)
    {
        return new Vector2(v.x, v.z).magnitude;
    }

    // Gets the angle from a vector3 in degrees
    public static float GetAngle(this Vector3 v)
    {
        return Mathf.Atan2(v.z, v.x) * Mathf.Rad2Deg;
    }

    // Get's the cartesion coords from the vector3
    // Only calculates the x and z values in my case
    public static Vector3 GetCartesionCoords(this Vector2 v)
    {
        float angle = v.y * Mathf.Deg2Rad;
        float x = v.x * Mathf.Cos(angle);
        float z = v.x * Mathf.Sin(angle);
        return new Vector3(x, v.y, z);
    }

    // Snaps the vector3 v to the nearest polar coord vector3
    public static Vector3 RoundPolar(this Vector3 v, float scale, float angle)
    {
        float vAngle = v.GetAngle().Snap(angle);
        float vScale = v.GetScale().Snap(scale);
        return new Vector2(vScale, vAngle).GetCartesionCoords();

    }
    #endregion

    #region Helper Methods
    public static float AtLeast(this float v, float min) => Mathf.Max(v, min);
    public static int AtLeast(this int v, int min) => Mathf.Max(v, min);
    #endregion
}
