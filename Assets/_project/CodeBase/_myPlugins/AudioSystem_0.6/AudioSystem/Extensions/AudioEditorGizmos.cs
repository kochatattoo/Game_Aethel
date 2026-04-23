#if UNITY_EDITOR
using Infrastructure.AudioSystem.Components;
using UnityEditor;
using UnityEngine;

public static class AudioEditorGizmos
{
    public static void DrawZoneBounds(EnvironmentAudioComponent zone, Color color)
    {
        var col = zone.GetComponent<Collider>();
        if (col == null) return;

        Handles.color = color;
        if (col is BoxCollider box)
        {
            Handles.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Handles.DrawWireDisc(sphere.center, Vector3.up, sphere.radius);
            Handles.DrawWireDisc(sphere.center, Vector3.right, sphere.radius);
            Handles.DrawWireDisc(sphere.center, Vector3.forward, sphere.radius);
        }
    }

    public static void DrawPortal(EnvironmentPortalComponent portal, Color color)
    {
        var col = portal.GetComponent<Collider>();
        if (col == null) return;

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
