using NSMB.Utils;
using Photon.Pun;
using UnityEngine;

public class ModelKoopaWalk : HoldableEntity
{
    [SerializeField] private Vector2 outShellHitboxSize, inShellHitboxSize;
    [SerializeField] private Vector2 outShellHitboxOffset, inShellHitboxOffset;
    [SerializeField] protected float walkSpeed, kickSpeed, wakeup = 15;
    public bool red, blue, shell, stationary, upsideDown, canBeFlipped = true, flipXFlip, putdown;

    private readonly Vector2 blockOffset = new(0, 0.05f);
    protected int combo;
    private float dampVelocity, currentSpeed;
    private Vector2 velocityLastFrame;
    protected float wakeupTimer;

    public GameObject koopaModel;

    private BoxCollider2D worldHitbox;

    public bool IsStationary => !holder && stationary;

    #region Public Methods

    public override void InteractWithPlayer(PlayerController player)
    {
        var damageDirection = (player.body.position - body.position).normalized;
        var attackedFromAbove = damageDirection.y > 0;
        if (holder)
            return;


        if (shell && blue && player.groundpound && !player.onGround)
        {
            photonView.RPC(nameof(BlueBecomeItem), RpcTarget.All);
            return;
        }

        if (!attackedFromAbove && player.state == Enums.PowerupState.BlueShell && player.crouching && !player.inShell)
        {
            player.body.velocity = new Vector2(0, player.body.velocity.y);
            photonView.RPC(nameof(SetLeft), RpcTarget.All, damageDirection.x > 0);
        }
        else if (player.sliding || player.inShell || player.invincible > 0 ||
                 player.state == Enums.PowerupState.MegaMushroom)
        {
            var originalFacing = player.facingRight;

            photonView.RPC(nameof(SpecialKill), RpcTarget.All, !originalFacing, false, player.StarCombo++);
        }
        else if (player.groundpound && player.state != Enums.PowerupState.MiniMushroom && attackedFromAbove)
        {
            photonView.RPC(nameof(EnterShell), RpcTarget.All);
            
            if (!blue)
            {
                photonView.RPC(nameof(Kick), RpcTarget.All, player.body.position.x < body.position.x, 1f,
                    player.groundpound);
                player.photonView.RPC(nameof(PlayerController.SetHoldingOld), RpcTarget.All, photonView.ViewID);
                previousHolder = player;
            }
        }
        else if (attackedFromAbove && (!shell || !IsStationary))
        {
            if (player.state == Enums.PowerupState.MiniMushroom)
            {
                if (player.groundpound)
                {
                    player.groundpound = false;
                    photonView.RPC(nameof(EnterShell), RpcTarget.All);
                    
                }

                player.bounce = true;
            }
            else
            {
                photonView.RPC(nameof(EnterShell), RpcTarget.All);
                player.bounce = !player.groundpound;
                
            }

            player.photonView.RPC(nameof(PlaySound), RpcTarget.All, Enums.Sounds.Enemy_Generic_Stomp);
            player.drill = false;
        }
        else
        {
            if (shell && IsStationary)
            {
                if (!holder)
                {
                    if (player.CanPickup())
                    {
                        photonView.RPC(nameof(Pickup), RpcTarget.All, player.photonView.ViewID);
                        player.photonView.RPC(nameof(PlayerController.SetHolding), RpcTarget.All, photonView.ViewID);
                    }
                    else
                    {
                        photonView.RPC(nameof(Kick), RpcTarget.All, player.body.position.x < body.position.x,
                            Mathf.Abs(player.body.velocity.x) / player.RunningMaxSpeed, player.groundpound);
                        player.photonView.RPC(nameof(PlayerController.SetHoldingOld), RpcTarget.All, photonView.ViewID);
                        previousHolder = player;
                    }
                }
            }
            else if (player.hitInvincibilityCounter <= 0)
            {
                player.photonView.RPC(nameof(PlayerController.Powerdown), RpcTarget.All, false);
                if (!shell)
                    photonView.RPC(nameof(SetLeft), RpcTarget.All, damageDirection.x > 0);
            }
        }
    }

    #endregion

    #region Helper Methods

