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
    [SerializeField] private int damageAmount = 5;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] public int currentHealth;
    [SerializeField] private Vector2 attackRange = new Vector2(1f, 1f);
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] private LayerMask enemyLayer;
    #endregion

    #region Private Variables
    [HideInInspector] public Rigidbody2D rigidBody;
    [HideInInspector] public Animator animator;
    private BoxCollider2D boxCollider;
    private bool isAttacking;
    private bool isWalking;
    private bool isJumping;
    private enum State { Idle, Walking, Jumping, Attacking };
    private State currentState;
    #endregion

    #region Animator Hash Fields
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int yVelocityHash = Animator.StringToHash("yVelocity");
    private readonly int isGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int isAttackingHash = Animator.StringToHash("IsAttacking");
    #endregion

    #region Physics Fields
    private float gravityModifier;
    #endregion

    #region Unity Methods
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        gravityModifier = Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
    }

    void Update()
    {
        HandleJump();
        HandleAttack();
        animator.SetFloat(yVelocityHash, rigidBody.velocity.y);
    }

    private void FixedUpdate()
    {
        HandleMovement();
        ApplyGravityModifiers();
        HandleAnimation();
    }
    #endregion

    #region Custom Methods
    private void HandleJump()
    {
        if (IsGrounded() && !isAttacking)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isJumping = true;
                rigidBody.velocity = Vector2.up * jumpForce;
                currentState = State.Jumping;
            }
        }
    }

    private void HandleAttack()
    {
        if (!isAttacking && IsGrounded() && !isWalking && !isJumping)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                isAttacking = true;
                animator.SetBool(isAttackingHash, true);
                currentState = State.Attacking;
            }
        }
    }

    private void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackRange, 0f, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyBehaviour>().TakeDamage(damageAmount);
        }
    }

    private void HandleMovement()
    {
        if (!isAttacking)
        {
            float moveInput = Input.GetAxisRaw("Horizontal");
            rigidBody.velocity = new Vector2(moveInput * speed, rigidBody.velocity.y);
            if (moveInput > 0 && transform.localScale.x < 0 || moveInput < 0 && transform.localScale.x > 0)
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
        else if (rigidBody.velocity.y > 0 && !Input.GetKey(KeyCode.Space) && !IsGrounded())
        {
            rigidBody.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void HandleAnimation()
    {
        if (isAttacking)
        {
            return;
        }

        if (IsGrounded())
        {
            isJumping = false;
            currentState = isWalking ? State.Walking : State.Idle;
        }
        else
        {
            isJumping = true;
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
        currentState = IsGrounded() ? State.Idle : State.Jumping;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        animator.SetBool("IsDead", true);

        rigidBody.simulated = false;
        enabled = false;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }

    public void Flip()
    {
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireCube(attackPoint.position, attackRange);
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }
    #endregion
}