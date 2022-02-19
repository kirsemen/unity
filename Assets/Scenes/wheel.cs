using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class wheel : MonoBehaviour
{
    public float MaxUpWeight = 0.3f;
    public float MinUpWeight = 0.3f;
    public float ForceSpring = 10000;
    public float ForceDamper = 10000;
    public float ShereRadius = 0.3f;
    private Vector3 StartPos = new Vector3(0, 0, 0);
    private bool GameIsStarted = false;

    private void OnDrawGizmosSelected()
    {

        if (GameIsStarted)
        {
            Gizmos.color = new Color(1, 0, 0, 1);
            Gizmos.DrawLine(transform.up * MaxUpWeight + transform.position + (StartPos - transform.localPosition).y * transform.up,
                transform.position + (StartPos - transform.localPosition).y * transform.up);

            Gizmos.color = new Color(1, 1, 0, 1);
            Gizmos.DrawLine(transform.position + (StartPos - transform.localPosition).y * transform.up,
                transform.up * -MinUpWeight + transform.position + (StartPos - transform.localPosition).y * transform.up);

            Gizmos.color = new Color(0, 0, 1, 1);
            Gizmos.DrawSphere(transform.position, 0.02f);
        }
        else
        {
            Gizmos.color = new Color(1, 0, 0, 1);
            Gizmos.DrawLine(transform.up * MaxUpWeight + transform.position, transform.position);
            Gizmos.color = new Color(1, 1, 0, 1);
            Gizmos.DrawLine(transform.position, transform.up * -MinUpWeight + transform.position);

            Gizmos.color = new Color(0, 0.9f, 0, 1);
            Gizmos.DrawWireSphere(transform.position, ShereRadius);
        }
    }
    private void Start()
    {
        GameIsStarted = true;

        var sj = gameObject.AddComponent<SpringJoint>();
        sj.connectedBody = GameObject.Find("tank").GetComponent<Rigidbody>();
        sj.spring = ForceSpring;
        sj.damper = ForceDamper;


        var cj = gameObject.AddComponent<ConfigurableJoint>();
        cj.connectedBody = GameObject.Find("tank").GetComponent<Rigidbody>();
        cj.xMotion = ConfigurableJointMotion.Locked;
        cj.yMotion = ConfigurableJointMotion.Limited;
        cj.zMotion = ConfigurableJointMotion.Locked;
        cj.angularXMotion = ConfigurableJointMotion.Locked;
        cj.angularYMotion = ConfigurableJointMotion.Locked;
        cj.angularZMotion = ConfigurableJointMotion.Locked;
        var solim = cj.linearLimit;
        solim.limit = (MaxUpWeight + MinUpWeight) / 2;
        cj.linearLimit = solim;
        cj.autoConfigureConnectedAnchor = false;
        cj.connectedAnchor += transform.up * (MaxUpWeight - MinUpWeight) / 2;

        var child = new GameObject(gameObject.name);
        child.transform.SetParent(gameObject.transform);
        child.layer = 3;
        child.transform.localPosition = new Vector3(0, 0, 0);
        child.transform.localRotation = new Quaternion(0, 0, 0, 0);

        var go = child.AddComponent<SphereCollider>();
        go.radius = ShereRadius;

        var cj1 = child.AddComponent<ConfigurableJoint>();
        cj1.connectedBody = gameObject.GetComponent<Rigidbody>();
        cj1.xMotion = ConfigurableJointMotion.Locked;
        cj1.yMotion = ConfigurableJointMotion.Locked;
        cj1.zMotion = ConfigurableJointMotion.Locked;
        cj1.angularYMotion = ConfigurableJointMotion.Locked;
        cj1.angularZMotion = ConfigurableJointMotion.Locked;



        StartPos = transform.localPosition;
    }
    private void LateUpdate()
    {

        if ((transform.localPosition - StartPos).y < -MinUpWeight)
        {
            gameObject.transform.localPosition = new Vector3(StartPos.x, StartPos.y - MinUpWeight, StartPos.z);
        }
        else if ((transform.localPosition - StartPos).y > MaxUpWeight)
        {
            gameObject.transform.localPosition = new Vector3(StartPos.x, StartPos.y + MaxUpWeight, StartPos.z);

        }
    }
}
