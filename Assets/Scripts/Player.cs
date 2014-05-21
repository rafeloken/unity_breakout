using UnityEngine;
using System;
using System.Collections;

public class Player : MonoBehaviour {
    float yStartPosition = -4.75f;
    float xBoundary = 7.65f;
    float speed = 10f;

    // Cache the transform since we are using it a few times.
    Transform t;

    GUIText lives;
    int numLives = 3;
    public int NumLives {
        get { return numLives; }
        set { numLives = value; }
    }

    void Awake() {
        t = transform;
        lives = GameObject.Find("lives").guiText;
        AdjustLives(0);
    }

	void Start() {
        GameManager.Instance.SettingUpNewGame += (object s, EventArgs e) => { ResetPlayer(); };
        GameManager.Instance.PlayerDied += (object s, EventArgs e) => { AdjustLives(-1); };
        GameManager.Instance.PlayerRespawn += (object s, EventArgs e) => { ResetPlayer(false); };
	}

    public void ResetPlayer(bool newGame=true) {
        t.position = new Vector3(0f, yStartPosition, 0f);
        if(newGame) {
            NumLives = 3;
            lives.text = "Lives: " + NumLives.ToString();
        }
    }

	public void UpdatePlayer() {
        float moveDir = Input.GetAxisRaw("Horizontal");
        if(moveDir != 0) {
            t.Translate(new Vector3(speed, 0f, 0f) * Time.deltaTime * moveDir);
            if(t.position.x >= xBoundary || t.position.x <= -xBoundary) {
                t.transform.position = new Vector3(xBoundary * moveDir, yStartPosition, 0f);
            }
        }
	}

    public void AdjustLives(int adjustment) {
        NumLives += adjustment;
        if(NumLives < 0) {
            GameManager.Instance.mainFSM.ChangeState(GameManager.State.GameOver);
        } else {
            lives.text = "Lives: " + NumLives.ToString();
        }
    }
}
