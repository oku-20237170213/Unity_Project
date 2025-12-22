using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Player_HealthBar : MonoBehaviour
{
    public Image BackGroundImg;
    public Image HealtImg;

    public float MaxHealth;
    float CurrentHealth;
    public bool isDead = false;
    void Start()
    {
        CurrentHealth = MaxHealth;

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)){
            GetDamage(10);
        }
        if (HealtImg.fillAmount != BackGroundImg.fillAmount)
        {
            BackGroundImg.fillAmount = Mathf.Lerp(BackGroundImg.fillAmount, HealtImg.fillAmount, 0.01f);
        }

    }
     public void GetDamage(float damage) {
        CurrentHealth -= damage;
        HealtImg.fillAmount = CurrentHealth / MaxHealth;
        if(CurrentHealth < 0)
        {
            CurrentHealth = 0;
            isDead = true;
        }
        

    }
}
