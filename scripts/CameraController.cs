using UnityEngine;
using System.Collections;
using iRunner;

public class CameraController : MonoBehaviour {

	public Transform tPlayer;	//игрок
	public float oX,oY,oZ; 		//x,y,z отступ

	private Transform tCamera;

	void Start (){
		tCamera = this.transform;
	}

	public void launchGame (){	

	}

	void FixedUpdate (){
		tCamera.position = new Vector3(tPlayer.position.x+oX,tPlayer.position.y+oY,tPlayer.position.z+oZ);
	}
}