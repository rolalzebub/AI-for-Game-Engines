using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour
{
    public GameObject Player;
    PlayerHandler Handler;

    private void Awake()
    {
        Handler = Player.GetComponent<PlayerHandler>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Handler.TakeDamage(Handler.Health);
        }
    }
}
