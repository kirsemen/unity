using System;
using System.Collections.Generic;
using UnityEngine;

public class tank : MonoBehaviour
{

    private int[] MousePos;
    private GameObject Turret;
    private GameObject Barrel;
    private GameObject _Camera;
    [Header("Camera Settings")]
    public float CameraDistance = 8;
    public float SpeedCameraRotation = 10;
    public float MaxCameraAngle = 80;
    public float MinCameraAngle = 25;
    [Header("Tank Settings")]
    public float ForwardForce = 4000;
    public float BackForce = 2000;
    public float TorgueRotation = 200;
    [Header("Turret Settings")]
    public float MaxSpeedRotationTurret = 0.1f;
    [Header("Barrel Settings")]
    public float MaxSpeedRotationBarrel = 0.01f;
    public float MaxBarrelAngle = 10;
    public float MinBarrelAngle = 10;

    private void Start()
    {
        MousePos = GetMouesePosition();
        Turret = gameObject.transform.GetChild(1).gameObject;
        Barrel = Turret.transform.GetChild(1).gameObject;
        _Camera = gameObject.transform.GetChild(3).gameObject;
        _Camera.transform.GetChild(0).transform.localPosition = new Vector3(0, 0, -CameraDistance);
    }
    private void Update()
    {
        float[] ear = Div(Sub(GetMouesePosition(), MousePos), SpeedCameraRotation);

        Vector3 eulerang = _Camera.transform.localEulerAngles + new Vector3(-ear[1], ear[0], 0);
        eulerang.x = Min(Max(
            ConvertEuler(eulerang.x),
            -MinCameraAngle), MaxCameraAngle);
        _Camera.transform.localEulerAngles = eulerang;


        float a = eulerang.y;
        a = a % 360;
        float b = Turret.transform.localEulerAngles.y;
        b = b % 360;
        float c;
        if (Math.Abs(a - b + 360) < Math.Abs(a - b) && Math.Abs(a - b + 360) < Math.Abs(a - b - 360))
        {
            c = a - b + 360;
        }
        else if (Math.Abs(a - b) < Math.Abs(a - b - 360))
        {
            c = a - b;
        }
        else
        {
            c = a - b - 360;
        }
        Turret.transform.localEulerAngles += new Vector3(0,
            Min(Max(
                c,
                -MaxSpeedRotationTurret), MaxSpeedRotationTurret),
            0);


        Barrel.transform.localEulerAngles = new Vector3(
            Min(Max(
                ConvertEuler(
                    Min(Max(
                    -ConvertEuler(Barrel.transform.localEulerAngles.x - eulerang.x),
                    -MaxSpeedRotationBarrel), MaxSpeedRotationBarrel) + Barrel.transform.localEulerAngles.x),
                -MinBarrelAngle), MaxBarrelAngle),
            0, 0);
        ushort layerMask = (ushort)(1 << LayerMask.NameToLayer("tank"));
        layerMask = (ushort)~layerMask;
        RaycastHit hit;
        Vector3 pos = _Camera.transform.GetChild(0).transform.position - _Camera.transform.position;
        if (Physics.Raycast(_Camera.transform.position, pos, out hit, Mathf.Infinity, Convert.ToInt32(layerMask)))
        {
            if (hit.distance < CameraDistance)
            {
                _Camera.transform.GetChild(0).transform.localPosition = new Vector3(0, 0, -hit.distance + 0.2f);
            }
            else
            {
                _Camera.transform.GetChild(0).transform.localPosition = new Vector3(0, 0, -CameraDistance);
            }
        }
        MousePos = GetMouesePosition();

        if (Input.GetKey(KeyCode.W))
        {
            gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * ForwardForce);
        }
        if (Input.GetKey(KeyCode.S))
        {
            gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * -BackForce);
        }
        if (Input.GetKey(KeyCode.A))
        {
            gameObject.GetComponent<Rigidbody>().AddTorque(Vector3.down * TorgueRotation * (Input.GetKey(KeyCode.S) ? -1 : 1));
        }
        if (Input.GetKey(KeyCode.D))
        {
            gameObject.GetComponent<Rigidbody>().AddTorque(Vector3.up * TorgueRotation * (Input.GetKey(KeyCode.S) ? -1 : 1));
        }
    }


    int[] GetMouesePosition()
    {
        return new int[2] { Convert.ToInt32(Input.mousePosition.x), Convert.ToInt32(Input.mousePosition.y) };
    }
    int[] Sub(int[] a, int[] b)
    {
        return new int[2] { a[0] - b[0], a[1] - b[1] };
    }
    float[] Div(int[] a, float b)
    {
        return new float[2] { a[0] / b, a[1] / b };
    }
    float Min(float a, float b)
    {
        return a < b ? a : b;
    }
    float Max(float a, float b)
    {
        return a < b ? b : a;

    }
    float ConvertEuler(float a)
    {
        a = a % 360;
        if (a > 180) return -(360 - a);
        else return a;
    }
}
