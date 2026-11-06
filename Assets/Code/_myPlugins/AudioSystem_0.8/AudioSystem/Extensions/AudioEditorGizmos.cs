#if UNITY_EDITOR
using Infrastructure.AudioSystem.Components;
using UnityEditor;
using UnityEngine;

public static class AudioEditorGizmos
{
    public static void DrawZoneBounds(EnvironmentAudioComponent zone, Color color)
    {
        var col = zone.GetComponent<Collider>();
        if (col == null)
            return;

        Handles.color = color;
        Transform t = zone.transform; // или col.transform

        if (col is BoxCollider box)
        {
            Vector3 worldCenter = t.TransformPoint(box.center);
            Vector3 worldSize = Vector3.Scale(box.size, t.lossyScale);
            Handles.DrawWireCube(worldCenter, worldSize);
        }
        else if (col is SphereCollider sphere)
        {
            Vector3 worldCenter = t.TransformPoint(sphere.center);
            float worldRadius = sphere.radius * Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);
            // Рисуем три диска в плоскостях, совпадающих с локальными осями объекта
            Handles.DrawWireDisc(worldCenter, t.up, worldRadius);
            Handles.DrawWireDisc(worldCenter, t.right, worldRadius);
            Handles.DrawWireDisc(worldCenter, t.forward, worldRadius);
        }
    }

    public static void DrawPortal(EnvironmentPortalComponent portal, Color color)
    {
        var col = portal.GetComponent<Collider>();
        if (col == null)
            return;

        Handles.color = color;
        var bounds = col.bounds;
        Handles.DrawWireCube(bounds.center, bounds.size);

        // Ось портала
        Handles.color = Color.yellow;
        var axis = portal.Axis.normalized;
        Handles.ArrowHandleCap(0, bounds.center, Quaternion.LookRotation(axis), bounds.extents.magnitude, EventType.Repaint);
    }
}
#endif
