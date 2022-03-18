using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableWaves : MonoBehaviour
{

    public GameObject EnemySpawner;
    public GameObject FalseFloor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            FalseFloor.SetActive(false);
            EnemySpawner.SetActive(true);
        }
    }
    
}
