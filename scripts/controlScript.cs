using iRunner;
using System.Collections;
using UnityEngine;

public class controlScript : MonoBehaviour {

	//private Strafe StrafeDirection;
	private Transform tPlayer;	
	public Transform rPlayer;
    public patchRandomizer pRandomizer;
    public animScript player;
    public floorScript floor;

    private Transform frontCollider;    //контроль коллайдера спереди
    private readonly ir_State gState = ir_State.GetInstance();
    private Transform tShadow;	        //тень
    private RaycastHit hitInfo;	        //что под ногами?

	private float currAngle;	
	private float jumpForwardFactor;	
	private float currUpwardVelocity;	
	private float currHeight;
	private float contactPointY;	
	
	//состояние игрока по ходу игры
	private bool bInAir;
	private bool bJumpFlag;
	private bool bInJump;
	private bool bInslide;			
	private bool bDiveFlag;			
	private bool bExecuteLand;
	private bool bInStrafe;
	
	private float forwardAccleration;
	private Vector3 currDir;			//расчетный поворот игрока

	
	private bool groundHit;	
	private float horizontalDistance;	//расчетное положение
	
	private float currForwardSpeed = .5f;	
	private float currDistance;		
	private float currMileage;		
	
	private float pitFallLerpValue;
	private float pitFallForwardSpeed;
	private float pitPositionX;
	
	private bool  jumpFirstTime = true;

	private int lanePosition;			//текущая линия -- -1, 0 or 1
	private bool  mouseReleased = true;
	private bool  controlsEnabled = true;

    //для расчета скорости
    private float currRunSpeed;
    private float speedMultiplier = 5.0f;
    private const float startRunSpeed = 150.0f;
    private const float endRunSpeed = 230.0f;
    private const int acceleration = 500;
    private const float currAcceleration = 0.5f;

    //высота прыжка
    private const float jumpPush = 185;

    private float currDistanceOnPath;

    void Start (){
		tPlayer = transform;

		bInAir = false;
		currDistanceOnPath = 50.0f;
		currDistance = 0.0f;
		currMileage = 0.0f;
		currAngle = 0.0f;	
		pitFallLerpValue = 0.0f;
		pitFallForwardSpeed = 0.0f;
		pitPositionX = 0.0f;
		groundHit = false;	
		bJumpFlag = false;
		bInJump = false;
		currUpwardVelocity = 0;
		currHeight = 0;

		lanePosition = 0;	//центр	
		currRunSpeed = startRunSpeed;
	}

	void Update (){
		if(gState.IsPausedOrDead())
			return;
		
		MovementControl();
	}
	
	void FixedUpdate (){
		if(gState.IsPausedOrDead())
			return;
		
		SetForwardSpeed();
		SetTransform();

		if(!bInAir)
		{
			if(bExecuteLand)
			{
				bExecuteLand = false;
				jumpFirstTime = true;
			}
		} else {		
			if(jumpFirstTime&&bInJump)
			{
				player.Extern("slide",false);
				player.Extern("jump",true);
				bInslide = false;
			}
		}

		if(bJumpFlag)
		{		
			bJumpFlag = false;
			bExecuteLand = true;
			bInJump = true;
			bInAir = true;
			currUpwardVelocity = jumpPush;
			currHeight = tPlayer.position.y;
		}

		if(currRunSpeed<endRunSpeed)
			currRunSpeed += (currAcceleration * Time.fixedDeltaTime);
	}

