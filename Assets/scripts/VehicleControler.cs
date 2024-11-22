using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleController : MonoBehaviour
{
    private float accelerationInput;
    private float currentTurnInput;
    private float targetTurnInput;
    public float maxTurnAngle = 25f;
    public float handBrake = 5000f;
    private Vector3 currentVelocity;
    private bool isHandBrakePressed = false;

    public Rigidbody carBody;
    public float carHorsePower = 400f;

    [Header("Nitro Settings")]
    public float nitroBoostMultiplier = 2f;   
    public float nitroDuration = 3f;          
    public float nitroCooldown = 5f;          
    private bool isNitroActive = false;
    private bool nitroOnCooldown = false;

    [Header("Wheel Colliders")]
    public WheelCollider wc_FrontLeft;
    public WheelCollider wc_FrontRight;
    public WheelCollider wc_BackLeft;
    public WheelCollider wc_BackRight;

    [Header("Jump Settings")]
    public float jumpForce = 10f;

    void Start()
    {
        carBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        currentVelocity = carBody.velocity;

        accelerationInput = Input.GetAxis("Vertical");
        targetTurnInput = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isHandBrakePressed = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            isHandBrakePressed = false;
        }

        if (Input.GetKeyDown(KeyCode.K) && GroundCheck())
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !nitroOnCooldown)
        {
            StartCoroutine(ActivateNitro());
        }

        bool GroundCheck()
        {
            return Physics.Raycast(transform.position, transform.up * -1f, 1.1f);
        }
    }

    private void Jump()
    {
        carBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        Debug.Log("Car Jumped!!");
    }

    private void FixedUpdate()
    {
        Vector3 combinedInput = transform.forward * accelerationInput * -1;
        float dotProduct = Vector3.Dot(currentVelocity.normalized, combinedInput);

        if (dotProduct < 0)
        {
            wc_BackLeft.motorTorque = 0;
            wc_BackRight.motorTorque = 0;
            wc_FrontLeft.motorTorque = 0;
            wc_FrontRight.motorTorque = 0;

            wc_FrontLeft.brakeTorque = 1000f;
            wc_FrontRight.brakeTorque = 1000f;
            wc_BackLeft.brakeTorque = 0;
            wc_BackRight.brakeTorque = 0;
        }
        else
        {
            wc_FrontRight.brakeTorque = 0;
            wc_FrontLeft.brakeTorque = 0;
            wc_BackLeft.brakeTorque = 0;
            wc_BackRight.brakeTorque = 0;

            float finalHorsePower = accelerationInput * carHorsePower;
            if (isNitroActive)
            {
                finalHorsePower *= nitroBoostMultiplier;
            }

            wc_BackRight.motorTorque = finalHorsePower * -1;
            wc_BackLeft.motorTorque = finalHorsePower * -1;
        }

        if (isHandBrakePressed)
        {
            wc_BackLeft.brakeTorque = handBrake;
            wc_BackRight.brakeTorque = handBrake;

            wc_BackLeft.motorTorque = 0;
            wc_BackRight.motorTorque = 0;
        }
        else if (!isHandBrakePressed && dotProduct > 0) 
        {
            wc_BackLeft.brakeTorque = 0;
            wc_BackRight.brakeTorque = 0;
        }

        
        string keyPressed = accelerationInput > 0 ? "W" : accelerationInput < 0 ? "S" : "No Key Pressed";
        Debug.Log("Input = " + keyPressed + " ||| Velocity = " + currentVelocity.normalized + "||| Dot Product = " + dotProduct);

        
        currentTurnInput = ApproachTargetValueWithIncrement(currentTurnInput, targetTurnInput, 0.07f);
        wc_FrontLeft.steerAngle = currentTurnInput * maxTurnAngle;
        wc_FrontRight.steerAngle = currentTurnInput * maxTurnAngle;
    }

    private float ApproachTargetValueWithIncrement(float currentValue, float targetValue, float increment)
    {
        if (currentValue == targetValue)
        {
            return currentValue;
        }

        return currentValue < targetValue ? currentValue + increment : currentValue - increment;
    }

    private IEnumerator ActivateNitro()
    {
        isNitroActive = true;
        Debug.Log("Nitro Activated!");
        yield return new WaitForSeconds(nitroDuration);
        isNitroActive = false;
        nitroOnCooldown = true;
        Debug.Log("Nitro Deactivated. Cooling down...");
        yield return new WaitForSeconds(nitroCooldown);
        nitroOnCooldown = false;
        Debug.Log("Nitro Ready to use again!");
    }
}