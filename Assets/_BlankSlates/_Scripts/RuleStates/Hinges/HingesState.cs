using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class HingesState : RuleStateController {

    private readonly int[,] _valueTable = new int[,] {
        { 7, 1, 2, 8, 4, 6, 5, 3 },
        { 1, 4, 8, 3, 6, 5, 7, 2 },
        { 2, 7, 1, 4, 3, 8, 6, 5 },
        { 8, 6, 5, 7, 2, 1, 3, 4 },
        { 4, 5, 6, 2, 7, 3, 1, 8 },
        { 3, 8, 7, 5, 1, 2, 4, 6 },
        { 5, 3, 4, 6, 8, 7, 2, 1 },
        { 6, 2, 3, 1, 5, 4, 8, 7 },
    };

    [SerializeField] private Hinge[] _hinges;

    // This is referring to the hinge stored in the first element of _hinges.
    private int _valueOfFirstHinge;

    private int _lowHinge;
    private int _highHinge;
    private int _targetRegionNumber;

    private void Start() {
        Array.ForEach(_hinges, h => h.SetSelectableActive(false));
    }

    public override IEnumerator OnStateEnter(Region pressedRegion) {
        Array.ForEach(_hinges, h => h.SetSelectableActive(true));
        int hingeToKill = Rnd.Range(0, 8);

        _valueOfFirstHinge = pressedRegion.Number - hingeToKill;
        if (_valueOfFirstHinge < 1) {
            _valueOfFirstHinge += 8;
        }

        _targetRegionNumber = _module.AvailableRegions.PickRandom();
        _highHinge = Enumerable.Range(0, 8).Where(i => _valueTable[i, hingeToKill] != _targetRegionNumber && _valueTable[i, i] != _targetRegionNumber).PickRandom() + 1;
        _lowHinge = Enumerable.Range(0, 8).First(i => _valueTable[_highHinge - 1, i] == _targetRegionNumber) + 1;
        Array.ForEach(_hinges, h => h.Selectable.OnInteract += delegate () { HandleHingePress(h); return false; });

        yield return StartCoroutine(KillHinge(_hinges[hingeToKill]));

        _module.Log($"Pressing region {pressedRegion.Number} caused a hinge to fall off.");
        _module.Log($"Hinge {_lowHinge} plays a lower-pitch sound.");
        _module.Log($"Hinge {_highHinge} plays a higher-pitch sound.");
        _module.Log($"The corresponding region to press is {_targetRegionNumber}.");
    }

    private void HandleHingePress(Hinge theHingeInQuestion) {
        int hingeValue = (_valueOfFirstHinge + theHingeInQuestion.Number - 1) % 8;
        if (hingeValue == 0) {
            hingeValue += 8;
        }

        if (hingeValue == _lowHinge) {
            _module.BombAudio.PlaySoundAtTransform("Hinges Tap Low", _hinges[theHingeInQuestion.Number - 1].transform);
        }
        else if (hingeValue == _highHinge) {
            _module.BombAudio.PlaySoundAtTransform("Hinges Tap High", _hinges[theHingeInQuestion.Number - 1].transform);
        }
    }

    private IEnumerator KillHinge(Hinge theHingeInQuestion) {
        _module.BombAudio.PlaySoundAtTransform("Hinge Rip", theHingeInQuestion.transform);
        yield return new WaitForSeconds(0.85f);
        Destroy(theHingeInQuestion.gameObject);
    }

    public override IEnumerator HandleRegionPress(Region pressedRegion) {
        int pressedNumber = pressedRegion.Number;

        if (pressedNumber == _targetRegionNumber) {
            _module.Log("Pressed the correct region!");
            Array.ForEach(_hinges, h => h.SetSelectableActive(false));
            // ! _module.GetNewState(pressedRegion);
            _module.Log("Correct!");
        }
        else {
            _module.Strike($"Incorrectly pressed region {pressedNumber}. Strike!");
        }

        yield return null;
    }

}
