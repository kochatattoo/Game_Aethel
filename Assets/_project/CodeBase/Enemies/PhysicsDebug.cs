using UnityEngine;

namespace CodeBase.Enemies
{
    public static class PhysicsDebug
    {
        public static void DrawDebug(Vector3 worldPos, float radius, float seconds)
        {
            Debug.DrawRay(worldPos, radius * Vector3.up, Color.red, seconds);
            Debug.DrawRay(worldPos, radius * Vector3.down, Color.red, seconds);
            Debug.DrawRay(worldPos, radius * Vector3.left, Color.red, seconds);
            Debug.DrawRay(worldPos, radius * Vector3.right, Color.red, seconds);
            Debug.DrawRay(worldPos, radius * Vector3.forward, Color.red, seconds);
            Debug.DrawRay(worldPos, radius * Vector3.back, Color.red, seconds);
        }

        public static void DrawViewSector(Transform origin, float horizontalAngle, float verticalAngle, float radius, Color color)
        {
            if (origin == null) return;

            Vector3 position = origin.position;
            Vector3 forward = origin.forward;
            Vector3 up = origin.up;
            Vector3 right = origin.right;

            // Рисуем границы сферы (дальность)
            Gizmos.color = new Color(color.r, color.g, color.b, 0.1f);
            Gizmos.DrawWireSphere(position, radius);

            // Горизонтальная дуга (Ширина)
            Gizmos.color = color;
            DrawSectorArc(position, forward, up, horizontalAngle, radius);

            // Вертикальная дуга (Высота)
            Gizmos.color = new Color(color.r, color.g, color.b, 0.5f); // Чуть прозрачнее
            DrawSectorArc(position, forward, right, verticalAngle, radius);

            //  Дополнительно: соединяем крайние точки для визуализации "пирамиды"
            Gizmos.color = new Color(color.r, color.g, color.b, 0.2f);
            Vector3 p1 = Quaternion.Euler(-verticalAngle / 2f, -horizontalAngle / 2f, 0) * Vector3.forward;
            Vector3 p2 = Quaternion.Euler(verticalAngle / 2f, horizontalAngle / 2f, 0) * Vector3.forward;
            // Трансформируем локальные направления в мировые
            Gizmos.DrawRay(position, origin.TransformDirection(p1) * radius);
            Gizmos.DrawRay(position, origin.TransformDirection(p2) * radius);
        }

        private static void DrawSectorArc(Vector3 center, Vector3 forward, Vector3 axis, float angle, float radius)
        {
            // Вычисляем начальную точку (левая или верхняя граница)
            Vector3 startBoundary = Quaternion.AngleAxis(-angle / 2f, axis) * forward;
            Vector3 endBoundary = Quaternion.AngleAxis(angle / 2f, axis) * forward;

            // Рисуем боковые лучи
            Gizmos.DrawRay(center, startBoundary * radius);
            Gizmos.DrawRay(center, endBoundary * radius);

            // Рисуем саму дугу сегментами
            int segments = 20;
            Vector3 prevPoint = startBoundary;
            for (int i = 1; i <= segments; i++)
            {
                float currentAngle = -angle / 2f + (angle / segments) * i;
                Vector3 nextPoint = Quaternion.AngleAxis(currentAngle, axis) * forward;
                Gizmos.DrawLine(center + prevPoint * radius, center + nextPoint * radius);
                prevPoint = nextPoint;
            }
        }
    }
}