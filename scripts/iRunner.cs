using UnityEngine;

namespace iRunner
{

    	public enum GameState //состояние
    	{
        	Menu = 0,   //в меню
        	Play = 1,   //игра запущена
        	Pause = 2,  //пауза
        	Death = 3   //смерть
    	}

    	public enum Strafe  //смена полосы
	{
		Left = 0,   //лево
		Right = 1   //право
	}

	public class ir_Math
	{

		public static Vector3 PointFromPercent (float percent, Vector3[] positions){
			Vector3 sideDir = new Vector3(0,0,1);
			Vector3 vForward = Interpolation(positions,(percent+0.001f));
			Vector3 vBack = Interpolation(positions,(percent-0.001f));
			Vector3 nextDir = vForward - vBack;
			sideDir = YVectorRotate(nextDir, 90.0f);
			sideDir.Normalize();

			return nextDir;
		}

		public static Vector3 YVectorRotate (Vector3 iVector, float rAngle){
			Vector3 rVector = Vector3.zero;
			rAngle = rAngle/57.3f;
			rVector.x = Mathf.Cos(rAngle) * iVector.x - Mathf.Sin(rAngle) * iVector.z;
			rVector.z = Mathf.Sin(rAngle) * iVector.x + Mathf.Cos(rAngle) * iVector.z;
			
			return rVector;
		}

		public static Vector2 ZVectorRotate (Vector2 iVector ,   float rAngle  ){
			Vector2 rVector = Vector2.zero;
			rAngle = rAngle/57.3f;
			rVector.x = Mathf.Cos(rAngle) * iVector.x - Mathf.Sin(rAngle) * iVector.y;
			rVector.y = Mathf.Sin(rAngle) * iVector.x + Mathf.Cos(rAngle) * iVector.y;
			
			return rVector;
		}

		public static float VectorAngle (Vector3 iVector){
			float iAngle = 57.3f * (Mathf.Atan2(iVector.z,iVector.x));
			if(iAngle<0.0f)	iAngle = iAngle + 360.0f;

			return iAngle;
		}

		public static Vector3 Interpolation(Vector3[] pts, float t){
			int num = pts.Length - 3;
			int currPt  = Mathf.Min(Mathf.FloorToInt(t * float.Parse(num.ToString())), num - 1);
			float u = t * float.Parse(num.ToString()) - float.Parse(currPt.ToString());

			Vector3 a = pts[currPt];
			Vector3 b = pts[currPt + 1];
			Vector3 c = pts[currPt + 2];
			Vector3 d = pts[currPt + 3];
			Vector3 rez = .5f * ((-a + 3f * b - 3f * c + d) * (u * u * u) + (2f * a - 5f * b + 4f * c - d) * (u * u) + (-a + c) * u + 2f * b);

			return rez;
		}
	}

    	public class ir_Health
    	{
        	private static ir_Health healthManager;

        	public static ir_Health GetInstance() {
            		return healthManager ?? (healthManager = new ir_Health());
        	}

        	private int health = 100;

        	public void LoseHealth(int damage) {
            		health -= damage;
        	}

        	public int GetHealth() {
            		return health;
        	}
    	}

    	public class ir_State
    	{
        	private static ir_State gameState;

        	public static ir_State GetInstance() {
            		return gameState ?? (gameState = new ir_State());
        	}

        	private GameState currentState;

        	public void SetState(GameState gState) {
            		currentState = gState;
        	}

        	public GameState GetState() {
            		return currentState;
        	}

        	public bool IsPausedOrDead() {
            		return ((currentState == GameState.Pause) || (currentState == GameState.Death));
        	}
    	}
}
