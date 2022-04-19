using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{

    PlayerHandler Player;
    PlayerMovement Movement;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        Player = GetComponent<PlayerHandler>();
        animator = GetComponent<Animator>();
        Movement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Movement.IsMoving)
        {
            animator.SetBool("IsWalking", true);
        }
        if (!Movement.IsMoving)
        {
            animator.SetBool("IsWalking", false);
        }
    }
    public void Attack()
    {
        animator.Play("Attack");
    }
}
