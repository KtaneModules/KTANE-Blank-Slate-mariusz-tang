using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HipsState : RuleStateController {

    [SerializeField] private TextMesh[] _textMeshes;

    private int _targetRegionNumber;

    private Coroutine _highlightsActive;
    private int _highlightedRegionNumber;

    private void Start() {
        foreach (Region r in _module.Regions) {
            r.Selectable.OnHighlight += delegate () { _highlightedRegionNumber = r.Number; };
            r.Selectable.OnHighlightEnded += delegate () { if (_highlightedRegionNumber == r.Number) { _highlightedRegionNumber = 0; } };
        }
    }

    public override IEnumerator OnStateEnter(Region pressedRegion) {
        _targetRegionNumber = _module.AvailableRegions.PickRandom();
        _module.Log("Nothing seems to have happened... the regions must be hiding some numbers...");

        SetDisplayDigits(_targetRegionNumber, pressedRegion.Number);
        _highlightsActive = StartCoroutine(TrackHighlights());
        yield return null;
    }

    private void SetDisplayDigits(int targetRegionNumber, int originRegionNumber) {
        List<int> availableDigits = Enumerable.Range(1, 8).ToList();
        availableDigits.Remove(targetRegionNumber);
        availableDigits.Remove(originRegionNumber);

        while (availableDigits.Count() > 0) {
            int displayPosition = availableDigits.PickRandom();
            availableDigits.Remove(displayPosition);
            int displayDigit = availableDigits.PickRandom();
            availableDigits.Remove(displayDigit);

            _textMeshes[displayPosition - 1].text = displayDigit.ToString();
            _module.Log($"Hovering over region {displayPosition} reveals a {displayDigit}.");
        }
        _module.Log($"The corresponding region to press is {targetRegionNumber}.");
    }

    private IEnumerator TrackHighlights() {
        while (true) {
            for (int i = 0; i < 8; i++) {
                if (i == _highlightedRegionNumber - 1) {
                    _textMeshes[i].color = new Color(0, 0, 0, Mathf.Min(_textMeshes[i].color.a + Time.deltaTime, 1));
                }
                else {
                    _textMeshes[i].color = new Color(0, 0, 0, Mathf.Max(_textMeshes[i].color.a - Time.deltaTime, 0));
                }
            }
            yield return null;
        }
    }

    public override IEnumerator HandleRegionPress(Region pressedRegion) {
        int pressedNumber = pressedRegion.Number;

        if (pressedNumber == _targetRegionNumber) {
            StopCoroutine(_highlightsActive);
            StartCoroutine(FadeOutNumbers());
            _module.Log("Pressed the correct region!");
            // ! _module.GetNewState(pressedRegion);
            _module.Log("Correct!");
        }
        else {
            _module.Strike($"Incorrectly pressed region {pressedNumber}. Strike!");
        }

        yield return null;
    }

    private IEnumerator FadeOutNumbers() {
        float elapsedTime = 0;
        while (elapsedTime <= 2) {
            for (int i = 0; i < 8; i++) {
                _textMeshes[i].color = new Color(0, 0, 0, Mathf.Max(_textMeshes[i].color.a - Time.deltaTime, 0));
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

}
