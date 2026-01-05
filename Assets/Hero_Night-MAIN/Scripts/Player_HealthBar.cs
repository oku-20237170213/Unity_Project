using UnityEngine;
using UnityEngine.UI;

public class Player_HealthBar : MonoBehaviour
{
    [Header("UI Görselleri")]
    public Image BackGroundImg;
    public Image HealthImg;

    [Header("Değerler")]
    public float MaxHealth = 100f;
    public float CurrentHealth;

    public bool isDead = false;

    void Start()
    {
        CurrentHealth = MaxHealth;
        isDead = false;

        if (HealthImg != null) HealthImg.fillAmount = 1f;
        if (BackGroundImg != null) BackGroundImg.fillAmount = 1f;
    }

    void Update()
    {
        if (BackGroundImg != null && HealthImg != null)
        {
            BackGroundImg.fillAmount = Mathf.Lerp(
                BackGroundImg.fillAmount,
                HealthImg.fillAmount,
                Time.deltaTime * 5f
            );
        }
    }

    public void GetDamage(float damage)
    {
        if (isDead) return;

        CurrentHealth -= damage;

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            isDead = true;
            Debug.Log("HealthBar: Oyuncu Öldü.");
        }
        UpdateUI();
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        CurrentHealth += amount;
        if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
        
        UpdateUI();
    }

    void UpdateUI()
    {
        if (HealthImg != null)
        {
            HealthImg.fillAmount = CurrentHealth / MaxHealth;
        }
    }
}