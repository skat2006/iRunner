using iRunner;
using UnityEngine;
using System.Collections;

public class ControllerScript : MonoBehaviour {

	private Strafe StrafeDirection;
	private Transform tPlayer;	
	public Transform tPlayerRotation;

	private Transform tFrontCollider;			//контроль коллайдера спереди

	private float currRunSpeed;
	private float fCurrentStrafePosition = 0.0f;	
	private float fSpeedMultiplier = 5.0f;	
	
	private float tCurrentAngle = 0.0f;	
	private float fJumpForwardFactor = 0.0f;	
	private float fCurrentUpwardVelocity = 0.0f;	
	private float fCurrentHeight = 0.0f;
	private float fContactPointY = 0.0f;	
	
	//состояние игрока по ходу игры
	private bool bInAir = false;
	private bool bJumpFlag = false;
	private bool bInJump = false;
	private bool bInslide = false;			
	private bool bDiveFlag = false;			
	private bool bExecuteLand = false;
	private bool bInStrafe = false;
	
	private float fForwardAccleration = 0.0f;
	private Transform tBlobShadowPlane;	//тень
	private Vector3 currDir;			//расчетный поворот игрока

	private RaycastHit hitInfo;	//что под ногами?
	private bool bGroundhit = false;	
	private float fHorizontalDistance = 0.0f;	//расчетное положение
	
	private float fCurrentForwardSpeed = .5f;	
	private float fCurrentDistance = 0.0f;		
	private float fCurrentMileage = 0.0f;		
	
	private float fPitFallLerpValue = 0.0f;
	private float fPitFallForwardSpeed = 0.0f;
	private float fPitPositionX = 0.0f;
	
	private bool  JumpAnimationFirstTime = true;

	private int iLanePosition;						//текущая линия -- -1, 0 or 1
	private int iLastLanePosition; 					//предыдущая линия
	private bool  bMouseReleased = true;
	private bool  bControlsEnabled = true;

	//для расчета скорости
	private float startRunSpeed = 150.0f;
	private float endRunSpeed = 230.0f;	
	private float currAcceleration = 0.5f;	
	
	//высота прыжка
	private float fJumpPush = 185;			
	private int getAccleration (){ return 500; }
	
	private float fCurrentDistanceOnPath = 0.0f;

	//подключаем скрипты
	public PatchesRandomizer _pRandomizer;
	public GameScript _gScript;
	public Animator _anim;
	public floorControll _fCtrl;

	void Start (){
		tPlayer = transform;

		bInAir = false;
		fCurrentDistanceOnPath = 50.0f;
		fCurrentDistance = 0.0f;
		fCurrentMileage = 0.0f;
		tCurrentAngle = 0.0f;	
		fPitFallLerpValue = 0.0f;
		fPitFallForwardSpeed = 0.0f;
		fPitPositionX = 0.0f;
		bGroundhit = false;	
		bJumpFlag = false;
		bInJump = false;
		fCurrentUpwardVelocity = 0;
		fCurrentHeight = 0;

		iLanePosition = 0;	//центр	
		currRunSpeed = startRunSpeed;

		_anim.SetBool("pause",true);
	}

	public void launchGame (){
		//запуск
		_anim.SetFloat("runSpeed",Mathf.Clamp((currRunSpeed/startRunSpeed)/1.1f, 0.8f, 1.2f));
		_anim.SetBool("pause",false);
	}

	void Update (){
		if(_gScript.isGamePaused())
			return;
		
		MovementControl ();
	}
	
	void FixedUpdate (){
		if(_gScript.isGamePaused() == true)
			return;
		
		setForwardSpeed();
		SetTransform();

		if(!bInAir)
		{
			if(bExecuteLand)
			{
				bExecuteLand = false;
				JumpAnimationFirstTime = true;
			}
		} else {		
			if(JumpAnimationFirstTime&&bInJump==true)
			{
				_anim.SetBool("slide",false);
				_anim.SetBool("jump",true);
				bInslide = false;
			}
		}

		if(bJumpFlag==true)
		{		
			bJumpFlag = false;
			bExecuteLand = true;
			bInJump = true;
			bInAir = true;
			fCurrentUpwardVelocity = fJumpPush;
			fCurrentHeight = tPlayer.position.y;
		}

		if(currRunSpeed<endRunSpeed)
			currRunSpeed += (currAcceleration * Time.fixedDeltaTime);
		
		_anim.SetFloat("runSpeed",Mathf.Clamp((currRunSpeed/startRunSpeed)/1.1f, 0.8f, 1.2f ));
	}

