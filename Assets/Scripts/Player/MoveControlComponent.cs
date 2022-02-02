using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveControlComponent : MonoBehaviour
{
    protected Player player;
    protected World world;

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
    public bool IsGroundNow { get; protected set; }

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
        calcVelocity();
        if (JumpRequest) Jump();
        rotatePlayerView();

        transform.Translate(velocity, Space.Self);
    }

    protected void calcVelocity()
    {
        void calcHotizontal()
        {
            velocity = (Vector3.forward * moveV + Vector3.right * moveH) * Time.fixedDeltaTime;
            velocity *= IsSprintNow ? SprintSpeed : WalkSpeed;
        }
        void calcVertical()
        {
            verticalMomentum += verticalMomentum > Gravity ? (Time.fixedDeltaTime * Gravity) : 0;
            velocity += Vector3.up * Time.fixedDeltaTime * verticalMomentum;
        }
        void checkAndRefineCollidingBlock()
        {
            void refineHorizontal(Vector3 pos)
            {
                Block right = world.GetBlock(pos + Vector3.right * player.Width / 2);
                Block left = world.GetBlock(pos + Vector3.left * player.Width / 2);
                Block forward = world.GetBlock(pos + Vector3.forward * player.Width / 2);
                Block back = world.GetBlock(pos + Vector3.back * player.Width / 2);

                if ((velocity.x > 0 && right.IsSolid) || (velocity.x < 0 && left.IsSolid))
                    velocity.x = 0;
                if ((velocity.z > 0 && forward.IsSolid) || (velocity.z < 0 && back.IsSolid))
                    velocity.z = 0;
            }
            bool checkVertical(Vector3 pos, float speed)
            {
                float wh = player.Width / 2;

                return world.GetBlock(new Vector3(pos.x - wh, pos.y + speed, pos.z - wh)).IsSolid ||
                    world.GetBlock(new Vector3(pos.x + wh, pos.y + speed, pos.z - wh)).IsSolid ||
                    world.GetBlock(new Vector3(pos.x + wh, pos.y + speed, pos.z + wh)).IsSolid ||
                    world.GetBlock(new Vector3(pos.x - wh, pos.y + speed, pos.z + wh)).IsSolid;
            }

            // Players height is 2
            refineHorizontal(transform.position);
            refineHorizontal(transform.position + Vector3.up);

            if (velocity.y > 0 && checkVertical(transform.position + Vector3.up * 2, velocity.y))
            {
                velocity.y = 0;
            }
            else if (velocity.y < 0)
            {
                IsGroundNow = checkVertical(transform.position, velocity.y);
                velocity.y = IsGroundNow ? 0 : velocity.y;
            }
        }

        calcHotizontal();
        calcVertical();
        checkAndRefineCollidingBlock();
    }

    protected void rotatePlayerView()
    {
        transform.Rotate(Vector3.up * mouseH);
        player.cam.Rotate(Vector3.right * -mouseV);
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

    public void Jump()
    {
        verticalMomentum = JumpForce;
        JumpRequest = false;
    }
}