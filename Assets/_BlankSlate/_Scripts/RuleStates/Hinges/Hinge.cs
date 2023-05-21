using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hinge : MonoBehaviour {

    [SerializeField] private KMSelectable _selectable;

    public KMSelectable Selectable { get { return _selectable; } }
    public int Number { get; private set; }

    private void Awake() {
        Number = name[6] - '0';
    }

    public void SetSelectableActive(bool value) {
        Selectable.transform.localScale = value ? Vector3.one * 11 : Vector3.zero;
    }
}
