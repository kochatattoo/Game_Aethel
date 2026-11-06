#if UNITY_EDITOR
using AudioSystem.Components.Interfaces.Components;
using Infrastructure.AudioSystem.Components.Occlusions;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace AudioSystem.DebugComponent
{
    [ExecuteInEditMode]
    public class OcclusionDebugVisualizer : MonoBehaviour
    {
        private IOcclusionService _occlusionService;

        [Inject]
        private void Construct(IOcclusionService occlusionService) => _occlusionService = occlusionService;

        private void OnDrawGizmos()
        {
            if (_occlusionService is not OcclusionService service)
                return;

            // Собираем последние записи для каждого эмиттера
            var lastEntries = new Dictionary<IOcclusionEmitter, OcclusionDebugEntry>();
            foreach (var entry in service.DebugEntries)
            {
                if (entry.Emitter == null)
                    continue;
                lastEntries[entry.Emitter] = entry; // перезаписываем, остаётся последняя
            }

            foreach (var kvp in lastEntries)
            {
                var entry = kvp.Value;
                // Цвет луча зависит от obstruction (0 = зелёный, 1 = красный)
                Gizmos.color = Color.Lerp(Color.green, Color.red, entry.ObstructionValue);
                Gizmos.DrawLine(entry.ListenerPos, entry.EmitterPos);

                // Сфера на эмиттере
                Gizmos.DrawWireSphere(entry.EmitterPos, 0.2f);

                // Текстовая метка
                UnityEditor.Handles.Label(entry.EmitterPos + Vector3.up * 0.5f, $"Obs: {entry.ObstructionValue:F2}");
            }
        }
    }
}
#endif