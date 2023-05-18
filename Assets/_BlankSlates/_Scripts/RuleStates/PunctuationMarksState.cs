using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunctuationMarksState : RuleStateController {

    private const string COLOUR_ORDER = "ROYGBPKW";
    private readonly Vector3 _buttonScale = new Vector3(0.0002f, 0.00007f, 0.0002f);
    private readonly Vector3 _glitchScaleSmall = new Vector3(0.3839059f, 0.02574724f, 0.3839059f);
    private readonly Vector3 _glitchScaleBig = new Vector3(0.8879244f, 0.05955003f, 0.8879244f);

    [SerializeField] private Color[] _glitchColours;
    [SerializeField] private Transform _glitchParentTransform;
    [SerializeField] private SpriteRenderer[] _glitchSquares;

    [SerializeField] private KMSelectable[] _logicDiveButtons;
    [SerializeField] private Color[] _logicDiveColours;

    private void Start() {
        foreach (KMSelectable button in _logicDiveButtons) {
            button.GetComponent<MeshRenderer>().enabled = true;
            button.transform.localScale = Vector3.zero;
        }
    }

    public override IEnumerator HandleRegionPress(Region pressedRegion) {
        throw new System.NotImplementedException();
    }

    public override IEnumerator OnStateEnter(Region pressedRegion) {
        _glitchParentTransform.position = pressedRegion.transform.position;
        _glitchParentTransform.localScale = _glitchScaleSmall;
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
