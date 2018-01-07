using System;
using UnityEngine;
using UnityEngine.Collections;
using System.Collections.Generic;

public class DestroyableTree : MonoBehaviour {
	public int terrainIndex;

	public void Delete() {
		Terrain terrain = Terrain.activeTerrain;

		List<TreeInstance> trees = new List<TreeInstance>(terrain.terrainData.treeInstances);
//		trees[terrainIndex] = new TreeInstance();
		TreeInstance tree = trees[terrainIndex];
		tree.prototypeIndex = 1;
		trees [terrainIndex] = tree;

		terrain.terrainData.treeInstances = trees.ToArray();

		Destroy(gameObject);
	}
}