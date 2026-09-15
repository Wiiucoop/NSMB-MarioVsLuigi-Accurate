using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using NSMB.Utils;


public class UIUpdater : MonoBehaviour {

    public static UIUpdater Instance;
    public GameObject playerTrackTemplate, starTrackTemplate, storedItem, PingObject;
    public PlayerController player, other;
    public Sprite storedItemNull;
    public TMP_Text uiStar1,uiStar2,uiStar3,uiStar4,uiStar5, uiCoins, uiDebug, uiLives, uiCountdown;

    public TMP_Text p2UiStar1,p2UiStar2,p2UiStar3,p2UiStar4,p2UiStar5, p2UiLives;
    public Image itemReserve, itemColor;
    public Image itemReserveP2; //ACCURACY: LOCAL SPLIT-SCREEN - Luigi's reserve item box (Mario keeps the original itemReserve/itemColor)
    public float pingSample = 0;

    public bool isLocalGame = false;


//---Static Variables
    private static readonly int ParamIn = Animator.StringToHash("in");
    private static readonly int ParamOut = Animator.StringToHash("out");
    private static readonly int ParamHasItem = Animator.StringToHash("has-item");
    [SerializeField] private Animator reserveAnimator;
    private Powerup previousPowerup;
    [SerializeField] private Animator reserveAnimatorP2;
    private Powerup previousPowerupP2;

    private Material timerMaterial;
    private GameObject starsParent, coinsParent, livesParent, p2LivesParent, timerParent;

    public GameObject middleColumnParent;
    public GameObject leftColumnLocalCopy, rightColumnLocalCopy; //ACCURACY: LOCAL SPLIT-SCREEN - passive TextMirror copies near Luigi's half, inactive/unused online
    public GameObject pingLocalCopy; //ACCURACY: LOCAL SPLIT-SCREEN - passive TextMirror copy of the ping indicator, moved near center

    public RectTransform leftColumnParent, rightColumnParent, track1, track2;
    private readonly List<Image> backgrounds = new();
    private bool uiHidden;
    private string pingIcon;
    private bool is1v1 = false;
    private bool shouldAnimate = false;
    private bool shouldP2Animate = false;
    private bool isP2AnimationRunning = true;

    private bool isP2LifeAnimationRunning = false;

    private int coins = -1, coins2 = -1, stars = -1, p2stars = -1, lives = -1, p2lives = -1, timer = -1;

    public void Start() {
        Instance = this;
        pingSample = PhotonNetwork.GetPing();
        pingIcon = "<sprite=49>"; 

       // starsParent = uiStars.transform.parent.gameObject;
        coinsParent = uiCoins.transform.parent.gameObject;
        livesParent = uiLives.transform.parent.gameObject;
        p2LivesParent = p2UiLives.transform.parent.gameObject;
        timerParent = uiCountdown.transform.parent.gameObject;

       // backgrounds.Add(starsParent.GetComponentInChildren<Image>());
        backgrounds.Add(coinsParent.GetComponentInChildren<Image>());
        backgrounds.Add(livesParent.GetComponentInChildren<Image>());
        backgrounds.Add(timerParent.GetComponentInChildren<Image>());

        isLocalGame = GameManager.Instance.isLocalGame;

        if(isLocalGame){
            middleColumnParent.transform.position += new Vector3(-10f, 28f, 0f);
            uiCountdown.text = Utils.GetSymbolString("C" + "0" + "/" + GameManager.Instance.coinRequirement);

            //ACCURACY: LOCAL SPLIT-SCREEN. Timer no longer lives inside middleColumnParent (moved out for the screen-center
            //fix), so scale it and coinsParent separately rather than scaling middleColumnParent as a whole.
            coinsParent.transform.localScale = Vector3.one * 0.8f;
            timerParent.transform.localScale = Vector3.one * 0.8f;

            //ACCURACY: LOCAL SPLIT-SCREEN. Mario keeps the original reserve box, just moved to right-center so it isn't
            //sitting inside Luigi's half of the screen (online keeps its default bottom-right position, untouched).
            //Luigi gets a second box (itemReserveP2), which defaults to inactive and to Mario's old bottom-right
            //spot in the prefab - just activate it here.
            RectTransform marioReserveRect = (RectTransform) itemReserve.transform.parent;
            marioReserveRect.anchorMin = marioReserveRect.anchorMax = marioReserveRect.pivot = new Vector2(1f, 0.5f);
            marioReserveRect.anchoredPosition = new Vector2(-10f, 40f);

            itemReserveP2.transform.parent.gameObject.SetActive(true);

            //ACCURACY: LOCAL SPLIT-SCREEN. Stars/lives stay left/right like the originals, just duplicated lower down
            //so Luigi's half of the split screen has its own visible copy. These are passive TextMirror copies -
            //the actual tracking/update logic above is untouched.
            leftColumnLocalCopy.SetActive(true);
            rightColumnLocalCopy.SetActive(true);

            leftColumnParent.transform.localScale = Vector3.one * 0.9f;
            rightColumnParent.transform.localScale = Vector3.one * 0.9f;
            leftColumnLocalCopy.transform.localScale = Vector3.one * 0.9f;
            rightColumnLocalCopy.transform.localScale = Vector3.one * 0.9f;

            //ACCURACY: LOCAL SPLIT-SCREEN. Ping indicator, duplicated the same passive-mirror way, moved near center.
            pingLocalCopy.SetActive(true);
        }

        



        foreach (Image bg in backgrounds)
            bg.color = GameManager.Instance.levelUIColor;
        itemColor.color = new(GameManager.Instance.levelUIColor.r - 0.2f, GameManager.Instance.levelUIColor.g - 0.2f, GameManager.Instance.levelUIColor.b - 0.2f, GameManager.Instance.levelUIColor.a);
    }





