using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

public class BlankSlates : MonoBehaviour {

    private KMBombInfo _bomb;
    private KMAudio _audio;
    private KMBombModule _module;

    [SerializeField] private Region[] _regions;
    [SerializeField] private RuleStateController[] _rulesStates;
    [SerializeField] private RuleStateController _currentRuleState;

    private List<int> _availableRuleStates;

    private void Awake() {
        _bomb = GetComponent<KMBombInfo>();
        _audio = GetComponent<KMAudio>();
        _module = GetComponent<KMBombModule>();
        _availableRuleStates = Enumerable.Range(0, _rulesStates.Count()).ToList();
    }

    private void Start() {
        foreach (Region region in _regions) {
            region.Selectable.OnInteract += delegate () { HandleRegionPress(region.Position); return false; };
        }
    }

    private void HandleRegionPress(int pressedRegion) {
        _currentRuleState.HandleRegionPress(pressedRegion);
    }

    public void GetNewState(int pressedRegion) {
        int stateIndex = _availableRuleStates.PickRandom();
        _currentRuleState = _rulesStates[stateIndex];
        StartCoroutine(_currentRuleState.OnStateEnter(pressedRegion));
    }
}
