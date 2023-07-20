using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class player : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField]
    private float movespeed;
    [SerializeField]
    private float clamp_fallspeed;
    [SerializeField ]
    private float fallHeightDeath;

    [Header("Jump")]
    [SerializeField]
    private float maxJumpForce;
    [SerializeField]
    private float minJumpForce;
    [SerializeField]
    private float jumpChargeRate;

    [Header("etc")]
    [SerializeField]
    private Transform directionPointer;
    [SerializeField]
    private SpriteRenderer playerSprite;
    [SerializeField]
    private PhysicsMaterial2D bouncyMaterial;
    [SerializeField]
    private PhysicsMaterial2D frictionMaterial;
    [SerializeField]
    private Transform jumpBar;
    [SerializeField]
    private GameObject deathFX;
    [SerializeField]
    private Animator player_animation;

    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI ui_distanceTraveled;
    [SerializeField]
    private TextMeshProUGUI ui_deaths;

    [Header("Audio")]
    [SerializeField]
    private AudioSource sfx_jump;
    [SerializeField]
    private AudioSource sfx_bump;
    [SerializeField]
    private AudioSource sfx_death;

    private player_state current_state;
    private RaycastHit2D ground_boxCast; // stores boxcast player uses to detect is_grounded
    private Vector3 groundBoxCast_displacement; // position of boxCast (used to avoid repeating computation on update())
    private Vector2 collider_size; // stores players collider dimensions for reference
    private Vector2 checkpointPosition; // stores current checkpoint position (default is spawn location)
    private Rigidbody2D thisRigid;
    private bool isGrounded;
    private bool isJumping;
    private bool isChargingJump;
    private int direction;
    private int deathCount;
    private float distanceTraveled;
    private float jumpCurrentCharge;
    private string currentAnimationName;
    private int fallingHeightCounter;

    private enum player_state
    {
        idle,
        walk,
        rising,
        falling
    }

    void Start()
    {
        thisRigid = GetComponent<Rigidbody2D>();
        collider_size = GetComponent<Collider2D>().bounds.size;
        groundBoxCast_displacement = Vector3.down * (collider_size.y / 2 + 0.1f);
        isGrounded = false;
        checkpointPosition = transform.position;
        current_state = player_state.idle;
        currentAnimationName = "idle";
        deathCount = 0;
        distanceTraveled = transform.position.y;
    }

    void Update()
    {
        keyBoardControls();
        handle_bounceMechanics();
        update_animation();
    }

    private void FixedUpdate()
    {
        handle_groundCheck();
        handle_jumpPointer();
        handle_ui();
        handle_distanceTraveled();
        fallHeightCounter();
        movement();
    }

    private void keyBoardControls()
    {
       if (isGrounded)
        {
            if (Input.GetKey(KeyCode.A) && !isJumping)    // move left
            {
                change_direction(-1);
                thisRigid.sharedMaterial = null;
                current_state = player_state.walk;
            }
            else if (Input.GetKey(KeyCode.D) && !isJumping)    // move right
            {
                change_direction(1);
                thisRigid.sharedMaterial = null;
                current_state = player_state.walk;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) && !isJumping)      // jump
            {
                StartCoroutine(coroutine_chargeJump());
            }

            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && !isJumping && isGrounded)    // release movement
            {
                change_direction(0);
                thisRigid.sharedMaterial = frictionMaterial;
                current_state = player_state.idle;
            }
        }

       if (Input.GetKeyDown(KeyCode.Z))
        {
            do_suicide();
        }
    }

    private void movement()
    {
        if (thisRigid.velocity.y < clamp_fallspeed)
        {
            thisRigid.velocity = new Vector2(thisRigid.velocity.x, clamp_fallspeed);
        }

        if (thisRigid.velocity.y <= 0)
        {
            isJumping = false;
        }

        if (isGrounded && direction != 0 && !isChargingJump && !isJumping)   // if player is on ground and pressing movement buttons
        {
            thisRigid.velocity = new Vector2(movespeed * Time.deltaTime * direction, thisRigid.velocity.y);
        }

        if (!isGrounded)
        {
            if (thisRigid.velocity.y > 0)
            {
                current_state = player_state.rising;
            }
            if (thisRigid.velocity.y < 0)
            {
                current_state = player_state.falling;
            }

            if (thisRigid.velocity.x > 0)
            {
                change_direction(1);
            }
            else
            {
                change_direction(-1);
            }
        }
    }

    private void handle_bounceMechanics()
    {
        if (!isGrounded)
        {
            thisRigid.sharedMaterial = bouncyMaterial;
        }
        else if (direction == 0)
        {
            thisRigid.sharedMaterial = frictionMaterial;
        }
    }

    private void do_suicide()
    {
        thisRigid.velocity = Vector2.zero;
        deathFX.SetActive(true);
        playerSprite.enabled = false;
        StartCoroutine(coroutine_respawnDelay());
        enabled = false;
        sfx_death.Play();
        thisRigid.bodyType = RigidbodyType2D.Static;
        deathCount++;
    }
    private void handle_distanceTraveled()
    {
        if (transform.position.y > distanceTraveled)
        {
            distanceTraveled = transform.position.y;
        }
    }
    private void handle_ui()
    {
        ui_deaths.text = "" + deathCount;
        ui_distanceTraveled.text = "DIST: " + (int)(distanceTraveled * 50);
    }

    private void Jump()
    {
        sfx_jump.volume = (float)jumpCurrentCharge / (float)maxJumpForce;
        sfx_jump.Play();

        thisRigid.AddForce(directionPointer.right * jumpCurrentCharge);
        jumpCurrentCharge = 0;
        isJumping = true;
        isChargingJump = false;
        jumpBar.localScale = new Vector2(0,0.1f);

        if (directionPointer.localEulerAngles.z >= 90 && directionPointer.localEulerAngles.z <= 270)
            change_direction(-1);
        else
            change_direction(1);
    }

    private void fallHeightCounter()
    {
        if (!isGrounded)
        {
            fallingHeightCounter++;
        }
        else
        {
            fallingHeightCounter = 0;
        }
    }

    private void handle_jumpPointer()
    {
        Vector2 rotta = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        directionPointer.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(rotta.y, rotta.x) * Mathf.Rad2Deg);
    }

    private IEnumerator coroutine_chargeJump()
    {
        thisRigid.velocity = Vector2.zero;
        isChargingJump = true;
        jumpCurrentCharge = minJumpForce;

        while (!Input.GetKeyUp(KeyCode.Mouse0) && jumpCurrentCharge <= maxJumpForce)
        {
            if (jumpCurrentCharge < maxJumpForce)
            {
                jumpCurrentCharge += jumpChargeRate * Time.deltaTime * 100;
            }

            jumpBar.localScale = new Vector2((float)jumpCurrentCharge / (float)maxJumpForce, 0.1f);
            yield return null;
        }

        Jump();
    }

    private IEnumerator coroutine_respawnDelay()
    {
        yield return new WaitForSeconds(2.0f);
        transform.position = checkpointPosition;
        deathFX.SetActive(false);
        playerSprite.enabled = true;
        thisRigid.bodyType = RigidbodyType2D.Dynamic;
        enabled = true;
    }

    private void update_animation()
    {
        switch (current_state)
        {
            case player_state.idle:
                if (currentAnimationName != "idle")
                {
                    player_animation.CrossFade("p_idle", 0, 0);
                    currentAnimationName = "idle";
                }
                break;
            case player_state.walk:
                if (currentAnimationName != "walk")
                {
                    player_animation.CrossFade("p_walk", 0, 0);
                    currentAnimationName = "walk";
                }
                break;
            case player_state.rising:
                if (currentAnimationName != "rising")
                {
                    player_animation.CrossFade("p_rising", 0, 0);
                    currentAnimationName = "rising";
                }
                break;
            case player_state.falling:
                if (currentAnimationName != "falling")
                {
                    player_animation.CrossFade("p_falling", 0, 0);
                    currentAnimationName = "falling";
                }
                break;
        }
    }

    private void handle_groundCheck()
    {
        ground_boxCast = Physics2D.BoxCast(transform.position + groundBoxCast_displacement, new Vector2(collider_size.x - 0.05f, 0.1f), 0, Vector2.up, 0.1f);
        isGrounded = check_ifGrounded();
    }

    private bool check_ifGrounded()
    {
        if (ground_boxCast.collider != null && ground_boxCast.collider.tag == "wall" ) // layer 6 is wall layer
        {
            if (!isGrounded && fallingHeightCounter > fallHeightDeath)
            {
                do_suicide();
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    private void change_direction(int value)
    {
        direction = value;

        if (value == 1)     // flip player to direction pressed by player
        {
            playerSprite.flipX = false;
        }
        else if (value == -1)
        {
            playerSprite.flipX = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "checkpoint")
        {
            collision.GetComponent<checkpoint>().do_activateCheckpoint();
            checkpointPosition = collision.transform.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "wall")
        {
            sfx_bump.Play();
        }
    }
    private void OnValidate()
    {
        if (clamp_fallspeed > -1)
        {
            clamp_fallspeed = -1;
        }
    }
}
