using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region SerializeField Variables
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private string groundTag = "Ground";
    #endregion

    #region Private Variables
    private Rigidbody2D rigidBody;
    private Animator animator;
    private Transform transforms;

    private bool isGrounded;
    private bool isAttacking;
    private bool isWalking;
    private enum State { Idle, Walking, Jumping, Attacking };
    private State currentState;
    #endregion

    #region Animator Hash Fields
    // Хэш параметров аниматора
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int yVelocityHash = Animator.StringToHash("yVelocity");
    private readonly int isGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int isAttackingHash = Animator.StringToHash("IsAttacking");
    #endregion

    #region Physics Fields
    private float gravityModifier;
    #endregion

    #region Unity Methods
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        transforms = GetComponent<Transform>();
        currentState = State.Idle;

        gravityModifier = Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isAttacking)
        {
            rigidBody.velocity = Vector2.up * jumpForce;
        }
        animator.SetFloat(yVelocityHash, rigidBody.velocity.y);

        if (Input.GetKeyDown(KeyCode.Mouse0) && !isAttacking && isGrounded && !isWalking)
        {
            isAttacking = true;
            animator.SetBool(isAttackingHash, true);
        }*/
        HandleJump();
        HandleAttack();
        animator.SetFloat(yVelocityHash, rigidBody.velocity.y);
    }

    private void FixedUpdate()
    {
        /*if (!isAttacking)
        {
            float moveInput = Input.GetAxisRaw("Horizontal");
            rigidBody.velocity = new Vector2(moveInput * speed, rigidBody.velocity.y);

            isWalking = true;
            if (moveInput > 0 && transforms.localScale.x < 0 || moveInput < 0 && transforms.localScale.x > 0)
            {
                Flip();
            }

            if (moveInput == 0)
            {
                isWalking = false;
            }

            ApplyGravityModifiers();

            animator.SetFloat(speedHash, Mathf.Approximately(moveInput, 0) ? 0 : 1);
        }*/
        HandleMovement();
        ApplyGravityModifiers();
        HandleAnimation();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = true;
            //animator.SetBool(isGroundedHash, true);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = false;
            //animator.SetBool(isGroundedHash, false);
        }
    }
    #endregion

    #region Custom Methods
    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isAttacking)
        {
            rigidBody.velocity = Vector2.up * jumpForce;
            currentState = State.Jumping;
        }
    }

    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !isAttacking && isGrounded && !isWalking)
        {
            isAttacking = true;
            animator.SetBool(isAttackingHash, true);
            currentState = State.Attacking;
        }
    }

    private void HandleMovement()
    {
        if (!isAttacking)
        {
            float moveInput = Input.GetAxisRaw("Horizontal");
            rigidBody.velocity = new Vector2(moveInput * speed, rigidBody.velocity.y);
            if (moveInput > 0 && transforms.localScale.x < 0 || moveInput < 0 && transforms.localScale.x > 0)
            {
                Flip();
            }
            isWalking = Mathf.Abs(moveInput) > 0;
        }
    }

    private void ApplyGravityModifiers()
    {
        if (rigidBody.velocity.y < 0)
        {
            rigidBody.velocity += Vector2.up * gravityModifier;
        }
        else if (rigidBody.velocity.y > 0 && !Input.GetKey(KeyCode.Space) && !isGrounded)
        {
            rigidBody.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void HandleAnimation()
    {
        if (isAttacking)
        {
            return;
        }
        if (isGrounded)
        {
            currentState = isWalking ? State.Walking : State.Idle;
        }
        else
        {
            currentState = State.Jumping;
        }
        switch (currentState)
        {
            case State.Idle:
                animator.SetFloat(speedHash, 0);
                animator.SetBool(isGroundedHash, true);
                break;
            case State.Walking:
                animator.SetFloat(speedHash, 1);
                animator.SetBool(isGroundedHash, true);
                break;
            case State.Jumping:
                animator.SetBool(isGroundedHash, false);
                break;
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        animator.SetBool(isAttackingHash, false);
        currentState = isGrounded ? State.Idle : State.Jumping;
    }

    private void Flip()
    {
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }
    #endregion
}