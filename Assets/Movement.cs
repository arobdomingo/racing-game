using UnityEngine;

public class Movement : MonoBehaviour
{

    public WheelCollider[] tires = new WheelCollider[4];
    public int maxEngineTorque;
    public int brakeTorque;

    public float steerSensitivity = 1f;
    public float maxAngle = 30f;

    public float throttleInput = 0f;
    public float brakeInput = 0f;
    public float steeringInput = 0f;

    //made public to access in Script class

    public float speed;
    public int gear = 1;
    float[] gearRatios = { 3.1f, 2.5f, 2.0f, 1.75f, 1.45f, 1.3f, 1.1f, 0.9f };
    float fdRatio = 4.9f;

    float maxEngineRPM = 15000f;
    public AnimationCurve torqueCurve;

    Rigidbody rigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        gearChange();
        Drive();
        Steer();

    }

    void Steer()
    {
        float angleChange = computeAngleChange() * steerSensitivity;

        steeringInput += angleChange;
        steeringInput = Mathf.Clamp(steeringInput, -maxAngle, maxAngle);

        for (int i = 0; i < 2; i++)
        {
            tires[i].steerAngle = steeringInput;
        }

    }

    float computeAngleChange()
    {

        float angleChange = 0;

        bool leftSteer = Input.GetKey(KeyCode.A);
        bool rightSteer = Input.GetKey(KeyCode.D);
        bool forward = Input.GetKey(KeyCode.W);

        if (leftSteer) //to steer left
        { angleChange += -1f; }

        if (rightSteer) //to steer right
        { angleChange += 1f; }

        if (forward)
        {
            if (leftSteer ^ rightSteer) { angleChange = angleChange / 2f; }

            else
            {
                float currentAngle = tires[0].steerAngle;

                if (currentAngle > 0f) { angleChange = -0.5f; }
                else { angleChange = 0.5f; }

            }

        }

        return angleChange;

    }

    float computeWheelTorque()
    {
        float wheelRPM = (tires[2].rpm + tires[3].rpm) / 2; //average rear wheel RPM

        float engineRPM = wheelRPM * gearRatios[gear - 1] * fdRatio; //relationship between engine and wheel RPM
        if (engineRPM > maxEngineRPM) engineRPM = maxEngineRPM;

        float normalizedRPM = engineRPM / maxEngineRPM;

        float engineTorque = torqueCurve.Evaluate(normalizedRPM) * maxEngineTorque; // normalizeTorque * maxWheelTorque

        Debug.Log("Engine RPM:" + engineRPM);

        return engineTorque * gearRatios[gear - 1] * fdRatio;  // ( = wheelTorque) relationship between engine and wheel torque

    }

    void Drive()
    {

        float wheelTorque = computeWheelTorque();

        //acceleration
        if (Input.GetKey(KeyCode.P)) { throttleInput += 0.01f; }
        else { throttleInput -= 0.01f; }

        throttleInput = Mathf.Clamp(throttleInput, 0f, 1f);

        for (int i = 2; i < 4; i++)
        {
            tires[i].motorTorque = throttleInput * (wheelTorque / 2);
        }

        //braking
        if (Input.GetKey(KeyCode.O)) { brakeInput += 0.01f; }
        else { brakeInput -= 0.01f; }

        brakeInput = Mathf.Clamp(brakeInput, 0f, 1f);

        for (int i = 0; i < 2; i++) 
        {
            tires[i].brakeTorque = brakeInput * (brakeTorque * 0.7f); //front tyres
        }

        for (int i = 2; i < 4; i++)
        {
            tires[i].brakeTorque = brakeInput * (brakeTorque * 0.3f); //rear tyres
        }

        speed = rigidbody.linearVelocity.magnitude;
    }

    void gearChange()
    {
        if (Input.GetKeyDown(KeyCode.Period)) // > for gear up
        {
            gear += 1;
        }

        else if (Input.GetKeyDown(KeyCode.Comma)) // < for gear down
        {
            gear -= 1;
        }

        gear = Mathf.Clamp(gear, 1, 8);

    }
}
