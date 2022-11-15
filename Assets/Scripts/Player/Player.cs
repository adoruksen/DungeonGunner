using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#region REQUIRE COMPONENTS
[RequireComponent(typeof(SortingGroup))] //sorting group component is used where you have a gameobjects composed of multiple gameobject with multiple sprite renderers.
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))] //not istrigger
[RequireComponent(typeof(PolygonCollider2D))] //istrigger
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
[DisallowMultipleComponent]
#endregion REQUIRE COMPONENS
public class Player : MonoBehaviour
{
    [HideInInspector] public PlayerDetailsSO playerDetails;
    [HideInInspector] public Health health;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public Animator animator;

    private void Awake()
    {
        //load components
        health = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// initialize the player
    /// </summary>
    public void Initialize(PlayerDetailsSO playerDetails)
    {
        this.playerDetails = playerDetails;

        //set the player health
        SetPlayerHealth();
    }

    /// <summary>
    /// set player health from playerDetailsSO
    /// </summary>
    private void SetPlayerHealth()
    {
        health.SetStartingHealth(playerDetails.playerHealthAmount);
    }
}
