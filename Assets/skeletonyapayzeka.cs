using UnityEngine;

public class skeletonyapayzeka : MonoBehaviour
{
    [Header("Ayarlar")]
    public Transform player;         
    public float moveSpeed = 2f;      // İskelet ağır olduğu için yavaş
    public float chaseDist = 6f;     
    public float attackDist = 1.2f;   

    [Header("Kalkan Özellikleri")]
    public bool hasShield = true;     // İskeletin kalkanı var
    [Range(0, 100)] 
    public int blockChance = 50;      // %50 ihtimalle bloklasın

    [Header("Devriye (Patrol)")]
    public float patrolSpeed = 1.5f;   
    public float patrolTime = 3f;    
    public float waitTime = 2f;      
    
    private float patrolTimer;
    private float waitTimer;
    private bool isWalking = false;  
    private int patrolDirection = 1; 

    [Header("Saldırı")]
    public float attackCooldown = 2f; // Vuruşları daha ağır ve yavaş
    public float damageToPlayer = 25f; 
    private float nextAttackTime = 0f;

    [Header("Can")]
    public int health = 4; // Canı Goblin'den fazla

    private Animator anim;
    private Rigidbody2D rb;
    private bool isDead = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        patrolTimer = patrolTime;
        waitTimer = waitTime;

        // Player'ı otomatik bul
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

        // 1. SALDIRI MESAFESİ
        if (dist < attackDist)
        {
            StopMovement();
            if (Time.time > nextAttackTime)
            {
                AttackPlayer(); 
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        // 2. KOVALAMA MESAFESİ
        else if (dist < chaseDist)
        {
            ChasePlayer();
        }
        // 3. DEVRİYE (PATROL)
        else
        {
            Patrol();
        }
    }

    void AttackPlayer()
    {
        anim.SetTrigger("attack");

        // Player'ın scriptine ulaşıp canını azalt
        Player_Script playerScript = player.GetComponent<Player_Script>();
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
            
            // Yüzünü dön (Scale koruyarak)
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

        // --- İSKELETE ÖZEL KALKAN SİSTEMİ ---
        if (hasShield)
        {
            int sans = Random.Range(0, 100);
            if (sans < blockChance)
            {
                anim.SetTrigger("block"); // Blok animasyonunu oynat
                return; // Hasar almadan fonksiyondan çık
            }
        }
        // ------------------------------------

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