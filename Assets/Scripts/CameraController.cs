using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform followObject;
    [SerializeField] Vector3 offset;
    [SerializeField] float speed = 7;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (followObject)
        {
            transform.position = Vector3.Lerp(transform.position, followObject.position + offset, Time.deltaTime * speed);
        }
    }
}
