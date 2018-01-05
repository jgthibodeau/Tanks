using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class MyInputManager : MonoBehaviour {

	public static AccelerationEvent GetAccelerationEvent (int index){
		return Input.GetAccelerationEvent(index);
	}
	public static float GetAxis (string name){
		#if UNITY_EDITOR
		return Input.GetAxis(name);
		#else
		return CrossPlatformInputManager.GetAxis(name);
		#endif
	}
	public static float GetAxisRaw (string name){
		#if UNITY_EDITOR
		return Input.GetAxisRaw(name);
		#else
		return CrossPlatformInputManager.GetAxisRaw(name);
		#endif
	}
	public static bool GetButton (string name){
		#if UNITY_EDITOR
		return Input.GetButton(name);
		#else
		return CrossPlatformInputManager.GetButton(name);
		#endif
	}
	public static bool GetButtonDown (string name){
		#if UNITY_EDITOR
		return Input.GetButtonDown(name);
		#else
		return CrossPlatformInputManager.GetButtonDown(name);
		#endif
	}
	public static bool GetButtonUp (string name){
		#if UNITY_EDITOR
		return Input.GetButtonUp(name);
		#else
		return CrossPlatformInputManager.GetButtonUp(name);
		#endif
	}
	public static string[] GetJoystickNames (){
		return Input.GetJoystickNames();
	}
	public static bool GetKey (string name){
		return Input.GetKey(name);
	}
	public static bool GetKeyDown (string name){
		return Input.GetKeyDown(name);
	}
	public static bool GetKeyUp (string name){
		return Input.GetKeyUp(name);
	}
	public static bool GetMouseButton (int index){
		return Input.GetMouseButton(index);
	}
	public static bool GetMouseButtonDown (int index){
		return Input.GetMouseButtonDown(index);
	}
	public static bool GetMouseButtonUp (int index){
		return Input.GetMouseButtonUp(index);
	}
	public static Touch GetTouch (int index){
		return Input.GetTouch(index);
	}
}
