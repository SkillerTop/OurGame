using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hero : Entity
{
   
    [SerializeField] private int health;

    [SerializeField] private Image[] hearts;
    
    [SerializeField] private Sprite aliveHeart;
    [SerializeField] private Sprite deadHeart ;

    public bool isAttacking = false;
    public bool isRecharged = true;
    public bool isDead = false;
    public bool isJumpAttack = false;

    public Transform attackPos;
    public float attackRange;
    public LayerMask enemy;
    

    private Rigidbody2D rb;
    private Animator anim;
   

    public static Hero Instance {get; set;}
    public float jumpForce = 15f;
void Jump()
{
    if(Input.GetKeyDown(KeyCode.S))
    {
        Physics2D.IgnoreLayerCollision(10, 11, true);
        Invoke("IgnoreLayerOff", 1f); 
    }
 if (onGround && Input.GetKeyDown(KeyCode.Space))
 {
 rb.velocity = new Vector2(rb.velocity.x, 0);
 rb.velocity = new Vector2(rb.velocity.x, jumpForce);
 }
 if (rb.velocity.y == 0){anim.SetBool("zeroVelocityY", true);}
 else{ anim.SetBool("zeroVelocityY", false);}
 if (!onGround && Input.GetButtonDown("Fire1"))
 {
    anim.SetTrigger("JumpAttack");
        isJumpAttack = true;
        isRecharged = false;

            StartCoroutine(AttackAnimation());
            StartCoroutine(AttackCollDown());
 }  
}


public bool onGround;
public LayerMask Ground;
public Transform GroundCheck;
private float GroundCheckRadius;
void CheckingGround()
{
 onGround = Physics2D.OverlapCircle(GroundCheck.position, GroundCheckRadius, Ground);
 anim.SetBool("onGround", onGround);
}


void IgnoreLayerOff()
{
    Physics2D.IgnoreLayerCollision(10, 11, false);}




    public Vector2 moveVector;
public int speed = 5;
void Walk()
{
moveVector.x = Input.GetAxisRaw("Horizontal");
rb.velocity = new Vector2(moveVector.x * speed, rb.velocity.y);
anim.SetFloat("moveX", Mathf.Abs(moveVector.x));
}

public bool faceRight = true;
void Reflect()
{
   if ((moveVector.x > 0 && !faceRight) || (moveVector.x < 0 && faceRight))
  {
transform.localScale *= new Vector2(-1, 1);
faceRight = !faceRight;
  }
}



    private void Awake()
    {
        lives = 5;
        health = lives;
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        isRecharged = true;
        GroundCheckRadius = GroundCheck.GetComponent<CircleCollider2D>().radius;

        
        WallCheckRadiusDown = WallCheckDown.GetComponent<CircleCollider2D>().radius;
        gravityDef= rb.gravityScale;
    }

 
    public override void GetDamage()
    {
        health -= 1;
        anim.SetTrigger("Hurt");
       if (health == 0)
       {
        foreach (var h in hearts)
        h.sprite = deadHeart;
       // this.enabled = false ;

       }
    }

void Update()
 {
 Attack();
 Walk();
  Reflect();
 Jump();
MoveOnWall();
WallJump();
LedgeGo();


if(health > lives)
        health = lives;
 for (int i = 0; i < hearts.Length; i++)
        {
            if(i < health)
            hearts[i].sprite = aliveHeart;  
            else 
            hearts[i].sprite = deadHeart;

            if(i <lives)
            hearts[i].enabled = true;
            else
            hearts[i].enabled = false;
        }
}
private void FixedUpdate()
{
CheckingGround();
CheckingWall();
CheckingLedge();
}
   private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
    }

    private IEnumerator AttackAnimation()
    {
        yield return new WaitForSeconds(0.7f);
        isAttacking = false;
    }

    private IEnumerator AttackCollDown()
    {
        yield return new WaitForSeconds(0.5f);
        isRecharged = true;
    }
    
    private void Attack()
    {
        if(onGround && isRecharged && Input.GetButtonDown("Fire1"))
        {
            anim.SetTrigger("Attack");
            isAttacking = true;
            isRecharged = false;

            StartCoroutine(AttackAnimation());
            StartCoroutine(AttackCollDown());
        }
    }

    private void OnAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackPos.position, attackRange, enemy);

        for (int i=0; i < colliders.Length; i++)
        {
            colliders[i].GetComponent<Entity>().GetDamage();
        }
    }
    void Dead()
    {
        if( health == 0)
        {
            anim.SetBool("Death", true);
        }
    }
