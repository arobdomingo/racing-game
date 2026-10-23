using System;
using UnityEngine;

// Small state machine independent of scene objects, input and presentation.
public sealed class CircuitProgress
{
    public int GateCount { get; }
    public int NextGate { get; private set; } = 1;
    public int LastGate { get; private set; }
    public int CompletedLaps { get; private set; }

    public CircuitProgress(int gateCount)
    {
        if (gateCount < 2) throw new ArgumentOutOfRangeException(nameof(gateCount));
        GateCount = gateCount;
    }

    public bool TryPass(int gate)
    {
        if (gate != NextGate) return false;
        LastGate = gate;
        NextGate = (gate + 1) % GateCount;
        if (gate == 0) CompletedLaps++;
        return true;
    }

    // Sweep the vehicle center across a gate plane. The plane's +Z is race direction.
    // Interpolate the actual crossing point, rather than testing only the end position.
    public static bool CrossedGate(Vector3 previous, Vector3 current, float halfWidth,
        float halfHeight)
    {
        if (previous.z >= 0f || current.z < 0f) return false;
        float t = -previous.z / (current.z - previous.z);
        Vector3 crossing = Vector3.Lerp(previous, current, t);
        return Mathf.Abs(crossing.x) <= halfWidth && Mathf.Abs(crossing.y) <= halfHeight;
    }
}
