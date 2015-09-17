using iRunner;
using UnityEngine;

public class patchRandomizer : MonoBehaviour {

	public GameObject[] patchesPrefabs; //из чего будем генерировать
	public Transform tPlayer;           //игрок
	public controlScript ctrlScript;
    	private readonly ir_State gState = ir_State.GetInstance();

    	private Vector3 currDir;
    	private Vector2 currMidPoint;
    	private Vector3[] currCPPositions;
    	private Vector3[] nextCPPositions;

    	private GameObject prevPatch;
	private GameObject currPatch;
	private GameObject nextPatch;
	private GameObject currPath;
	private GameObject nextPath;
	private float patchDistance;	    //длин участка
	private float prevDistance;	        //пройдено
    	public float backOffset = 200.0f;   //удаляем старые через?
    	private float currAngle;
    	private float currentPercent;

    	void Start (){
        	InstantiateStartPatch();		
	    	SetChildGroups();
	    	SetCurrentPatchCPs();
	    	SetNextPatchCPs();
    	}

	void Update (){
	    	if(gState.IsPausedOrDead())
		    	return;
	
		if(prevPatch && tPlayer.position.x>prevDistance-patchDistance+backOffset)
		    Destroy(prevPatch);
	}

	public void NewPatch (){ //создаем новый участок
		prevPatch = currPatch;
		currPatch = nextPatch;
	
		InstantiateNextPatch();
		SetChildGroups();
		prevDistance += patchDistance;
	}

	private void InstantiateNextPatch (){
		pathDrawer pDrawer = currPatch.GetComponentInChildren<pathDrawer>();
		patchDistance = pDrawer.patchLength;
		nextPatch = (GameObject)Instantiate(patchesPrefabs[Random.Range(0,patchesPrefabs.Length)],new Vector3(prevDistance+patchDistance,0,0),new Quaternion());
	}

	private void InstantiateStartPatch (){ //для начала
		currPatch = (GameObject)Instantiate(patchesPrefabs[Random.Range(0,patchesPrefabs.Length)],new Vector3(0,0,0),new Quaternion());
		pathDrawer pDrawer = currPatch.GetComponentInChildren<pathDrawer>();
		patchDistance = pDrawer.patchLength;
		prevDistance += patchDistance;
		nextPatch = (GameObject)Instantiate(patchesPrefabs[Random.Range(0,patchesPrefabs.Length)],new Vector3(patchDistance,0,0),new Quaternion());
	}

	public void SetChildGroups (){
		currPath = currPatch.transform.GetChild(0).gameObject;
		nextPath = nextPatch.transform.GetChild(0).gameObject;
	}
	
	public void SetCurrentPatchCPs (){	//берем контрольные точки
		currAngle = 90.0f;
		pathDrawer pDrawer = currPath.GetComponent<pathDrawer>();
		patchDistance = pDrawer.patchLength;
		
		currCPPositions = new Vector3[pDrawer.CP_pos.Length];
		for(int i=0;i<currCPPositions.Length;i++)
		{
			currCPPositions[i] = pDrawer.CP_pos[i].position;
			currCPPositions[i].x = currCPPositions[i].x;
		}
	}
	
	public void SetNextPatchCPs (){ 
		pathDrawer pDrawer = nextPath.GetComponent<pathDrawer>();
		
		nextCPPositions = new Vector3[pDrawer.CP_pos.Length];
		for(int i=0;i<nextCPPositions.Length;i++)
		{
			nextCPPositions[i] = pDrawer.CP_pos[i].position;
			nextCPPositions[i].x = nextCPPositions[i].x + patchDistance;
		}
	}

	public float SetNextMidPointandRotation (float currentDistanceOnPath ,   float currentForwardSpeed){ //середина, с поворотом
		currentPercent = (currentDistanceOnPath+currentForwardSpeed)/patchDistance;

		if(currentPercent>=1.0f)
		{
			float previousPathLength = patchDistance;
			NewPatch();
			
			currentDistanceOnPath = (currentDistanceOnPath+currentForwardSpeed) - previousPathLength;
			currentDistanceOnPath = Mathf.Abs(currentDistanceOnPath);
							ctrlScript.SetCurrDistanceOnPath(currentDistanceOnPath);
			SetCurrentPatchCPs();
			SetNextPatchCPs();
			
			currentPercent = (currentDistanceOnPath+currentForwardSpeed)/patchDistance;
		}
		
		Vector3 midPointVector3 = ir_Math.Interpolation(currCPPositions,currentPercent);
		
		currMidPoint.x = midPointVector3.x;
		currMidPoint.y = midPointVector3.z;
		
		Vector3 forwardPointVector3 = ir_Math.Interpolation(currCPPositions,currentPercent+0.001f);
		Vector3 backPointVector3 = ir_Math.Interpolation(currCPPositions,currentPercent-0.001f);
		currDir = forwardPointVector3 - backPointVector3;
		currAngle = ir_Math.VectorAngle(currDir);
		if(currAngle>180.0f)
			currAngle = currAngle+360f;

		return (currentDistanceOnPath+currentForwardSpeed);
	}

	public float GetCoveredDistance (){ return prevDistance; } 
	public float GetCurrAngle (){ return currAngle; }
	public Vector3 GetCurrentDirection (){ return currDir; }
	public Vector2 GetCurrentMidPoint (){ return currMidPoint; }
	public GameObject GetCurrentPatch (){ return currPatch; }
	public GameObject GetNextPatch (){ return nextPatch; }
}
