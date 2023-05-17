using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunctuationMarksState : RuleStateController {

    private readonly Vector3 _buttonScale = new Vector3(0.0002f, 0.00007f, 0.0002f);

    [SerializeField] private KMSelectable[] _buttons;
    [SerializeField] private Color[] _glitchColours;
    [SerializeField] private Transform _glitchParentTransform;
    [SerializeField] private SpriteRenderer[] _glitchSquares;

    private void Start() {
        foreach (KMSelectable button in _buttons) {
            button.GetComponent<MeshRenderer>().enabled = true;
            button.transform.localScale = Vector3.zero;
        }
    }

    public override IEnumerator HandleRegionPress(Region pressedRegion) {
        throw new System.NotImplementedException();
    }

    public override IEnumerator OnStateEnter(Region pressedRegion) {
        _glitchParentTransform.position = pressedRegion.transform.position;
        yield return StartCoroutine(GlitchAnimation());
        yield return null;
    }

    private IEnumerator GlitchAnimation() {
        for (int i = 0; i < 10; i++) {
            foreach (SpriteRenderer square in _glitchSquares) {
                square.color = _glitchColours.PickRandom();
            }
            yield return new WaitForSeconds(0.02f);
        }
        foreach (SpriteRenderer square in _glitchSquares) {
            square.color = Color.black * 0;
        }
    }

}
