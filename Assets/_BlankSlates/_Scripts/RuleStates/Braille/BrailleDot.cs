using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class BrailleDot : MonoBehaviour {

    public MeshRenderer Renderer { get; private set; }
    public int Position { get; private set; }

    private void Awake() {
        Renderer = GetComponent<MeshRenderer>();
        Position = int.Parse(transform.name);
    }

}
