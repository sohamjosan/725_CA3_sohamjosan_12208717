using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIVehicleController : MonoBehaviour
{
    public GameObject targetObject;
    private float distanceToTarget;

    

    private float appliedMotorTorque;
    private float appliedBrakeTorque;

    private float appliedTurnAngle;

    [Header("Wheel Colliders and Rigid Body")]
    public WheelCollider wc_fl;
    public WheelCollider wc_fr;
    public WheelCollider wc_rl;
    public WheelCollider wc_rr;
    public Rigidbody aiCarBody;

    // Update is called once per frame
    void Update()
    {
       

        // Calculate distance betweent the AI car and the target position
        // Keep in mind that transform.position in the line below refers to the position of this AI car
        distanceToTarget = Vector3.Distance(targetObject.transform.position, transform.position);

        // If target is less than 5m away, apply brakes
        if (distanceToTarget < 5)
        {
            appliedBrakeTorque = 1000f;
            appliedMotorTorque = 0f;
        }
        else // If the target is not close and is far away. This is the condition for accelerating forward or backward
        {
            // set applied brake torque to 0 as soon as we enter this else condition
            appliedBrakeTorque = 0;

            // Get the normalised direction towards the target
            Vector3 targetDirection = (targetObject.transform.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward * -1, targetDirection);

            if (dotProduct > 0) // if the object is in front accelerate forward
            {
                appliedMotorTorque = 1000;
            }
            // the else below is the condition for when the dot product is less than 0
            else
            {
                if (distanceToTarget > 20) // if the target is more than 20m away, do not reverse and turn around instead
                {
                    appliedMotorTorque = 1000;
                }
                else // if the object is less than 20m away, reverse the car
                {
                    appliedMotorTorque = -1000;
                }
            }

            // Calculate an angle between forward, targetDirection, with the Up-Axis as the pivot axis
            float targetAngle = Vector3.SignedAngle(transform.forward * -1, targetDirection, Vector3.up);

            // Convert the angle from degrees to radians
            float targetAngleRadians = targetAngle * Mathf.Deg2Rad;

            // Calculate the Sin to get a value between -1 and 1
            appliedTurnAngle = Mathf.Sin(targetAngleRadians) * 25;
        }
    }

    private void FixedUpdate()
    {
        wc_fl.motorTorque = appliedMotorTorque * -1;
        wc_fl.brakeTorque = appliedBrakeTorque;
        wc_fl.steerAngle = appliedTurnAngle;

        wc_fr.motorTorque = appliedMotorTorque * -1;
        wc_fr.brakeTorque = appliedBrakeTorque;
        wc_fr.steerAngle = appliedTurnAngle;
    }
}
