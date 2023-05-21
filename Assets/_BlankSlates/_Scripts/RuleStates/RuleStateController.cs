using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RuleStateController : MonoBehaviour {

    protected BlankSlatesModule _module;
    protected int _targetRegionNumber;

    protected void Awake() {
        _module = GetComponentInParent<BlankSlatesModule>();
    }

    // On second thought, OnStateEnter and HandleRegionPress should not have been coroutines by default, but oh well.
    public abstract IEnumerator OnStateEnter(Region pressedRegion);

    public abstract IEnumerator HandleRegionPress(Region pressedRegion);

    public virtual IEnumerator SolveAnimation() {
        _module.Solve();
        yield return null;
    }

    public virtual IEnumerator HandleTP(string command) {
        // The yield breaks are necessary after sendtochaterrors because it is possible for an overriding method to
        // yield return null before calling this base method.
        string[] splitCommands = command.Trim().ToUpper().Split(' ');

        if (splitCommands.Length < 2 || splitCommands[1].Length != 1 || !char.IsDigit(char.Parse(splitCommands[1]))) {
            yield return "sendtochaterror Invalid command!";
            yield break;
        }

        int firstDigit = int.Parse(splitCommands[1]);

        if (firstDigit < 1 || firstDigit > 8) {
            yield return $"sendtochaterror '{firstDigit}' is not a valid region!";
            yield break;
        }

        if (splitCommands[0] == "PRESS") {
            if (splitCommands.Length == 2) {
                yield return null;
                _module.Regions[firstDigit - 1].Selectable.OnInteract();
            }
            else if (splitCommands.Length == 4 && splitCommands[2] == "AT" && splitCommands[3].Length == 1 && char.IsDigit(char.Parse(splitCommands[3]))) {
                yield return null;
                while (Mathf.FloorToInt(_module.BombInfo.GetTime()) % 10 != int.Parse(splitCommands[3])) {
                    yield return "trycancel";
                }
                _module.Regions[firstDigit - 1].Selectable.OnInteract();
            }
            else {
                yield return "sendtochaterror Invalid command!";
                yield break;
            }
        }
        else if (splitCommands[0] == "HOVER") {
            var hoverDigits = new List<int>();

            for (int i = 1; i < splitCommands.Length; i++) {
                if (splitCommands[i].Length != 1 || !char.IsDigit(char.Parse(splitCommands[i])) || splitCommands[i] == "9" || splitCommands[i] == "0") {
                    yield return $"sendtochaterror '{splitCommands[i]}' is not a valid region!";
                    yield break;
                }
                hoverDigits.Add(int.Parse(splitCommands[i]));
            }

            yield return null;
            foreach (int digit in hoverDigits) {
                _module.Regions[digit - 1].Selectable.OnHighlight();
                yield return new WaitForSeconds(1);
                _module.Regions[digit - 1].Selectable.OnHighlightEnded();
                yield return "trycancel";
            }
        }
        else {
            yield return "sendtochaterror Invalid command!";
            yield break;
        }
    }

    public virtual IEnumerator Autosolve() {
        _module.Regions[_targetRegionNumber - 1].Selectable.OnInteract();
        yield return new WaitForSeconds(0.5f);
    }
}
