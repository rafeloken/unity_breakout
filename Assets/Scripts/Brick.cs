using UnityEngine;
using System.Collections;

// Each value is how many points the block is worth.
public enum BrickType { Red = 10, Green = 25, Blue = 50 };

public class Brick : MonoBehaviour {
    BrickType brickType;
    LevelBuilder levelBuilder;

    void Awake() {
        levelBuilder = GameObject.Find("Bricks").GetComponent<LevelBuilder>() as LevelBuilder;
    }

    public void InitBrick(BrickType brickType) {
        this.brickType = brickType;
        MeshRenderer mr = GetComponent<MeshRenderer>();
        switch(brickType) {                
            case BrickType.Green:
                mr.material = Resources.Load("Materials/blockGreenMat") as Material;
                break;
            case BrickType.Blue:
                mr.material = Resources.Load("Materials/blockBlueMat") as Material;
                break;
            default: // Default is red.
                mr.material = Resources.Load("Materials/blockRedMat") as Material;
                break;
        }
    }

    public int GetBrickValue() {
        return (int)brickType;
    }

    void OnCollisionEnter(Collision c) {
        if(c.gameObject.name == "ball") {            
            levelBuilder.RemoveBrick(gameObject);
            Destroy(gameObject);
            if(levelBuilder.IsLevelComplete()) {
                GameManager.Instance.gameFSM.ChangeState(GameManager.GameState.NextStage);
            }
        }
    }
}
