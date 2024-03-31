using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerMovement.State == PlayerState.FALLING || playerMovement.State == PlayerState.JUMPING)
        {
            if (playerMovement.velocity.x == 0.0f)
                playerMovement.ChangePlayerState(PlayerState.IDLE);
            else
                playerMovement.ChangePlayerState(PlayerState.RUNNING);
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (playerMovement.State == PlayerState.FALLING || playerMovement.State == PlayerState.JUMPING)
        {
            if (playerMovement.velocity.x == 0.0f)
                playerMovement.ChangePlayerState(PlayerState.IDLE);
            else
                playerMovement.ChangePlayerState(PlayerState.RUNNING);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ContactPoint2D[] contacts = new ContactPoint2D[1];
        int x = other.GetContacts(contacts);
        if(playerMovement.State != PlayerState.JUMPING && playerMovement.State != PlayerState.DASHING)
            playerMovement.ChangePlayerState(PlayerState.FALLING);
    }
}
