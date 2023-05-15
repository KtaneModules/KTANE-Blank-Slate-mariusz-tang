using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

public class BlankSlatesModule : MonoBehaviour {

    // ! Need to check for the case where all the colour values for Polygons rule share parity,
    // ! in which case change the way polygons is selected.
    // ! Punctuation Marks rule must not be last, to allow for the solution colour to move with the rest.

    private KMBombInfo _bomb;
    private KMAudio _audio;
    private KMBombModule _module;

    private static int _moduleCounter = 0;
    private int _moduleId;

    [SerializeField] private Region[] _regions;
    [SerializeField] private RuleStateController[] _rulesStates;

    private List<int> _availableRuleStates;
    private RuleStateController _currentRuleState;

    public List<int> AvailableRegions { get; private set; }
    public Region[] Regions { get { return _regions.ToArray(); } }

    private void Awake() {
        _moduleId = _moduleCounter++;
        _bomb = GetComponent<KMBombInfo>();
        _audio = GetComponent<KMAudio>();
        _module = GetComponent<KMBombModule>();
        _availableRuleStates = Enumerable.Range(0, _rulesStates.Count()).ToList();

        AvailableRegions = Enumerable.Range(1, 8).ToList();
    }

    private void Start() {
        foreach (Region region in _regions) {
            region.Selectable.OnInteract += delegate () { HandleRegionPress(region); return false; };
        }
    }

    private void HandleRegionPress(Region pressedRegion) {
        if (_currentRuleState == null) {
            GetNewState(pressedRegion);
            return;
        }
        StartCoroutine(_currentRuleState.HandleRegionPress(pressedRegion));
    }

    public void GetNewState(Region pressedRegion) {
        int stateIndex = _availableRuleStates.PickRandom();
        _currentRuleState = _rulesStates[stateIndex];
        StartCoroutine(_currentRuleState.OnStateEnter(pressedRegion));
    }

    public void Log(string message) {
        Debug.Log($"[Blank Slates #{_moduleId}] {message}");
    }

    public void Strike(string message) {
        Log($"✕ {message}");
        _module.HandleStrike();
    }
}
