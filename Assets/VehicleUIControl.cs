using UnityEngine;
using TMPro;

public class VehicleUIControl : MonoBehaviour
{
    public Movement movement;

    public RectTransform throttleFill;
    public RectTransform throttleTotal;

    public RectTransform brakeFill;
    public RectTransform brakeTotal;

    public RectTransform steerFill;
    public RectTransform steerTotal;

    public TMP_Text speedText;
    public TMP_Text gearText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (movement == null || throttleFill == null || throttleTotal == null ||
            brakeFill == null || brakeTotal == null || steerFill == null ||
            steerTotal == null || speedText == null || gearText == null)
        {
            Debug.LogError("VehicleUIControl requires a vehicle and all HUD references.", this);
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        throttleBarUpdate();
        brakeBarUpdate();
        steerBarUpdate();
        speedometerUpdate();
    }

    void speedometerUpdate()
    {

        float speedMS = movement.speed;
        float speedKPH = Mathf.RoundToInt(speedMS * 3.6f);

        int gear = movement.gear;

        speedText.text = speedKPH + " KPH";
        gearText.text = gear.ToString();

    }

    void throttleBarUpdate()
    {
        float throttleInput = movement.throttleInput;

        Vector2 totalSize = throttleTotal.sizeDelta;
        Vector2 barSize = throttleFill.sizeDelta;

        barSize.y = totalSize.y * throttleInput;

        throttleFill.sizeDelta = barSize;

    }

    void brakeBarUpdate()
    {
        float brakeInput = movement.brakeInput;

        Vector2 totalSize = brakeTotal.sizeDelta;
        Vector2 barSize = brakeFill.sizeDelta;

        barSize.y = totalSize.y * brakeInput;

        brakeFill.sizeDelta = barSize;
    }

    void steerBarUpdate()
    {

        float steeringInput = movement.steeringInput;
        float maxAngle = movement.maxAngle;
        float normalizedInput = steeringInput / Mathf.Max(1f, maxAngle); //from -1 to 1

        float maxSlide = steerTotal.rect.width / 2;

        Vector2 anchorPosition = steerFill.anchoredPosition;
        anchorPosition.x = normalizedInput * maxSlide;
        steerFill.anchoredPosition = anchorPosition;

    }
}
