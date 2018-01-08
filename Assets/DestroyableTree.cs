using System;
using UnityEngine;
using UnityEngine.Collections;
using System.Collections.Generic;

public class DestroyableTree : MonoBehaviour, IHittable {
	public int terrainIndex;
	public GameObject deadTree;
	public bool replaceTree;

	public void Hit(float damage, GameObject hitter) {
		Delete ();
	}

	public void Delete() {
		Terrain terrain = Terrain.activeTerrain;

		List<TreeInstance> treesInstances = new List<TreeInstance>(terrain.terrainData.treeInstances);
		if (replaceTree) {
			TreeInstance treeInstance = treesInstances [terrainIndex];
			treeInstance.prototypeIndex = 1;
			treesInstances [terrainIndex] = treeInstance;
		} else {
			treesInstances [terrainIndex] = new TreeInstance ();
		}


		terrain.terrainData.treeInstances = treesInstances.ToArray();


		GameObject deadTreeInst = GameObject.Instantiate (deadTree);
		deadTreeInst.transform.parent = terrain.transform;
		Vector3 position = transform.position;
//		position.y += 0.2f;
		deadTreeInst.transform.position = position;
//		deadTreeInst.transform.localScale = new Vector3 (treeInstance.heightScale, treeInstance.heightScale, treeInstance.heightScale);
//		deadTreeInst.transform.localPosition = Vector3.Scale(treeInstance.position, terrain.terrainData.size);
//		Debug.Break ();


		Destroy(gameObject);
	}
}