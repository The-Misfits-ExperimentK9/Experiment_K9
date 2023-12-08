using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialBehaviour2 : MonoBehaviour
{
    List<LevelStateTrigger2> activatedTriggers = new();
    [SerializeField] private bool tutorialEnabled = true;
    [SerializeField] private PressButtonBehaviour spawnCubeButton;
    [SerializeField] private BallReceiver_2D ballReceiver2D;
    int index = -1;

    float timer = 0;
    float prevTimer = 0;
    int indexOffset = 0;


    // Update is called once per frame
    void Update()
    {
        if (tutorialEnabled)
        {
            switch (index)
            {
                case 0:
                    HandleFirstTrigger();   //turn on DOG message
                    break;
                case 4:
                    HandleSecondTrigger();  //leave wall message
                    break;
                case 5:
                    HandleEnterLeaveWallMessage();   //move to yellow wall and leave wall message
                    break;
                case 8:
                    Handle2DBallMessage();
                    break;
                default:
                    PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
                    break;
            }
        }

        prevTimer = timer;
        timer += Time.deltaTime;
    }
    public void SetState(LevelStateTrigger2 trigger, int index)
    {
        if (!tutorialEnabled) return;
        if (activatedTriggers.Contains(trigger)) return;
        PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();  //never show more than 1 tutorial message at a time
        activatedTriggers.Add(trigger);
        this.index = index;
        timer = 0;
        PlayerBehaviour.Instance.interfaceScript.DisplayTutorialMessageByIndex(index);
    }
    public void AdvanceState(LevelStateTrigger2 trigger)
    {
        if (!tutorialEnabled) return;
        if (activatedTriggers.Contains(trigger)) return;                    //never show the same tutorial message twice

        PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();  //never show more than 1 tutorial message at a time
        activatedTriggers.Add(trigger);
        index++;
        PlayerBehaviour.Instance.interfaceScript.DisplayTutorialMessageByIndex(index);
    }
    public void BackupAState(LevelStateTrigger2 trigger)
    {
        index--;
    }
    //message to turn on D.O.G. will hide when the player turns on D.O.G.
    private void HandleFirstTrigger()
    {
        if (timer >= 8.5f && prevTimer <= 8.5f)
        {
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
            PlayerBehaviour.Instance.interfaceScript.DisplayTutorialMessageByIndex(1);
        }
        if (timer >= 26f && prevTimer <= 26f)
        {
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
            PlayerBehaviour.Instance.interfaceScript.DisplayTutorialMessageByIndex(2);
        }
        if (timer >= 34.2f && prevTimer <= 34.2f)
        {
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
            PlayerBehaviour.Instance.interfaceScript.DisplayTutorialMessageByIndex(3);
        }
        if (timer >= 44.5f && prevTimer <= 44.5f)
        {
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
        }
    }
    //message to leave the wall will hide when the player leaves the wall
    private void HandleSecondTrigger()
    {
        if (timer >= 14f && prevTimer <= 14f)
        {
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
        }
    }
    private void HandleEnterLeaveWallMessage()
    {
        if (timer >= 11.5f && prevTimer <= 11.5f)
        {
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
            PlayerBehaviour.Instance.interfaceScript.DisplayTutorialMessageByIndex(6);
        }
        if (timer >= 14.5f && prevTimer <= 14.5f)
        {
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
            PlayerBehaviour.Instance.interfaceScript.DisplayTutorialMessageByIndex(7);
        }
        if (timer >= 17f && prevTimer <= 17f)
        {
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
        }
    }
    private void Handle2DBallMessage()
    {
        if (timer >= 4.5f && prevTimer <= 4.5f)
        {
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
            PlayerBehaviour.Instance.interfaceScript.DisplayTutorialMessageByIndex(9);
        }
        if (timer >= 13.5f && prevTimer <= 13.5f)
        {
            PlayerBehaviour.Instance.interfaceScript.DisableActiveTutorials();
        }
    }
}
