using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float HealthPoints
    {
        get
        {
            return HealthPoints;
        }
        set
        {
            HealthPoints = value;

            //What if we reach 0?
            if(HealthPoints <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
    [SerializeField]
    private float healthPoints = 100f;
}
