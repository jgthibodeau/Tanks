using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class MatchMaking : MonoBehaviour {
	public GameObject matchButton;
	public RectTransform matchList;
	public InputField matchNameInput;
	public NetworkManager networkManager;
	public Menu matchMakingMenu;
	public bool clearOldMatches;

	// Use this for initialization
	void OnEnable () {
		Debug.Log ("Starting matchmaker...");
		networkManager.StartMatchMaker ();
		ListMatches (clearOldMatches);
	}

	public void ListMatches(bool clearMatches) {
		if (clearMatches) {
			Debug.Log ("Removing old matches...");
			foreach (Transform child in matchList.transform) {
				GameObject.Destroy (child.gameObject);
			}
		}
		Debug.Log ("Finding matches...");
		networkManager.matchMaker.ListMatches (0, 10, "", true, 0, 0, OnMatchList);
	}

	public void CreateMatch() {
		string matchName = matchNameInput.text;
		Debug.Log ("Creating match " + matchName + "...");
		networkManager.matchMaker.CreateMatch (matchName, 4, true, "", "", "", 0, 0, OnMatchCreated);
	}

	void OnMatchCreated(bool success, string extendedInfo, MatchInfo matchInfo) {
		if (success) {
			Debug.Log ("Created match");
			MatchInfo hostInfo = matchInfo;
			NetworkServer.Listen (hostInfo, 9000);
			networkManager.StartHost (hostInfo);
			matchMakingMenu.Hide ();
		} else {
			Debug.Log ("Failed to create match");
		}
	}

	void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches) {
		if (success) {
			Debug.Log ("Found matches");
			if (matches.Count != 0) {
				foreach (MatchInfoSnapshot matchInfo in matches) {
					GameObject newMatch = GameObject.Instantiate (matchButton, matchList);
					newMatch.GetComponentInChildren<Text> ().text = "Join Match: " + matchInfo.name;
					newMatch.GetComponent<Button> ().onClick.AddListener (delegate {
						JoinMatch (matchInfo.networkId);
					});
				}
			}
		} else {
			Debug.Log ("Failed to find matches");
		}
	}

	public void JoinMatch(UnityEngine.Networking.Types.NetworkID networkID) {
		Debug.Log ("Joining match " + networkID);
		networkManager.matchMaker.JoinMatch (networkID, "", "", "", 0, 0, OnMatchJoined);
		matchMakingMenu.Hide ();
	}

	void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo) {
		if (success) {
			Debug.Log ("Joined match " + matchInfo.networkId);
			MatchInfo hostInfo = matchInfo;
			networkManager.StartClient (hostInfo);
		} else {
			Debug.Log ("Failed to join match "+matchInfo.networkId);
		}
	}
}
