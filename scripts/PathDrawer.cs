using iRunner;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;


public class PathDrawer : MonoBehaviour { //рисуем путь

	public Color DrawColor = Color.white;
	public Transform[] CP_pos;
	public float PlatformLength;

	void OnDrawGizmos () {
		Vector3[] _array = new Vector3[CP_pos.Length];
		for(int i=0;i<CP_pos.Length;i++) {
			_array[i] = CP_pos[i].position;
		}
		DrawPath(_array, DrawColor);
	}

	public static void DrawPath(Vector3[] path, Color color) {
		if(path.Length>0){
			Vector3[] v3 = PathGen(path);
			Vector3 prevPt = ir_Math.Interpolation(v3,0f);
			Gizmos.color=color;
			int SmoothAmount = path.Length*20;
			for (int i = 1; i <= SmoothAmount; i++) {
				float pm = (float) i / SmoothAmount;
				Vector3 currPt = ir_Math.Interpolation(v3,pm);
				Gizmos.DrawLine(currPt, prevPt);
				prevPt = currPt;
			}
		}
	}	

	private static Vector3[] PathGen(Vector3[] path){
		Vector3[] gottenPath = path;
		Vector3[] v3 = new Vector3[gottenPath.Length+2];
		Array.Copy(gottenPath,0,v3,1,gottenPath.Length);
		v3[0] = v3[1] + (v3[1] - v3[2]);
		v3[v3.Length-1] = v3[v3.Length-2] + (v3[v3.Length-2] - v3[v3.Length-3]);
		return(v3);
	}

}
