using UnityEngine;

// Owns checkpoint progress and recovery; Movement remains responsible for driving.
public class CircuitSession : MonoBehaviour
{
    Movement vehicle;
    Rigidbody body;
    Transform[] gates;
    float roadWidth;
    Vector3 previousPosition;
    Vector3 recoveryPosition;
    Quaternion recoveryRotation;
    bool recoveryRequested;
    float recoveryCooldown;
    string feedback = "Follow the arrows. Pass every gate in order.";
    public CircuitProgress Progress { get; private set; }

    public void Initialize(Movement movement, Transform[] orderedGates, float width)
    {
        vehicle = movement;
        body = vehicle.GetComponent<Rigidbody>();
        gates = orderedGates;
        roadWidth = width;
        Progress = new CircuitProgress(gates.Length);
        SetRecoveryPose(gates[0]);
        Recover();
    }

    public void RequestRecovery() => recoveryRequested = true;

    void Update()
    {
        if (body != null && Input.GetKeyDown(KeyCode.R)) RequestRecovery();
    }

    void FixedUpdate()
    {
        if (body == null) return;
        if (recoveryRequested || body.position.y < -8f)
        {
            Recover();
            recoveryRequested = false;
            return;
        }
        if (recoveryCooldown > 0f)
        {
            recoveryCooldown -= Time.fixedDeltaTime;
            previousPosition = body.position;
            return;
        }
        // At most one accepted gate per physics step. Gates are spaced well apart.
        for (int i = 0; i < gates.Length; i++)
        {
            Transform gate = gates[i];
            Vector3 before = gate.InverseTransformPoint(previousPosition);
            Vector3 after = gate.InverseTransformPoint(body.position);
            if (!CircuitProgress.CrossedGate(before, after, roadWidth * 0.5f, 3f)) continue;
            if (Progress.TryPass(i))
            {
                SetRecoveryPose(gate);
                feedback = i == 0 ? "Lap complete! Keep going." : "Checkpoint accepted.";
            }
            else feedback = "Gate out of order. Head to gate " + Progress.NextGate + ".";
            break;
        }
        previousPosition = body.position;
    }

    void SetRecoveryPose(Transform gate)
    {
        // Land beyond the gate, facing forward, so recovery cannot award progress.
        recoveryPosition = gate.position + gate.forward * 6f + Vector3.up * 0.3f;
        recoveryRotation = gate.rotation;
    }

    void Recover()
    {
        vehicle.ResetForRecovery(recoveryPosition, recoveryRotation);
        previousPosition = recoveryPosition;
        recoveryCooldown = 0.5f;
        feedback = "Recovered. Next gate: " + Progress.NextGate;
        foreach (CameraMotion camera in FindObjectsByType<CameraMotion>(FindObjectsSortMode.None))
            if (camera.movement == vehicle) camera.SnapToTarget();
    }

    void OnGUI()
    {
        if (Progress == null) return;
        float scale = Mathf.Clamp(Screen.height / 800f, 0.7f, 1.8f);
        Matrix4x4 original = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
        GUI.Box(new Rect(16, 16, 440, 150), GUIContent.none);
        GUI.Label(new Rect(30, 24, 410, 26), "CIRCUIT PRACTICE  |  Laps completed: " + Progress.CompletedLaps);
        GUI.Label(new Rect(30, 50, 410, 26), "NEXT: " + (Progress.NextGate == 0 ? "START / FINISH" : "GATE " + Progress.NextGate));
        GUI.Label(new Rect(30, 76, 410, 26), feedback);
        GUI.Label(new Rect(30, 104, 410, 26), "P accelerate  O brake  A/D steer  ,/. gears  R recover");
        GUI.Label(new Rect(30, 130, 410, 26), Mathf.RoundToInt(vehicle.speed * 3.6f)
            + " km/h  |  Gear " + vehicle.gear);
        GUI.matrix = original;
    }
}
