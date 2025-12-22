using UnityEngine;

public class goblinyapayzeka : MonoBehaviour
{
    [Header("Ayarlar")]
    public Transform player;         
    public float moveSpeed = 3f;     
    public float chaseDist = 6f;     
    public float attackDist = 1.5f;  

    [Header("Rastgele Gezme")]
    public float patrolSpeed = 2f;   
    public float patrolTime = 2f;    
    public float waitTime = 2f;      
    
    private float patrolTimer;
    private float waitTimer;
    private bool isWalking = false;  
    private int patrolDirection = 1; 

    [Header("Saldırı Ayarı")]
    public float attackCooldown = 1.5f; 
    public float damageToPlayer = 20f; // Oyuncuya kaç vursun?
    private float nextAttackTime = 0f;

    [Header("Can")]
    public int health = 3; 

    private Animator anim;
    private Rigidbody2D rb;
    private bool isDead = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        patrolTimer = patrolTime;
        waitTimer = waitTime;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // 1. SALDIRI MESAFESİNDEYSE
        if (dist < attackDist)
        {
            StopMovement();
            if (Time.time > nextAttackTime)
            {
                AttackPlayer(); // Yeni Saldırı Fonksiyonunu Çağır
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        // 2. GÖRME MESAFESİNDEYSE (KOVALA)
        else if (dist < chaseDist)
        {
            ChasePlayer();
        }
        // 3. UZAKTAYSA (RASTGELE GEZ)
        else
        {
            Patrol();
        }
    }

    void AttackPlayer()
    {
        anim.SetTrigger("attack");

        // Oyuncunun üzerindeki Script'i bul
        Player_Script playerScript = player.GetComponent<Player_Script>();
        
        // Eğer Script ve Can Barı varsa hasar ver
        if (playerScript != null && playerScript.healthBar != null)
        {
            playerScript.healthBar.GetDamage(damageToPlayer);
        }
    }

    void Patrol()
    {
        if (isWalking)
        {
            anim.SetBool("isRunning", true);
            rb.linearVelocity = new Vector2(patrolSpeed * patrolDirection, rb.linearVelocity.y);
            
            // Mevcut boyutu koruyarak dön
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * patrolDirection, transform.localScale.y, transform.localScale.z);

            patrolTimer -= Time.deltaTime;
            
            if (patrolTimer <= 0)
            {
                isWalking = false;
                waitTimer = waitTime;
                StopMovement();
            }
        }
        else
        {
            StopMovement();
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0)
            {
                isWalking = true;
                patrolTimer = patrolTime;
                patrolDirection = Random.Range(0, 2) == 0 ? -1 : 1;
            }
        }
    }

    void ChasePlayer()
    {
        anim.SetBool("isRunning", true);

        if (player.position.x > transform.position.x)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    void StopMovement()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("isRunning", false);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        anim.SetTrigger("hit"); 

        if (health <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("death");
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        rb.gravityScale = 0;
        this.enabled = false; 
    }
}