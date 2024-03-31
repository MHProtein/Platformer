using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigidBody;

    void Start()
    {
        rigidBody.velocity = new Vector2(1.0f, 0.0f);
    }


    void Update()
    {
        
    }
}
