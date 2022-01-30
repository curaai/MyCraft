using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveControlComponent : MonoBehaviour
{
    public Player player;
    public World world;

    private float moveH;
    private float moveV;
    private float mouseH;
    private float mouseV;
    private Vector3 velocity;

    public float WalkSpeed = 3f;
    public float SprintSpeed = 6f;
    public float JumpForce = 5f;
    protected float verticalMomentum = 0f;
    public static readonly float Gravity = -9.8f;

    protected bool JumpRequest;


    public bool IsJumpNow { get; protected set; }
    public bool IsSprintNow { get; protected set; }
    public bool IsGroundNow
    {
        get
        {
            float wh = player.Width / 2;
            return (world.GetBlock(new Vector3(transform.position.x - wh, transform.position.y + velocity.y, transform.position.z - wh))?.IsSolid ??
                world.GetBlock(new Vector3(transform.position.x + wh, transform.position.y + velocity.y, transform.position.z - wh))?.IsSolid ??
                world.GetBlock(new Vector3(transform.position.x + wh, transform.position.y + velocity.y, transform.position.z + wh))?.IsSolid ??
                world.GetBlock(new Vector3(transform.position.x - wh, transform.position.y + velocity.y, transform.position.z + wh))?.IsSolid) ?? false;
        }
    }

    public void Start()
    {
        player = GetComponent<Player>();
        world = GameObject.Find("World").GetComponent<World>();
    }

    private void Update()
    {
        FetchPlayerInput();
    }

    private void FixedUpdate()
    {
        // Horizontal Move
        velocity = (Vector3.forward * moveV + Vector3.right * moveH) * Time.fixedDeltaTime;
        velocity *= IsSprintNow ? SprintSpeed : WalkSpeed;

        // Vertical Move
        verticalMomentum += verticalMomentum > Gravity ? Time.fixedDeltaTime * Gravity : 0;
        velocity += Vector3.up * Time.fixedDeltaTime * verticalMomentum;

        // todo: Check Colliding block 
        // ex) if ((velocity.x > 0 && world.GetBlock(transform.position + Vector3.left * player.Width / 2).IsSolid))
        velocity.y = IsGroundNow ? 0 : velocity.y;

        // Camera Control
        transform.Rotate(Vector3.up * mouseH);
        player.cameraTransform.Rotate(Vector3.right * -mouseV);

        transform.Translate(velocity, Space.Self);
    }

    public void FetchPlayerInput()
    {
        moveH = Input.GetAxis("Horizontal");
        moveV = Input.GetAxis("Vertical");
        mouseH = Input.GetAxis("Mouse X");
        mouseV = Input.GetAxis("Mouse Y");

        IsSprintNow = Input.GetButton("Sprint");
        if (IsGroundNow && Input.GetButton("Jump"))
            JumpRequest = true;
    }
}