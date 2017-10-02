using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayahMove : MonoBehaviour
{
    public const float GRAVITY = .2f;
    private float downVelocity = 0;
    private float upVelocity = 0;
    private const float JUMP_SPEED = 4.0f;
    private float horizontalVelocity = 0;
    private float velocityAngle = 0;
    private float grappleTorque = 0;
    private const float ACCELERATION = .1f;
    private const float SPEED_MAX = 1.0f;
    private int facingDirection = 1;
    private bool isJumping = false;
    private bool isGrounded = false;
    private bool isGrappling = false;

    private GameObject grappleTarget = null;
    private float grappleRadius = 0;
    private float grappleAngle = 0;

    private SpriteRenderer sprite;
    private Vector2 position;
    private Vector2 spriteSize;

	void Start ()
    {
        sprite = GetComponent<SpriteRenderer>();
        spriteSize = sprite.bounds.size;
	}
	
	void Update ()
    {
        position = transform.position;
        if (!isGrappling)
        {
            HandleMovement();
            HandleJumping();
            HandleGravity();
        }
        HandleGrappling();
        //CheckCollisionDown();
        CheckCollisionSide();
        transform.position = position;
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Grapple Target")
            grappleTarget = collision.gameObject;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == grappleTarget)
            grappleTarget = null;
    }

    private void HandleGrappling()
    {
        if (grappleTarget != null)
        {
            Vector2 otherPosition = grappleTarget.transform.position;
            if (Input.GetButtonDown("Grapple"))
            {
                isGrappling = true;
            }

            if (Input.GetButtonUp("Grapple"))
            {
                isGrappling = false;
                
            }
        }
    }

    private void HandleJumping()
    {

        /*if (Input.GetButtonDown("Jump") && isGrounded)
            upVelocity = JUMP_SPEED;

        if (Input.GetButtonUp("Jump"))
        {
            float verticalness = upVelocity - downVelocity;
            if (verticalness > 0)
            {
                upVelocity = verticalness;
                downVelocity = 0;
            }
        }

        if (!Input.GetButton("Jump"))
        {
            if (!isGrounded && upVelocity - downVelocity > 0)
            {
                if (upVelocity > .9f)
                    upVelocity *= .8f;
                else
                    upVelocity = 0;
            }
        }*/
    }

    private void HandleMovement()
    {
        int lastFacingDirection = facingDirection;
        int facing = (int) Mathf.Round(Input.GetAxisRaw("Horizontal"));
        horizontalVelocity += facing * ACCELERATION;
        if (Mathf.Abs(horizontalVelocity) > SPEED_MAX)
            horizontalVelocity = SPEED_MAX * Mathf.Sign(horizontalVelocity);
        if (facing == 0)
            horizontalVelocity *= .8f;
        else
            facingDirection = facing;

        if (facingDirection != lastFacingDirection)
            horizontalVelocity = facingDirection * ACCELERATION;

        if (Mathf.Abs(horizontalVelocity) < .1f)
            horizontalVelocity = 0;
        position.x += horizontalVelocity * Time.deltaTime;
    }

    private void CheckCollisionSide()
    {
        float newX = position.x;
        RaycastHit2D[] hitsTop = new RaycastHit2D[4];
        RaycastHit2D[] hitsBottom = new RaycastHit2D[4];
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();
        Physics2D.Raycast(new Vector2(position.x, position.y + spriteSize.y / 2), -Vector2.left * facingDirection, filter, hitsTop, spriteSize.x / 2 + Mathf.Abs(horizontalVelocity) * Time.deltaTime);
        Physics2D.Raycast(new Vector2(position.x, position.y - spriteSize.y / 2), -Vector2.left * facingDirection, filter, hitsBottom, spriteSize.x / 2 + Mathf.Abs(horizontalVelocity) * Time.deltaTime);
        foreach (RaycastHit2D[] hits in new RaycastHit2D[][] { hitsTop, hitsBottom })
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];
                if (hit && hit.point.y > position.y - spriteSize.y / 2 && hit.collider.tag != "Player" && hit.collider.tag != "Grapple Target")
                {
                    newX = hit.point.x + spriteSize.x / 2;
                    position.x = newX;
                    horizontalVelocity = 0;
                }
            }
    }

    private void CheckCollisionDown()
    {
        float newY = position.y;
        RaycastHit2D[] hitsL = new RaycastHit2D[4];
        RaycastHit2D[] hitsM = new RaycastHit2D[4];
        RaycastHit2D[] hitsR = new RaycastHit2D[4];
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();
        Physics2D.Raycast(new Vector2(position.x + spriteSize.x / 2, position.y), -Vector2.up, filter, hitsL, spriteSize.y / 2 - GetVerticalVelocity() * Time.deltaTime);
        Physics2D.Raycast(new Vector2(position.x - spriteSize.x / 2, position.y), -Vector2.up, filter, hitsR, spriteSize.y / 2 - GetVerticalVelocity() * Time.deltaTime);
        Physics2D.Raycast(new Vector2(position.x, position.y), -Vector2.up, filter, hitsR, spriteSize.y / 2 - GetVerticalVelocity() * Time.deltaTime);
        bool hitAnything = false;
        foreach (RaycastHit2D[] hits in new RaycastHit2D[][]{hitsL, hitsM, hitsR})
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            
            if (hit && hit.collider.tag != "Player" && hit.collider.tag != "Grapple Target")
            {
                newY = hit.point.y + spriteSize.y / 2;
                position.y = newY;
                upVelocity = 0;
                isGrounded = true;
                hitAnything = true;
            }
        }
        if (!hitAnything)
            isGrounded = false;
    }

    private void HandleGravity()
    {
        position.y += GetVerticalVelocity() * Time.deltaTime;
        if (isGrounded)
            downVelocity = 0;
        else
            downVelocity += GRAVITY;
    }

    private float GetVerticalVelocity()
    {
        return upVelocity - downVelocity;
    }
}
