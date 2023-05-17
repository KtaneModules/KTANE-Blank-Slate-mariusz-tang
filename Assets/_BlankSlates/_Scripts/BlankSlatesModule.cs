using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

public class BlankSlatesModule : MonoBehaviour {

    // ! Punctuation Marks rule must not be last, to allow for the solution colour to move with the rest.
    // ! If Punctuation Marks is second-to-last and polygons requires a specific parity, then pick Punctuation Marks but
    // ! force it to pick the other parity.
    private KMBombModule _module;

    private static int _moduleCounter = 0;
    private int _moduleId;

    [SerializeField] private Region[] _regions;
    // Polygons is special in that in rare cases it is unable to pick an odd region or an even region.
    // Punctuation marks is special in that it the correct region is undecided, so it must not be last.
    [SerializeField] private RuleStateController[] _rulesStatesMinusPolygons;
    [SerializeField] private PolygonsState _polygons;

    private List<int> _availableRuleStates;
    private RuleStateController _currentRuleState;

    public List<int> AvailableRegions { get; private set; }
    public Region[] Regions { get { return _regions.ToArray(); } }
    public KMBombInfo BombInfo { get; private set; }
    public KMAudio BombAudio { get; private set; }

    private void Awake() {
        _moduleId = _moduleCounter++;
        _module = GetComponent<KMBombModule>();
        _availableRuleStates = Enumerable.Range(0, _rulesStatesMinusPolygons.Count() + 1).ToList();

        BombInfo = GetComponent<KMBombInfo>();
        BombAudio = GetComponent<KMAudio>();
        AvailableRegions = Enumerable.Range(1, 8).ToList();
    }

    private void Start() {
        foreach (Region region in _regions) {
            region.Selectable.OnInteract += delegate () { HandleRegionPress(region); return false; };
        }

        Log("Press any region to start.");
    }

    private void HandleRegionPress(Region pressedRegion) {
        if (_currentRuleState == null) {
            Log($"Pressed region {pressedRegion.Number}.");
            AvailableRegions.Remove(pressedRegion.Number);
            GetNewState(pressedRegion);
            return;
        }
        StartCoroutine(_currentRuleState.HandleRegionPress(pressedRegion));
    }

    public void GetNewState(Region pressedRegion) {
        int stateIndex = _availableRuleStates.PickRandom();

        // Handle the possibility of polygons not being able to select an available region.
        if (!_polygons.CanPickEven && AvailableRegions.Count(r => r % 2 != 0) == 1) {
            stateIndex = 0;
        }
        else if (!_polygons.CanPickOdd && AvailableRegions.Count(r => r % 2 != 1) == 1) {
            stateIndex = 0;
        }

        if (stateIndex == 0) {
            _currentRuleState = _polygons;
        }
        else {
            _currentRuleState = _rulesStatesMinusPolygons[stateIndex - 1];
        }

        _availableRuleStates.Remove(stateIndex);
        AvailableRegions.Remove(pressedRegion.Number);

        Log("-=-=-=-=-=-");
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
