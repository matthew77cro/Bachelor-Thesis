using System;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{

    public const float MOVE_SPEED = 3f;
    public const float JUMP_SPEED = 4;

    public delegate void GameOverFunction(int id);

    public List<GameOverFunction> gof = new List<GameOverFunction>();
    public int id;
    private bool jump;

    void FixedUpdate()
    {

        float vx = 0f, vy = 0f, vz = 0f;

        vx = MOVE_SPEED;
        if (jump)
        {
            vy = JUMP_SPEED;
            jump = false;
        }
        else
        {
            vy = GetComponent<Rigidbody>().velocity.y;
        }

        GetComponent<Rigidbody>().velocity = new Vector3(vx, vy, vz);

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "pipe" || collision.gameObject.tag == "ground")
        {
            NotifyListeners();
        }
    }

    private void NotifyListeners()
    {
        foreach (var gof in this.gof)
        {
            gof(this.id);
        }
    }

    public void Jump()
    {
        jump = true;
    }

    public void GameOverSubscribe(GameOverFunction gof)
    {
        this.gof.Add(gof);
    }

}
