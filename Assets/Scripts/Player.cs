using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float Width = 0.6f;
    public Transform cameraTransform;

    public void Start()
    {
        cameraTransform = GameObject.Find("Main Camera").transform;
    }
}