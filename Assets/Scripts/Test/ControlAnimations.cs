using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlAnimations : MonoBehaviour
{

    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            anim.SetBool("WalkForward", true);
        }
        else
        {
            anim.SetBool("WalkForward", false);
        }

        if (Input.GetKey(KeyCode.S))
        {
            anim.SetBool("WalkBackward", true);
        }
        else
        {
            anim.SetBool("WalkBackward", false);
        }

        if (Input.GetKey(KeyCode.A))
        {
            anim.SetBool("WalkLeft", true);
        }
        else
        {
            anim.SetBool("WalkLeft", false);
        }

        if (Input.GetKey(KeyCode.D))
        {
            anim.SetBool("WalkRight", true);
        }
        else
        {
            anim.SetBool("WalkRight", false);
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            anim.SetBool("Sprinting", true);
        }
        else
        { 
            anim.SetBool("Sprinting", false);
        }
        
        if (Input.GetKey(KeyCode.Mouse1))
        {
            anim.SetBool("Aiming", true);
        }
        else
        {
            anim.SetBool("Aiming", false);
        }
    }
    
}
