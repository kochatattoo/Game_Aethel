using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using CodeBase.StaticData;

namespace CodeBase.Editor
{
    [CustomEditor(typeof(LevelStaticData))]
    public class LevelStaticDataEditor : UnityEditor.Editor
    {
        private const string InitialPointTag = "InitialPoint";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LevelStaticData levelData = (LevelStaticData)target;

            if (GUILayout.Button("Collect"))
            {
                levelData.LevelKey = SceneManager.GetActiveScene().name;

                levelData.InitialHeroPosition = GameObject.FindGameObjectWithTag(InitialPointTag).transform.position;
            }

            EditorUtility.SetDirty(target);
        }
    }
}