    private void HandleTile()
    {
        if (holder)
            return;
        physics.UpdateCollisions();

        var collisions = new ContactPoint2D[20];
        var collisionAmount = worldHitbox.GetContacts(collisions);
        for (var i = 0; i < collisionAmount; i++)
        {
            var point = collisions[i];
            var p = point.point + point.normal * -0.15f;
            if (Mathf.Abs(point.normal.x) == 1 && point.collider.gameObject.layer == Layers.LayerGround)
            {
                if (!putdown && shell && !stationary)
                {
                    var tileLoc = Utils.WorldToTilemapPosition(p + blockOffset);
                    var tile = GameManager.Instance.tilemap.GetTile(tileLoc);
                    if (tile == null)
                        continue;
                    if (!shell)
                        continue;

                    if (tile is InteractableTile it)
                        it.Interact(this, InteractableTile.InteractionDirection.Up,
                            Utils.TilemapToWorldPosition(tileLoc));
                }
            }
            else if (point.normal.y > 0 && putdown)
            {
                body.velocity = new Vector2(0, body.velocity.y);
                putdown = false;
            }
        }
    }

    #endregion

    #region Unity Methods

    public override void Start()
    {
        base.Start();
        hitbox = transform.GetChild(0).GetComponent<BoxCollider2D>();
        worldHitbox = GetComponent<BoxCollider2D>();
        
        body.velocity = new Vector2(-walkSpeed, 0);


        animator = koopaModel.GetComponent<Animator>();

    }

