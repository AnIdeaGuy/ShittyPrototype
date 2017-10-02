using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayahMovePhysics : MonoBehaviour
{
    Rigidbody2D rigidbod;
    DistanceJoint2D joint;
    Rigidbody2D grappleTarget = null;
    private const float ACCELERATION = .2f;
    private const float SPEED_MAX = 1.5f;
    int facingDirection = 1;
    Vector2 velocity;
    float swingSpeed = 0;
    float jumpVelocity = 0;
    bool isGrounded = false;
    bool isGrappling = false;
    const float JUMP_SPEED = 4.0f;
    const float DECREASE_JUMP = .8f;
    Vector2 spriteSize;

	void Start ()
    {
        rigidbod = GetComponent<Rigidbody2D>();
        joint = GetComponent<DistanceJoint2D>();
        spriteSize = GetComponent<SpriteRenderer>().bounds.size;
    }
	
	void Update ()
    {
        bool wasGrappling = isGrappling;
        if (!isGrappling)
            velocity = rigidbod.velocity - Vector2.up * jumpVelocity;
        HandleJumping();
        HandleGrapple();
        HandleInputAndMovement();
        if (!wasGrappling)
            rigidbod.velocity = velocity + Vector2.up * jumpVelocity;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Grapple Target")
        {
            grappleTarget = collision.gameObject.GetComponent<Rigidbody2D>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Grapple Target")
            grappleTarget = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.point.y <= transform.position.y - spriteSize.y / 2)
            {
                isGrounded = true;
                jumpVelocity = 0;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    private void HandleGrapple()
    {
        if (Input.GetButton("Grapple"))
        {
            if (grappleTarget != null)
            {
                isGrappling = true;
                joint.enabled = true;
                joint.connectedBody = grappleTarget;
                swingSpeed *= .95f;
                if (swingSpeed < .1f)
                    swingSpeed = 0;
            }
        }
        if (Input.GetButtonUp("Grapple") && isGrappling)
        {
            joint.enabled = false;
            float angle = GetGrappleAngle();
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            //rigidbod.AddForce(direction * 100);
            isGrappling = false;
        }
    }

    private void HandleJumping()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
            jumpVelocity = JUMP_SPEED;

        if (!Input.GetButton("Jump") && !isGrounded && !isGrappling)
        {
            jumpVelocity *= DECREASE_JUMP;
            if (jumpVelocity < .1f)
                jumpVelocity = 0;
        }
    }

    private void HandleInputAndMovement()
    {
        
        int lastFacingDirection = facingDirection;
        int dir = (int)Mathf.Round(Input.GetAxisRaw("Horizontal"));

        velocity.x += dir * ACCELERATION;
        if (Mathf.Abs(velocity.x) > SPEED_MAX)
            velocity.x = SPEED_MAX * Mathf.Sign(velocity.x);
        if (dir == 0)
        {
            if (isGrounded)
                velocity.x *= .8f;
        }
        else
            facingDirection = dir;

        if (facingDirection != lastFacingDirection)
            velocity.x = facingDirection * ACCELERATION;

        if (Mathf.Abs(velocity.x) < .1f)
            velocity.x = 0;
    }

    private float GetGrappleAngle()
    {
        if (grappleTarget != null)
        {
            Vector2 posA = transform.position;
            Vector2 posB = grappleTarget.gameObject.transform.position;
            int direction = posA.x < posB.x ? -1 : 1;
            int direction2 = posA.y < posB.y ? -1 : 1;
            direction *= direction2;
            return Mathf.Atan2(posA.y - posB.y, posA.x - posB.x) + direction * 0;
        }
        return Mathf.Atan2(velocity.y, velocity.x);
    }
}
