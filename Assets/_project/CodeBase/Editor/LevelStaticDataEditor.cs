using CodeBase.Logic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using CodeBase.StaticData;
using CodeBase.Utils;

namespace CodeBase.Editor
{
    [CustomEditor(typeof(LevelStaticData))]
    public class LevelStaticDataEditor : UnityEditor.Editor
    {
        private const string InitialPointTag = "InitialPoint";
        private const string TransferToPositionTag = "TransferToPoint";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LevelStaticData levelData = (LevelStaticData)target;

            if (GUILayout.Button("Collect"))
            {
                levelData.EnemySpawners =
                    FindObjectsOfType<SpawnMarker>()
                    .Select(x => new EnemySpawnerData(x.GetComponent<UniqueId>().Id, x.MonsterTypeID, x.transform.position))
                    .ToList();

                levelData.LevelKey = SceneManager.GetActiveScene().name;

                levelData.InitialHeroPosition = GameObject.FindGameObjectWithTag(InitialPointTag).transform.position;

                var transferToPoint = GameObject.FindGameObjectWithTag(TransferToPositionTag).GetComponent<LevelTransferTrigger>();
                levelData.LevelTransferData.TransferToPosition = transferToPoint.transform.position;
                levelData.LevelTransferData.LevelTo = transferToPoint.TransferTo;
            }

            EditorUtility.SetDirty(target);
        }
    }
}
