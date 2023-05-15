using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrailleState : RuleStateController {

    // I have only included the braille characters used in this module since the word list is unlikely to change.
    private readonly Dictionary<string, int[]> _braille = new Dictionary<string, int[]> {
        { "a", new int[] { 1 }},
        { "b", new int[] { 1, 2 }},
        { "c", new int[] { 1, 4 }},
        { "d", new int[] { 1, 4, 5 }},
        { "e", new int[] { 1, 5 }},
        { "f", new int[] { 1, 2, 4 }},
        { "h", new int[] { 1, 2, 5 }},
        { "i", new int[] { 2, 4 }},
        { "k", new int[] { 1, 3 }},
        { "l", new int[] { 1, 2, 3 }},
        { "m", new int[] { 1, 3, 4 }},
        { "n", new int[] { 1, 3, 4, 5 }},
        { "o", new int[] { 1, 3, 5 }},
        { "p", new int[] { 1, 2, 3, 4 }},
        { "r", new int[] { 1, 2, 3, 5 }},
        { "s", new int[] { 2, 3, 4 }},
        { "t", new int[] { 2, 3, 4, 5 }},
        { "u", new int[] { 1, 3, 6 }},
        { "w", new int[] { 2, 4, 5, 6 }},
        { "x", new int[] { 1, 3, 4, 6 }},
        { "y", new int[] { 1, 3, 4, 5, 6 }},
        { "ar", new int[] { 3, 4, 5 }},
        { "ch", new int[] { 1, 6 }},
        { "ea", new int[] { 2 }},
        { "en", new int[] { 2, 6 }},
        { "gh", new int[] { 1, 2, 6 }},
        { "in", new int[] { 3, 5 }},
        { "ou", new int[] { 1, 2, 5, 6 }},
        { "ow", new int[] { 2, 4, 6 }},
        { "sh", new int[] { 1, 4, 6 }},
        { "st", new int[] { 3, 4 }},
        { "th", new int[] { 1, 4, 5, 6 }},
    };
    private readonly string[,][] _wordsSplit = new string[,][] {
        { new string[] { "en", "d", "s" }, new string[] { "d", "ow", "n" }, new string[] { "t", "ea", "m" } },
        { new string[] { "ow", "n", "s" }, new string[] { "y", "e", "s" }, new string[] { "b", "o", "x" } },
        { new string[] { "l", "in", "e" }, new string[] { "r", "u", "n" }, new string[] { "c", "en", "t" } },
        { new string[] { "sh", "o", "t" }, new string[] { "e", "ch", "o" }, new string[] { "ea", "s", "y" } },
        { new string[] { "s", "i", "r" }, new string[] { "s", "a", "y" }, new string[] { "st", "ar", "s" } },
        { new string[] { "h", "a", "t" }, new string[] { "h", "o", "st" }, new string[] { "f", "a", "n" } },
        { new string[] { "b", "e", "e" }, new string[] { "w", "ea", "k" }, new string[] { "w", "e", "t" } },
        { new string[] { "sh", "ow", "n" }, new string[] { "p", "o", "p" }, new string[] { "th", "ou", "gh" } },
    };
    private readonly Color[] _colours = new Color[] { Color.red, Color.green, Color.blue };

    [SerializeField] private BrailleDisplay _brailleGrid;

    private int _originRegion;
    private int _targetRegion;
    private int _wordNumber;
    private string[] _flashingWordSplit;

    private Coroutine _flashingSequence;

    public override IEnumerator HandleRegionPress(Region pressedRegion) {
        int pressedPosition = pressedRegion.Position;

        StopFlashing();

        if (pressedPosition != _originRegion) {
            if (pressedPosition == _targetRegion) {
                // _module.GetNewState(pressedRegion);
                _module.Log("Correct!");
            }
            else {
                _module.Strike($"Incorrectly pressed region {pressedPosition}. Strike!");
                yield return new WaitForSeconds(1);
                _flashingSequence = StartCoroutine(FlashSequence());
            }
        }
        else {
            _flashingSequence = StartCoroutine(FlashSequence());
        }
    }

    private void StopFlashing() {
        if (_flashingSequence != null) {
            StopCoroutine(_flashingSequence);
            _brailleGrid.Clear();
        }
    }

    public override IEnumerator OnStateEnter(Region pressedRegion) {
        yield return null;
        _originRegion = pressedRegion.Position;
        _module.Log("The pressed region is cycling braille.");

        _targetRegion = _module.AvailableRegions.PickRandom();
        _module.AvailableRegions.Remove(_targetRegion);

        _wordNumber = UnityEngine.Random.Range(0, 3);
        _flashingWordSplit = _wordsSplit[_targetRegion - 1, _wordNumber];

        _module.Log($"The cycled word is {CombineWord(_flashingWordSplit)}.");
        _module.Log($"The corresponding region is press is {_targetRegion}.");

        transform.position = pressedRegion.transform.position;
        _flashingSequence = StartCoroutine(FlashSequence());
    }

    private IEnumerator FlashSequence() {
        Debug.Log("Flashing");
        for (int i = 0; i < 3; i++) {
            _brailleGrid.Display(_braille[_flashingWordSplit[i]], _colours[i]);
            yield return new WaitForSeconds(0.8f);
        }
        _brailleGrid.Clear();
    }

    private string CombineWord(string[] parts) {
        if (parts.Length != 3) {
            throw new RankException("Each word is composed of exactly three parts.");
        }

        return $"{parts[0]}{parts[1]}{parts[2]}";
    }
}
