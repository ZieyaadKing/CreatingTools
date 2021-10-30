using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class PropPlacer : EditorWindow
{
    [MenuItem("Tools/Prop Placer")]
    public static void OpenWindow() => GetWindow<PropPlacer>("Prop Placer");


    #region Properties
    public float radius   = 2f;
    public int spawnCount = 8;

    Vector2[] randomPoints;
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

        GenerateRandomPoints();

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
        propRadius.floatValue = propRadius.floatValue.AtLeast(1f);   // Always positive
        EditorGUILayout.PropertyField(propSpawnCount);
        propSpawnCount.intValue = propSpawnCount.intValue.AtLeast(1);   // Always positive


        // Any changes made in the editor will immediately be updated
        // in the sceneview
        if (so.ApplyModifiedProperties()) 
        {
            GenerateRandomPoints();
            SceneView.RepaintAll();
        }

        // if you clicked the left mouse button in the editor window deselect
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
            GUI.FocusControl(null);
            Repaint();  // No delay when clicked
        }
    }

    // Handles everything in the scene view (Props, diagrams, etc)
    void DuringSceneGUI(SceneView sceneView) {
        // if (Event.current.type != EventType.Repaint) return;

        Handles.zTest = CompareFunction.LessEqual;
        Transform cameraTransform = sceneView.camera.transform;

        // Redraw scene when mouse is moved
        if (Event.current.type == EventType.MouseMove) {
            SceneView.RepaintAll();
        }
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition); 

        bool holdingAlt = (Event.current.modifiers & EventModifiers.Alt) != 0;

        // Change radius when mouse wheel is scrolled and not holding alt
        if (Event.current.type == EventType.ScrollWheel && !holdingAlt) {
            float scrollDirection = Mathf.Sign(Event.current.delta.y);

            so.Update();
            propRadius.floatValue -= scrollDirection * .025f;
            so.ApplyModifiedProperties();
            Repaint();
            Event.current.Use();    // Any other overlapping events after this will not occur
        }
        
        // Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit)) {

            // Setting up the tangent space
            // Tangent space: A space without any origin
            Vector3 hitNormal     = hit.normal;
            Vector3 hitTangent    = Vector3.Cross(hitNormal, cameraTransform.up);
            Vector3 hitBitangent  = Vector3.Cross(hitNormal, hitTangent);

            foreach (Vector2 point in randomPoints)
            {
                Vector3 rayOrigin    = hit.point + (hitTangent * point.x + hitBitangent * point.y) * radius;
                rayOrigin += hitNormal * 2; // Offset
                Vector3 rayDirection = -hitNormal;
                Ray     pointRay     = new Ray(rayOrigin, rayDirection);
                if (Physics.Raycast(pointRay, out RaycastHit pointHit)) {
                    DrawSphere(pointHit.point);
                    // Handles.DrawAAPolyLine(pointHit.point, pointHit.point + pointHit.normal );
                }
            }

            // Drawing tanngent space
            Handles.color = Color.blue;
            Handles.DrawAAPolyLine(5, hit.point, hit.point + hitTangent);
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(5, hit.point, hit.point + hitBitangent);
            Handles.color = Color.red;
            Handles.DrawAAPolyLine(5, hit.point, hit.point + hitNormal);
            Handles.color = Color.white;

            Handles.DrawWireDisc(hit.point, hit.normal, radius);
            
        }
    } 
    #endregion

    #region Draw Methods
    void DrawSphere(Vector3 pos) {
        Handles.SphereHandleCap(-1, pos, Quaternion.identity, .02f, EventType.Repaint);
    }
    #endregion
    void GenerateRandomPoints() {
        randomPoints = new Vector2[spawnCount];
        for (int i = 0; i < randomPoints.Length; i++)
        {
            randomPoints[i] = Random.insideUnitCircle;
        }
    }
}
