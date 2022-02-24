using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Photon.Realtime;

public class PlayerManager : MonoBehaviour
{
	PhotonView PV;

	GameObject controller;

	Player[] allPlayers;
	int myNumberInRoom;


	void Start()
	{
		
		PV = GetComponent<PhotonView>();
		myNumberInRoom = PV.ViewID % SpawnManager.Instance.spawnpoints.Length;
		if (PV.IsMine)
		{
			CreateController();
		}
	}


	void CreateController()
	{
		//Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
		controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), SpawnManager.Instance.spawnpoints[myNumberInRoom].position, SpawnManager.Instance.spawnpoints[myNumberInRoom].rotation, 0, new object[] { PV.ViewID });
		
	}

	public void Die()
	{
		PhotonNetwork.Destroy(controller);
		CreateController();
	}
}