	private void setForwardSpeed (){
		//Игрок на земле?
		if(_fCtrl.isFallingInPit())
		{		
			if(transform.position.x>fPitPositionX)
				fCurrentForwardSpeed = 0.0f;
			else
				fCurrentForwardSpeed = Mathf.Lerp(fPitFallForwardSpeed,0.01f,(Time.time-fPitFallLerpValue)*3.5f);
			return;
		}
	
		if(bInAir)
			fForwardAccleration = 1.0f;
		else
			fForwardAccleration = 2.0f;
		
		fJumpForwardFactor = 1 + ((1/currRunSpeed)*50);
		
		if(bInJump==true)
			fCurrentForwardSpeed = Mathf.Lerp(fCurrentForwardSpeed,currRunSpeed*Time.fixedDeltaTime*fJumpForwardFactor,Time.fixedDeltaTime*fForwardAccleration);
		else
			fCurrentForwardSpeed = Mathf.Lerp(fCurrentForwardSpeed,(currRunSpeed)*Time.fixedDeltaTime,Time.fixedDeltaTime*fForwardAccleration);
	}

	private void SetTransform (){
		int iStrafeDirection = iLanePosition;//текущая полоса (-1, 0 or 1)

		fCurrentDistanceOnPath = _pRandomizer.SetNextMidPointandRotation(fCurrentDistanceOnPath, fCurrentForwardSpeed);//расстояние на текущем участке
		//Debug.Log (fCurrentDistanceOnPath);
		fCurrentDistance = fCurrentDistanceOnPath + _pRandomizer.getCoveredDistance();//расстояния с начала забега
		//Debug.Log (fCurrentDistance);
		fCurrentMileage = fCurrentDistance/12.0f;//расчет дистанции для UI
		
		tCurrentAngle = _pRandomizer.getCurrentAngle();//расчет угла к пути

		tPlayerRotation.localEulerAngles = new Vector3(tPlayerRotation.localEulerAngles.x,-tCurrentAngle,tPlayerRotation.localEulerAngles.z);


		currDir = _pRandomizer.getCurrentDirection();
		Vector3 Desired_Horinzontal_Pos = calculateHorizontalPosition(iStrafeDirection);

		bGroundhit = Physics.Linecast(Desired_Horinzontal_Pos + new Vector3(0,20,0),Desired_Horinzontal_Pos + new Vector3(0,-100,0),out hitInfo,(1<<9));	
		
		if(bGroundhit && _fCtrl.isFallingInPit()==false)//расчет позиции по Y
			fContactPointY = hitInfo.point.y;
		else {
			fContactPointY = -10000.0f;
			if(!bInAir)
			{
				if(!bInJump)
				{
					if(reConfirmPitFalling(Desired_Horinzontal_Pos,iStrafeDirection)==true)
					{
						_fCtrl.setPitValues();
					}
				}
				bInAir = true;
				fCurrentUpwardVelocity = 0;
				fCurrentHeight = tPlayer.position.y;
			}
		}
		
		if(!bInAir)
		{
			fCurrentHeight=fContactPointY+0.6f;
		}
		else
		{
			if (bDiveFlag)	
			{
				setCurrentDiveHeight();
			}
			else			//Прыг
			{
				setCurrentJumpHeight();
			}
		}
		
		tPlayer.position = new Vector3(Desired_Horinzontal_Pos.x,fCurrentHeight,Desired_Horinzontal_Pos.z);//положение игрока по XZ
		//Debug.Log (Desired_Horinzontal_Pos.x);
	}

	private Vector3 calculateHorizontalPosition (int iStrafeDirection){
	
		Vector2 SideDirection_Vector2 = ir_Math.zVectorRotate(new Vector2(currDir.x,currDir.z),90.0f);
		SideDirection_Vector2.Normalize();
		fHorizontalDistance = Mathf.Lerp(fHorizontalDistance,-iStrafeDirection * 40.0f, 0.05f*fCurrentForwardSpeed);		
		fHorizontalDistance = Mathf.Clamp(fHorizontalDistance,-20.0f,20.0f);		
		Vector2 fHorizontalPoint = _pRandomizer.getCurrentMidPoint() + SideDirection_Vector2*fHorizontalDistance;
		return new Vector3(fHorizontalPoint.x,tPlayerRotation.position.y,fHorizontalPoint.y);
	
	}

