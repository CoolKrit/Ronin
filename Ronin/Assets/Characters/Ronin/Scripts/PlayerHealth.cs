using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    #region SerializeField Variables
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    #endregion

    public PlayerController playerController;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        playerController.animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        playerController.animator.SetBool("IsDead", true);

        playerController.rigidBody.simulated = false;
        playerController.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }
}
