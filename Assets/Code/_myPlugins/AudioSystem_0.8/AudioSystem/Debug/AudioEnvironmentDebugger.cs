using Infrastructure.AudioSystem.Components.Interfaces;
using UnityEngine;

namespace AudioSystem.DebugComponent
{
    [ExecuteInEditMode]
    public class AudioEnvironmentDebugger : MonoBehaviour
    {
        [SerializeField]
        private GameObject _target;
        [SerializeField]
        private bool _showZones = true;
        [SerializeField]
        private bool _showPortals = true;

#if UNITY_EDITOR
        private IAudioEnviromentMaker _maker;

        private void OnEnable()
        {
            if (_target == null) _target = gameObject;
            _maker = _target.GetComponent<IAudioEnviromentMaker>();
            if (_maker == null) enabled = false;
        }

        private void OnDrawGizmos()
        {
            if (_maker == null)
                return;

            var auxData = _maker.GetCurrentAuxSendData();
            var style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;

            string text;
            if (auxData.IsBlended)
            {
                text = $"Portal Blend:\n{auxData.AuxBusA?.Name}: {auxData.VolumeA:P0}\n{auxData.AuxBusB?.Name}: {auxData.VolumeB:P0}";
            }
            else if (auxData.AuxBusA != null)
            {
                text = $"Zone: {auxData.AuxBusA.Name} ({auxData.VolumeA:P0})";
            }
            else
            {
                text = "No Environment";
            }

            UnityEditor.Handles.Label(_target.transform.position + Vector3.up * 2f, text, style);
        }
#endif
    }
}
