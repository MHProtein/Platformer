using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WallDetector : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerMovement.State != PlayerState.DANGLING)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                playerMovement.ChangePlayerState(PlayerState.DANGLING);
            }
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (playerMovement.State != PlayerState.DANGLING)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                playerMovement.ChangePlayerState(PlayerState.DANGLING);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(playerMovement.State != PlayerState.JUMPING)
            playerMovement.ChangePlayerState(PlayerState.FALLING);
    }
    
}
