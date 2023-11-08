using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ReceivableParent : MonoBehaviour {

    [SerializeField] GlowPath glowPath;
    [SerializeField] AudioSource audioSource;


    public virtual void Activate() {
        if (glowPath != null) {
            glowPath.Activate();
        }
        if (audioSource != null) {
            audioSource.Play();
        }
    }
    public virtual void Deactivate() {
        if (glowPath != null) {
            glowPath.Deactivate();
        }
    }

}