    public void loadOtherPlayer(IEnumerable<PlayerController> players) {
        is1v1 = true;
        foreach (PlayerController player in players) {
            if (!player)
                continue;

     
        //ACCURACY: LOAD PLAYER 2
            if(player != GameManager.Instance.localPlayer.GetComponent<PlayerController>()){
                other = player;
            }
            other = player;



         
        }

       
    }

    public void Update() {
        pingSample = Mathf.Lerp(pingSample, PhotonNetwork.GetPing(), Mathf.Clamp01(Time.unscaledDeltaTime * 0.5f));
        if (pingSample == float.NaN)
            pingSample = 0;

        if(pingSample <= 70){
            pingIcon = "<sprite=49>"; 
        }else if(pingSample > 70 && pingSample <= 120){
            pingIcon = "<sprite=50>"; 
        }else if(pingSample > 120 && pingSample <= 170){
            pingIcon = "<sprite=51>"; 
        }else{
            pingIcon = "<sprite=52>"; 
        }

        uiDebug.text = ""+pingIcon;

       //+" " + (int) pingSample + "ms</font>"



        //Player stuff update.//
        if (!player && GameManager.Instance.localPlayer)
            player = GameManager.Instance.localPlayer.GetComponent<PlayerController>();

        if (!player) {
            if (!uiHidden)
                ToggleUI(true);

            return;
        }

        if (uiHidden)
            ToggleUI(false);

        UpdateStoredItemUI();
        UpdateTextUI();
        if(is1v1){
            UpdateP2TextUI();
        }
    }

    private void ToggleUI(bool hidden) {
        uiHidden = hidden;

        starsParent.SetActive(!hidden);
        livesParent.SetActive(!hidden);
        coinsParent.SetActive(!hidden);
        timerParent.SetActive(!hidden);
    }

     private void UpdateStoredItemUI() {
        if (player)
            UpdateReserveDisplay(player.storedPowerup, itemReserve, reserveAnimator, ref previousPowerup);

        //ACCURACY: LOCAL SPLIT-SCREEN. Luigi's box, only tracked/shown in local play.
        if (isLocalGame && other)
            UpdateReserveDisplay(other.storedPowerup, itemReserveP2, reserveAnimatorP2, ref previousPowerupP2);
    }

    private void UpdateReserveDisplay(Powerup powerup, Image reserveImage, Animator animator, ref Powerup previousPowerupField) {
        animator.SetBool(ParamHasItem, powerup && powerup.reserveSprite);

        if (!powerup) {
            if (previousPowerupField != powerup) {
                animator.SetTrigger(ParamOut);
                previousPowerupField = powerup;
            }
            return;
        }

        reserveImage.sprite = powerup.reserveSprite ? powerup.reserveSprite : storedItemNull;
        if (previousPowerupField != powerup) {
            animator.SetTrigger(ParamIn);
            previousPowerupField = powerup;
        }
    }

    public void OnReserveItemStaticStarted() {
        itemReserve.sprite = storedItemNull;
    }

