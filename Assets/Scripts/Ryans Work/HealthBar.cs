using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    public Slider Slider;
    
    // Manages the slider for health display
    public void SetMaxHealth(float Health)
    {
        Slider.maxValue = Health;
        Slider.value = Health;
    }
    public void setHealth(float Health)
    {
        Slider.value = Health;
    }


}


