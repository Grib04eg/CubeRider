using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] ParticleSystem shieldParticle;
    [SerializeField] GameObject shieldPickupParticle;
    [SerializeField] GameObject scorePickupParticle;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] HingeJoint hingeJoint;
    [SerializeField] GameManager manager;
    [SerializeField] GameObject tutorial;
    Vector3 hingePoint;
    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    float attraction = 0f;
    float line = 0f;
    bool everythingUnderControl = true;
    void Update()
    {
        if (!everythingUnderControl)
            return;
        if (Input.GetMouseButtonDown(0))
        {
            if (tutorial.activeSelf)
                tutorial.SetActive(false);
            RaycastHit hit;
            Ray ray = new Ray(transform.position, new Vector3(0.3f, 1, 0));
            if (Physics.Raycast(ray, out hit, 100, 1<<8))
            {
                attraction = 0f;
                line = 0f;
                hingeJoint.autoConfigureConnectedAnchor = true;
                hingeJoint.connectedBody = null;
                hingePoint = hit.point;
                hingeJoint.transform.position = hit.point;
                hingeJoint.connectedAnchor = hingePoint - transform.position;
                hingeJoint.connectedBody = rb;
                
            }
            //lineRenderer.enabled = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (hingeJoint)
                hingeJoint.connectedBody = null;
            //lineRenderer.enabled = false;
        }
        if (Input.GetMouseButton(0))
        {
            hingeJoint.autoConfigureConnectedAnchor = false;
            hingeJoint.connectedAnchor = Vector3.Lerp(hingeJoint.connectedAnchor, Vector3.zero, Time.deltaTime * attraction);
            attraction += Time.deltaTime;
            if (line < 1f)
                line += Time.deltaTime * 7f;
        }
        else
            line -= Time.deltaTime * 7f;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, Vector3.Lerp(transform.position, hingePoint, Mathf.Clamp01(line)));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == 8)
        {
            if (shieldUp)
                StartCoroutine(DisableShield());
            else
            {
                everythingUnderControl = false;
                manager.Lose();
            }
        }
    }
    bool shieldUp = false;
    IEnumerator DisableShield()
    {
        yield return new WaitForSeconds(0.5f);
        shieldParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        shieldUp = false;
    }
    int bonusScore = 0;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "powerup")
        {
            shieldUp = true;
            Instantiate(shieldPickupParticle, other.transform.position, Quaternion.identity);
            Destroy(other.gameObject);
            shieldParticle.Play();
        } else if (other.tag == "score")
        {
            manager.AddBonusScore(500);
            Destroy(other.gameObject);
            Instantiate(scorePickupParticle, other.transform.position, Quaternion.identity);
        }
    }
}