    public void OnReserveItemStaticStartedP2() {
        itemReserveP2.sprite = storedItemNull;
    }



private System.Collections.IEnumerator LastLifeAnimation()
{
    //ACCURACY: LOCAL SPLIT-SCREEN. Blank the text instead of toggling livesParent's active state, so the duplicated
    //column (a passive TextMirror of uiLives) blinks in sync too, instead of only the original.
    string onText = uiLives.text;
    yield return new WaitForSeconds(4.5f);
    while(player.lives == 1){
        yield return new WaitForSeconds(0.2f);
        uiLives.text = "";
        yield return new WaitForSeconds(0.2f);
        uiLives.text = onText;
        yield return null;
    }
}

private System.Collections.IEnumerator p2LastLifeAnimation()
{
    isP2LifeAnimationRunning = true;
    string onText = p2UiLives.text;
    yield return new WaitForSeconds(4.5f);
    while(other.lives == 1){
        yield return new WaitForSeconds(0.2f);
        p2UiLives.text = "";
        yield return new WaitForSeconds(0.2f);
        p2UiLives.text = onText;
        yield return null;
    }
}

private System.Collections.IEnumerator WavyAnimation(TMP_Text uiStar)
{
    float originalSize = uiStar.fontSize;
    float newSize = originalSize * 1.5f;  // Adjust the scale factor as needed
    float frequency = 1.0f;
    if(uiStar.text.Contains("<sprite=26>")){
        frequency = 1.12f;  // Adjust the frequency of the wavy for the 5-STAR SYMBOL
    }
    
    float elapsedTime = 0f; //

    while (shouldAnimate)
    {
        float t = Mathf.Sin(elapsedTime * frequency * 2 * Mathf.PI) * 0.5f + 0.5f; // Sine function for wavy effect
        float lerpedSize = Mathf.Lerp(originalSize, newSize, t);
        uiStar.fontSize = lerpedSize;

        elapsedTime += Time.deltaTime;
        yield return null;
    }
    uiStar.fontSize = originalSize;
}

private System.Collections.IEnumerator p2WavyAnimation(TMP_Text uiStar)
{
    isP2AnimationRunning = true;
    float originalSize = 40; //Hardcoded due to P2 loop issues
    //Debug.Log(uiStar.fontSize);
    float newSize = originalSize * 1.5f;  // Adjust the scale factor as needed
    float frequency = 1.0f;
    if(uiStar.text.Contains("<sprite=26>")){
        frequency = 1.12f;  // Adjust the frequency of the wavy for the 5-STAR SYMBOL
    }
    
    float elapsedTime = 0f; //

    while (shouldP2Animate)
    {
        float t = Mathf.Sin(elapsedTime * frequency * 2 * Mathf.PI) * 0.5f + 0.5f; // Sine function for wavy effect
        float lerpedSize = Mathf.Lerp(originalSize, newSize, t);
        uiStar.fontSize = lerpedSize;

        elapsedTime += Time.deltaTime;
        yield return null;
    }
    uiStar.fontSize = originalSize;
}

private System.Collections.IEnumerator DelayWinningStarAnim(string p1)
{
    if(p1.Equals("p1")){
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(WavyAnimation(uiStar1));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(WavyAnimation(uiStar2));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(WavyAnimation(uiStar3));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(WavyAnimation(uiStar4));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(WavyAnimation(uiStar5));
    }else if(p1.Equals("p2") && !isP2AnimationRunning){
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(p2WavyAnimation(p2UiStar1));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(p2WavyAnimation(p2UiStar2));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(p2WavyAnimation(p2UiStar3));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(p2WavyAnimation(p2UiStar4));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(p2WavyAnimation(p2UiStar5));
    }
}


private void flushStars(string p1) {
    if(p1.Equals("p1")){
        uiStar2.text = "";
        uiStar3.text = "";
        uiStar4.text = "";
        uiStar5.text = "";
    }else if(p1.Equals("p2")){
        p2UiStar1.text = "";
        p2UiStar2.text = "";
        p2UiStar3.text = "";
        p2UiStar4.text = "";
        p2UiStar5.text = "";
    }

}