public bool onWall;
public bool onWallUp;
public bool onWallDown;
public LayerMask Wall;
public Transform WallCheckUp;
public Transform WallCheckDown;
public float WallCheckRayDistance = 1f;
private float WallCheckRadiusDown;
public bool onLedge;
public float ledgeRayCorrectY = 0.5f;
void CheckingWall()
{
  onWallUp = Physics2D.Raycast
(
WallCheckUp.position,
new Vector2(transform.localScale.x, 0),
WallCheckRayDistance,
Wall
);
 onWallDown = Physics2D.OverlapCircle(WallCheckDown.position, WallCheckRadiusDown, Wall);
 onWall = (onWallUp && onWallDown);
 anim.SetBool("onWall", onWall);
 if (onWallUp && !onWallDown) { anim.SetBool("wallCheckUp", true); }
 else { anim.SetBool("wallCheckUp", false); }
}
void CheckingLedge()
{
 if (onWallUp)
{
 onLedge = !Physics2D.Raycast
(
 new Vector2(WallCheckUp.position.x, WallCheckUp.position.y + ledgeRayCorrectY),
 new Vector2(transform.localScale.x, 0),
 WallCheckRayDistance,
 Wall
);
}
 else { onLedge = false; }
    anim.SetBool("onLedge", onLedge);

 if ((onLedge && Input.GetAxisRaw("Vertical") != -1) || blockMoveXYforLedge)
{
 rb.gravityScale = 0;
 rb.velocity = new Vector2(0, 0);
 offsetCalculateAndCorrect();
}
}
 public float minCorrectDistance = 0.01f;
 public float offsetY;
 void offsetCalculateAndCorrect()
{
 offsetY = Physics2D.Raycast
(
 new Vector2(WallCheckUp.position.x + WallCheckRayDistance * transform.localScale.x,
 WallCheckUp.position.y + ledgeRayCorrectY),
 Vector2.down,
 ledgeRayCorrectY,
 Ground
).distance;
 if (offsetY > minCorrectDistance * 1.5f)
{
 transform.position = new Vector3(transform.position.x, transform.position.y - offsetY +
 minCorrectDistance, transform.position.z);
}
}
 private bool blockMoveXYforLedge;
 void LedgeGo()
{
 if (onLedge && Input.GetKeyDown(KeyCode.UpArrow))
{
 blockMoveXYforLedge = true;
 if (onWallUp && !onWallDown) { anim.Play("platformLedgeClimb"); }
 else { anim.Play("wallLedgeClimb"); }
}
}
 public Transform finishLedgePosition;
 void FinishLedge()
{
 transform.position = new Vector3(finishLedgePosition.position.x, finishLedgePosition.position.y,
 finishLedgePosition.position.z);
   anim.Play("idle");
 blockMoveXYforLedge = false;
}
 public float upDownSpeed = 4f;
 public float slideSpeed = 0;
 private float gravityDef;
 void MoveOnWall()
{
 if (onWall && !onGround)
{
 moveVector.y = Input.GetAxisRaw("Vertical");
 anim.SetFloat("UpDown", moveVector.y);
 if (!blockMoveXforJump && moveVector.y == 0)
{
 rb.gravityScale = 0;
 rb.velocity = new Vector2(0, slideSpeed);
}

 if (!blockMoveXYforLedge)
{
 if (moveVector.y > 0)
{
 rb.velocity = new Vector2(rb.velocity.x, moveVector.y * upDownSpeed / 2);
}
 else if (moveVector.y < 0)
{
 rb.velocity = new Vector2(rb.velocity.x, moveVector.y * upDownSpeed);
}
}
}
 else if (!onGround && !onWall) { rb.gravityScale = gravityDef; }
}
      private bool blockMoveXforJump;
  public float jumpWallTime = 0.5f;
 private float timerJumpWall;
 public Vector2 jumpAngle = new Vector2(3.5f, 10);
 void WallJump()
{
  if (onWall && !onGround && Input.GetKeyDown(KeyCode.Space))
{
 blockMoveXforJump = true;
 moveVector.x = 0;
 anim.StopPlayback();
 anim.Play("wallJump");
 transform.localScale *= new Vector2(-1, 1);
 faceRight = !faceRight;
 rb.gravityScale = gravityDef;
 rb.velocity = new Vector2(0, 0);
 rb.velocity = new Vector2(transform.localScale.x * jumpAngle.x, jumpAngle.y);
}
 if (blockMoveXforJump && (timerJumpWall += Time.deltaTime) >= jumpWallTime)
{
 if (onWall || onGround || Input.GetAxisRaw("Horizontal") != 0)
{
 blockMoveXforJump = false;
 timerJumpWall = 0;
}
}
}
 private void OnDrawGizmos()
{
 Gizmos.color = Color.blue;
 Gizmos.DrawLine
(
 WallCheckUp.position,
 new Vector2(WallCheckUp.position.x + WallCheckRayDistance * transform.localScale.x,
 WallCheckUp.position.y)
);
 Gizmos.color = Color.red;
 Gizmos.DrawLine
(   
 new Vector2(WallCheckUp.position.x, WallCheckUp.position.y + ledgeRayCorrectY),

 new Vector2(WallCheckUp.position.x + WallCheckRayDistance * transform.localScale.x,
 WallCheckUp.position.y + ledgeRayCorrectY)
);
 Gizmos.color = Color.green;
 Gizmos.DrawLine
(
   new Vector2(WallCheckUp.position.x + WallCheckRayDistance * transform.localScale.x,
  WallCheckUp.position.y + ledgeRayCorrectY),
 new Vector2(WallCheckUp.position.x + WallCheckRayDistance * transform.localScale.x,
 WallCheckUp.position.y)
);
}
}