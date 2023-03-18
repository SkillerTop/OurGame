using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hero : Entity
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private int health;
    [SerializeField] private float jumpForce = 15f;
    private bool isGrounded = false;

    [SerializeField] private Image[] hearts;
    
    [SerializeField] private Sprite aliveHeart;
    [SerializeField] private Sprite deadHeart ;

    public bool isAttacking = false;
    public bool isRecharged = true;
    public bool isDead = false;

    public Transform attackPos;
    public float attackRange;
    public LayerMask enemy;
    

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;

    public static Hero Instance {get; set;}

    private States State
    {
        get { return (States)anim.GetInteger("state"); }
        set { anim.SetInteger("state", (int)value); }
    }
    private void Awake()
    {
        lives = 5;
        health = lives;
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        isRecharged = true;
    }
    private void Run()
    {
        if (isGrounded) State = States.Run;
        Vector3 dir = transform.right * Input.GetAxis("Horizontal");
        transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, speed * Time.deltaTime);
        sprite.flipX = dir.x < 0.0f;
    }
    private void FixedUpdate()
    {
        CheckGround();
    }
    private void Dead()
    {
        anim.SetBool("Death", true);
        jumpForce = 0f;
        speed = 0f;
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
    private void Update()
    {
        if (isGrounded && !isAttacking)  State = States.Idle;
        if (!isAttacking && Input.GetButton("Horizontal"))
            Run();
        if (!isAttacking && isGrounded && Input.GetButtonDown("Jump"))
            Jump();
        if (Input.GetButtonDown("Fire1"))
        Attack();
        if (health < 1)
        Dead();
            

        if(health > lives)
        health = lives;

        for (int i = 0; i < hearts.Length; i++)
        {
            if(i < health)
            hearts[i].sprite = aliveHeart;  
            else 
            hearts[i].sprite =deadHeart;

            if(i <lives)
            hearts[i].enabled = true;
            else
            hearts[i].enabled = false;
        }
    }
    private void Jump()
    {
        rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
    }
    
    private void CheckGround()
    {
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, 0.3f);
        isGrounded = collider.Length > 1;

        if (!isGrounded) State = States.Jump;
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
        if(isGrounded && isRecharged)
        {
            State = States.attack;
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

}



public enum States
{
    Idle,
    Run,
    Jump,
    attack,
    Hurt,
    Death
}