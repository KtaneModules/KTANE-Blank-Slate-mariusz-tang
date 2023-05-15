using System.Collections;
using System.Collections.Generic;
using KModkit;
using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
public class Region : MonoBehaviour {

    public KMSelectable Selectable { get; private set; }
    public int Position { get; private set; }

    void Start() {
        Selectable = GetComponent<KMSelectable>();
        Position = int.Parse(transform.name);
        
        Selectable.OnInteract += delegate () { Selectable.AddInteractionPunch(); return false; };
    }

}
