using UnityEngine.UI;
using iRunner;
using UnityEngine;
using System.Collections;

public class GameScript : MonoBehaviour {

	public Text distance;
	private int pHP = 100;	//здоровье
	private int score = 0;	//текущий счет

	public ControllerScript _Ctrl;
	public CameraController _camCtrl;
	public Animator _anim; //аниматор меню
	public Animator _aMenu; //аниматор игрока

	private GameState gStatus = GameState.Menu;

	private bool  bGameOver = false;
	public bool  bGamePaused = true;
	
	void Start (){
		RenderSettings.fogColor = Color.black;
		RenderSettings.fog = true;				//включает черный туман
	}
	
	void Update (){	
		if (gStatus == GameState.Menu)	{
			bGamePaused = true;
		} else if(gStatus == GameState.Pause)	{	
			pauseGame();
		} else if(gStatus == GameState.Play) {	
			bGamePaused = false;	
			distance.text = "Distance: "+_Ctrl.getCurrentMileage().ToString ();
		}

		if (pHP <= 0 && !bGamePaused) {
			DeathMenu();
		}
		
		if (bGamePaused == true)
			return;
	}
	
	public void pauseGame (){
		_anim.SetBool("pause",true);
		bGamePaused = true;
	}
	
	public void launchGame (){	
		_aMenu.SetBool("showInterface",true);
		_anim.SetBool("pause",false);

		gStatus = GameState.Play;

		_Ctrl.launchGame();
		_camCtrl.launchGame();
	}

	public void DeathMenu (){
		bGamePaused = true;	
		_aMenu.SetBool("death",true);
		_anim.SetBool("death",true);
		_anim.SetBool("pause",true);
	}
	
	public void mainMenu (){
		Application.LoadLevel(0);
	}

	public void Resume() {
		_anim.SetBool("pause",false);
	}

	public void Restart (){
			Application.LoadLevel(0);
			launchGame();
	}

	public void collidedWithObstacle (){
		gotDamage(100);
	}
	
	void OnApplicationPause (bool pause){
		if(Application.isEditor==false)
		{
			if(bGamePaused==false&&pause==false)
			{
				pauseGame();
			}
		}	
	}

	public bool isDead () { return (pHP <= 0 ? true : false); }
	public bool isGamePaused (){ return bGamePaused; }
	public int getLevelScore (){ return score; }
	public int getHP (){ return pHP; }
	public void incrementScore (int iValue) { score += iValue; }
	public void gotDamage (int iValue) { pHP -= iValue; }
	
}
