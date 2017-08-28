﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclePoison : MonoBehaviour, IObstacle
{
	#region GUID_IDENTIFIED
	[SerializeField]
	private string GUID = null;

	public void SetGUID()
	{
		if (GetGUID() == null)
		{
			GUID = Guid.NewGuid().ToString();
		}
	}

	public string GetGUID()
	{
		return GUID;
	}
	#endregion

	public float hpPerSecond;
	public Vector4 offset;
	public MeshRenderer meshRenderer;

	void Awake()
	{
		foreach (CollisionTrigger trigger in GetComponentsInChildren<CollisionTrigger>())
		{
			trigger.collisionStay += Signal_collisionStay;
		}
		gameObject.SetActive(false);
	}

	void Start()
	{
		meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.material = Resources.Load<Material>("ObstaclePoisonMaterial");
	}

	private void Signal_collisionStay(CollisionTrigger trigger, Collider coll)
	{
		OnTriggerStay(coll);
	}

	void OnTriggerStay(Collider coll)
	{
		if (coll.gameObject == Ball.Instance.gameObject)
		{
			Ball.Instance.Hit(this);
		}
	}

	public float HpLossOnTick()
	{
		return hpPerSecond * Time.fixedDeltaTime;
	}
	void Update()
	{
		offset = new Vector4(0.5f - (transform.position.x / transform.localScale.x) % 1, 0.5f - (transform.position.y / transform.localScale.y) % 1, 0.0f, 0.0f);
		meshRenderer.material.SetFloat("repeatsX", Mathf.RoundToInt(transform.localScale.x));
		meshRenderer.material.SetFloat("repeatsY", Mathf.RoundToInt(transform.localScale.y));
	}
}