	private bool reConfirmPitFalling ( Vector3 Desired_Horinzontal_Pos ,   float iStrafeDirection  ){
		bool  bGroundhit = false;
		
		if(iStrafeDirection>=0)
			bGroundhit = Physics.Linecast(Desired_Horinzontal_Pos + new Vector3(1,20,5),Desired_Horinzontal_Pos + new Vector3(0,-100,5),out hitInfo,1<<9);
		else
			bGroundhit = Physics.Linecast(Desired_Horinzontal_Pos + new Vector3(1,20,-5),Desired_Horinzontal_Pos + new Vector3(0,-100,-5),out hitInfo,1<<9);
		
		if(!bGroundhit)
			return true;
		else
			return false;
	}

	void MovementControl (){	
		if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))// прыг
		{
			if(!bInAir)
			{					
				bJumpFlag = true;
			}
		}
		else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))//лево
		{
			if (iLanePosition != 1) 
			{
				iLastLanePosition = iLanePosition;
				iLanePosition++;
				
				strafePlayer(Strafe.right);
				
			}
		}
		else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))//право
		{
			if (iLanePosition != -1) 
			{
				iLastLanePosition = iLanePosition;
				iLanePosition--;
				
				strafePlayer(Strafe.left);
				
			}
		}
		else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && !bInAir && !bInslide) //вниз!
		{
			slidePlayer();
		}
		else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && bInAir && !bInslide)
		{
			bDiveFlag = true;
		}

		if (Input.GetKeyUp(KeyCode.LeftArrow))	{
			_anim.SetBool("left",false);
		} else if (Input.GetKeyUp(KeyCode.RightArrow)) {
			_anim.SetBool("right",false);
		}
		
	}

	private void setCurrentJumpHeight()		//высота прыжка
	{
		fCurrentUpwardVelocity-=Time.fixedDeltaTime*getAccleration();
		fCurrentUpwardVelocity = Mathf.Clamp(fCurrentUpwardVelocity,-fJumpPush,fJumpPush);
		fCurrentHeight+=fCurrentUpwardVelocity*(Time.fixedDeltaTime/1.4f);
		
		if(fCurrentHeight<fContactPointY)
		{
			fCurrentHeight = fContactPointY;
			bInAir = false;
			bInJump = false;
			_anim.SetBool("jump",false);

			if (bDiveFlag)	
				return;
		}
	}

	private void setCurrentDiveHeight()
	{
		fCurrentUpwardVelocity-=Time.fixedDeltaTime*2000;
		fCurrentUpwardVelocity = Mathf.Clamp(fCurrentUpwardVelocity,-fJumpPush,fJumpPush);
		if(_fCtrl.isFallingInPit() == false)
			fCurrentHeight+=fCurrentUpwardVelocity*Time.fixedDeltaTime;
		else
		{
			fCurrentHeight-=40.0f*Time.fixedDeltaTime;
		}	
		
		if(fCurrentHeight<=fContactPointY)
		{
			fCurrentHeight = fContactPointY;
			
			bInAir = false;
			bInJump = false;
			
			slidePlayer();//катимся
			bDiveFlag = false;		
		}
	}

	void strafePlayer (Strafe strafeDirection){
		if (isInAir())
		{	
			_anim.SetBool(strafeDirection.ToString(),true);
		}
		else
		{
			_anim.SetBool(strafeDirection.ToString(),true);
			bInStrafe = true;
		}
	}

	private void switchStrafeToSprint (){
		_anim.SetBool("right",false);
		_anim.SetBool("left",false);
		bInStrafe = false;
	}

	private void slidePlayer (){
		bInslide = true;
		_anim.SetBool("slide",true);
		StartCoroutine(wSlide(0.75f));
	}

	private IEnumerator wSlide(float time) {
		yield return new WaitForSeconds(time);
		bInslide = false;
		_anim.SetBool("slide",false);
	}

	public bool isInAir (){
		if (bInAir || bJumpFlag || bInJump || bDiveFlag)
			return true;
		else
			_anim.SetBool("jump",false);
			return false;
	}

	public void setCurrentDistanceOnPath (float iValue) { fCurrentDistanceOnPath = iValue; }
	public void setPitFallLerpValue (float iValue){ fPitFallLerpValue = iValue; }
	public void setPitFallForwardSpeed (float iValue){ fPitFallForwardSpeed = iValue; }
	public float getCurrentForwardSpeed (){ return fCurrentForwardSpeed; }
	public float getCurrentPlayerRotation (){ return tCurrentAngle; }
	public int getCurrentMileage() { return (int)fCurrentMileage; }

}