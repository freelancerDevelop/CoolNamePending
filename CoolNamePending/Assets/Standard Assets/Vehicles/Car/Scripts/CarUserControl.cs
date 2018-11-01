using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using SharpDX.DirectInput;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof (CarController))]
    public class CarUserControl : MonoBehaviour
    {
        public bool Controller;
        public bool SteeringWheel;
        public bool Pedals;
        public Transform SteeringWheelMesh;
        private CarController m_Car; // the car controller we want to use
        private static int SteerMultiplier = 450;

        private void Awake()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
        }


        private void FixedUpdate()
        {

            float h;
            // pass the input to the car!
            if (Controller)
            {
                float v = Input.GetAxis("Right Trigger") - Input.GetAxis("Left Trigger");
                h = Input.GetAxis("Left Joystick");
                m_Car.Move(h, v, v, 0);
            }
            else if (SteeringWheel)
            {
                h = Input.GetAxis("SteeringWheel"); // -1 to 1
                
                if (Pedals)
                {
                    float footbrake = Input.GetAxis("Footbrake"); // -1 to 1 => 0 to 1
                    footbrake = (footbrake + 1) / 2;
                    float v = Input.GetAxis("Accelerator"); // -1 to 1 => 0 to 1
                    v = (v + 1) / 2;
                    //print(v + " " + h + " " + footbrake);
                    m_Car.Move(h, v, footbrake, 0);
                }
                else
                {
                    
                }
                
            }
            else
            {
                h = CrossPlatformInputManager.GetAxis("Horizontal");
                float v = CrossPlatformInputManager.GetAxis("Vertical");
                m_Car.Move(h, v, v, 0);
            }

            SteeringWheelMesh.eulerAngles = new Vector3(SteeringWheelMesh.eulerAngles.x, SteeringWheelMesh.eulerAngles.y, h * SteerMultiplier);
        }
    }
}
