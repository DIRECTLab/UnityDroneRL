using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//drones built by following: https://www.youtube.com/watch?v=3R_V4gqTs_I&list=PLPAgqhxd1Ib1YYqYnZioGyrSUzOwead17&index=1

public class DroneMovement : MonoBehaviour
{
    Rigidbody DroneChasis;

    private void Awake()
    {
        DroneChasis = GetComponent<Rigidbody>();
    }

    float upForce;
    private void FixedUpdate()
    {
        MovementUpDown();
        DroneChasis.AddRelativeForce(Vector3.up * upForce);
    }

    void MovementUpDown() {

        if (Input.GetKey(KeyCode.I))
        {
            upForce = 450;
        }
        else if (Input.GetKey(KeyCode.K))
        {
            upForce = -200;
        }
        else { 
            upForce = 98.1f;
        }

    }
}
