using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

public class CrackState : RuleStateController {

    [SerializeField] private MeshRenderer _moduleRenderer;
    [SerializeField] private Texture[] _crackTextures;
    [SerializeField] private Texture[] _solveTextures;

    private Texture _originalTexture;
    private int _originRegionNumber;
    private int _targetTime;

    private bool _isPassing;

    public override IEnumerator OnStateEnter(Region pressedRegion) {
        _originRegionNumber = pressedRegion.Number;
        _originalTexture = _moduleRenderer.material.GetTexture("_MainTex");
        _moduleRenderer.material.SetTexture("_MainTex", _crackTextures[_originRegionNumber - 1]);

        transform.position = pressedRegion.transform.position;
        _module.BombAudio.PlaySoundAtTransform("Crack Forward", transform);

        _module.Log($"The module cracked around region {_originRegionNumber}.");
        _targetRegionNumber = GetTargetRegionNumber(_originRegionNumber);
        _targetTime = _module.BombInfo.GetSerialNumberNumbers().First();
        _module.Log($"Press region {_targetRegionNumber} when the last digit of the timer is {_targetTime}.");
        yield return null;
    }

    private int GetTargetRegionNumber(int originRegionNumber) {
        int target = 0;
        IEnumerable<int> serialDigits = _module.BombInfo.GetSerialNumberNumbers();
        foreach (int digit in serialDigits) {
            target += digit;
        }
        target %= 9;

        if (target == 0) {
            target = (originRegionNumber - serialDigits.Last() + 16) % 8;
            if (target == 0) {
                target = 8;
            }
        }

        while (!_module.AvailableRegions.Contains(target)) {
            target += target <= 5 ? 3 : -5;
        }

        return target;
    }

    public override IEnumerator HandleRegionPress(Region pressedRegion) {
        yield return null;
        int pressedNumber = pressedRegion.Number;
        int pressedTime = Mathf.FloorToInt(_module.BombInfo.GetTime()) % 10;

        if (pressedNumber != _targetRegionNumber) {
            _module.Strike($"Incorrectly pressed region {pressedNumber}. Strike!");
        }
        else if (pressedTime != _targetTime) {
            _module.Strike($"Pressed the correct region when the last digit was {pressedTime}. Strike!");
        }
        else {
            if (!_module.ReadyToSolve) {
                _isPassing = true;
                _module.BombAudio.PlaySoundAtTransform("Crack Backward", transform);
                yield return new WaitForSeconds(1);
                _moduleRenderer.material.SetTexture("_MainTex", _originalTexture);
            }
            _module.Log("Pressed the correct region at the right time!");
            _module.GetNewState(pressedRegion);
        }
    }

    public override IEnumerator SolveAnimation() {
        _isPassing = true;
        _module.BombAudio.PlaySoundAtTransform("Crack Forward", transform);
        _moduleRenderer.material.SetTexture("_MainTex", _solveTextures[_originRegionNumber - 1]);
        yield return new WaitForSeconds(2);
        yield return StartCoroutine(base.SolveAnimation());
    }

    public override IEnumerator Autosolve() {
        if (_isPassing) {
            yield return null;
            yield break;
        }
        while (Mathf.FloorToInt(_module.BombInfo.GetTime()) % 10 != _targetTime) {
            yield return null;
        }
        yield return base.Autosolve();
    }
}
