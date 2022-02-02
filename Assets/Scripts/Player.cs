using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float Width = 0.6f;
    public float Height = 2f;

    [HideInInspector]
    public Transform cam;

    public void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
    }
}