using iRunner;
using UnityEngine;

public class gameScript : MonoBehaviour {

	private readonly ir_State gState = ir_State.GetInstance();
    private readonly ir_Health pHealth = ir_Health.GetInstance();

	void Start() {
		RenderSettings.fogColor = Color.black;
		RenderSettings.fog = true;				//включает черный туман

        gState.SetState(GameState.Pause);
	}
	
	void Update() {	
		if (pHealth.GetHealth() <= 0 && !gState.IsPausedOrDead()) {
			Death();
		}
	}
	
	public void PauseGame() {
		gState.SetState(GameState.Pause);
	}
	
	public void LaunchGame() {
        gState.SetState(GameState.Play);
	}

	public void Death() {
		gState.SetState(GameState.Death);	
	}
	
	public void MainMenu() {
		Application.LoadLevel(0);
	}

	public void Restart() {
			Application.LoadLevel(0);
			LaunchGame();
	}
	
	void OnApplicationPause(bool pause) {
		if(Application.isEditor==false)
		{
			if(!gState.IsPausedOrDead()&&!pause)
			{
			    gState.SetState(GameState.Pause);
			}
		}	
	}
}
