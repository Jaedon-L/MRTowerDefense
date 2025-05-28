using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class PatternedPathGenerator
{
    /// <summary>
    /// Inserts N zig-zag segments between start and end.
    /// </summary>
    public static List<Vector3> ZigZagPath(
        Vector3 start,
        Vector3 end,
        int numSegments,
        float amplitude)
    {
        // 1) Calculate the straight path
        var navPath = new NavMeshPath();
        if (!NavMesh.CalculatePath(start, end, NavMesh.AllAreas, navPath)
            || navPath.status != NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning("ZigZagPath: NavMesh path incomplete");
            return null;
        }

        // 2) Sample evenly along that path
        List<Vector3> sampled = SampleAlongCorners(navPath.corners, numSegments + 1);

        // 3) Build the zig-zag by offsetting alternating midpoints
        List<Vector3> result = new List<Vector3>();
        result.Add(sampled[0]);
        bool toLeft = true;

        for (int i = 1; i < sampled.Count; i++)
        {
            Vector3 prev = sampled[i - 1];
            Vector3 curr = sampled[i];

            Vector3 mid = (prev + curr) * 0.5f;
            Vector3 forward = (curr - prev).normalized;
            Vector3 right   = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 offset = (toLeft ? right : -right) * amplitude;
            toLeft = !toLeft;

            result.Add(mid + offset);
            result.Add(curr);
        }

        return result;
    }

    /// <summary>
    /// Samples exactly count points (including first and last) along the polyline of corners.
    /// </summary>
    private static List<Vector3> SampleAlongCorners(Vector3[] corners, int count)
    {
        List<Vector3> pts = new List<Vector3>();
        if (corners == null || corners.Length == 0 || count < 2)
            return pts;

        // compute total length
        float total = 0f;
        for (int i = 1; i < corners.Length; i++)
            total += Vector3.Distance(corners[i - 1], corners[i]);

        float step = total / (count - 1);
        float distAcc = 0f;
        int segment = 0;
        float segDistAcc = 0f;

        for (int i = 0; i < count; i++)
        {
            float target = step * i;

            // advance segments until segment covers target
            while (segment < corners.Length - 1 &&
                   distAcc + Vector3.Distance(corners[segment], corners[segment + 1]) < target)
            {
                distAcc += Vector3.Distance(corners[segment], corners[segment + 1]);
                segment++;
            }

            Vector3 A = corners[segment];
            Vector3 B = corners[Mathf.Min(segment + 1, corners.Length - 1)];
            float segLen = Vector3.Distance(A, B);
            float t = segLen > 0f ? (target - distAcc) / segLen : 0f;
            pts.Add(Vector3.Lerp(A, B, t));
        }

        return pts;
    }

    /// <summary>
    /// Builds a sine-wave pattern along the navmesh path.
    /// </summary>
    public static List<Vector3> SineWavePath(
        Vector3 start,
        Vector3 end,
        int numPoints,
        float amplitude,
        float frequency)
    {
        // 1) Calculate the straight path
        var navPath = new NavMeshPath();
        bool ok = NavMesh.CalculatePath(start, end, NavMesh.AllAreas, navPath)
                  && navPath.status == NavMeshPathStatus.PathComplete;

        Vector3[] corners = ok ? navPath.corners : new Vector3[] { start, end };
        // 2) Sample points along those corners
        List<Vector3> sampled = SampleAlongCorners(corners, numPoints);

        // 3) Offset each by a sine function
        List<Vector3> result = new List<Vector3>();
        int count = sampled.Count;
        for (int i = 0; i < count; i++)
        {
            Vector3 p = sampled[i];
            // choose forward direction
            Vector3 forward;
            if (i < count - 1)
                forward = (sampled[i + 1] - p).normalized;
            else
                forward = (p - sampled[i - 1]).normalized;

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            float u = i / (float)(count - 1);
            float sine = Mathf.Sin(2f * Mathf.PI * frequency * u);
            result.Add(p + right * amplitude * sine);
        }

        return result;
    }
}
