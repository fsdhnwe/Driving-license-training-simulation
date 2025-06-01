using UnityEngine;

public class RealisticCarController : MonoBehaviour
{
    [Header("Wheels")]
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelBL;
    public WheelCollider wheelBR;

    public Transform wheelFLTransform;
    public Transform wheelFRTransform;
    public Transform wheelBLTransform;
    public Transform wheelBRTransform;

    [Header("Car Settings")]
    public float maxMotorTorque = 1500f;
    public float maxSteeringAngle = 30f;
    public float brakeTorque = 3000f;

    [Header("Cameras")]
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0); // 降低重心

        SetupWheelFriction(wheelFL);
        SetupWheelFriction(wheelFR);
        SetupWheelFriction(wheelBL);
        SetupWheelFriction(wheelBR);

        SetupSuspension(wheelFL);
        SetupSuspension(wheelFR);
        SetupSuspension(wheelBL);
        SetupSuspension(wheelBR);

        SwitchToThirdPerson();
    }

    void Update()
    {
        // 輪子模型旋轉同步
        UpdateWheelVisuals(wheelFL, wheelFLTransform);
        UpdateWheelVisuals(wheelFR, wheelFRTransform);
        UpdateWheelVisuals(wheelBL, wheelBLTransform);
        UpdateWheelVisuals(wheelBR, wheelBRTransform);

        // 鏡頭切換
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (firstPersonCamera.enabled)
                SwitchToThirdPerson();
            else
                SwitchToFirstPerson();
        }
    }

    void FixedUpdate()
    {
        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        wheelFL.steerAngle = steering;
        wheelFR.steerAngle = steering;

        wheelFL.motorTorque = motor;
        wheelFR.motorTorque = motor;

        // 煞車
        if (Input.GetKey(KeyCode.Space))
        {
            wheelFL.brakeTorque = brakeTorque;
            wheelFR.brakeTorque = brakeTorque;
            wheelBL.brakeTorque = brakeTorque;
            wheelBR.brakeTorque = brakeTorque;
        }
        else
        {
            wheelFL.brakeTorque = 0f;
            wheelFR.brakeTorque = 0f;
            wheelBL.brakeTorque = 0f;
            wheelBR.brakeTorque = 0f;
        }
    }

    void SetupWheelFriction(WheelCollider wheel)
    {
        WheelFrictionCurve forwardFriction = wheel.forwardFriction;
        forwardFriction.stiffness = 1.5f;
        wheel.forwardFriction = forwardFriction;

        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
        sidewaysFriction.stiffness = 2.0f;
        wheel.sidewaysFriction = sidewaysFriction;
    }

    void SetupSuspension(WheelCollider wheel)
    {
        JointSpring suspension = wheel.suspensionSpring;
        suspension.spring = 35000f;
        suspension.damper = 4500f;
        suspension.targetPosition = 0.5f;
        wheel.suspensionSpring = suspension;
    }

    void UpdateWheelVisuals(WheelCollider collider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);

        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    void SwitchToFirstPerson()
    {
        firstPersonCamera.enabled = true;
        thirdPersonCamera.enabled = false;
    }

    void SwitchToThirdPerson()
    {
        firstPersonCamera.enabled = false;
        thirdPersonCamera.enabled = true;
    }
}
