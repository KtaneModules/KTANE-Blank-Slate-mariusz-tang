using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class PunctuationMarksState : RuleStateController {

    private const string COLOUR_ORDER = "ROYGBPKW";
    private const string CB_COLOUR_ORDER = "roygbp  ";
    private readonly Vector3 _cbButtonScale = new Vector3(0.02748407f, 0.002005622f, 0.02748407f);
    private readonly Vector3 _logicDiveButtonScale = Vector3.one * 0.019956f;
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

    [SerializeField] private KMSelectable _cbButton;
    [SerializeField] private TextMesh[] _cbTexts;
    [SerializeField] private TextMesh _cbInfoText;

    [SerializeField] private KMSelectable[] _logicDiveButtons;
    [SerializeField] private Color[] _logicDiveColours;

    private int _originRegionNumber;
    private int _displayDigit;
    private bool _hasRevealedDigit = false;
    private bool _hasLoggedDigit = false;

    private Coroutine _displayCbModeInfo;
    private bool _cbModeEnabled = false;

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

        _cbButton.transform.localScale = Vector3.zero;
        _cbButton.OnInteract += delegate () { ToggleCbMode(); return false; };
    }

    private void ToggleCbMode() {
        _cbModeEnabled = !_cbModeEnabled;
        Array.ForEach(_cbTexts, t => t.color = _cbModeEnabled ? Color.black : Color.black * 0);

        if (_displayCbModeInfo != null) {
            StopCoroutine(_displayCbModeInfo);
        }
        _displayCbModeInfo = StartCoroutine(DisplayCbModeInfo());
    }

    private IEnumerator DisplayCbModeInfo() {
        _cbInfoText.text = _cbModeEnabled ? "cb:on" : "cb:off";
        yield return new WaitForSeconds(1);
        _cbInfoText.text = string.Empty;
    }

    public override IEnumerator OnStateEnter(Region pressedRegion) {
        _originRegionNumber = pressedRegion.Number;
        _displayDigit = Rnd.Range(1, 9);

        _cbButton.transform.localScale = _cbButtonScale;

        _movingPart.position = pressedRegion.transform.position;
        _movingPart.localScale = _movingPartScaleSmall;
        _module.BombAudio.PlaySoundAtTransform("PM Begin", transform);

        _module.Log($"Region {_originRegionNumber} emitted a high-pitched sound.");
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
        _digitText.text = string.Empty;

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
        _cbTexts[_currentTargetPosition - 1].text = CB_COLOUR_ORDER[_targetColourIndex].ToString();

        List<int> _availableColourIndices = Enumerable.Range(0, 8).ToList();
        _availableColourIndices.Remove(_targetColourIndex);

        for (int i = 0; i < _logicDiveButtons.Length; i++) {
            if (i != _currentTargetPosition - 1) {
                int index = _availableColourIndices.PickRandom();
                _availableColourIndices.Remove(index);
                _logicDiveButtons[i].GetComponent<MeshRenderer>().material.color = _logicDiveColours[index];
                _cbTexts[i].text = CB_COLOUR_ORDER[index].ToString();
            }
        }
    }

    private IEnumerator HandleButtonPress(int buttonNumber) {
        StopCoroutine(_logicDive);
        SetButtonsActive(false);

        if (buttonNumber == _currentTargetPosition) {
            _cbButton.transform.localScale = Vector3.zero;
            _module.Log("Pressed the correct colour!");
            _module.GetNewState(_module.Regions[buttonNumber - 1]);
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
        Array.ForEach(_cbTexts, t => t.text = string.Empty);
    }

    public override IEnumerator SolveAnimation() {
        _module.BombAudio.PlaySoundAtTransform("PM solve", transform);
        return base.SolveAnimation();
    }

    private string[] _cbCommands = new string[] { "CB", "COLORBLIND", "COLOURBLIND", "STATUSLIGHT", "STATUS LIGHT", "SL" };

    public override IEnumerator HandleTP(string command) {
        if (_cbCommands.Contains(command.Trim().ToUpper())) {
            yield return null;
            _cbButton.OnInteract();
            yield break;
        }

        // This is mostly repeated code but oh well <|3.
        if (_logicDive == null) {
            yield return null;
            yield return base.HandleTP(command);
            yield break;
        }

        // Need to handle pressing logic dive buttons :(.
        string[] splitCommands = command.Trim().ToUpper().Split(' ');

        if (splitCommands.Length < 2 || splitCommands[1].Length != 1 || !char.IsDigit(char.Parse(splitCommands[1]))) {
            yield return "sendtochaterror Invalid command!";
        }

        int firstDigit = int.Parse(splitCommands[1]);

        if (firstDigit < 1 || firstDigit > 8) {
            yield return $"sendtochaterror '{firstDigit}' is not a valid region!";
        }

        if (splitCommands[0] == "PRESS") {
            if (splitCommands.Length == 2) {
                yield return null;
                // These are the only different lines.
                _logicDiveButtons[firstDigit - 1].OnInteract();
                yield break;
            }
            else if (splitCommands.Length == 4 && splitCommands[2] == "AT" && splitCommands[3].Length == 1 && char.IsDigit(char.Parse(splitCommands[3]))) {
                yield return null;
                while (Mathf.FloorToInt(_module.BombInfo.GetTime()) % 10 != int.Parse(splitCommands[3])) {
                    yield return "trycancel";
                }
                // These are the only different lines.
                _logicDiveButtons[firstDigit - 1].OnInteract();
                yield break;
            }
            else {
                yield return "sendtochaterror Invalid command!";
            }
        }

        yield return null;
        yield return base.HandleTP(command);
    }

    public override IEnumerator Autosolve() {
        if (!_hasRevealedDigit && _logicDive == null) {
            _module.Regions[_originRegionNumber - 1].Selectable.OnInteract();
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("okay, " + _hasRevealedDigit.ToString());

        if (_hasRevealedDigit) {
            _module.Regions[_originRegionNumber - 1].Selectable.OnInteract();
            yield return new WaitForSeconds(0.5f);
        }

        _logicDiveButtons[_currentTargetPosition - 1].OnInteract();
        yield return new WaitForSeconds(0.5f);
    }
}
