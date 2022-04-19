using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    //Public
    public int MaxHealth;
    public int Damage;
    public float Range;
    public float TimeBetweenSwing;
    public bool CanHit; 
    public int Health;
    

    //References
    public GameObject Player;
    public RaycastHit RayHit;
    public LayerMask WhatIsEnemy;
    PlayerAnimator PlayerAnimator;

    private void Awake()
    {
        CanHit = true;
        PlayerAnimator = GetComponent<PlayerAnimator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        Health = MaxHealth;
    }
    private void Update()
    {
        // Checks for user input
        if (Input.GetButtonDown("Fire1") && CanHit == true)
        {
            SwingSword();
        }
    }

    void SwingSword()
    {
        CanHit = false;
        Player.GetComponent<PlayerAnimator>().Attack();

        if (Physics.Raycast(Player.transform.position, Player.transform.forward, out RayHit, Range, WhatIsEnemy))
        {
            Debug.Log("Hit Enemy");
        }

        Invoke("DrawBackSword", TimeBetweenSwing);

    }

    void DrawBackSword()
    {
        CanHit = true;
    }

    public void TakeDamage(int Damage)
    {
        // Takes the passed damage amount and updates visuals
        Health = Health - Damage;
        //HealthBar.setHealth(CurrentHealth);

        // Kills player
        if (Health <= 0)
        {
            Destroy(gameObject);
            LevelManager.instance.LoadScene("Menu");
        }
    }
}
