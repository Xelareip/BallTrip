﻿using System.Collections.Generic;
using UnityEngine;
using System;

public class InfiniteLevelGoal : MonoBehaviour
{
	public List<CollisionTrigger> collisionTriggers;
	public Action<InfiniteLevelGoal> goalHit;

	public InfiniteLevel level;
	public InfiniteLevelStart boundStart;

	void Awake()
	{
		foreach (CollisionTrigger trigger in collisionTriggers)
		{
			trigger.collisionExit += GoalExited;
		}
	}

	public void GoalExited(Collider collider)
	{
		if (collider.gameObject == Ball.Instance.gameObject && collider.transform.position.y > transform.position.y)
		{
			if (goalHit != null)
			{
				goalHit(this);
			}
		}
	}
}