    public override void FixedUpdate()
    {
        if (GameManager.Instance && GameManager.Instance.gameover)
        {
            body.velocity = Vector2.zero;
            body.angularVelocity = 0;
            animator.enabled = false;
            body.isKinematic = true;
            return;
        }

        base.FixedUpdate();
        
        animator.SetBool("shell", shell || holder != null);
        animator.SetFloat("xVel", -body.velocity.x * (upsideDown ? -1 : 1));

        if (Frozen || dead)
            return;

        sRenderer.flipX = !FacingLeftTween ^ flipXFlip;
        if (FacingLeftTween)//ACCURACY: 3D KOOPA FLIPPING
        {
            // Set rotation to =120f on the Y-axis
            koopaModel.transform.rotation = Quaternion.Euler(0f, -120f, 0f);
        }
        else
        {
            // Set rotation to 120f on the Y-axis
            koopaModel.transform.rotation = Quaternion.Euler(0f, 120f, 0f);
        }

        if (upsideDown)
        {
            dampVelocity = Mathf.Min(dampVelocity + Time.fixedDeltaTime * 3, 1);
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                transform.eulerAngles.y,
                Mathf.Lerp(transform.eulerAngles.z, 180f, dampVelocity) +
                (wakeupTimer < 3 && wakeupTimer > 0 ? Mathf.Sin(wakeupTimer * 120f) * 15f : 0));

            koopaModel.transform.eulerAngles = new Vector3(
                koopaModel.transform.eulerAngles.x,
                koopaModel.transform.eulerAngles.y,
                Mathf.Lerp(koopaModel.transform.eulerAngles.z, 180f, dampVelocity) +
                (wakeupTimer < 3 && wakeupTimer > 0 ? Mathf.Sin(wakeupTimer * 120f) * 15f : 0));
        }
        else
        {
            dampVelocity = 0;
            koopaModel.transform.eulerAngles = new Vector3(
                koopaModel.transform.eulerAngles.x,
                koopaModel.transform.eulerAngles.y,
                wakeupTimer < 3 && wakeupTimer > 0 ? Mathf.Sin(wakeupTimer * 120f) * 15f : 0);

            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                transform.eulerAngles.y,
                wakeupTimer < 3 && wakeupTimer > 0 ? Mathf.Sin(wakeupTimer * 120f) * 15f : 0);
        }

        if (shell)
        {
            worldHitbox.size = hitbox.size = inShellHitboxSize;
            worldHitbox.offset = hitbox.offset = inShellHitboxOffset;

            if (stationary)
            {
                if (physics.onGround)
                    body.velocity = new Vector2(0, body.velocity.y);
                if ((wakeupTimer -= Time.fixedDeltaTime) < 0)
                    if (photonView.IsMine)
                        photonView.RPC(nameof(WakeUp), RpcTarget.All);
            }
            else
            {
                wakeupTimer = wakeup;
            }
        }
        else
        {
            worldHitbox.size = hitbox.size = outShellHitboxSize;
            worldHitbox.offset = hitbox.offset = outShellHitboxOffset;
        }

        if (physics.hitRight && !FacingLeftTween)
        {
            if (photonView && photonView.IsMine)
                photonView.RPC(nameof(Turnaround), RpcTarget.All, false, velocityLastFrame.x);
            else
                Turnaround(false, velocityLastFrame.x);
        }
        else if (physics.hitLeft && FacingLeftTween)
        {
            if (photonView && photonView.IsMine)
                photonView.RPC(nameof(Turnaround), RpcTarget.All, true, velocityLastFrame.x);
            else
                Turnaround(true, velocityLastFrame.x);
        }

        if (physics.onGround && Physics2D.Raycast(body.position, Vector2.down, 0.5f, Layers.MaskAnyGround) && red &&
            !shell)
        {
            Vector3 redCheckPos = body.position + new Vector2(0.1f * (FacingLeftTween ? -1 : 1), 0);
            if (GameManager.Instance)
                Utils.WrapWorldLocation(ref redCheckPos);

            if (!Physics2D.Raycast(redCheckPos, Vector2.down, 0.5f, Layers.MaskAnyGround))
            {
                if (photonView && photonView.IsMine)
                    photonView.RPC(nameof(Turnaround), RpcTarget.All, FacingLeftTween, velocityLastFrame.x);
                else
                    Turnaround(FacingLeftTween, velocityLastFrame.x);
            }
        }

        if (physics.onGround)
        {
            if (stationary)
            {
                //body.velocity = new(body.velocity.x, 0);
            }
            else
            {
                if (isRotating) body.velocity = new Vector2(0, 0);
                else body.velocity = new Vector2((shell ? currentSpeed : walkSpeed) * (FacingLeftTween ? -1 : 1), body.velocity.y);
            }
        }

        velocityLastFrame = body.velocity;

        if (!photonView.IsMineOrLocal())
            return;

        HandleTile();
    }

    public new void OnTriggerEnter2D(Collider2D collider)
    {
        if (!shell)
            base.OnTriggerEnter2D(collider);

        if (!photonView.IsMineOrLocal() || !shell || IsStationary || putdown || dead)
            return;

        var obj = collider.gameObject;
        var killa = obj.GetComponentInParent<KillableEntity>();
        switch (obj.tag)
        {
            case "koopa":
            case "bobomb":
            case "bulletbill":
            case "frozencube":
            case "goomba":
                if (killa.dead)
                    break;
                killa.photonView.RPC(nameof(SpecialKill), RpcTarget.All, killa.body.position.x > body.position.x, false,
                    combo++);
                if (holder)
                    photonView.RPC(nameof(SpecialKill), RpcTarget.All, killa.body.position.x < body.position.x, false,
                        combo++);
                break;
            case "piranhaplant":
                if (killa.dead)
                    break;
                killa.photonView.RPC(nameof(Kill), RpcTarget.All);
                if (holder){
                    photonView.RPC(nameof(Kill), RpcTarget.All);
                    PhotonNetwork.Destroy(photonView);
                }
                    

                break;
            case "coin":
                if (!holder && !stationary && previousHolder)
                    previousHolder.photonView.RPC(nameof(PlayerController.AttemptCollectCoin), RpcTarget.AllViaServer,
                        obj.GetPhotonView().ViewID,
                        new Vector2(obj.transform.position.x, collider.transform.position.y));
                break;
            case "loosecoin":
                if (!holder && !stationary && previousHolder)
                {
                    var parent = obj.transform.parent;
                    previousHolder.photonView.RPC(nameof(PlayerController.AttemptCollectCoin), RpcTarget.AllViaServer,
                        parent.gameObject.GetPhotonView().ViewID, (Vector2)parent.position);
                }

                break;
        }
    }

    #endregion

    #region PunRPCs

    [PunRPC]
    public override void Freeze(int cube)
    {
        base.Freeze(cube);
        stationary = true;
    }

    [PunRPC]
    public override void Kick(bool fromLeft, float kickFactor, bool groundpound)
    {
       // PlayerController controller = new PlayerController();
        //playerFacingRight = controller.facingRight;
        //facingLeft = !fromLeft;
        FacingLeftTween = !fromLeft;

        left = !fromLeft;
        stationary = false;
        currentSpeed = kickSpeed + 1.5f * kickFactor;
        body.velocity = new Vector2(currentSpeed * (fromLeft  ? -1 : 1), groundpound ? 3.5f : 0);
        PlaySound(Enums.Sounds.Enemy_Shell_Kick);
    }

    [PunRPC]
    public override void Throw(bool fromLeft, bool crouch, Vector2 pos)
    {
        if (holder == null)
            return;
        FacingLeftTween = !holder.facingRight;
        stationary = crouch;
        currentSpeed = kickSpeed + 1.5f * (Mathf.Abs(holder.body.velocity.x) / holder.RunningMaxSpeed);
        body.position = pos;

        Debug.DrawLine(body.position + hitbox.offset - hitbox.size / 2f,
            body.position + hitbox.offset + hitbox.size / 2f, Color.white, 10f);
        if (Utils.IsAnyTileSolidBetweenWorldBox(body.position + hitbox.offset, hitbox.size))
            transform.position = body.position = new Vector2(holder.transform.position.x, transform.position.y);

        previousHolder = holder;
        holder = null;
        shell = true;
        photonView.TransferOwnership(PhotonNetwork.MasterClient);
        // facingLeft = fromLeft;
        Debug.Log("RIGHT "+previousHolder.facingRight);
        if (crouch)
        {
            body.velocity = new Vector2(2f * (fromLeft ? -1 : 1), body.velocity.y);
            putdown = true;
        }
        else
        {
            body.velocity = new Vector2(currentSpeed * (fromLeft ? -1 : 1), body.velocity.y);
        }
    }

    [PunRPC]
    public void WakeUp()
    {
        shell = false;
        body.velocity = new Vector2(-walkSpeed, 0);
        tweenableRotation = false;
        FacingLeftTween = true;
        upsideDown = false;
        stationary = false;
        if (holder && photonView.IsMine)
            holder.photonView.RPC("HoldingWakeup", RpcTarget.All);
        holder = null;
        previousHolder = null;
    }

    [PunRPC]
    public void EnterShell()
    {
        if (blue && !shell)
        {
            BlueBecomeItem();
            return;
        }

        body.velocity = Vector2.zero;
        wakeupTimer = wakeup;
        combo = 0;
        shell = true;
        stationary = true;
        tweenableRotation = false;
    }

    [PunRPC]
    public void BlueBecomeItem()
    {
        if (photonView.IsMine)
            PhotonNetwork.Destroy(photonView);

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Instantiate("Prefabs/Powerup/BlueShell", transform.position, Quaternion.identity, 0,
                new object[] { 0.1f });
    }

    [PunRPC]
    protected void Turnaround(bool hitWallOnLeft, float x)
    {
        if (IsStationary)
            return;

        if (shell && hitWallOnLeft != FacingLeftTween)
            PlaySound(Enums.Sounds.World_Block_Bump);

        if (shell)
        {
            PlaySound(Enums.Sounds.World_Block_Bump);
            facingLeft = !hitWallOnLeft;
        }
        else
            FacingLeftTween = !hitWallOnLeft;
        body.velocity = new Vector2((x > 0.5f ? Mathf.Abs(x) : currentSpeed) * (FacingLeftTween ? -1 : 1), body.velocity.y);
    }

    [PunRPC]
    public void Bump()
    {
        if (dead)
            return;

        if (!shell)
        {
            stationary = true;
            putdown = true;
        }

        wakeupTimer = wakeup;
        shell = true;
        upsideDown = canBeFlipped;
        PlaySound(Enums.Sounds.Enemy_Shell_Kick);
        body.velocity = new Vector2(body.velocity.x, 5.5f);
    }

    [PunRPC]
    public override void Kill()
    {
        EnterShell();
    }

    [PunRPC]
    public override void SpecialKill(bool right, bool groundpound, int combo)
    {
        base.SpecialKill(right, groundpound, combo);
        shell = true;
        if (holder)
            holder.photonView.RPC("SetHolding", RpcTarget.All, -1);

        holder = null;
    }

    #endregion
}