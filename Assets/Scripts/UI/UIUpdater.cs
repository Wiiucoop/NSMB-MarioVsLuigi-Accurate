using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using NSMB.Utils;


public class UIUpdater : MonoBehaviour {

    public static UIUpdater Instance;
    public GameObject playerTrackTemplate, starTrackTemplate;
    public PlayerController player;
    public Sprite storedItemNull;
    public TMP_Text uiStar1,uiStar2,uiStar3,uiStar4,uiStar5, uiCoins, uiDebug, uiLives, uiCountdown;
    public Image itemReserve, itemColor;
    public float pingSample = 0;

    private Material timerMaterial;
    private GameObject starsParent, coinsParent, livesParent, timerParent;
    private readonly List<Image> backgrounds = new();
    private bool uiHidden;
    private bool shouldAnimate = false;

    private int coins = -1, stars = -1, lives = -1, timer = -1;

    public void Start() {
        Instance = this;
        pingSample = PhotonNetwork.GetPing();

       // starsParent = uiStars.transform.parent.gameObject;
        coinsParent = uiCoins.transform.parent.gameObject;
        livesParent = uiLives.transform.parent.gameObject;
        timerParent = uiCountdown.transform.parent.gameObject;

       // backgrounds.Add(starsParent.GetComponentInChildren<Image>());
        backgrounds.Add(coinsParent.GetComponentInChildren<Image>());
        backgrounds.Add(livesParent.GetComponentInChildren<Image>());
        backgrounds.Add(timerParent.GetComponentInChildren<Image>());

        foreach (Image bg in backgrounds)
            bg.color = GameManager.Instance.levelUIColor;
        itemColor.color = new(GameManager.Instance.levelUIColor.r - 0.2f, GameManager.Instance.levelUIColor.g - 0.2f, GameManager.Instance.levelUIColor.b - 0.2f, GameManager.Instance.levelUIColor.a);
    }

    public void Update() {
        pingSample = Mathf.Lerp(pingSample, PhotonNetwork.GetPing(), Mathf.Clamp01(Time.unscaledDeltaTime * 0.5f));
        if (pingSample == float.NaN)
            pingSample = 0;

        uiDebug.text = "<mark=#000000b0 padding=\"20, 20, 20, 20\"><font=\"defaultFont\">Ping: " + (int) pingSample + "ms</font>";

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
    }

    private void ToggleUI(bool hidden) {
        uiHidden = hidden;

        starsParent.SetActive(!hidden);
        livesParent.SetActive(!hidden);
        coinsParent.SetActive(!hidden);
        timerParent.SetActive(!hidden);
    }

    private void UpdateStoredItemUI() {
        if (!player)
            return;

        itemReserve.sprite = player.storedPowerup != null ? player.storedPowerup.reserveSprite : storedItemNull;
    }


private System.Collections.IEnumerator WavyAnimation(TMP_Text uiStar)
{
    float originalSize = uiStar.fontSize;
    float newSize = originalSize * 1.5f;  // Adjust the scale factor as needed
    float frequency = 1.0f;
    if(uiStar.GetParsedText().Contains("<sprite=26>")){
        frequency = 1.3f;  // Adjust the frequency of the wavy for the 5-STAR SYMBOL
    }
    Debug.Log(uiStar.GetParsedText());
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

private System.Collections.IEnumerator DelayWinningStarAnim()
{
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
}


private void flushStars() {
    uiStar2.text = "";
    uiStar3.text = "";
    uiStar4.text = "";
    uiStar5.text = "";
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
                flushStars();
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
    
                StartCoroutine(DelayWinningStarAnim());
                break;
            }
            
            else if (i == 4)
            {//FIVE STARS
               flushStars();
               starcount = fivestar;

            }
            else if (i > 4)
            {//SIX OR MORE STARS
               flushStars();
               starcount = fivestar += singlestar;
            }
            else
            {//ONE STAR UNTIL 4 STARS
                flushStars();
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
            }
        } else {
            livesParent.SetActive(false);
        }

        if (GameManager.Instance.timedGameDuration > 0) {
            int seconds = Mathf.CeilToInt((GameManager.Instance.endServerTime - PhotonNetwork.ServerTimestamp) / 1000f);
            seconds = Mathf.Clamp(seconds, 0, GameManager.Instance.timedGameDuration);
            if (seconds != timer) {
                timer = seconds;
                uiCountdown.text = Utils.GetSymbolString("cx" + (timer / 60) + ":" + (seconds % 60).ToString("00"));
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
        } else {
            timerParent.SetActive(false);
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
