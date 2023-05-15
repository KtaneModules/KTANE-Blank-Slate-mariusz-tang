using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RuleStateController : MonoBehaviour {

    public abstract IEnumerator OnStateEnter(int pressedRegion);

    public abstract void HandleRegionPress(int pressedRegion);

}
