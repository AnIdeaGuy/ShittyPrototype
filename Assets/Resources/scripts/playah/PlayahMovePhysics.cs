using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayahMovePhysics : MonoBehaviour
{
    public bool flightMode = false;

    Rigidbody2D rigidbod;
    DistanceJoint2D joint;
    Rigidbody2D grappleTarget = null;
    private const float ACCELERATION = .2f;
    private const float SPEED_MAX = 1.5f;
    int facingDirection = 1;
    Vector2 velocity;
    float jumpVelocity = 0;
    bool isGrounded = false;
    bool isGrappling = false;
    int wallWalkingSide = 0;
    float grappleSwing = 0;
    float inputTimeOut = 0;
    int moveDir = 0;
    Vector2 spriteSize;
    const float TIME_OUT_MAX = .2f;
    const float SWING_START = 20.0f;
    const float JUMP_SPEED = 4.0f;
    const float DECREASE_JUMP = .8f;
    const float MAX_GRAPPLE_DISTANCE = 1;
    const float DESCEND_SPEED = .1f;

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
        Vector2 position = transform.position;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.point.y <= position.y - spriteSize.y / 2)
            {
                isGrounded = true;
                jumpVelocity = 0;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Vector2 position = transform.position;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.point.y > position.y - spriteSize.y / 2)
            {
                if (contact.point.x < position.x - spriteSize.x / 2 && moveDir < 0)
                    wallWalkingSide = -1;
                else if (contact.point.x > position.x + spriteSize.x / 2 && moveDir > 0)
                    wallWalkingSide = 1;
            }
        }
    }

    private void HandleGrapple()
    {
        if (Input.GetButton("Grapple"))
        {
            if (grappleTarget != null)
            {
                if (!isGrappling)
                {
                    float d = Vector2.Distance(transform.position, grappleTarget.position);
                    joint.distance = d;
                }
                isGrappling = true;
                joint.enabled = true;
                joint.connectedBody = grappleTarget;
            }
        }

        if (Input.GetButtonUp("Grapple") && isGrappling)
        {
            joint.enabled = false;
            Vector2 mahVelociteh = rigidbod.velocity;
            mahVelociteh.y = 3;
            rigidbod.velocity = mahVelociteh;
            isGrappling = false;
        }

        if (isGrappling)
        {
            if (joint.distance > MAX_GRAPPLE_DISTANCE)
                joint.distance = MAX_GRAPPLE_DISTANCE;
        }
    }

    private void HandleJumping()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
                jumpVelocity = JUMP_SPEED;
            if (wallWalkingSide != 0)
                JumpOffWall();
        }

        if (!isGrounded && !isGrappling)
        {
            if (!Input.GetButton("Jump"))
            {
                DoJumpDecrease();
            }


            if (Input.GetButton("Jump") && flightMode)
            {
                if (velocity.y + jumpVelocity < -DESCEND_SPEED)
                {
                    jumpVelocity = 0;
                    velocity.y = -DESCEND_SPEED;
                }
            }
        }
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();
        RaycastHit2D[][] hitsArr = new RaycastHit2D[3][];
        for (int i = 0; i < 3; i++)
            hitsArr[i] = new RaycastHit2D[4];

        int hitnum = Physics2D.Raycast(transform.position, Vector2.down, filter, hitsArr[0], spriteSize.y / 2 + .05f);
        hitnum += Physics2D.Raycast((Vector2)(transform.position) + Vector2.right * (spriteSize.x / 2), Vector2.down, filter, hitsArr[1], spriteSize.y / 2 + .05f);
        hitnum += Physics2D.Raycast((Vector2)(transform.position) + Vector2.left * (spriteSize.x / 2), Vector2.down, filter, hitsArr[2], spriteSize.y / 2 + .05f);
        bool gotHit = false;
        if (hitnum > 0)
            gotHit = RaycastCollision(hitsArr);
        isGrounded = gotHit;
    }

    private bool RaycastCollision(RaycastHit2D[][] hitsArr)
    {
        foreach (RaycastHit2D[] hits in hitsArr)
            foreach (RaycastHit2D hit in hits)
                if (hit && hit.collider.tag == "Ground")
                    return true;
        return false;
    }

    private bool RaycastCollision(RaycastHit2D[] hits)
    {
        foreach (RaycastHit2D hit in hits)
            if (hit && hit.collider.tag == "Ground")
                return true;
        return false;
    }

    private void DoJumpDecrease()
    {
        jumpVelocity *= DECREASE_JUMP;
        if (jumpVelocity < .1f)
            jumpVelocity = 0;
    }

    private void HandleInputAndMovement()
    {
        
        int lastFacingDirection = facingDirection;
        int lastDir = moveDir;
        if (inputTimeOut <= 0)
            moveDir = (int)Mathf.Round(Input.GetAxisRaw("Horizontal"));

        velocity.x += moveDir * ACCELERATION;
        if (isGrappling)
        {
            if (lastDir == 0 && moveDir != 0)
                grappleSwing = SWING_START * moveDir;
            rigidbod.AddForce(Vector2.left * -grappleSwing);
            grappleSwing *= .8f;
            if (Mathf.Abs(grappleSwing) < .1f)
            {
                grappleSwing = 0;
            }
        }
        if (Mathf.Abs(velocity.x) > SPEED_MAX)
            velocity.x = SPEED_MAX * Mathf.Sign(velocity.x);
        if (moveDir == 0)
        {
            if (isGrounded)
                velocity.x *= .8f;
        }
        else
            facingDirection = moveDir;

        if (facingDirection != lastFacingDirection && !isGrounded)
            velocity.x = facingDirection * ACCELERATION;

        if (Mathf.Abs(velocity.x) < .1f)
            velocity.x = 0;

        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();
        RaycastHit2D[] hits = new RaycastHit2D[4];
        Physics2D.Raycast(transform.position, Vector2.right * wallWalkingSide, filter, hits, 1);
        if (moveDir == wallWalkingSide && moveDir != 0)
        {
            bool gotHit = RaycastCollision(hits);
            if (gotHit)
            {
                velocity.y = 1.5f;
            }
            else
                FallOffWall();
        }
        else
        {
            if (wallWalkingSide != 0)
                FallOffWall();
        }

        inputTimeOut -= Time.deltaTime;
        if (inputTimeOut < 0)
            inputTimeOut = 0;
    }

    private void FallOffWall()
    {
        inputTimeOut = TIME_OUT_MAX;
        velocity.x = -wallWalkingSide;
        wallWalkingSide = 0;
        moveDir = 0;
    }

    private void JumpOffWall()
    {
        inputTimeOut = TIME_OUT_MAX;
        velocity.x = -wallWalkingSide * 3;
        jumpVelocity = JUMP_SPEED;
        wallWalkingSide = 0;
        moveDir = -moveDir;
    }
}
