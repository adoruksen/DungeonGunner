using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent] //only can be added one to gameobject
public class GameManager : SingletonMonobehaviour<GameManager>
{
    #region Header DUNGEON LEVELS
    [Space(10)]
    [Header("DUNGEON LEVELS")]
    #endregion Header DUNGEON LEVELS
    #region Tooltip
    [Tooltip("Populate with the dungeon level scriptable objects")]
    #endregion Tooltip

    [SerializeField] private List<DungeonLevelSO> _dungeonLevelList;

    #region Tooltip
    [Tooltip("Populate with the starting Dungeon level for testing , first level =0")]
    #endregion

    [SerializeField] private int currentDungeonLevelListIndex = 0;

    [HideInInspector] public GameState gameState;

    private void Start()
    {
        gameState = GameState.gameStarted;
    }
    private void Update()
    {
        HandleGameState();

        if (Input.GetKeyDown(KeyCode.R))
        {
            gameState = GameState.gameStarted;
        }
    }

    /// <summary>
    /// Handle game state
    /// </summary>
    private void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:
                //Play first level
                PlayDungeonLevel(currentDungeonLevelListIndex);
                gameState = GameState.playingLevel;
                break;
        }
    }

    private void PlayDungeonLevel(int dungeonLevelIndex)
    {

    }

    #region Valitadion
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(_dungeonLevelList), _dungeonLevelList);
    }
#endif
    #endregion Validation
}
