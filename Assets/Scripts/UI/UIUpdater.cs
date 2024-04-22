using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using NSMB.Utils;


public class UIUpdater : MonoBehaviour {

    public static UIUpdater Instance;
    public GameObject playerTrackTemplate, starTrackTemplate;
    public PlayerController player, other;
    public Sprite storedItemNull;
    public TMP_Text uiStar1,uiStar2,uiStar3,uiStar4,uiStar5, uiCoins, uiDebug, uiLives, uiCountdown;

    public TMP_Text p2UiStar1,p2UiStar2,p2UiStar3,p2UiStar4,p2UiStar5, p2UiLives;
    public Image itemReserve, itemColor;
    public float pingSample = 0;

    public bool isLocalGame = false;


//---Static Variables
    private static readonly int ParamIn = Animator.StringToHash("in");
    private static readonly int ParamOut = Animator.StringToHash("out");
    private static readonly int ParamHasItem = Animator.StringToHash("has-item");
    [SerializeField] private Animator reserveAnimator;
    private Powerup previousPowerup;

    private Material timerMaterial;
    private GameObject starsParent, coinsParent, livesParent, p2LivesParent, timerParent;

    public GameObject middleColumnParent;
    private readonly List<Image> backgrounds = new();
    private bool uiHidden;
    private string pingIcon;
    private bool is1v1 = false;
    private bool shouldAnimate = false;
    private bool shouldP2Animate = false;
    private bool isP2AnimationRunning = true;

    private bool isP2LifeAnimationRunning = false;

    private int coins = -1, coins2 = -1, stars = -1, p2stars = -1, lives = -1, timer = -1;

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
            middleColumnParent.transform.position += new Vector3(0f, 28f, 0f);
            uiCountdown.text = Utils.GetSymbolString("C" + "0" + "/" + GameManager.Instance.coinRequirement);
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

        uiDebug.text = "<mark=#000000b0 padding=\"20, 20, 20, 20\"><font=\"defaultFont\">"+pingIcon+" " + (int) pingSample + "ms</font>";

       // other = GameManager.Instance..GetComponent<PlayerController>();

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
        if (!player ) {
            return;
        }

        Powerup powerup = player.storedPowerup;
       
        
        reserveAnimator.SetBool(ParamHasItem, powerup && powerup.reserveSprite);
        
        if (!powerup) {
            if (previousPowerup != powerup) {
                reserveAnimator.SetTrigger(ParamOut);
                previousPowerup = powerup;
            }
            return;
        }

        itemReserve.sprite = powerup.reserveSprite ? powerup.reserveSprite : storedItemNull;
        if (previousPowerup != powerup) {
            reserveAnimator.SetTrigger(ParamIn);
            previousPowerup = powerup;
        }
    }

    public void OnReserveItemStaticStarted() {
        itemReserve.sprite = storedItemNull;
    }



private System.Collections.IEnumerator LastLifeAnimation()
{
    yield return new WaitForSeconds(4.5f);
    while(player.lives == 1){
        yield return new WaitForSeconds(0.2f);
        livesParent.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        livesParent.SetActive(true);
        yield return null;
    }
}

private System.Collections.IEnumerator p2LastLifeAnimation()
{
    isP2LifeAnimationRunning = true;
    yield return new WaitForSeconds(4.5f);
    while(other.lives == 1){
        yield return new WaitForSeconds(0.2f);
        p2LivesParent.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        p2LivesParent.SetActive(true);
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





    public GameObject CreatePlayerIcon(PlayerController player) {
        GameObject trackObject = Instantiate(playerTrackTemplate, playerTrackTemplate.transform.parent);
        TrackIcon icon = trackObject.GetComponent<TrackIcon>();
        icon.target = player.gameObject;

        trackObject.SetActive(true);

        return trackObject;
    }
}
