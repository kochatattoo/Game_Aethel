using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RealStep.Demo
{
    [System.Serializable]
    public class Waypoint
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;
    }

    public class PingPongMover : MonoBehaviour
    {
        [Tooltip("List of points the platform will loop through (A -> B -> C -> A...)")]
        public List<Waypoint> waypoints = new List<Waypoint>();
        
        [Space(5)]
        public float moveSpeed = 2f;
        public float rotationSpeed = 90f;
        public float scaleSpeed = 1f;

        private int currentIndex = 0;

        void Start()
        {
            if (waypoints.Count > 0)
            {
                transform.position = waypoints[0].position;
                transform.eulerAngles = waypoints[0].rotation;
                transform.localScale = waypoints[0].scale;
            }
        }

        void Update()
        {
            if (waypoints.Count < 2) return;

            int nextIndex = (currentIndex + 1) % waypoints.Count;
            Waypoint target = waypoints[nextIndex];

            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(target.rotation), rotationSpeed * Time.deltaTime);
            transform.localScale = Vector3.MoveTowards(transform.localScale, target.scale, scaleSpeed * Time.deltaTime);

            bool posReached = Vector3.Distance(transform.position, target.position) <= 0.001f;
            bool rotReached = Quaternion.Angle(transform.rotation, Quaternion.Euler(target.rotation)) <= 0.1f;
            bool scaleReached = Vector3.Distance(transform.localScale, target.scale) <= 0.001f;

            if (posReached && rotReached && scaleReached)
            {
                currentIndex = nextIndex;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PingPongMover))]
    public class PingPongMoverEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PingPongMover mover = (PingPongMover)target;

            DrawDefaultInspector();

            GUILayout.Space(10);
            if (GUILayout.Button("Add Current Transform as Waypoint", GUILayout.Height(30)))
            {
                Undo.RecordObject(mover, "Add Waypoint");
                mover.waypoints.Add(new Waypoint
                {
                    position = mover.transform.position,
                    rotation = mover.transform.eulerAngles,
                    scale = mover.transform.localScale
                });
                EditorUtility.SetDirty(mover);
            }

            if (mover.waypoints.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label("Edit Waypoints from Scene", EditorStyles.boldLabel);

                for (int i = 0; i < mover.waypoints.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    
                    GUILayout.Label($"Point {i}", GUILayout.Width(50));

                    if (GUILayout.Button("Get (Save)", GUILayout.Width(80)))
                    {
                        Undo.RecordObject(mover, "Get Transform");
                        mover.waypoints[i].position = mover.transform.position;
                        mover.waypoints[i].rotation = mover.transform.eulerAngles;
                        mover.waypoints[i].scale = mover.transform.localScale;
                        EditorUtility.SetDirty(mover);
                    }

                    if (GUILayout.Button("Set (Load)", GUILayout.Width(80)))
                    {
                        Undo.RecordObject(mover.transform, "Set Transform");
                        mover.transform.position = mover.waypoints[i].position;
                        mover.transform.eulerAngles = mover.waypoints[i].rotation;
                        mover.transform.localScale = mover.waypoints[i].scale;
                    }

                    GUILayout.FlexibleSpace();

                    GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                    if (GUILayout.Button("X", GUILayout.Width(30)))
                    {
                        Undo.RecordObject(mover, "Remove Waypoint");
                        mover.waypoints.RemoveAt(i);
                        EditorUtility.SetDirty(mover);
                        break;
                    }
                    GUI.backgroundColor = Color.white;
                    
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
#endif
}