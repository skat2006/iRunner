using iRunner;
using UnityEngine;
using System.Collections;

public class PatchesRandomizer : MonoBehaviour { //случайные сцены

	public GameObject[] patchesPrefabs;//из чего будем генерировать
	public Transform tPlayer;//игрок
	public ControllerScript _Ctrl;
	public GameScript _gScript;

	public float backOffset = 200.0f; //удаляем старые через?
	private GameObject prevPatch;
	private GameObject currPatch;
	private GameObject nextPatch;
	private GameObject currPath;
	private GameObject nextPath;
	private float patchDistance;	//длин участка
	private float nextPatchDistance = 0.0f;//длина следующего участка
	private float prevDistance = 0.0f;	//пройдено

	private float CurrentAngle = 0.0f;
	private Vector3 currDir;
	private Vector2 currMidPoint;
	
	private Vector3[] CPPositions;
	private Vector3[] NextCPPositions;
	
	private float WaypointAngle = 0.0f;
	private float CurrentPercent = 0.0f;

void Start (){
	instantiateStartPatch();		
	
	setChildGroups();
	SetCurrentPatchCPs();
	SetNextPatchCPs();
}

	void Update (){
	if(_gScript.bGamePaused)
		return;
	
		if(prevPatch && tPlayer.position.x>prevDistance-patchDistance+backOffset)
		{
			Destroy(prevPatch);
		}
	}

	public void newPatch (){ //создаем новый участок
		prevPatch = currPatch;
		currPatch = nextPatch;
	
		instantiateNextPatch();
		setChildGroups();
		prevDistance += patchDistance;
	}

	private void instantiateNextPatch (){
		PathDrawer _pd = (PathDrawer)currPatch.GetComponentInChildren<PathDrawer>();
		patchDistance = _pd.PlatformLength;
		nextPatch = (GameObject)Instantiate(patchesPrefabs[Random.Range(0,patchesPrefabs.Length)],new Vector3(prevDistance+patchDistance,0,0),new Quaternion());
	}

	private void instantiateStartPatch (){ //для начала
		currPatch = (GameObject)Instantiate(patchesPrefabs[Random.Range(0,patchesPrefabs.Length)],new Vector3(0,0,0),new Quaternion());
		PathDrawer _pd = (PathDrawer)currPatch.GetComponentInChildren<PathDrawer>();
		patchDistance = _pd.PlatformLength;
		prevDistance += patchDistance;
		nextPatch = (GameObject)Instantiate(patchesPrefabs[Random.Range(0,patchesPrefabs.Length)],new Vector3(patchDistance,0,0),new Quaternion());
	}

	public void setChildGroups (){
		currPath = currPatch.transform.GetChild(0).gameObject;
		nextPath = nextPatch.transform.GetChild(0).gameObject;
	}
	
	public void SetCurrentPatchCPs (){	//берем контрольные точки
		CurrentAngle = 90.0f;
		PathDrawer _pd = currPath.GetComponent<PathDrawer>();
		patchDistance = _pd.PlatformLength;
		
		CPPositions = new Vector3[_pd.CP_pos.Length];
		for(int i=0;i<CPPositions.Length;i++)
		{
			CPPositions[i] = _pd.CP_pos[i].position;
			CPPositions[i].x = CPPositions[i].x;
		}
	}
	
	public void SetNextPatchCPs (){ 
		PathDrawer _pd = nextPath.GetComponent<PathDrawer>();
		nextPatchDistance = _pd.PlatformLength;
		
		NextCPPositions = new Vector3[_pd.CP_pos.Length];
		for(int i=0;i<NextCPPositions.Length;i++)
		{
			NextCPPositions[i] = _pd.CP_pos[i].position;
			NextCPPositions[i].x = NextCPPositions[i].x + patchDistance;//prevDistance;
		}
	}

	public float SetNextMidPointandRotation ( float CurrentDistanceOnPath ,   float CurrentForwardSpeed  ){ //середина, с поворотом
		CurrentPercent = (CurrentDistanceOnPath+CurrentForwardSpeed)/patchDistance;

		if(CurrentPercent>=1.0f)
		{
			float PreviousPathLength = patchDistance;
			newPatch();
			
			CurrentDistanceOnPath = (CurrentDistanceOnPath+CurrentForwardSpeed) - PreviousPathLength;
			CurrentDistanceOnPath = Mathf.Abs(CurrentDistanceOnPath);
							_Ctrl.setCurrentDistanceOnPath(CurrentDistanceOnPath);
			SetCurrentPatchCPs();
			SetNextPatchCPs();
			
			CurrentPercent = (CurrentDistanceOnPath+CurrentForwardSpeed)/patchDistance;
		}
		
		Vector3 MidPointVector3 = ir_Math.Interpolation(CPPositions,CurrentPercent);
		
		currMidPoint.x = MidPointVector3.x;
		currMidPoint.y = MidPointVector3.z;
		//Debug.Log (currMidPoint.x);
		
		Vector3 ForwardPointVector3 = ir_Math.Interpolation(CPPositions,CurrentPercent+0.001f);
		Vector3 BackPointVector3 = ir_Math.Interpolation(CPPositions,CurrentPercent-0.001f);
		currDir = ForwardPointVector3 - BackPointVector3;
		CurrentAngle = ir_Math.vectorAngle(currDir);
		if(CurrentAngle>180.0f)
			CurrentAngle = CurrentAngle+360f;
		//Debug.Log(CurrentAngle);
		return (CurrentDistanceOnPath+CurrentForwardSpeed);
	}

	public Vector3 getCurrentWSPointBasedOnPercent (float percent){
		WaypointAngle = ir_Math.vectorAngle(ir_Math.pointFromPercent(percent,CPPositions));
		if(WaypointAngle>180.0f) WaypointAngle = WaypointAngle-360.0f;
		
		return ir_Math.Interpolation(CPPositions,percent);
	}
	
	public Vector3 getNextWSPointBasedOnPercent (float percent){
		WaypointAngle = ir_Math.vectorAngle(ir_Math.pointFromPercent(percent,NextCPPositions));
		if(WaypointAngle>180.0f) WaypointAngle = WaypointAngle-360.0f;
		
		return ir_Math.Interpolation(NextCPPositions,percent);
	}

	public float getCoveredDistance (){ return prevDistance; } 
	public float getCurrentAngle (){ return CurrentAngle; }
	public float getWaypointAngle (){ return WaypointAngle; }
	public Vector3 getCurrentDirection (){ return currDir; }
	public Vector2 getCurrentMidPoint (){ return currMidPoint; }
	public GameObject getCurrentPatch (){ return currPatch; }
	public GameObject getNextPatch (){ return nextPatch; }
}