using UnityEngine;
using Photon.Pun;
using NSMB.Utils;

public class GoombaWalk : KillableEntity {
    [SerializeField] float speed, deathTimer = -1, terminalVelocity = -8;

    public GameObject goombaModel;
    public new void Start() {
        base.Start();
        body.velocity = new Vector2(speed * (left ? -1 : 1), body.velocity.y);

       // body = goombaModel.GetComponent<Rigidbody2D>();
     //   hitbox = goombaModel.GetComponent<BoxCollider2D>();
        animator = goombaModel.GetComponent<Animator>();
     //   audioSource = goombaModel.GetComponent<AudioSource>();
      //  sRenderer = goombaModel.GetComponent<SpriteRenderer>();
      //  physics = goombaModel.GetComponent<PhysicsEntity>();

        animator.SetBool("dead", false);
    }

    public new void FixedUpdate() {
        if (GameManager.Instance && GameManager.Instance.gameover) {
            body.velocity = Vector2.zero;
            body.angularVelocity = 0;
            animator.enabled = false;
            body.isKinematic = true;
            return;
        }

        base.FixedUpdate();
        if (dead) {
            if (deathTimer >= 0 && (photonView?.IsMine ?? true)) {
                Utils.TickTimer(ref deathTimer, 0, Time.fixedDeltaTime);
                if (deathTimer == 0)
                    PhotonNetwork.Destroy(gameObject);
            }
            return;
        }


        physics.UpdateCollisions();
        if (physics.hitLeft || physics.hitRight) {
            left = physics.hitRight;
        }
        body.velocity = new Vector2(speed * (left ? -1 : 1), Mathf.Max(terminalVelocity, body.velocity.y));
        sRenderer.flipX = !left;
        if (left)//ACCURACY: 3D GOOMBA FLIPPING
        {
            // Set rotation to =120f on the Y-axis
            goombaModel.transform.rotation = Quaternion.Euler(0f, -120f, 0f);
        }
        else
        {
            // Set rotation to 120f on the Y-axis
            goombaModel.transform.rotation = Quaternion.Euler(0f, 120f, 0f);
        }
    }

    [PunRPC]
    public override void Kill() {
        body.velocity = Vector2.zero;
        body.isKinematic = true;
        speed = 0;
        dead = true;
        deathTimer = 0.5f;
        hitbox.enabled = false;
        animator.SetBool("dead", true);
    }
}