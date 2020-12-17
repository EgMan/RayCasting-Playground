using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.DownArrow))
        {
            transform.Rotate(new Vector3(.5f, 0, 0));
        }
        if (Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.UpArrow))
        {
            transform.Rotate(new Vector3(-.5f, 0, 0));
        }
        if (Input.GetKey(KeyCode.H) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(new Vector3(0, -.5f, 0));
        }
        if (Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(new Vector3(0, .5f, 0));
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(transform.forward * 0.25f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Quaternion.Euler(0, 90, 0) * -transform.forward * 0.25f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(-transform.forward * 0.25f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Quaternion.Euler(0, 90, 0) * transform.forward * 0.25f);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            transform.Translate(Vector3.up * 0.25f);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.Translate(Vector3.down * 0.25f);
        }
    }
}