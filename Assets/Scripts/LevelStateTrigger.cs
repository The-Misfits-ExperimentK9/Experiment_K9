using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStateTrigger : MonoBehaviour
{

    [SerializeField] TutorialBehaviour tutorialBehaviour;
    [SerializeField] int index;

    private void Awake() {
      //  tutorialBehaviour = GameObject.Find("TutorialBehaviour").GetComponent<TutorialBehaviour>();
    }

    private void OnTriggerEnter(Collider other) {
        //tutorialBehaviour.AdvanceState(this);
        tutorialBehaviour.SetState(this, index);
    }
    
    
}
