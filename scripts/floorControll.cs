using UnityEngine;
using System.Collections;

public class floorControll : MonoBehaviour {

	private Transform tPlayer;
	private bool  startFalling = false;
	private float healthLoseSpeed = 10.0f;

	public GameScript _gScript;
	public ControllerScript _Ctrl;

	void Start (){
		tPlayer = GameObject.Find("Player").transform;
		startFalling = false;
	}

	void Update (){
		if(_gScript.isGamePaused()==true)
			return;
	
		if(startFalling) {
			_gScript.gotDamage((int)((_gScript.getHP()/healthLoseSpeed) + Time.deltaTime*100));
		}
	}

	public void setPitValues (){
		startFalling = true;
		
		_Ctrl.setPitFallLerpValue(Time.time);
		_Ctrl.setPitFallForwardSpeed(_Ctrl.getCurrentForwardSpeed());
			
		//Debug.Log("fell");
	}

	public bool isFallingInPit (){ return startFalling; }
}