    private void SetForwardSpeed (){
		//Игрок на земле?
		if(floor.IsFallingInPit())
		{
		    currForwardSpeed =  transform.position.x>pitPositionX ? 
                                0.0f : 
                                Mathf.Lerp(pitFallForwardSpeed,0.01f,(Time.time-pitFallLerpValue)*3.5f);
		    return;
		}

	    forwardAccleration = bInAir ? 1.0f : 2.0f;
		
		jumpForwardFactor = 1 + ((1/currRunSpeed)*50);
		
		currForwardSpeed =  bInJump ? 
                            Mathf.Lerp(currForwardSpeed,currRunSpeed*Time.fixedDeltaTime*jumpForwardFactor,Time.fixedDeltaTime*forwardAccleration) : 
                            Mathf.Lerp(currForwardSpeed,(currRunSpeed)*Time.fixedDeltaTime,Time.fixedDeltaTime*forwardAccleration);
	}

	private void SetTransform (){
		int strafeDirection = lanePosition;//текущая полоса (-1, 0 or 1)

		currDistanceOnPath = pRandomizer.SetNextMidPointandRotation(currDistanceOnPath, currForwardSpeed);//расстояние на текущем участке
		currDistance = currDistanceOnPath + pRandomizer.GetCoveredDistance();//расстояния с начала забега
		currMileage = currDistance/12.0f;//расчет дистанции для UI
		currAngle = pRandomizer.GetCurrAngle();//расчет угла к пути
		rPlayer.localEulerAngles = new Vector3(rPlayer.localEulerAngles.x,-currAngle,rPlayer.localEulerAngles.z);
	    currDir = pRandomizer.GetCurrentDirection();
		Vector3 desiredHorinzontalPos = CalculateHorizontalPosition(strafeDirection);

		groundHit = Physics.Linecast(desiredHorinzontalPos + new Vector3(0,20,0),desiredHorinzontalPos + new Vector3(0,-100,0),out hitInfo,(1<<9));	
		
		if(groundHit && floor.IsFallingInPit()==false)//расчет позиции по Y
			contactPointY = hitInfo.point.y;
		else {
			contactPointY = -10000.0f;
			if(!bInAir)
			{
				if(!bInJump)
				{
					if(ReConfirmPitFalling(desiredHorinzontalPos,strafeDirection))
					{
						floor.SetPitValues();
					}
				}
				bInAir = true;
				currUpwardVelocity = 0;
				currHeight = tPlayer.position.y;
			}
		}
		
		if(!bInAir)
		{
			currHeight=contactPointY+0.6f;
		}
		else
		{
			if (bDiveFlag)	
				SetCurrDiveHeight();
			else			//Прыг
				SetCurrJumpHeight();
		}
		
		tPlayer.position = new Vector3(desiredHorinzontalPos.x,currHeight,desiredHorinzontalPos.z);//положение игрока по XZ
	}

	private Vector3 CalculateHorizontalPosition (int strafeDirection){
	
		Vector2 sideDirectionVector2 = ir_Math.ZVectorRotate(new Vector2(currDir.x,currDir.z),90.0f);
		sideDirectionVector2.Normalize();
		horizontalDistance = Mathf.Lerp(horizontalDistance,-strafeDirection * 40.0f, 0.05f*currForwardSpeed);		
		horizontalDistance = Mathf.Clamp(horizontalDistance,-20.0f,20.0f);		
		Vector2 horizontalPoint = pRandomizer.GetCurrentMidPoint() + sideDirectionVector2*horizontalDistance;
		return new Vector3(horizontalPoint.x,rPlayer.position.y,horizontalPoint.y);
	
	}

	private bool ReConfirmPitFalling (Vector3 desiredHorinzontalPos , float strafeDirection){
		bool gHit = strafeDirection>=0 ? 
                    Physics.Linecast(desiredHorinzontalPos + new Vector3(1,20,5),desiredHorinzontalPos + new Vector3(0,-100,5),out hitInfo,1<<9) : 
                    Physics.Linecast(desiredHorinzontalPos + new Vector3(1,20,-5),desiredHorinzontalPos + new Vector3(0,-100,-5),out hitInfo,1<<9);
		
		return !gHit;
	}

