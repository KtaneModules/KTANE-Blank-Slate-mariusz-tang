using System.Collections;
using System.Collections.Generic;
using KModkit;
using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
public class Region : MonoBehaviour {

    public KMSelectable Selectable { get; private set; }
    public int Number { get; private set; }

    private void Awake() {
        Selectable = GetComponent<KMSelectable>();
        Number = int.Parse(transform.name);
    }

    private void Start() {
        Selectable.OnInteract += delegate () { Selectable.AddInteractionPunch(); return false; };
    }

}
