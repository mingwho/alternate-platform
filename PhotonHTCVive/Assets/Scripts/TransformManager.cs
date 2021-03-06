﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformManager : Photon.MonoBehaviour {

	public float speed = 10f;

	PhotonView photonView;

	void Start(){
		photonView = PhotonView.Get (this);
	}

	// Update is called once per frame
	void Update () {
		if (!photonView.isMine) {
			SyncedMovement ();
		}
	}

	private float lastSynchronizationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		Rigidbody rb = GetComponent<Rigidbody> ();
		if (stream.isWriting)
		{
			stream.SendNext(rb.position);
			stream.SendNext(rb.velocity);
		}
		else
		{
			Vector3 syncPosition = (Vector3)stream.ReceiveNext();
			Vector3 syncVelocity = (Vector3)stream.ReceiveNext();

			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;

			syncEndPosition = syncPosition + syncVelocity * syncDelay;
			syncStartPosition = rb.position;
		}
	}

	private void SyncedMovement()
	{
		Rigidbody rb = GetComponent<Rigidbody> ();
		syncTime += Time.deltaTime;
		rb.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
	}

	public void StartColorChange(Vector3 color){
		photonView.RPC("ChangeColorTo",PhotonTargets.All, color);
	}

	[PunRPC] void ChangeColorTo(Vector3 color)
	{
		GetComponent<Renderer>().material.color = new Color(color.x, color.y, color.z, 1f);

		if (photonView.isMine)
			photonView.RPC("ChangeColorTo", PhotonTargets.OthersBuffered, color);
	}
}
