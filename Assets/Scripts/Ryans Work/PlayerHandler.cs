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
    public LevelManager LevelManager;
    public GameObject Manager;

    public HealthBar HealthBar;

    private void Awake()
    {
        HealthBar.SetMaxHealth(MaxHealth);
        CanHit = true;
        PlayerAnimator = GetComponent<PlayerAnimator>();
        Manager = GameObject.Find("LevelManager");
        LevelManager = Manager.GetComponent<LevelManager>();
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
            LevelManager.GainScore(100);
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
        HealthBar.setHealth(Health);

        // Kills player
        if (Health <= 0)
        {
            LevelManager.PlayerLives = LevelManager.PlayerLives - 1;
            if (LevelManager.PlayerLives == 0)
            {
                LevelManager.PlayerLives = 3;
                LevelManager.PlayerScore = 0;
                LevelManager.instance.LoadScene("Menu");
            }
            else
            {
                LevelManager.instance.LoadScene(LevelManager.CurrentScene);
            }
            
        }
    }
}
