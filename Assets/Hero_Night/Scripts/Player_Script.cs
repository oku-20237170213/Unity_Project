using UnityEngine;

public class Player_Script : MonoBehaviour
{
    [Header("Bileşenler")]
    public Player_HealthBar healthBar;
    public Animator animator;
    private Rigidbody2D rgb;
    private SpriteRenderer sr; 
    
    [Header("Hareket")]
    public float speed;
    public float jumpForce = 6f;
    float moveInput;
    
    [Header("Zemin")]
    public Transform groundCheck;
    public float radius = 0.2f;
    public LayerMask groundLayer;
    bool isGrounded;

    [Header("Saldırı (Burası Önemli)")]
    public Transform attackPoint;    
    public float attackRange = 0.5f; 
    public LayerMask enemyLayers;    
    public int attackDamage = 1;     

    bool died = false;

    void Start()
    {
        // Resim bileşenini (SpriteRenderer) buluyoruz
        sr = GetComponent<SpriteRenderer>(); 
        if(sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        if(animator == null) animator = GetComponentInChildren<Animator>();
        rgb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (died) return;
        
        moveInput = Input.GetAxisRaw("Horizontal");
        
        // --- HAREKET VE DÖNME (Işınlanma Yapmayan Yöntem) ---
        if(moveInput != 0)
        {
            animator.SetBool("runn", true);
            if(moveInput > 0) // SAĞA GİDERKEN
            {
                // 1. Resmi düzelt
                sr.transform.localScale = new Vector3(1, 1, 1); 
                
                // 2. Kılıç noktasını sağa al (Pozitif yap)
                if(attackPoint != null)
                    attackPoint.localPosition = new Vector3(Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, 0);
            }
            else if(moveInput < 0) // SOLA GİDERKEN
            {
                // 1. Resmi ters çevir
                sr.transform.localScale = new Vector3(-1, 1, 1);
                
                // 2. Kılıç noktasını sola al (Negatif yap)
                if(attackPoint != null)
                    attackPoint.localPosition = new Vector3(-Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, 0);
            }
        }
        else
        {
            animator.SetBool("runn", false);
        }

        // --- ZIPLAMA ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rgb.linearVelocity = new Vector2(rgb.linearVelocity.x, jumpForce);
            animator.SetTrigger("jump");
        }

        // --- SALDIRI ---
        if (Input.GetKeyDown(KeyCode.Alpha1)) 
        {
            animator.SetTrigger("attack1");
            Attack(); 
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            animator.SetTrigger("attack2");
            Attack();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            animator.SetTrigger("attack3");
            Attack();
        }

        // --- DİĞER ---
        if (Input.GetKeyDown(KeyCode.Z)) animator.SetTrigger("roll");
        if (Input.GetKey(KeyCode.LeftShift)) speed = 7f; else speed = 5f;

        // --- ÖLÜM ---
        if (healthBar.isDead && !died)
        {
            died = true;
            animator.SetTrigger("deathT");
            rgb.simulated = false; 
            this.enabled = false;  
        }
    }

    void FixedUpdate()
    {
        rgb.linearVelocity = new Vector2(moveInput * speed, rgb.linearVelocity.y);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, radius, groundLayer);
    }

    // --- HASAR VERME FONKSİYONU (GÜNCELLENDİ) ---
    void Attack()
    {
        if (attackPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach(Collider2D enemy in hitEnemies)
        {
            // 1. GOBLİN Mİ?
            goblinyapayzeka goblin = enemy.GetComponent<goblinyapayzeka>();
            if(goblin != null)
            {
                goblin.TakeDamage(attackDamage);
            }

            // 2. İSKELET Mİ? (Yeni Eklenen Kısım)
            skeletonyapayzeka skeleton = enemy.GetComponent<skeletonyapayzeka>();
            if(skeleton != null)
            {
                skeleton.TakeDamage(attackDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}