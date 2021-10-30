using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PropPlacer : EditorWindow
{
    [MenuItem("Tools/Prop Placer")]
    public static void OpenWindow() => GetWindow<PropPlacer>("Prop Placer");


    #region Properties
    public float radius   = 2f;
    public int spawnCount = 8;
    #endregion

    #region Serialized Properties
    SerializedObject   so;
    SerializedProperty propRadius;
    SerializedProperty propSpawnCount;
    #endregion

    #region Enable and Disable
    void OnEnable() {
        so             = new SerializedObject(this);
        propRadius     = so.FindProperty("radius");
        propSpawnCount = so.FindProperty("spawnCount");

        SceneView.duringSceneGui += DuringSceneGUI;
    }

    void OnDisable() {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }
    #endregion

    #region Main GUI Handlers
    // Handles everything in the Window (GUI Layout and content)
    void OnGUI() {
        // Enables Undo and Redo
        so.Update();
        EditorGUILayout.PropertyField(propRadius);
        EditorGUILayout.PropertyField(propSpawnCount);
        so.ApplyModifiedProperties();
    }

    // Handles everything in the scene view (Props, diagrams, etc)
    void DuringSceneGUI(SceneView sceneView) {

    } 
    #endregion
}