	void MovementControl (){

        #if UNITY_ANDROID || UNITY_IOS

        if (Input.touchCount == 0)
        {
            player.Extern("left", false);
            player.Extern("right", false);
        }
        
        #else
        
        if (Input.GetKeyDown(KeyCode.UpArrow))// прыг
		{
			UiJump();
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))//право
		{
			Move(true);
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))//лево
		{
			Move(false);
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow)) //вниз!
		{
		    UiSlide();
		}

        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            player.Extern("left", false);
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            player.Extern("right", false);
        }
        
        #endif
	}

	public void UiSlide() {
		if(!bInslide) 
			if (!bInAir) {
				SlidePlayer();
		    } else {
			    bDiveFlag = true;
		    }
	}

	public void UiJump() {
		if(!bInAir)	{					
			bJumpFlag = true;
		}
	}

	public void UiLeft() {
		Move(false);
	}

	public void UiRight() {
		Move(true);
	}

	private void Move(bool side) { //0 - left, 1 - right
		if (side) {
			if (lanePosition != 1) 
			{
				lanePosition++;
				
				StrafePlayer(Strafe.Right);
			}
		} else {
			if (lanePosition != -1) {
				lanePosition--;
				
				StrafePlayer(Strafe.Left);
			}
		}
	}

	private void SetCurrJumpHeight()		//высота прыжка
	{
		currUpwardVelocity-=Time.fixedDeltaTime*acceleration;
		currUpwardVelocity = Mathf.Clamp(currUpwardVelocity,-jumpPush,jumpPush);
		currHeight+=currUpwardVelocity*(Time.fixedDeltaTime/1.4f);
		
		if(currHeight<contactPointY)
		{
			currHeight = contactPointY;
			bInAir = false;
			bInJump = false;
			player.Extern("jump",false);
		}
	}

	private void SetCurrDiveHeight()
	{
		currUpwardVelocity-=Time.fixedDeltaTime*2000;
		currUpwardVelocity = Mathf.Clamp(currUpwardVelocity,-jumpPush,jumpPush);
		if(floor.IsFallingInPit() == false)
			currHeight+=currUpwardVelocity*Time.fixedDeltaTime;
		else
		{
			currHeight-=40.0f*Time.fixedDeltaTime;
		}	
		
		if(currHeight<=contactPointY)
		{
			currHeight = contactPointY;
			
			bInAir = false;
			bInJump = false;
			
			SlidePlayer();//катимся
			bDiveFlag = false;		
		}
	}

	void StrafePlayer (Strafe strafeDirection){
		if (IsInAir())
		{	
			player.Extern(strafeDirection.ToString(),true);
		}
		else
		{
			player.Extern(strafeDirection.ToString(),true);
			bInStrafe = true;
		}
	}

    /*
	private void SwitchStrafeToSprint (){
		player.Extern("right",false);
		player.Extern("left",false);
		bInStrafe = false;
	}
    */

	private void SlidePlayer (){
		bInslide = true;
		player.Extern("slide",true);
		StartCoroutine(WSlide(0.75f));
	}

	private IEnumerator WSlide(float time) {
		yield return new WaitForSeconds(time);
		bInslide = false;
		player.Extern("slide",false);
	}

	public bool IsInAir (){
		if (bInAir || bJumpFlag || bInJump || bDiveFlag)
			return true;
		else
			player.Extern("jump",false);
			return false;
	}

	public void SetCurrDistanceOnPath (float value) { currDistanceOnPath = value; }
	public void SetPitFallLerpValue (float value){ pitFallLerpValue = value; }
	public void SetPitFallForwardSpeed (float value){ pitFallForwardSpeed = value; }
	public float GetCurrForwardSpeed (){ return currForwardSpeed; }
//	public float GetCurrPlayerRotation (){ return currAngle; }
    public float GetRunSpeed() { return Mathf.Clamp((currRunSpeed/startRunSpeed)/1.1f, 0.8f, 1.2f); }
	public string GetCurrMileage() { return "Distance: " + ((int)currMileage).ToString(); }

}