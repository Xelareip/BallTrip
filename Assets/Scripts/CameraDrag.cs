﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDrag : MonoBehaviour
{
	public int touchId = -1;
	public Vector3 dragOrigin;
	public Vector3 currentDrag;

	public Vector3 nullVect = new Vector3(-10000, -10000);

	public Transform ballTransform;

	void Update()
	{
		if (ballTransform == null || TutoManager.Instance.GetCanDragCamera() == false || InfiniteGameManager.Instance.gameIsOver)
		{
			return;
		}

		if (Ball.Instance.ballRigidbody.velocity.magnitude != 0.0f || InfiniteGameManager.Instance.GetMode() != LAUNCH_MODE.LOOK)
		{
			touchId = -1;
            dragOrigin = nullVect;
            return;
		}

		Vector3 position = nullVect;
		if (touchId == -1)
		{
			if (Input.touchCount != 0)
			{
				Touch touch = Input.GetTouch(0);
				if (touch.phase == TouchPhase.Began)
				{
					touchId = touch.fingerId;
					position = touch.position;
				}
			}
			else if (Input.GetMouseButtonDown(0))
			{
				touchId = 0;
				position = Input.mousePosition;
			}

			dragOrigin = position;
		}
		if (dragOrigin == nullVect)
		{
			return;
		}


		if (Input.touchCount != 0)
		{
			Touch touch = Input.GetTouch(touchId);
			currentDrag = touch.position;
		}
		else if (Input.GetMouseButton(0))
		{
			currentDrag = Input.mousePosition;
		}
		else if (Input.GetMouseButtonUp(0))
		{
			currentDrag = Input.mousePosition;
		}
		// Touch cancelled, loss of focus, etc...
		else
		{
			Ball.Instance.launchDirection = Vector3.zero;
			touchId = -1;
			return;
		}

		Vector3 drag = (dragOrigin - currentDrag) * XUtils.ScreenCamRatio();
		Vector3 newPos = transform.position + drag;
		dragOrigin = currentDrag;

		Vector3[] possiblePos = new Vector3[] { newPos, new Vector3(newPos.x, transform.position.y, newPos.z), new Vector3(transform.position.x, newPos.y, newPos.z) };
		
        for (int idx = 0; idx < possiblePos.Length; ++idx)
		{
			for (int levelIdx = 0; levelIdx < InfiniteLevelsManager.Instance.levels.Count; ++levelIdx)
			{
				Vector3 targetPoint = new Vector3(possiblePos[idx].x, possiblePos[idx].y, InfiniteLevelsManager.Instance.levels[levelIdx].transform.position.z);
				Bounds currentBounds = InfiniteLevelsManager.Instance.levels[levelIdx].GetCurrentBounds();
				if (currentBounds.Contains(targetPoint))
				{
					transform.position = possiblePos[idx];
					return;
				}
				continue;
			}
		}
	}
}
