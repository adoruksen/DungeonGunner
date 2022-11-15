using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// getting data from different scene(character selection scene) by using player details so

[CreateAssetMenu(fileName = "CurrentPlayer", menuName = "Scriptable Objects/Player/Current Player")]
public class CurrentPlayerSO : ScriptableObject
{
    public PlayerDetailsSO playerDetails;
    public string playerName;
}
