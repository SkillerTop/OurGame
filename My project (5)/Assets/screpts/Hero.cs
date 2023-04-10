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
 CheckingGround();


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

}
