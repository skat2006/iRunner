using UnityEngine;
using iRunner;

public class colliderScript : MonoBehaviour { //ловим врезания

    private readonly ir_Health pHealth = ir_Health.GetInstance();

    private void OnCollisionEnter(Collision col) {
		if (col.gameObject.tag == "Obstacle")
            pHealth.LoseHealth(100);
	}
}
