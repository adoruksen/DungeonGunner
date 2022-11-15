using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    private int _startingHealth;
    private int _currentHealth;


    /// <summary>
    /// set starting health
    /// </summary>
    public void SetStartingHealth(int startingHealth)
    {
        this._startingHealth = startingHealth;
        _currentHealth = startingHealth;
    }

    /// <summary>
    /// get the starting health
    /// </summary>
    public int GetStartingHealth()
    {
        return _startingHealth;
    }
}
