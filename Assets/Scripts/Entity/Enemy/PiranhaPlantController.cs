﻿using NSMB.Utils;
using Photon.Pun;
using UnityEngine;

public class PiranhaPlantController : KillableEntity
{
    [SerializeField] private float playerDetectSize = 1;
    [SerializeField] private float popupTimerRequirement = 6f;
    public bool upsideDown;

    private float popupTimer;

    public new void Start()
    {
        base.Start();
        upsideDown = transform.eulerAngles.z != 0;
    }

    public void Update()
    {
        var gm = GameManager.Instance;
        if (gm)
        {
            if (gm.gameover)
            {
                animator.enabled = false;
                return;
            }

            if (!gm.musicEnabled)
                return;
        }

        FacingLeftTween = false;

        if ((!dead && photonView && photonView.IsMine &&
            Utils.GetTileAtWorldLocation(transform.position + (upsideDown ? Vector3.up : Vector3.down * 0.1f)) == null  && !upsideDown))
        {
            photonView.RPC("Kill", RpcTarget.All);
            return;
        }



        animator.SetBool("dead", dead);
        if (dead || (photonView && !photonView.IsMine))
            return;

        if ((popupTimer += Time.deltaTime) >= popupTimerRequirement)
        {
            if (gm)
                foreach (var pl in gm.players)
                {
                    if (!pl)
                        continue;

                    if (Utils.WrappedDistance(transform.position, pl.transform.position) < playerDetectSize)
                        return;
                }

            animator.SetTrigger("popup");
            popupTimer = 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(
            transform.position +
            (Vector3)(playerDetectSize * new Vector2(0, transform.eulerAngles.z != 0 ? -0.5f : 0.5f)),
            playerDetectSize);
    }

    public override void InteractWithPlayer(PlayerController player) {
        if (player.invincible > 0 || player.inShell || player.state == Enums.PowerupState.MegaMushroom) {
            photonView.RPC("Kill", RpcTarget.All);
        } else {
            if ((player.crouching || player.groundpound) && player.state == Enums.PowerupState.BlueShell)
               return;
            if(animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Equals("pakkun_chomp"))
                player.photonView.RPC("Powerdown", RpcTarget.All, false);
        }
    }



    [PunRPC]
    public void Respawn()
    {
        if (Frozen || !dead)
            return;

        Frozen = false;
        dead = false;
        popupTimer = 3;
        animator.Play("end", 0, 1);

        hitbox.enabled = true;
    }

    [PunRPC]
    public override void Kill()
    {
        if(!animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Equals("pakkun_chomp"))
            return;

        PlaySound(Enums.Sounds.Enemy_PiranhaPlant_Death);
        PlaySound(Frozen ? Enums.Sounds.Enemy_Generic_FreezeShatter : Enums.Sounds.Enemy_Shell_Kick);

        dead = true;
        hitbox.enabled = false;
        Instantiate(Resources.Load("Prefabs/Particle/Puff"),
            transform.position + new Vector3(0, upsideDown ? -0.5f : 0.5f, 0), Quaternion.identity);
        if (photonView.IsMine){
            Utils.GetCustomProperty(Enums.NetRoomProperties.NewPowerups, out bool betaAnims); //ACCURACY: ENABLE E3 BETA COIN DROPS
            if(betaAnims){
                PhotonNetwork.Instantiate("Prefabs/BetaLooseCoin",
                transform.position + new Vector3(0, upsideDown ? -1f : 1f, 0), Quaternion.identity);
            }else{
                PhotonNetwork.Instantiate("Prefabs/LooseCoin",
                transform.position + new Vector3(0, upsideDown ? -1f : 1f, 0), Quaternion.identity);
            }
        }
            
    }

    [PunRPC]
    public override void SpecialKill(bool right, bool groundpound, int combo)
    {
        Kill();
    }
}