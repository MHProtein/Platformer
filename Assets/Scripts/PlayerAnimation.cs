using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    int PLAYER_IDLE = Animator.StringToHash("Player_Idle");
    int PLAYER_RUN = Animator.StringToHash("Player_Run");
    int PLAYER_JUMP = Animator.StringToHash("Player_Jump");
    int PLAYER_FALL = Animator.StringToHash("Player_Fall");
    int PLAYER_DANGLE = Animator.StringToHash("Player_Dangle");
    private Animator animator;
    private Rigidbody2D rigidbody;
    private bool isJumping = false;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponentInParent<Rigidbody2D>();
    }
    
    private void OnEnable()
    {
        PlayerMovement.OnStateChange += OnStateChange;
    }
    
    private void OnDisable()
    {
        PlayerMovement.OnStateChange -= OnStateChange;
    }

    private void OnStateChange(PlayerState newState)
    {
        switch (newState)
        {
            case PlayerState.IDLE:
            {
                animator.Play(PLAYER_IDLE);
                break;
            }
            case PlayerState.RUNNING:
            {
                animator.Play(PLAYER_RUN);
                break;
            }
            case PlayerState.FALLING:
            {
                animator.Play(PLAYER_FALL);
                break;
            }
            case PlayerState.JUMPING:
            {
                animator.Play(PLAYER_JUMP);
                isJumping = true;
                break;
            }
            case PlayerState.DANGLING:
            {
                animator.Play(PLAYER_DANGLE);
                break;
            }
        }
    }

    private void Update()
    {
        if (isJumping)
        {
            if (rigidbody.velocity.y < 0)
            {
                animator.Play(PLAYER_FALL);
                isJumping = false;
            }
        }
    }
}
