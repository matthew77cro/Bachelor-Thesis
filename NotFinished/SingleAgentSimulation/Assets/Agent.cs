using System;
using System.Collections.Generic;
using UnityEngine;
using NEAT;

public class Agent : MonoBehaviour
{

    public const int AGENT_ACTION_COUNT = 6;
    public const int RAYS_ROWS = 4;
    public const int RAYS_PER_ROW = 4;
    public const float FOV = 60; // degrees

    public bool PressurePlateActivated { get; private set; }

    public enum AgentAction : int
    {
        FORWARD = 0,
        BACKWARD = 1,
        LEFT = 2,
        RIGHT = 3,
        ROTATE_AC = 4, // anti-clockwise around y axis
        ROTATE_C = 5 // clockwise around y axis
    }

    void Update()
    {

        foreach (var ray in Rays())
        {
            Debug.DrawRay(ray.origin, ray.direction * 2, Color.magenta, 0, false);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "pressure_plate")
            PressurePlateActivated = PressurePlateActivated ? false : true;
    }

    public void ApplyAgentAction(bool[] actions)
    {

        if (actions.Length != AGENT_ACTION_COUNT)
            return;

        float speed = 4;
        float rotationAngleStep = 2;
        float vx = 0, vy = 0, vz = 0;
        float rotate = 0;

        Rigidbody rb = GetComponent<Rigidbody>();

        var locVel = transform.InverseTransformDirection(rb.velocity);

        if (actions[(int)AgentAction.FORWARD] && actions[(int)AgentAction.BACKWARD])
            vz = 0;
        else if (actions[(int)AgentAction.FORWARD])
            vz = speed;
        else if (actions[(int)AgentAction.BACKWARD])
            vz = -speed;

        if (actions[(int)AgentAction.LEFT] && actions[(int)AgentAction.RIGHT])
            vx = 0;
        else if (actions[(int)AgentAction.RIGHT])
            vx = speed;
        else if (actions[(int)AgentAction.LEFT])
            vx = -speed;

        if (actions[(int)AgentAction.ROTATE_C] && actions[(int)AgentAction.ROTATE_AC])
            rotate = 0;
        else if (actions[(int)AgentAction.ROTATE_C])
            rotate = rotationAngleStep;
        else if (actions[(int)AgentAction.ROTATE_AC])
            rotate = -rotationAngleStep;

        locVel.x = vx;
        locVel.y = vy;
        locVel.z = vz;

        rb.velocity = transform.TransformDirection(locVel);

        transform.Rotate(0, rotate, 0);

    }

    public IEnumerable<Ray> Rays()
    {
        Vector3 rayStart = transform.position + transform.TransformVector(new Vector3(0, 0.4f, 0.7f));

        Vector3 rayDir = Quaternion.AngleAxis(-FOV / 2, transform.up) * transform.forward;
        Vector3 verticalRotationAxis = Quaternion.AngleAxis(-FOV / 2, transform.up) * transform.right;
        Quaternion rotationX = Quaternion.AngleAxis(FOV / (RAYS_PER_ROW - 1), transform.up);

        for (int i = 0; i < RAYS_PER_ROW; i++)
        {

            Vector3 rayD = Quaternion.AngleAxis(-FOV / 2, verticalRotationAxis) * rayDir;
            Quaternion rotationY = Quaternion.AngleAxis(FOV / (RAYS_ROWS - 1), verticalRotationAxis);

            for (int j = 0; j < RAYS_ROWS; j++)
            {

                yield return new Ray(rayStart, rayD);
                rayD = rotationY * rayD;

            }

            rayDir = rotationX * rayDir;
            verticalRotationAxis = rotationX * verticalRotationAxis;

        }
    }

}
