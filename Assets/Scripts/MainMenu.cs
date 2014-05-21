using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
    GUISkin guiSkin;

    void Awake() {
        guiSkin = Resources.Load("defaultSkin") as GUISkin;
    }

    void OnGUI() {
        GUI.skin = guiSkin;
        
        if(GUI.Button(new Rect((int)(Screen.width*0.4)-87, Screen.height>>1, 175, 80), "PLAY")) {
            GameManager.Instance.mainFSM.ChangeState(GameManager.State.SetupNewGame);
        }
        if(GUI.Button(new Rect((int)(Screen.width*0.6)-87, Screen.height >> 1, 175, 80), "QUIT")) {
            Application.Quit();
        }
    }
}