    private void UpdateTextUI() {
        if (!player || GameManager.Instance.gameover)
            return;
//ACCURACY: STAR ICON as counter instead of numbers



    if (player.stars != stars)
    {

        

        stars = player.stars;
        string singlestar = Utils.GetSymbolString("s");
        string fivestar = Utils.GetSymbolString("S");
        string starcount = "";


       
        for (int i = 0; i < player.stars; i++)
        {

            if (player.stars == GameManager.Instance.starRequirement-1)
            {//ONE STAR LEFT TO END
                shouldAnimate = true;
                flushStars("p1");
                if(GameManager.Instance.starRequirement == 3){
                    uiStar1.text = singlestar;
                    uiStar2.text = singlestar;
                }else if(GameManager.Instance.starRequirement == 5){
                    uiStar1.text = singlestar;
                    uiStar2.text = singlestar;
                    uiStar3.text = singlestar;
                    uiStar4.text = singlestar;                
                }else if(GameManager.Instance.starRequirement == 10){
                    uiStar1.text = fivestar;
                    uiStar2.text = singlestar;
                    uiStar3.text = singlestar;
                    uiStar4.text = singlestar;   
                    uiStar5.text = singlestar;
                }
    
                StartCoroutine(DelayWinningStarAnim("p1"));
                break;
            }
            
            else if (i == 4)
            {//FIVE STARS
               flushStars("p1");
               starcount = fivestar;

            }
            else if (i > 4)
            {//SIX OR MORE STARS
               flushStars("p1");
               starcount = fivestar += singlestar;
            }
            else
            {//ONE STAR UNTIL 4 STARS
                flushStars("p1");
                starcount += singlestar;
            }
        }
        if(player.stars != GameManager.Instance.starRequirement-1){
            uiStar1.text = starcount;
            shouldAnimate = false;
        }
        
    }
        if (player.coins != coins) {
            coins = player.coins;
            uiCoins.text = Utils.GetSymbolString("C" + coins + "/" + GameManager.Instance.coinRequirement);
        }
//ACCURACY: Player HEADS as life counter instead of numbers
        if (player.lives >= 0) {
            if (player.lives != lives) {
                lives = player.lives;
                string lifeIcon = Utils.GetCharacterData(player.photonView.Owner).uistring;
                if(player.gameObject.name.Equals("PlayerMario(Clone)")){
                    lifeIcon = "<sprite=3>";
                }

                string lifeAmount = "";
                for(int i = 0; i<player.lives; i++){
                    lifeAmount += ""+lifeIcon;
                }
                uiLives.text = lifeAmount;
                if(player.lives == 1){
                    StartCoroutine(LastLifeAnimation());
                }
            }
        } else {
            livesParent.SetActive(false);
        }

        if (GameManager.Instance.timedGameDuration > 0 && !isLocalGame) {
            int seconds = Mathf.CeilToInt((GameManager.Instance.endServerTime - PhotonNetwork.ServerTimestamp) / 1000f);
            seconds = Mathf.Clamp(seconds, 0, GameManager.Instance.timedGameDuration);
            if (seconds != timer) {
                timer = seconds;
                uiCountdown.text = Utils.GetSymbolString("" + (timer / 60) + ":" + (seconds % 60).ToString("00"));
            }
            timerParent.SetActive(true);

            if (GameManager.Instance.endServerTime - PhotonNetwork.ServerTimestamp < 0) {
                if (timerMaterial == null) {
                    CanvasRenderer cr = uiCountdown.transform.GetChild(0).GetComponent<CanvasRenderer>();
                    cr.SetMaterial(timerMaterial = new(cr.GetMaterial()), 0);
                }

                float partialSeconds = (GameManager.Instance.endServerTime - PhotonNetwork.ServerTimestamp) / 1000f % 2f;
                byte gb = (byte) (Mathf.PingPong(partialSeconds, 1f) * 255);
                timerMaterial.SetColor("_Color", new Color32(255, gb, gb, 255));
            }
        }else if(isLocalGame){
            timerParent.SetActive(true);
            if(other == null){
                return;
            }
            if (other.coins != coins2) {
            coins2 = other.coins;
            uiCountdown.text = Utils.GetSymbolString("C" + coins2 + "/" + GameManager.Instance.coinRequirement);
        }
        } else {
            timerParent.SetActive(false);
        }
    }


