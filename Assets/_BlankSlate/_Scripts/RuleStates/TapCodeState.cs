using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

public class TapCodeState : RuleStateController {

    // Do not account for K since it never appears in any word in the word list.
    private const string TAP_ALPHABET = "ABCDEFGHIJLMNOPQRSTUVWXYZ";
    private readonly string[][] _words = new string[][] {
        new string[] { "ROE", "LAW", "RAJ", "TEE", "NOT" },
        new string[] { "PHI", "JAY", "ORB", "MOL", "PUT" },
        new string[] { "RUE", "ONE", "LED", "NUN", "PAL" },
        new string[] { "ORE", "JUG", "SEE", "PEA", "LEG" },
        new string[] { "YES", "NOW", "TED", "HEN", "SAC" },
        new string[] { "WEE", "WRY", "MAC", "SON", "NIL" },
        new string[] { "RIB", "YEN", "TRY", "ZOO", "PIT" },
    };

    private string _tappedWord;
    private int _originRegionNumber;

    private Coroutine _playingTapCode;

    public override IEnumerator OnStateEnter(Region pressedRegion) {
        _originRegionNumber = pressedRegion.Number;
        _module.Log($"Region {_originRegionNumber} is playing tap code.");
        _targetRegionNumber = _module.AvailableRegions.PickRandom();

        int forwardDistance = (_targetRegionNumber - pressedRegion.Number + 8) % 8;
        _tappedWord = _words[forwardDistance - 1].PickRandom();

        _module.Log($"The word being transmitted is {_tappedWord}.");
        _module.Log($"The corresponding region to press is {_targetRegionNumber}.");

        transform.position = pressedRegion.transform.position;
        _playingTapCode = StartCoroutine(PlayTapCode());
        yield return null;
    }

    public override IEnumerator HandleRegionPress(Region pressedRegion) {
        int pressedPosition = pressedRegion.Number;

        StopPlaying();

        if (pressedPosition != _originRegionNumber) {
            if (pressedPosition == _targetRegionNumber) {
                _module.Log("Pressed the correct region!");
                _module.GetNewState(pressedRegion);
            }
            else {
                _module.Strike($"Incorrectly pressed region {pressedPosition}. Strike!");
                yield return new WaitForSeconds(0.5f);
                _playingTapCode = StartCoroutine(PlayTapCode());
            }
        }
        else {
            _playingTapCode = StartCoroutine(PlayTapCode());
        }
    }

    private IEnumerator PlayTapCode() {
        foreach (char letter in _tappedWord) {
            int position = TAP_ALPHABET.IndexOf(letter);
            int row = position / 5 + 1;
            int column = position % 5 + 1;

            for (int i = 0; i < row; i++) {
                _module.BombAudio.PlaySoundAtTransform("TapCode Tap", transform);
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < column; i++) {
                _module.BombAudio.PlaySoundAtTransform("TapCode Tap", transform);
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void StopPlaying() {
        if (_playingTapCode != null) {
            StopCoroutine(_playingTapCode);
        }
    }

    public override IEnumerator SolveAnimation() {
        _module.BombAudio.PlaySoundAtTransform("TapCode MiniTap", transform);
        return base.SolveAnimation();
    }
}
