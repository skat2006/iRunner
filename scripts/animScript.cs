using iRunner;
using UnityEngine;
using UnityEngine.UI;


public class animScript : MonoBehaviour {

    public Animator animPlayer;     //аниматор игрока
    public Animator animMenu;       //аниматор меню
    public Animator animControls;   //аниматор управления

    public Text distance;
    public controlScript ctrlScript;

    private GameState lastState;
    private readonly ir_State gState = ir_State.GetInstance();
	
	void Update ()
	{
        if (!Equals(lastState, gState.GetState()))
	    {
	        lastState = gState.GetState();

	        switch ((int)lastState)
	        {
	            case 0: break;
                case 1: Play(); break;
                case 2: Pause(); break;
                case 3: Death(); break;
	        }
	    }

	    if (gState.GetState() == GameState.Play)
	        animPlayer.SetFloat("runSpeed", ctrlScript.GetRunSpeed());
        distance.text = ctrlScript.GetCurrMileage();
    }

    private void Pause() {
        animPlayer.SetBool("pause",true);
        animMenu.SetBool("pause",true);        
    }

    private void Play() {
        Resume();
        animMenu.SetBool("showInterface",true);
        #if UNITY_ANDROID || UNITY_IOS
            animControls.SetBool("mobile",true);
        #endif
    }

    private void Death() {
        animPlayer.SetBool("death", true);
        animPlayer.SetBool("pause", true);
        animMenu.SetBool("death",true);       
    }

    public void Resume() {
        animPlayer.SetBool("pause",false);
        animMenu.SetBool("pause",false);
    }

    public void Extern(string parameter, bool value) {
        animPlayer.SetBool(parameter,value);
    }
}
