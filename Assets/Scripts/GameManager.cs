using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public enum GameState {
		none,
		playing,
		paused
	}
	private GameState gameState = GameState.none;

	void Start() 
	{
		//Immediately set us to the playing state
		gameState = GameState.none;
	}

	//Return the current game state
	public GameState GetGameState()
	{
		return gameState;
	}

	//Update the current game state with the new game state
	public void ChangeGameState(GameState newState)
	{
		gameState = newState;			
	}
}
