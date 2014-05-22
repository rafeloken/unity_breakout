using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelBuilder : MonoBehaviour {
    public GameObject brick;
    LinkedList<GameObject> bricks;

    void Awake() {
        bricks = new LinkedList<GameObject>();
    }

	void Start() {
        GenerateLevel(12, 18);
	}

    public void RemoveBrick(GameObject brick) {
        bricks.Remove(brick);
    }

    public bool IsLevelComplete() {
        return bricks.Count == 0;
    }

    public void GenerateLevel(int rows, int columns) {
        for(int r = 0; r < rows; ++r) {
            for(int c = 0; c < columns; ++c) {
                // Create brick from prefab.
                GameObject tmp = Instantiate(brick, transform.position, Quaternion.identity) as GameObject;
                // Set parent to gameObject.transform so it is nested under it in the hierarchy.
                tmp.transform.parent = transform;
                // Set the local position(in relation to parent) according to it's row and column.
                tmp.transform.localPosition = new Vector3(c * .96f, r * -0.25f, 0f);
                // Determine which color this brick should be.
                switch(Random.Range(0, 3)) {
                    case 0:
                        tmp.GetComponent<Brick>().InitBrick(BrickType.Red);
                        break;
                    case 1:
                        tmp.GetComponent<Brick>().InitBrick(BrickType.Green);
                        break;
                    default:
                        tmp.GetComponent<Brick>().InitBrick(BrickType.Blue);
                        break;
                }
                
                bricks.AddLast(tmp);
            }
        }
    }
}
