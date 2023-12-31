using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialBehaviour : MonoBehaviour {
    List<LevelStateTrigger> activatedTriggers = new();
  //  [SerializeField] private bool tutorialEnabled = true;
    [SerializeField] private PressButtonBehaviour spawnCubeButton;
    [SerializeField] private BallReceiver_2D ballReceiver2D;
    int index = -1;


    // Update is called once per frame
    void Update() {
        if (PlayerBehaviour.Instance.TutorialEnabled) {
            switch (index) {
                case 0:
                    HandleFirstTrigger();   //turn on DOG message
                    break;
                case 1:
                    HandleSecondTrigger();  //leave wall message
                    break;
                case 2:
                    HandleLeaveWallMessage();   //move to yellow wall and leave wall message
                    break;
                case 3:

                    HandleThirdTrigger();   //pickup item message
                    break;
                case 4:
                    HandleSpawnCubeMessage();   //spawn cube message
                    break;
                case 5:
                    Handle2DBallMessage();
                        break;
                case 6:
                    HandleDrop2DBallMessage();
                    break;
                default:
                    PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
                    break;
            }
        }
    }
    public void SetState(LevelStateTrigger trigger, int index) {
        if (!PlayerBehaviour.Instance.TutorialEnabled) return;
        if (activatedTriggers.Contains(trigger)) return;
        PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();  //never show more than 1 tutorial message at a time
        activatedTriggers.Add(trigger);
        this.index = index;
        PlayerBehaviour.Instance.interfaceScript.DisplayTutorialMessageByIndex(index);
    }
    public void AdvanceState(LevelStateTrigger trigger) {
        if (!PlayerBehaviour.Instance.TutorialEnabled) return;
        if (activatedTriggers.Contains(trigger)) return;                    //never show the same tutorial message twice

        PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();  //never show more than 1 tutorial message at a time
        activatedTriggers.Add(trigger);
        index++;
        PlayerBehaviour.Instance.interfaceScript.DisplayTutorialMessageByIndex(index);
    }
    public void BackupAState(LevelStateTrigger trigger) {
        index--;
    }
    //message to turn on D.O.G. will hide when the player turns on D.O.G.
    private void HandleFirstTrigger() {
        if (PlayerBehaviour.Instance.playerDimensionController.DOGEnabled) {
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
        }
    }
    //message to leave the wall will hide when the player leaves the wall
    private void HandleSecondTrigger() {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) {                    //disables the active message when player tries to leave the wall
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
            //then display the move to the yellow wall message
            index++;
            PlayerBehaviour.Instance.interfaceScript.DisplayTutorialMessageByIndex(index);
        }
        //disable any active messages when the player leaves the wall
        if (PlayerBehaviour.Instance.is3D)
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
    }
    private void HandleLeaveWallMessage() {
        if (PlayerBehaviour.Instance.is3D)
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
    }
    //message to pickup an item will hide when the player picks up an item
    private void HandleThirdTrigger() {
        if (PlayerBehaviour.Instance.pickupController.IsHoldingObject()) {
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
        }
    }
    private void Handle2DBallMessage() {
        if (PlayerBehaviour.Instance.pickupController.IsHoldingObject()) {
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
            index++;
            PlayerBehaviour.Instance.interfaceScript.DisplayTutorialMessageByIndex(index);
        }
    }
    private void HandleSpawnCubeMessage() {
        if (spawnCubeButton.IsPressed) {
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
        }
    }
    private void HandleDrop2DBallMessage() {
        if (ballReceiver2D.IsActivated) {
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
        }
    }

}
