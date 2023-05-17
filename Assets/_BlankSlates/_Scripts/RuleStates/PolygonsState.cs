using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

public class PolygonsState : RuleStateController {

    [SerializeField] GameObject[] _firstPair;
    [SerializeField] GameObject[] _secondPair;
    [SerializeField] GameObject[] _thirdPair;
    [SerializeField] GameObject[] _fourthPair;
    [SerializeField] GameObject _highlight;

    private GameObject[][] _pairs;
    private Dictionary<char, int> _colourParities;
    private Action _highlightShape;
    private Action _unhighlightShape;

    private Region _originRegion;
    private int _targetRegionNumber;

    public bool CanPickEven { get; private set; }
    public bool CanPickOdd { get; private set; }

    private void Start() {
        SetColourParities();

        _pairs = new GameObject[][] { _firstPair, _secondPair, _thirdPair, _fourthPair };

        var highlightRenderer = _highlight.GetComponent<MeshRenderer>();
        _highlightShape = delegate () { highlightRenderer.enabled = true; };
        _unhighlightShape = delegate () { highlightRenderer.enabled = false; };
    }

    private void SetColourParities() {
        _colourParities = new Dictionary<char, int> {
            { 'R', _module.BombInfo.GetBatteryHolderCount() % 2 },
            { 'O', _module.BombInfo.GetOffIndicators().Count() % 2 },
            { 'Y', _module.BombInfo.GetOnIndicators().Count() % 2 },
            { 'G', _module.BombInfo.GetSerialNumberNumbers().First() % 2 },
            { 'C', _module.BombInfo.GetPortPlateCount() % 2 },
            { 'B', _module.BombInfo.GetBatteryCount() % 2 },
            { 'M', _module.BombInfo.GetSerialNumberNumbers().Last() % 2 },
            { 'W', _module.BombInfo.GetPortCount() % 2 },
        };

        CanPickEven = _colourParities.ContainsValue(0);
        CanPickOdd = _colourParities.ContainsValue(1);
    }

    public override IEnumerator OnStateEnter(Region pressedRegion) {
        _originRegion = pressedRegion;

        if (!CanPickEven) {
            _targetRegionNumber = _module.AvailableRegions.Where(r => r % 2 != 0).PickRandom();
        }
        else if (!CanPickOdd) {
            _targetRegionNumber = _module.AvailableRegions.Where(r => r % 2 != 1).PickRandom();
        }
        else {
            _targetRegionNumber = _module.AvailableRegions.PickRandom();
        }

        GameObject displayedShape = GetShapeForTargetRegion(_targetRegionNumber);
        _highlight.GetComponent<MeshFilter>().mesh = displayedShape.GetComponentInChildren<MeshFilter>().sharedMesh;

        _module.Log($"{displayedShape.name.Substring(2)} appeared on region {pressedRegion.Number}.");
        _module.Log($"The corresponding region to press is {_targetRegionNumber}.");

        transform.position = pressedRegion.transform.position;
        _highlightShape();
        _originRegion.Selectable.OnHighlight += _highlightShape;
        _originRegion.Selectable.OnHighlightEnded += _unhighlightShape;
        yield return null;
    }

    private GameObject GetShapeForTargetRegion(int targetRegion) {
        // Return a shape which results in the required target region.
        return _pairs[(targetRegion - 1) / 2].Where(s => _colourParities[s.name[0]] == targetRegion % 2).PickRandom();
    }

    public override IEnumerator HandleRegionPress(Region pressedRegion) {
        int pressedPosition = pressedRegion.Number;

        if (pressedPosition != _targetRegionNumber) {
            _module.Strike($"Incorrectly pressed region {pressedPosition}. Strike!");
        }
        else {
            _originRegion.Selectable.OnHighlight -= _highlightShape;
            _originRegion.Selectable.OnHighlightEnded -= _unhighlightShape;
            // ! _module.GetNewState(pressedRegion);
            _module.Log("Correct!");
        }
        yield return null;
    }
}
