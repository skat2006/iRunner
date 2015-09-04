using UnityEngine;
using System.Collections;

public class pCollider : MonoBehaviour { //ловим врезания

	public GameScript _gScript;

	private void OnCollisionEnter( Collision col) {
		if (col.gameObject.tag == "Obstacle") {
			_gScript.gotDamage(100);
		}
	}

}
