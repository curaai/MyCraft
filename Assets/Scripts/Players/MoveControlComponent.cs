using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Players
{
    public class MoveControlComponent : MonoBehaviour
    {
        public static readonly float Gravity = -9.8f;

        public float WalkSpeed = 3f;
        public float SprintSpeed = 6f;
        public float JumpForce = 5f;

        protected Player player;
        protected World world;

        private float moveH;
        private float moveV;
        private float mouseH;
        private float mouseV;
        private Vector3 velocity;
        protected float verticalMomentum;
        protected bool JumpRequest;

        public bool IsJumping { get; protected set; }
        public bool IsSprinting { get; protected set; }
        public bool IsGrounding { get; protected set; }

        public void Start()
        {
            player = GetComponent<Player>();
            world = GameObject.Find("World").GetComponent<World>();

            verticalMomentum = 0f;
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
                velocity *= IsSprinting ? SprintSpeed : WalkSpeed;
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
                    bool right = world.IsSolidBlock(pos + Vector3.right * player.Width / 2);
                    bool left = world.IsSolidBlock(pos + Vector3.left * player.Width / 2);
                    bool forward = world.IsSolidBlock(pos + Vector3.forward * player.Width / 2);
                    bool back = world.IsSolidBlock(pos + Vector3.back * player.Width / 2);

                    if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
                        velocity.x = 0;
                    if ((velocity.z > 0 && forward) || (velocity.z < 0 && back))
                        velocity.z = 0;
                }
                bool checkVertical(Vector3 pos, float speed)
                {
                    float wh = player.Width / 2;

                    return world.IsSolidBlock(new Vector3(pos.x - wh, pos.y + speed, pos.z - wh)) ||
                        world.IsSolidBlock(new Vector3(pos.x + wh, pos.y + speed, pos.z - wh)) ||
                        world.IsSolidBlock(new Vector3(pos.x + wh, pos.y + speed, pos.z + wh)) ||
                        world.IsSolidBlock(new Vector3(pos.x - wh, pos.y + speed, pos.z + wh));
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
                    IsGrounding = checkVertical(transform.position, velocity.y);
                    velocity.y = IsGrounding ? 0 : velocity.y;
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

            IsSprinting = Input.GetButton("Sprint");
            if (IsGrounding && Input.GetButton("Jump"))
                JumpRequest = true;
        }

        public void Jump()
        {
            verticalMomentum = JumpForce;
            JumpRequest = false;
            IsJumping = true;
        }
    }
}