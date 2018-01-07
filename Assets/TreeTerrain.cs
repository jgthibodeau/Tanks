using System;
using UnityEngine;
using UnityEngine.Collections;
using System.Collections.Generic;

public class TreeTerrain : MonoBehaviour {
	public Terrain terrain;
	private TreeInstance[] _originalTrees;

	void Start () {
		terrain = GetComponent<Terrain>();

		// backup original terrain trees
		_originalTrees = terrain.terrainData.treeInstances;

		// create capsule collider for every terrain tree
		for (int i = 0; i < terrain.terrainData.treeInstances.Length; i++) {
			TreeInstance treeInstance = terrain.terrainData.treeInstances[i];
			TreePrototype treePrototype = terrain.terrainData.treePrototypes [treeInstance.prototypeIndex];
			GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
//			GameObject capsule = new GameObject();

			CapsuleCollider capsuleCollider = capsule.GetComponent<Collider>() as CapsuleCollider;
			float height = treePrototype.prefab.GetComponent<MeshRenderer> ().bounds.size.y * treeInstance.heightScale + 0.5f;
			capsuleCollider.height = height;
			capsuleCollider.center = new Vector3 (0, height / 2 - 0.5f, 0);

			DestroyableTree tree = capsule.AddComponent<DestroyableTree>();
			tree.terrainIndex = i;

			capsule.transform.parent = terrain.transform;
			capsule.transform.localPosition = Vector3.Scale(treeInstance.position, terrain.terrainData.size);
			capsule.tag = "Tree";
			capsule.GetComponent<MeshRenderer> ().enabled = false;
		}
	}

	void OnApplicationQuit() {
		// restore original trees
		terrain.terrainData.treeInstances = _originalTrees;
	}
}