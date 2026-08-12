using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryCameraPositioner : MonoBehaviour {
    bool destroyed = false;
    public void UpdatePosition() {
        if (destroyed)
            return;

        if (GameManager.Instance) {
            if (!GameManager.Instance.loopingLevel) {
                Destroy(gameObject);
                destroyed = true;
                return;
            }
            //ACCURACY: LOCAL SPLIT-SCREEN. Use our own rig's tracking camera instead of the shared Camera.main.
            bool right = transform.parent.position.x > GameManager.Instance.GetLevelMiddleX();
            transform.localPosition = new Vector3(GameManager.Instance.levelWidthTile * (right ? -1 : 1), 0, 0);
        }
    }
}