    private void UpdateP2TextUI() {

    if(other == null){
        return;
    }

//ACCURACY: STAR ICON as counter instead of numbers

    if (other.stars != GameManager.Instance.starRequirement+1)
    {
        p2stars = other.stars;
        string singlestar = Utils.GetSymbolString("s");
        string fivestar = Utils.GetSymbolString("S");


       
        
            //isP2AnimationRunning check is added to avoid P2 infinite animation loop
            if ((other.stars == GameManager.Instance.starRequirement-1) && !isP2AnimationRunning)
            {//ONE STAR LEFT TO END
                shouldP2Animate = true;
                flushStars("p2");
                StartCoroutine(DelayWinningStarAnim("p2"));
                
            }
            if (other.stars == 1)
            {//ONE STAR
               flushStars("p2");
               p2UiStar1.text = singlestar;

            }
            if (other.stars == 2)
            {//TWO STARS
               flushStars("p2");
               p2UiStar1.text = singlestar;
               p2UiStar2.text = singlestar;

            }
            if (other.stars == 3)
            {//THREE STARS
               flushStars("p2");
               p2UiStar1.text = singlestar;
               p2UiStar2.text = singlestar;
               p2UiStar3.text = singlestar;
            }
            if (other.stars == 4)
            {//FOUR STARS
               flushStars("p2");
               p2UiStar1.text = singlestar;
               p2UiStar2.text = singlestar;
               p2UiStar3.text = singlestar;
               p2UiStar4.text = singlestar;
            }
            if (other.stars == 5)
            {//FIVE STARS
               flushStars("p2");
               p2UiStar1.text = fivestar;

            }
            if (other.stars == 6)
            {//SIX 
               flushStars("p2");
               p2UiStar1.text = fivestar;
               p2UiStar2.text = singlestar;
            }
            if (other.stars == 7)
            {//SEVEN
               flushStars("p2");
               p2UiStar1.text = fivestar;
               p2UiStar2.text = singlestar;
               p2UiStar3.text = singlestar;
            }
            if (other.stars == 8)
            {//EIGHT
               flushStars("p2");
               p2UiStar1.text = fivestar;
               p2UiStar2.text = singlestar;
               p2UiStar3.text = singlestar;
               p2UiStar4.text = singlestar;
            }
            if (other.stars == 9)
            {//NINE
               flushStars("p2");
               p2UiStar1.text = fivestar;
               p2UiStar2.text = singlestar;
               p2UiStar3.text = singlestar;
               p2UiStar4.text = singlestar;
               p2UiStar5.text = singlestar;
            }
            if (other.stars == 10)
            {//TEN
               flushStars("p2");
               p2UiStar1.text = fivestar+singlestar;
               p2UiStar2.text = singlestar;
               p2UiStar3.text = singlestar;
               p2UiStar4.text = singlestar;
               p2UiStar5.text = singlestar;
            }
            if(other.stars == 0)
            {//ZERO
               flushStars("p2");
            }

        
        if(p2stars != GameManager.Instance.starRequirement-1){
            shouldP2Animate = false;
            isP2AnimationRunning = false;
        }
        
    }
//ACCURACY: Player HEADS as life counter instead of numbers
        string lifeIcon = Utils.GetCharacterData(other.photonView.Owner).uistring;
        
        if(isLocalGame){
            lifeIcon = "<sprite=4>";
        }

        //ACCURACY: LOCAL SPLIT-SCREEN. Gate on change like Mario's uiLives does - without this, the text gets
        //rewritten every frame while other.lives stays the same, fighting p2LastLifeAnimation's blink.
        if (other.lives != p2lives) {
            p2lives = other.lives;

            if (other.lives == 0) {
                p2UiLives.text = "";
                isP2LifeAnimationRunning = false;
            }else if (other.lives == 1){
                p2UiLives.text = lifeIcon;
                if(!isP2LifeAnimationRunning){
                    StartCoroutine(p2LastLifeAnimation());
                }
            }else if (other.lives == 2){
                p2UiLives.text = lifeIcon+lifeIcon;
            }else if (other.lives == 3){
                p2UiLives.text = lifeIcon+lifeIcon+lifeIcon;
            }else if (other.lives == 4){
                p2UiLives.text = lifeIcon+lifeIcon+lifeIcon+lifeIcon;
            }else if (other.lives == 5){
                p2UiLives.text = lifeIcon+lifeIcon+lifeIcon+lifeIcon+lifeIcon;
            }
        }
    }





    public GameObject CreatePlayerIcon(PlayerController player) {
        GameObject trackObject = Instantiate(playerTrackTemplate, playerTrackTemplate.transform.parent);
        TrackIcon icon = trackObject.GetComponent<TrackIcon>();
        icon.target = player.gameObject;

        trackObject.SetActive(true);

        return trackObject;
    }
}
