using UnityEngine;
using UnityEngine.UI;

public class FadeOutManager : MonoBehaviour
{
    public float fadeTimer, fadeTime, blackTime, totalTime;
    public Animator anim;
    public bool alreadyInitiallyFadedOut;
    private Coroutine fadeCoroutine;
    private bool isMario = false;
    private Image image;

    public void Start()
    {
        image = GetComponent<Image>();
        anim = GetComponent<Animator>();
    }


    public void SetInvisible(bool how)
    {
        transform.localScale = how ? Vector3.zero : new Vector3(1f, 1f, 1f);
    }

    public void SetIsMario(bool isMarioPlayer)
    {
        isMario = isMarioPlayer;
        if(isMario){
            anim.SetTrigger("isMario");
        }
    }

    public void FadeOutAndIn(float fadeTime, float blackTime)
    {
        anim.speed = 1;
        anim.SetTrigger("FadeInAndOut");
    }

    public void FadeOut()
    {
        anim.speed = 1;
        if (alreadyInitiallyFadedOut) return;
        anim.SetTrigger("FadeOut");
        alreadyInitiallyFadedOut = true;
    }

    public void FadePipe(bool direction)
    {
        anim.SetBool("PipeDirection", direction);
        anim.SetTrigger("FadePipe");
    }
}