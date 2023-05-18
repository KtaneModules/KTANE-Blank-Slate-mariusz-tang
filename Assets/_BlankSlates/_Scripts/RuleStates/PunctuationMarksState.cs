using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class PunctuationMarksState : RuleStateController {

    private const string COLOUR_ORDER = "ROYGBPKW";
    private readonly Vector3 _logicDiveButtonScale = new Vector3(0.0002f, 0.00007f, 0.0002f);
    private readonly Vector3 _movingPartScaleSmall = new Vector3(1 / 3f, 1, 1 / 3f);
    private readonly Vector3 _movingPartScaleBig = Vector3.one;
    private readonly char[,] _colourTable = new char[,] {
        { 'R', 'O', 'Y', 'G', 'B', 'P', 'K', 'W' },
        { 'K', 'W', 'G', 'O', 'P', 'B', 'R', 'Y' },
        { 'O', 'R', 'B', 'Y', 'K', 'G', 'W', 'P' },
        { 'B', 'K', 'W', 'P', 'O', 'Y', 'G', 'R' },
        { 'G', 'B', 'K', 'W', 'Y', 'R', 'P', 'O' },
        { 'P', 'Y', 'O', 'K', 'R', 'W', 'B', 'G' },
        { 'W', 'P', 'R', 'B', 'G', 'O', 'Y', 'K' },
        { 'Y', 'G', 'P', 'R', 'W', 'K', 'O', 'B' },
    };
    private readonly string[] _colourNames = new string[] { "red", "orange", "yellow", "green", "blue", "purple", "black", "white" };

    [SerializeField] private Transform _movingPart;
    [SerializeField] private Color[] _glitchColours;
    [SerializeField] private SpriteRenderer[] _glitchSquares;
    [SerializeField] private TextMesh _digitText;

    [SerializeField] private KMSelectable[] _logicDiveButtons;
    [SerializeField] private Color[] _logicDiveColours;

    private int _originRegionNumber;
    private int _displayDigit;
    private bool _hasRevealedDigit = false;
    private bool _hasLoggedDigit = false;

    private int _targetColourIndex;
    private Color _targetColour;

    private Coroutine _logicDive;
    private int _currentTargetPosition;

    private void Start() {
        for (int i = 0; i < _logicDiveButtons.Length; i++) {
            _logicDiveButtons[i].GetComponent<MeshRenderer>().enabled = true;
            _logicDiveButtons[i].transform.localScale = Vector3.zero;
            int buttonNumber = i + 1;
            _logicDiveButtons[i].OnInteract += delegate () { StartCoroutine(HandleButtonPress(buttonNumber)); return false; };
        }
    }

    public override IEnumerator OnStateEnter(Region pressedRegion) {
        _originRegionNumber = pressedRegion.Number;
        _displayDigit = Rnd.Range(1, 9);

        _movingPart.position = pressedRegion.transform.position;
        _movingPart.localScale = _movingPartScaleSmall;
        _module.BombAudio.PlaySoundAtTransform("PM Begin", transform);

        _module.Log($"Region {_originRegionNumber} emitted a high-pitched sound!");
        _module.Log($"Press this region again to reveal the number.");

        yield return null;
    }

    public override IEnumerator HandleRegionPress(Region pressedRegion) {
        yield return null;
        if (pressedRegion.Number != _originRegionNumber) {
            _module.Strike("Pressed a different region. Strike!");
            _hasRevealedDigit = false;
            StartCoroutine(GlitchAnimation());
        }
        else if (!_hasRevealedDigit) {
            _hasRevealedDigit = true;
            yield return StartCoroutine(GlitchAnimation());
            _digitText.text = _displayDigit.ToString();

            if (!_hasLoggedDigit) {
                _targetColourIndex = COLOUR_ORDER.IndexOf(_colourTable[_displayDigit - 1, _originRegionNumber - 1]);
                _targetColour = _logicDiveColours[_targetColourIndex];
                _module.Log($"The number is {_displayDigit}.");
                _module.Log($"The corresponding colour to press is {_colourNames[_targetColourIndex]}.");
                _hasLoggedDigit = true;
            }

            yield return new WaitForSeconds(2);
            _digitText.text = string.Empty;
        }
        else {
            SetButtonsActive(true);
            _logicDive = StartCoroutine(LogicDive());
            _hasRevealedDigit = false;
        }
    }

    private IEnumerator LogicDive() {
        if (!_module.TpActive) {
            for (int i = 0; i < 6; i++) {
                SetColours();
                yield return new WaitForSeconds(1);
            }
        }
        else {
            SetColours();
            yield return new WaitForSeconds(15);
        }
        _module.Strike("Took too long to press a region. Strike!");
        SetButtonsActive(false);
    }

    private void SetColours() {
        _currentTargetPosition = _module.AvailableRegions.PickRandom();
        _logicDiveButtons[_currentTargetPosition - 1].GetComponent<MeshRenderer>().material.color = _targetColour;

        List<int> _availableColourIndices = Enumerable.Range(0, 8).ToList();
        _availableColourIndices.Remove(_targetColourIndex);

        for (int i = 0; i < _logicDiveButtons.Length; i++) {
            if (i != _currentTargetPosition - 1) {
                int index = _availableColourIndices.PickRandom();
                _availableColourIndices.Remove(index);
                _logicDiveButtons[i].GetComponent<MeshRenderer>().material.color = _logicDiveColours[index];
            }
        }
    }

    private IEnumerator HandleButtonPress(int buttonNumber) {
        StopCoroutine(_logicDive);
        SetButtonsActive(false);

        if (buttonNumber == _currentTargetPosition) {
            _module.Log("Pressed the correct colour!");
            _module.BombAudio.PlaySoundAtTransform("PM solve", transform);
            // ! _module.GetNewState(_module.Regions[buttonNumber - 1]);
            _module.Log("Correct!");
        }
        else {
            _movingPart.transform.position = _module.Regions[3].transform.position;
            _movingPart.localScale = _movingPartScaleBig;
            yield return StartCoroutine(GlitchAnimation());
            _movingPart.position = _module.Regions[_originRegionNumber - 1].transform.position;
            _movingPart.localScale = _movingPartScaleSmall;
            _module.Strike("Pressed an incorrect colour. Strike!");
        }
    }

    private IEnumerator GlitchAnimation() {
        _module.BombAudio.PlaySoundAtTransform($"PM glitch{Rnd.Range(1, 6)}", _movingPart.transform);
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

    private void SetButtonsActive(bool value) {
        Array.ForEach(_logicDiveButtons, b => b.transform.localScale = value ? _logicDiveButtonScale : Vector3.zero);
        Array.ForEach(_module.Regions, r => r.transform.localScale = value ? Vector3.zero : Vector3.one);
    }
}
