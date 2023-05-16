using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonsState : RuleStateController {

    [SerializeField] GameObject[] _firstPair;
    [SerializeField] GameObject[] _secondPair;
    [SerializeField] GameObject[] _thirdPair;
    [SerializeField] GameObject[] _fourthPair;

    public override IEnumerator HandleRegionPress(Region pressedRegion) {
        throw new System.NotImplementedException();
    }

    public override IEnumerator OnStateEnter(Region pressedRegion) {
        throw new System.NotImplementedException();
    }
}
