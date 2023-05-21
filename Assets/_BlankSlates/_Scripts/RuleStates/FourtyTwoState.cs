using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class FourtyTwoState : RuleStateController {

    private readonly Vector3 _textSize = Vector3.one * 0.4f;

    [SerializeField] private Color[] _colours;
    [SerializeField] private TextMesh[] _textMeshes;

    private int _originRegionNumber;
    private int[][] _numberSequences;
    private Coroutine _cycling;

    public override IEnumerator OnStateEnter(Region pressedRegion) {
        _originRegionNumber = pressedRegion.Number;
        _targetRegionNumber = _module.AvailableRegions.PickRandom();
        _numberSequences = GenerateNumberSequences(_targetRegionNumber);
        _cycling = StartCoroutine(CycleNumbers());

        _module.Log("Numbers have started cycling on each region.");
        _module.Log($"Region {_targetRegionNumber}'s sequence contains 42. Press this region.");

        yield return null;
    }

    private int[][] GenerateNumberSequences(int targetRegionNumber) {
        var numberSequences = new int[8][];

        for (int i = 0; i < 8; i++) {
            numberSequences[i] = new int[10];
            for (int j = 0; j < 10; j++) {
                int newNumber = Rnd.Range(0, 99);
                if (newNumber >= 42) {
                    newNumber++;
                }
                numberSequences[i][j] = newNumber;
            }
        }

        int fourtyTwoPosition = Rnd.Range(0, 10);
        numberSequences[targetRegionNumber - 1][fourtyTwoPosition] = 42;

        return numberSequences;
    }

    private IEnumerator CycleNumbers() {
        _module.BombAudio.PlaySoundAtTransform("42 Hold", transform);
        int j = 0;
        while (true) {
            for (int i = 0; i < 8; i++) {
                if (i + 1 != _originRegionNumber) {
                    _textMeshes[i].text = $"{_numberSequences[i][j]:00}";
                    _textMeshes[i].color = _colours.PickRandom();
                }
            }
            yield return new WaitForSeconds(1);
            j++;
            j %= 10;
        }
    }

    public override IEnumerator HandleRegionPress(Region pressedRegion) {
        int pressedNumber = pressedRegion.Number;

        if (pressedNumber == _targetRegionNumber) {
            StopCoroutine(_cycling);
            if (!_module.ReadyToSolve) {
                Array.ForEach(_textMeshes, t => t.text = string.Empty);
            }
            _module.Log("Pressed the correct region!");
            _module.GetNewState(pressedRegion);
        }
        else {
            _module.Strike($"Incorrectly pressed region {pressedNumber}. Strike!");
        }

        yield return null;
    }

    public override IEnumerator SolveAnimation() {
        float elapsedTime = 0;

        yield return StartCoroutine(base.SolveAnimation());

        _module.BombAudio.PlaySoundAtTransform("42 Tick", transform);
        while (elapsedTime <= 1) {
            elapsedTime += Time.deltaTime;
            Array.ForEach(_textMeshes, t => t.transform.localScale = _textSize * Mathf.Lerp(1, 0, elapsedTime));
            yield return null;
        }
    }

}
