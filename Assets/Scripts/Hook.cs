using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Hook : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if(playerMovement.State == PlayerState.HOOKING)
            playerMovement.hookCollided = true;
    }
}
