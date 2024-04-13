using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
        if (playerMovement.State != PlayerState.JUMPING && playerMovement.State != PlayerState.DASHING
            && playerMovement.State != PlayerState.HOOKED)
        {
            ContactPoint2D[] contacts = new ContactPoint2D[1];
            int x = other.GetContacts(contacts);
            Invoke("ChangePlayerStateFalling", 0.05f);
        }

    }
    
    void ChangePlayerStateFalling()
    {
        playerMovement.ChangePlayerState(PlayerState.FALLING);
    }
    
}
