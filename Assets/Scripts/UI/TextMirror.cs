using TMPro;
using UnityEngine;

//ACCURACY: LOCAL SPLIT-SCREEN. Passive visual copy of another TMP_Text's text and font size, so a duplicated HUD
//element (e.g. a second stars/lives display near Luigi's half of the screen) stays in sync without needing any
//changes to whatever already updates the original.
public class TextMirror : MonoBehaviour {
    [SerializeField] private TMP_Text source;
    private TMP_Text target;

    public void Awake() {
        target = GetComponent<TMP_Text>();
    }

    public void LateUpdate() {
        target.text = source.text;
        target.fontSize = source.fontSize;
    }
}
