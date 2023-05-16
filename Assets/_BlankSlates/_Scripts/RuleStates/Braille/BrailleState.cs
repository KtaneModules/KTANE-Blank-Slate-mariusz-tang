using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

public class BrailleState : RuleStateController {

    // I have only included the braille characters used in this module since the word list is unlikely to change.
    private readonly Dictionary<string, List<int>> _braille = new Dictionary<string, List<int>> {
        { "a", new List<int> { 1 }},
        { "b", new List<int> { 1, 2 }},
        { "c", new List<int> { 1, 4 }},
        { "d", new List<int> { 1, 4, 5 }},
        { "e", new List<int> { 1, 5 }},
        { "f", new List<int> { 1, 2, 4 }},
        { "h", new List<int> { 1, 2, 5 }},
        { "i", new List<int> { 2, 4 }},
        { "k", new List<int> { 1, 3 }},
        { "l", new List<int> { 1, 2, 3 }},
        { "m", new List<int> { 1, 3, 4 }},
        { "n", new List<int> { 1, 3, 4, 5 }},
        { "o", new List<int> { 1, 3, 5 }},
        { "p", new List<int> { 1, 2, 3, 4 }},
        { "r", new List<int> { 1, 2, 3, 5 }},
        { "s", new List<int> { 2, 3, 4 }},
        { "t", new List<int> { 2, 3, 4, 5 }},
        { "u", new List<int> { 1, 3, 6 }},
        { "w", new List<int> { 2, 4, 5, 6 }},
        { "x", new List<int> { 1, 3, 4, 6 }},
        { "y", new List<int> { 1, 3, 4, 5, 6 }},
        { "ar", new List<int> { 3, 4, 5 }},
        { "ch", new List<int> { 1, 6 }},
        { "ea", new List<int> { 2 }},
        { "en", new List<int> { 2, 6 }},
        { "gh", new List<int> { 1, 2, 6 }},
        { "in", new List<int> { 3, 5 }},
        { "ou", new List<int> { 1, 2, 5, 6 }},
        { "ow", new List<int> { 2, 4, 6 }},
        { "sh", new List<int> { 1, 4, 6 }},
        { "st", new List<int> { 3, 4 }},
        { "th", new List<int> { 1, 4, 5, 6 }},
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
    private readonly int[][] _serialCharacterPositions = new int[][] {
        new int[] { 1, 2, 3 },
        new int[] { 4, 5, 6 },
        new int[] { 1, 3, 5 },
        new int[] { 2, 4, 6 },
        new int[] { 1, 3, 4 },
        new int[] { 2, 5, 6 },
        new int[] { 1, 4, 6 },
        new int[] { 2, 3, 5 },
    };
    private readonly Color[] _colours = new Color[] { Color.red, Color.green, Color.blue };

    [SerializeField] private BrailleDisplay _brailleGrid;

    private int _originRegionNumber;
    private int _targetRegionNumber;
    private int _wordNumber;

    private string[] _flashingWordSplit;
    private List<int>[] _flashingDots;

    private Coroutine _flashingSequence;

    public override IEnumerator OnStateEnter(Region pressedRegion) {
        yield return null;
        _originRegionNumber = pressedRegion.Number;
        _module.Log($"Region {_originRegionNumber} is cycling braille.");

        _targetRegionNumber = _module.AvailableRegions.PickRandom();

        _wordNumber = UnityEngine.Random.Range(0, 3);
        _flashingWordSplit = _wordsSplit[_targetRegionNumber - 1, _wordNumber];
        _flashingDots = GetFlashingDots(_flashingWordSplit);

        _module.Log($"The cycled word is \"{CombineWord(_flashingWordSplit)}\".");
        _module.Log($"The corresponding region is press is {_targetRegionNumber}.");

        transform.position = pressedRegion.transform.position;
        _flashingSequence = StartCoroutine(FlashSequence());
    }

    public override IEnumerator HandleRegionPress(Region pressedRegion) {
        int pressedPosition = pressedRegion.Number;

        StopFlashing();

        if (pressedPosition != _originRegionNumber) {
            if (pressedPosition == _targetRegionNumber) {
                // ! _module.GetNewState(pressedRegion);
                _module.Log("Correct!");
            }
            else {
                _module.Strike($"Incorrectly pressed region {pressedPosition}. Strike!");
                yield return new WaitForSeconds(0.5f);
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

    private List<int>[] GetFlashingDots(string[] flashingWordSplit) {
        var flashingDots = new List<int>[3];
        List<int> invertedPositions = GetInvertedPositions();

        for (int i = 0; i < 3; i++) {
            flashingDots[i] = _braille[flashingWordSplit[i]];

            foreach (int invertPosition in invertedPositions) {
                if (invertPosition > 6 * i && invertPosition <= 6 * (i + 1)) {
                    if (flashingDots[i].Contains(invertPosition - 6 * i)) {
                        flashingDots[i].Remove(invertPosition - 6 * i);
                    }
                    else {
                        flashingDots[i].Add(invertPosition - 6 * i);
                    }
                }
            }
        }

        return flashingDots;
    }

    private List<int> GetInvertedPositions() {
        var invertedPositionsReading = new List<int>();

        _module.Log("Inversions:");
        foreach (int serialPosition in _serialCharacterPositions[_originRegionNumber - 1]) {
            char character = _module.BombInfo.GetSerialNumber()[serialPosition - 1];
            int invertPosition;
            if (char.IsDigit(character)) {
                invertPosition = character - '0';
            }
            else {
                invertPosition = (character - 'A' + 1) % 18;
            }

            if (invertPosition == 0) {
                invertPosition = 1;
            }
            _module.Log($"The serial number character in position {serialPosition} is {character}, which yields position {invertPosition} in individual reading order.");

            if (invertedPositionsReading.Contains(invertPosition)) {
                invertedPositionsReading.Remove(invertPosition);
            }
            else {
                invertedPositionsReading.Add(invertPosition);
            }
        }

        // Convert from indivial reading order to individual braille order.
        List<int> invertedPositionsBraille = invertedPositionsReading.Select(p => p - p % 6 + (p % 6) / 2 + (p % 2 == 0 ? 3 : 1)).ToList();
        return invertedPositionsBraille;
    }

    private IEnumerator FlashSequence() {
        for (int i = 0; i < 3; i++) {
            _brailleGrid.Display(_flashingDots[i], _colours[i]);
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
