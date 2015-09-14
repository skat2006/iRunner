using UnityEngine;

public class cameraScript: MonoBehaviour {

    public Transform tPlayer;	//игрок
    public float oX,oY,oZ;      //x,y,z отступ

	void FixedUpdate (){
		transform.position = new Vector3(tPlayer.position.x+oX,tPlayer.position.y+oY,tPlayer.position.z+oZ);
	}
}