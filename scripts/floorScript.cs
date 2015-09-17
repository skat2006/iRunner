using UnityEngine;
using iRunner;

public class floorScript : MonoBehaviour {

	private bool startFalling;
    	private const float healthLoseSpeed = 10.0f;

    	private readonly ir_State gState = ir_State.GetInstance();
    	private readonly ir_Health pHealth = ir_Health.GetInstance();

    	public controlScript ctrlScript;

	void Start (){
		startFalling = false;
    	}

	void Update (){
		if(gState.IsPausedOrDead())
			return;
	
		if(startFalling) {
			pHealth.LoseHealth((int)((pHealth.GetHealth()/healthLoseSpeed) + Time.deltaTime*100));
		}
	}

	public void SetPitValues (){
		startFalling = true;
		
		ctrlScript.SetPitFallLerpValue(Time.time);
		ctrlScript.SetPitFallForwardSpeed(ctrlScript.GetCurrForwardSpeed());
	}

	public bool IsFallingInPit (){ return startFalling; }
}
