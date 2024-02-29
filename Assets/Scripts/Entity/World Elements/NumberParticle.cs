using UnityEngine.Serialization;
using UnityEngine;
using TMPro;

public class NumberParticle : MonoBehaviour
{
    public TMP_Text text;

    [SerializeField, FormerlySerializedAs("animation")] private Animation legacyAnimation;

    [SerializeField] private Vector3 colorOffset;
    [SerializeField] private Color overlay;

    private MaterialPropertyBlock mpb;
    private MeshRenderer mr;

    public Color colorDefault = new Color(0.98F, 0.32F, 0F); // Default color
    private Color targetColor = Color.white; // Target color for transition
    public float transitionDuration = 1f; // Duration of each color transition
    private float transitionTimer = 0f; // Timer for tracking the transition duration

    public void ApplyColor(Color color)
    {
        //ACCURACY: IGNORE COLOR SET BY PLAYER, ALWAYS DISPLAY THE DEFAULT COLOR
        text.ForceMeshUpdate();
        mr = GetComponentsInChildren<MeshRenderer>()[1];
        mpb = new MaterialPropertyBlock();
        mpb.SetColor("_Color", colorDefault);
        mr.SetPropertyBlock(mpb);

        legacyAnimation = GetComponentInChildren<Animation>();

    }

    public void Update()
    {
        if(text.text == "<sprite=35>"){
            legacyAnimation.enabled = true;
            // Increment transition timer
            transitionTimer += Time.deltaTime;

            // Calculate transition percentage
            float t = Mathf.PingPong(transitionTimer / transitionDuration, 1f);

            // Update color
            Color lerpedColor = Color.Lerp(targetColor, colorDefault, t);

            // Apply color to the text
            mr.GetPropertyBlock(mpb);
            mpb.SetVector("_ColorOffset", colorOffset);
            mpb.SetColor("_Color", lerpedColor);
            mr.SetPropertyBlock(mpb);

            // Check if transition is complete
            if (transitionTimer >= transitionDuration)
            {
                // Swap target color and reset transition timer
                targetColor = (targetColor == Color.white) ? colorDefault : Color.white;
                transitionTimer = 0f;
            }
        }
    }

    public void Kill()
    {
        Destroy(transform.parent.gameObject);
    }
}
