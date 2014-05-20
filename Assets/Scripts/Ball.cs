﻿using UnityEngine;
using System;
using System.Collections;

public class Ball : MonoBehaviour {
    float xStartPosition = 0f;
    float yStartPosition = -2.5f;
    float speedMultiplier = 1.1f;
    float minSpeed = 5f;
    float maxSpeed = 10f;
    public float currSpeed = 0f;

	void Start() {
        GameManager.Instance.SettingUpNewGame += (object s, EventArgs e) => { ResetBall(); };
        GameManager.Instance.GameStarted += (object a, EventArgs e) => { rigidbody.AddRelativeForce(new Vector3(0f, minSpeed, 0f)); };
        GameManager.Instance.PlayerDied += (object s, EventArgs e) => { FreezeBall(); };
        GameManager.Instance.PlayerRespawn += (object s, EventArgs e) => { 
            ResetBall();
            rigidbody.AddRelativeForce(new Vector3(0f, minSpeed, 0f));
        };
	}

    void FixedUpdate() {
        if(GameManager.Instance.gameFSM.CurrentState == GameManager.GameState.Playing) {
            currSpeed = Vector3.Magnitude(rigidbody.velocity);

            if(currSpeed > maxSpeed) {
                rigidbody.velocity /= currSpeed / maxSpeed;
            }
            if(currSpeed < minSpeed && currSpeed > 0f) {
                rigidbody.velocity /= currSpeed / minSpeed;
            }
        }
    }

    void OnCollisionEnter(Collision c) {
        print("hit: " + c.collider.name);
        if(c.gameObject.tag == "Brick") {
            Brick b = c.gameObject.GetComponent<Brick>();
            print(b.GetBrickValue());
            Score.AddScore(b.GetBrickValue());            
        }
        rigidbody.velocity += rigidbody.velocity * speedMultiplier;
    }

    void OnTriggerEnter(Collider c) {
        if(c.name == "floor") {
            // TODO: Some sort of explosion effect?
            GameManager.Instance.gameFSM.ChangeState(GameManager.GameState.Dead);
        }
    }

    void ResetBall() {
        transform.position = new Vector3(xStartPosition, yStartPosition, 0f);
        FreezeBall();
    }

    void FreezeBall() {
        // Start off with no velocity.
        rigidbody.velocity = Vector3.zero;
        currSpeed = 0f;
    }
}