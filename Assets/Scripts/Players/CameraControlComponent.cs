using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyCraft.Environment;
using MyCraft.Rendering;

namespace MyCraft.Players
{
    public class CameraControlComponent : MonoBehaviour
    {
        public CameraViewMode mode;

        private Transform cam => GetComponent<Player>().cam;
        private float mouseH;
        private float mouseV;

        public void Start()
        {
            mode = CameraViewMode.FirtPerson;
        }

        private void Update()
        {
            mouseH = Input.GetAxis("Mouse X");
            mouseV = Input.GetAxis("Mouse Y");

            if (Input.GetKeyUp(KeyCode.F5))
            {
                mode = (CameraViewMode)(((int)mode + 1) % 3);
                switch (mode)
                {
                    case CameraViewMode.FirtPerson:
                    // Need head object which seperated from player root object
                    case CameraViewMode.ThirdPersonRear:
                        cam.transform.localPosition = new Vector3(0, 2, 0);
                        cam.transform.localRotation = Quaternion.identity;
                        break;
                    case CameraViewMode.ThirdPerson:
                        cam.transform.localPosition = new Vector3(0, 0, 3);
                        cam.transform.localRotation = Quaternion.identity;
                        break;
                }
                Debug.Log($"CameraViewMode changed to: {mode}");
            }

            if (mouseV == 0 && mouseH == 0)
                return;

            switch (mode)
            {
                case CameraViewMode.FirtPerson:
                // Need head object which seperated from player root object
                case CameraViewMode.ThirdPersonRear:
                    transform.Rotate(Vector3.up * mouseH);
                    cam.Rotate(Vector3.right * -mouseV);
                    break;
                case CameraViewMode.ThirdPerson:
                    float clipRotation(float objRotation, float mouseV)
                    {
                        float res = mouseV;
                        if (objRotation < -35)
                            res = Math.Max(mouseV, 0);
                        else if (70 < objRotation)
                            res = Math.Min(mouseV, 0);
                        return res;
                    }

                    var camPos = cam.transform.position;

                    float rotation;
                    if (cam.eulerAngles.x <= 180f)
                        rotation = cam.eulerAngles.x;
                    else
                        rotation = cam.eulerAngles.x - 360f;

                    cam.RotateAround(transform.position + (Vector3.up * 2), Vector3.up, mouseH);
                    cam.RotateAround(transform.position + (Vector3.up * 2), Vector3.right, clipRotation(rotation, mouseV));
                    cam.LookAt(transform.position + (Vector3.up * 2));
                    break;
            }
        }
    }

    public enum CameraViewMode
    {
        FirtPerson,
        ThirdPersonRear,
        ThirdPerson,
    }
}
