using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    // Serialized names are retained so the existing prefab and scene keep their references.
    // Order: front left, front right, rear left, rear right.
    public WheelCollider[] tires = new WheelCollider[4];
    [SerializeField] Transform[] wheelVisuals = new Transform[4];
    [Min(0)] public int maxEngineTorque = 300;
    [Min(0)] public int brakeTorque = 5000;
    [Min(0)] public float steerSensitivity = 1f;
    [Min(1)] public float maxAngle = 30f;
    [SerializeField, Min(0.01f)] float pedalRate = 2f;
    [SerializeField, Min(1)] float steeringRate = 90f;

    public float throttleInput { get; private set; }
    public float brakeInput { get; private set; }
    public float steeringInput { get; private set; }
    public float speed { get; private set; }
    public int gear { get; private set; } = 1;
    public float EngineRPM { get; private set; }

    readonly float[] gearRatios = { 3.1f, 2.5f, 2f, 1.75f, 1.45f, 1.3f, 1.1f, 0.9f };
    const float FinalDriveRatio = 4.9f;
    const float IdleRPM = 2000f;
    const float MaxEngineRPM = 15000f;
    public AnimationCurve torqueCurve;

    Rigidbody vehicleBody;
    float throttleTarget;
    float brakeTarget;
    float steeringTarget;

    void Awake()
    {
        vehicleBody = GetComponent<Rigidbody>();
        if (tires == null || tires.Length != 4 ||
            System.Array.Exists(tires, tire => tire == null))
        {
            Debug.LogError("Movement requires four assigned WheelColliders (front pair, rear pair).", this);
            enabled = false;
            return;
        }
        if (torqueCurve == null || torqueCurve.length == 0)
        {
            Debug.LogError("Movement requires an engine torque curve.", this);
            enabled = false;
            return;
        }
        vehicleBody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        // Read button edges at render frequency so shifts aren't lost between physics ticks.
        throttleTarget = Input.GetKey(KeyCode.P) ? 1f : 0f;
        brakeTarget = Input.GetKey(KeyCode.O) ? 1f : 0f;
        steeringTarget = (Input.GetKey(KeyCode.D) ? 1f : 0f)
            - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        int shift = (Input.GetKeyDown(KeyCode.Period) ? 1 : 0)
            - (Input.GetKeyDown(KeyCode.Comma) ? 1 : 0);
        gear = Mathf.Clamp(gear + shift, 1, gearRatios.Length);
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        throttleInput = Mathf.MoveTowards(throttleInput, throttleTarget, pedalRate * dt);
        brakeInput = Mathf.MoveTowards(brakeInput, brakeTarget, pedalRate * dt);
        steeringInput = Mathf.MoveTowards(steeringInput, steeringTarget * maxAngle,
            steeringRate * steerSensitivity * dt);

        float wheelRPM = Mathf.Abs((tires[2].rpm + tires[3].rpm) * 0.5f);
        float ratio = gearRatios[gear - 1] * FinalDriveRatio;
        // An idle floor approximates clutch slip at launch; this is not a clutch simulation.
        float coupledRPM = wheelRPM * ratio;
        EngineRPM = Mathf.Clamp(coupledRPM, IdleRPM, MaxEngineRPM);
        float engineTorque = Mathf.Max(0f, torqueCurve.Evaluate(EngineRPM / MaxEngineRPM))
            * maxEngineTorque;
        float rearWheelTorque = coupledRPM >= MaxEngineRPM ? 0f
            : throttleInput * engineTorque * ratio * 0.5f;

        for (int i = 0; i < 4; i++)
        {
            tires[i].steerAngle = i < 2 ? steeringInput : 0f;
            tires[i].motorTorque = i < 2 ? 0f : rearWheelTorque;
            // brakeTorque is the total budget: 70% front, 30% rear, split per axle.
            tires[i].brakeTorque = brakeInput * brakeTorque * (i < 2 ? 0.35f : 0.15f);
        }
        speed = vehicleBody.linearVelocity.magnitude;
    }

    void LateUpdate()
    {
        // Animate only visual children; moving a collider would change the simulation.
        if (wheelVisuals == null) return;
        for (int i = 0; i < Mathf.Min(tires.Length, wheelVisuals.Length); i++)
        {
            Transform visual = wheelVisuals[i];
            if (visual == null || visual == tires[i].transform) continue;
            tires[i].GetWorldPose(out Vector3 position, out Quaternion rotation);
            visual.SetPositionAndRotation(position, rotation);
        }
    }

    void OnDisable()
    {
        throttleTarget = brakeTarget = steeringTarget = 0f;
        throttleInput = brakeInput = steeringInput = 0f;
        if (tires == null) return;
        foreach (WheelCollider tire in tires)
        {
            if (tire == null) continue;
            tire.motorTorque = tire.brakeTorque = tire.steerAngle = 0f;
        }
    }

    void OnApplicationFocus(bool focused)
    {
        if (!focused) throttleTarget = brakeTarget = steeringTarget = 0f;
    }
}
