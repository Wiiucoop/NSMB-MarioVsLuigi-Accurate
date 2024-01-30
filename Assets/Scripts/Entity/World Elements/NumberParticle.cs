using System.Collections;

using UnityEngine;
using TMPro;

public class NumberParticle : MonoBehaviour {

    public TMP_Text text;

    public Color colorDefault = new Color(0.98F, 0.32F, 0F); // <--------------------------- DEFAULT COLOR

    public void ApplyColor(Color color) {
        //ACCURACY: IGNORE COLOR SET BY PLAYER, ALWAYS DISPLAY THE DEFAULT COLOR
        text.ForceMeshUpdate();
        MeshRenderer mr = GetComponentsInChildren<MeshRenderer>()[1];
        MaterialPropertyBlock mpb = new();
        mpb.SetColor("_Color", colorDefault);
        mr.SetPropertyBlock(mpb);
    }

    public void Kill() {
        Destroy(transform.parent.gameObject);
    }
}