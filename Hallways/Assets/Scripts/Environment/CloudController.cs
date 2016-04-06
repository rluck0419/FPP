using UnityEngine;
using System.Collections;

public class CloudController : MonoBehaviour 
{
    void Update() 
    {
        transform.Rotate(Vector3.right * (Time.deltaTime / 4));
        transform.Rotate(Vector3.up * (Time.deltaTime / 4), Space.World);
    }
}