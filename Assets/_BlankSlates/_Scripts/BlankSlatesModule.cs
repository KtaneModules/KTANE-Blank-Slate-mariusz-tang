using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

public class BlankSlatesModule : MonoBehaviour {

    private KMBombModule _module;

    private static int _moduleCounter = 0;
    private int _moduleId;
    private bool _isSolved = false;

    [SerializeField] private Region[] _regions;
    // Polygons is special in that in rare cases it is unable to pick an odd region or an even region.
    // Punctuation marks is special in that it the correct region is undecided, so it must not be last.
    [SerializeField] private RuleStateController[] _rulesStatesMinusPolygons;
    [SerializeField] private PolygonsState _polygons;

    private List<int> _availableRuleStates;
    private RuleStateController _currentRuleState;
    private int _initiallyPressedRegion;
    private bool _hasReaddedInitialRegion = false;

    private bool TwitchPlaysActive;

    public List<int> AvailableRegions { get; private set; }
    public Region[] Regions { get { return _regions.ToArray(); } }
    public KMBombInfo BombInfo { get; private set; }
    public KMAudio BombAudio { get; private set; }
    public bool TpActive { get { return TwitchPlaysActive; } }
    public bool ReadyToSolve { get; private set; }

    private void Awake() {
        _moduleId = _moduleCounter++;
        _module = GetComponent<KMBombModule>();
        _availableRuleStates = Enumerable.Range(0, _rulesStatesMinusPolygons.Count() + 1).ToList();
        // ! _availableRuleStates = new List<int> { 1 };

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
        if (_isSolved) {
            return;
        }

        // In hindsight, I should have just added an initial rule state (and maybe a solved rule state).
        if (_currentRuleState == null) {
            _initiallyPressedRegion = pressedRegion.Number;
            Log($"Pressed region {pressedRegion.Number}.");
            GetNewState(pressedRegion);
            return;
        }
        StartCoroutine(_currentRuleState.HandleRegionPress(pressedRegion));
    }

    public void GetNewState(Region pressedRegion) {
        int statesLeft = _availableRuleStates.Count();
        if (statesLeft == 0) {
            StartCoroutine(_currentRuleState.SolveAnimation());
            return;
        }

        if (statesLeft == 1) {
            ReadyToSolve = true;
        }

        if (!_hasReaddedInitialRegion && !AvailableRegions.Contains(_initiallyPressedRegion)) {
            AvailableRegions.Add(_initiallyPressedRegion);
            _hasReaddedInitialRegion = true;
        }
        AvailableRegions.Remove(pressedRegion.Number);
        int stateIndex = _availableRuleStates.PickRandom();

        if (_availableRuleStates.Contains(0)) {
            // Handle the possibility of polygons not being able to select an available region.
            if (!_polygons.CanPickEven && AvailableRegions.Count(r => r % 2 != 0) == 1) {
                stateIndex = 0;
            }
            else if (!_polygons.CanPickOdd && AvailableRegions.Count(r => r % 2 != 1) == 1) {
                stateIndex = 0;
            }
        }

        if (stateIndex == 0) {
            _currentRuleState = _polygons;
        }
        else {
            _currentRuleState = _rulesStatesMinusPolygons[stateIndex - 1];
        }

        _availableRuleStates.Remove(stateIndex);

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

    public void Solve() {
        Log("-=-=-=-=-=-");
        Log("Solved!");
        _isSolved = true;
        _module.HandlePass();
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use '!{0} press <region> [at <digit>]' to press a region; regions are numbered 1-8 in reading order; "
                                                + "if a digit is specified, the region will be pressed when the last digit of the timer is that digit | "
                                                + "'!{0} hover <region>' to hover over a region; chain regions to hover over with spaces. "
                                                + "The following commands are section-specific: Section 5: '!{0} hinge <number>' to press a hinge; "
                                                + "hinges are numbered 1-7, going clockwise from the missing hinge | "
                                                + "Section 6: '!{0} <cb/colourblind/statuslight>' to toggle colourblind mode.";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command) {
        // Really should have just made an initial state, but too late now ig.
        if (_currentRuleState == null) {
            return _polygons.HandleTP(command);
        }
        return _currentRuleState.HandleTP(command);
    }


    private IEnumerator TwitchHandleForcedSolve() {
        if (_currentRuleState == null) {
            Regions[Random.Range(0, 8)].Selectable.OnInteract();
            yield return new WaitForSeconds(0.5f);
        }

        while (!_isSolved) {
            yield return _currentRuleState.Autosolve();
            yield return true;
        }
    }
}
