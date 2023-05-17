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
    [SerializeField] KMSelectable _selectable;

    private Dictionary<char, int> _colourParities;
    private KMHighlightable _originalHighlight;

    public bool CanPickEven { get; private set; }
    public bool CanPickOdd { get; private set; }

    private void Start() {
        SetColourParities();

        _selectable.OnHighlight += delegate () { _highlight.GetComponent<MeshRenderer>().enabled = true; };
        _selectable.OnHighlightEnded += delegate () { _highlight.GetComponent<MeshRenderer>().enabled = false; };
        _module.GetComponent<KMSelectable>().Children[8] = _selectable;
        _module.GetComponent<KMSelectable>().UpdateChildrenProperly();
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
        yield return null;
        _originalHighlight = pressedRegion.Selectable.Highlight;
        transform.position = pressedRegion.transform.position;

        _module.GetComponent<KMSelectable>().Children[8] = null;
        _module.GetComponent<KMSelectable>().UpdateChildrenProperly();
        _selectable.gameObject.SetActive(false);
    }

    public override IEnumerator HandleRegionPress(Region pressedRegion) {
        throw new System.NotImplementedException();
    }
}
