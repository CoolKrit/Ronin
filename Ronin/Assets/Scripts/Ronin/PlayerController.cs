using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private string groundTag = "Ground";


    private Rigidbody2D rigidBody;
    private Animator animator;
    private Transform transforms;

    private int speedHash;
    private int yVelocityHash;
    private int isGroundedHash;
    private float gravityModifier;
    private bool isGrounded;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        transforms = GetComponent<Transform>();

        // Кэш параметров аниматора
        speedHash = Animator.StringToHash("Speed");
        yVelocityHash = Animator.StringToHash("yVelocity");
        isGroundedHash = Animator.StringToHash("IsGrounded");

        // Кэш модификатора гравитации
        gravityModifier = Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rigidBody.velocity = Vector2.up * jumpForce;
        }
        animator.SetFloat(yVelocityHash, rigidBody.velocity.y);
    }

    private void FixedUpdate()
    {
        float moveInput = Input.GetAxis("Horizontal");
        rigidBody.velocity = new Vector2(moveInput * speed, rigidBody.velocity.y);

        if (moveInput > 0 && transforms.localScale.x < 0 || moveInput < 0 && transforms.localScale.x > 0)
        {
            Flip();
        }

        ApplyGravityModifiers();

        animator.SetFloat(speedHash, Mathf.Approximately(moveInput, 0) ? 0 : 1);
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

    private void Flip()
    {
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = true;
            animator.SetBool(isGroundedHash, true);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = false;
            animator.SetBool(isGroundedHash, false);
        }
    }
}