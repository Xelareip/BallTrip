﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InfiniteLevel : MonoBehaviour
{
	public static int count = 0;

#if UNITY_EDITOR
	public bool baseLevel;
#endif

	public Bounds levelBounds;

	public float closeDelay;
	
	public InfiniteLevelStart start;
	public List<InfiniteLevelGoal> ends;
	public InfiniteLevelDoor door;
	public int reward;

	public List<Vector3> pickupSpots;

	public int levelNumber;
	public float spawnChances = 0;
	

	void Awake()
	{
		foreach (InfiniteLevelGoal endLevel in ends)
		{
			endLevel.level = this;
			endLevel.goalHit += GoalPassed;
		}
		start.level = this;
		InfiniteLevelsManager.Instance.RegisterLevel(this);
		foreach (Vector3 pickupSpot in pickupSpots)
		{
			GameObject newPickup = PickupManager.Instance.GetRandomPickup();
			if (newPickup != null)
			{
				GameObject newInstance = Instantiate<GameObject>(newPickup);
				newInstance.transform.SetParent(transform);
				newInstance.transform.localPosition = pickupSpot;
			}
        }
	}

	public bool CanSpawn()
	{
		foreach (ICanSpawn canSpawner in GetComponentsInChildren<ICanSpawn>())
		{
			if (canSpawner.CanSpawn() == false)
			{
				return false;
			}
		}
		return true;
	}

	public void CalculateBounds()
	{
		List<Transform> children = new List<Transform>();
		List<Transform> newChildren = new List<Transform>();

		children.Add(transform);
		newChildren.Add(transform);

		while (newChildren.Count != 0)
		{
			List<Transform> nextChildren = new List<Transform>();
			foreach (Transform currentParent in newChildren)
			{
				for (int childIdx = 0; childIdx < currentParent.childCount; ++childIdx)
				{
					Transform child = currentParent.GetChild(childIdx);
					nextChildren.Add(child);
				}
			}
			children.AddRange(nextChildren);
			newChildren.Clear();
			newChildren.AddRange(nextChildren);
		}

		levelBounds = Utils.GetBounds(children);
	}

	public void GoalPassed(InfiniteLevelGoal goal)
	{
		if (Ball.Instance.freeBounce)
        {

			InfiniteGameManager.Instance.FreeLaunchIncrease();
		}
		Ball.Instance.freeBounce = true;
		InfiniteGameManager.Instance.AddScore(reward);
		InfiniteGameManager.Instance.currentColorCursor += 0.1f;
		InfiniteLevelsManager.Instance.RemoveLevel(this, goal.boundStart.level);
	}

	public void CloseLevel()
	{
		StartCoroutine(CloseInDelay());
	}

	public IEnumerator CloseInDelay()
	{
		//yield return new WaitForSeconds(closeDelay);
		door.gameObject.SetActive(true);
		yield return null;
	}

	public int GetDepth()
	{
		if (start.boundEnd == null)
		{
			return 0;
		}

		return start.boundEnd.level.GetDepth() + 1;
	}

	public void RemoveReferences()
	{
		if (start.boundEnd != null)
		{
			start.boundEnd.boundStart = null;
		}
		foreach (InfiniteLevelGoal end in ends)
		{
			if (end.boundStart != null)
			{
				end.boundStart.boundEnd = null;
            }
		}
	}

	public Bounds GetCurrentBounds()
	{
		return new Bounds(levelBounds.center + transform.position, levelBounds.extents);
	}

	void OnDrawGizmos()
	{
		//Gizmos.color = Color.yellow;
		//Gizmos.DrawWireCube(transform.position + levelBounds.center, levelBounds.extents * 2.0f);

		Gizmos.color = Color.blue;
		foreach (Vector3 pickupSpot in pickupSpots)
		{
			Gizmos.DrawSphere(transform.position + pickupSpot, 0.3f);
		}
	}
}