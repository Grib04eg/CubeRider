using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motor : MonoBehaviour
{
    [SerializeField] Vector3 axisRotation;
    void Update()
    {
        transform.Rotate(axisRotation * Time.deltaTime);
    }
}
