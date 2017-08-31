﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteLevelsManager : MonoBehaviour
{
	public static int count = 0;

	private static InfiniteLevelsManager instance;
	public static InfiniteLevelsManager Instance
	{
		get
		{
			return instance;
		}
	}

	public SpawnRatios levelsSpawnRatios;
	public Dictionary<string, int> levelsCurrentSpawnRatios;
	public List<GameObject> possibleLevels;
	public List<GameObject> spawnableLevels;
	public List<InfiniteLevel> levels;
	public List<GameObject> levelsObj;
	public List<GameObject> walls;
	public DictionaryIntGameobject specialLevels = new DictionaryIntGameobject();

	public Transform levelsRoot;

	public int depth;
	public int currentLevel;

	public float totalSpawnWeights = 0;

	public GameObject freeHeartLevelUI;
	public int nextFreeHeartLevel;

	void Awake()
	{
		levelsCurrentSpawnRatios = new Dictionary<string, int>();
		instance = this;
		GameObject newLevel = Instantiate(possibleLevels[0]);
		newLevel.transform.SetParent(levelsRoot);
		newLevel.transform.position = Vector3.zero;
		newLevel.GetComponent<InfiniteLevel>().CloseLevel();
		currentLevel = 0;
		nextFreeHeartLevel = InfiniteGameManager.Instance.GetFreeHeartDistance(GetFirstLevelNumber());
		SetNextHeartLevel();
		FillToDepth();
	}

	private void SetCurrentSpawnRatio(GameObject levelObj, int spawnChance)
	{
		if (levelsCurrentSpawnRatios.ContainsKey(levelObj.name))
		{
			levelsCurrentSpawnRatios[levelObj.name] = spawnChance;
		}
		else
		{
			levelsCurrentSpawnRatios.Add(levelObj.name, spawnChance);
		}
	}

	private int GetCurrentSpawnRatio(GameObject levelObj)
	{
		if (levelsCurrentSpawnRatios.ContainsKey(levelObj.name))
		{
			return levelsCurrentSpawnRatios[levelObj.name];
		}
		return 0;
	}
	
	public void UpdateSpawnChances()
	{
		for (int levelId = 0; levelId < possibleLevels.Count; ++levelId)
		{
			if (spawnableLevels.Contains(possibleLevels[levelId]))
			{
				continue;
			}
			if (possibleLevels[levelId].GetComponent<InfiniteLevel>().variationsData.layerLevels[0].value <= currentLevel)
			{
				spawnableLevels.Add(possibleLevels[levelId]);
			}
		}
		return;

		totalSpawnWeights = 0;
		foreach (var levelObj in possibleLevels)
		{
			InfiniteLevel level = levelObj.GetComponent<InfiniteLevel>();
			if (levelsSpawnRatios.ContainsKey(levelObj))
			{
				if (levelsSpawnRatios[levelObj].ContainsKey(currentLevel))
				{
					SetCurrentSpawnRatio(levelObj, levelsSpawnRatios[levelObj][currentLevel]);
				}
				else
				{
					SetCurrentSpawnRatio(levelObj, GetCurrentSpawnRatio(levelObj));
				}
			}
			else
			{
				SetCurrentSpawnRatio(levelObj, 0);
			}
			totalSpawnWeights += GetCurrentSpawnRatio(levelObj);
		}
	}

	public int PickLevelToSpawn()
	{
		float weightPicked = Random.Range(0.0f, totalSpawnWeights);
		for (int idx = 0; idx < possibleLevels.Count; ++idx)
		{
			weightPicked -= levelsCurrentSpawnRatios[possibleLevels[idx].name];
			if (weightPicked <= 0.0f)
			{
				return idx;
			}
		}
		return possibleLevels.Count - 1;
	}

	public void SpawnLevel()
	{
		currentLevel += 1;
		int scaleMod = 1;
		GameObject nextLevelModel;
		UpdateSpawnChances();
		if (specialLevels.ContainsKey(currentLevel) && specialLevels[currentLevel].GetComponent<InfiniteLevel>().CanSpawn())
		{
			nextLevelModel = specialLevels[currentLevel];
		}
		else
		{
			if (Random.Range(0, 2) == 0)
			{
				scaleMod = -1;
			}

			nextLevelModel = spawnableLevels[Random.Range(0, spawnableLevels.Count)];
		}

		InfiniteLevelGoal boundEnd = null;
		Vector3 endPrevious = Vector3.zero;
		int minDepth = int.MaxValue;
		for (int levelIdx = 0; levelIdx < levels.Count; ++levelIdx)
		{
			for (int endIdx = 0; endIdx < levels[levelIdx].ends.Count; ++endIdx)
			{
				int currentDepth = levels[levelIdx].GetDepth();
				if (currentDepth < minDepth && levels[levelIdx].ends[endIdx].boundStart == null)
				{
					minDepth = currentDepth;
					boundEnd = levels[levelIdx].ends[endIdx];
					endPrevious = levels[levelIdx].ends[endIdx].transform.position;
				}
			}
		}

		InfiniteLevel level = nextLevelModel.GetComponent<InfiniteLevel>();
		Vector3 newPos = endPrevious - level.start.transform.localPosition;

		GameObject newLevel = Instantiate(nextLevelModel, newPos, Quaternion.identity);
		newLevel.GetComponentInChildren<InfiniteLevel>().levelNumber = currentLevel;
		newLevel.transform.SetParent(levelsRoot);
		newLevel.name = nextLevelModel.name + currentLevel;
		newLevel.transform.position = newPos;
		boundEnd.boundStart = newLevel.GetComponentInChildren<InfiniteLevelStart>();
		boundEnd.boundStart.boundEnd = boundEnd;
		if (scaleMod != 1)
		{
			newLevel.transform.localScale = new Vector3(scaleMod * newLevel.transform.localScale.x, newLevel.transform.localScale.y, newLevel.transform.localScale.z);
		}
		if (nextFreeHeartLevel == currentLevel)
		{
			SetFreeHeartLevelPosition();
		}
	}

	public void RegisterLevel(InfiniteLevel level)
	{
		if (levels.Contains(level) == false)
		{
			foreach (Transform t in level.GetComponentsInChildren<Transform>())
			{
				if (t.gameObject.name == "Walls")
				{
					walls.Add(t.gameObject);
				}
			}
			levels.Add(level);
			levelsObj.Add(level.gameObject);
		}
	}

	public int DepthFilled()
	{
		InfiniteLevel topLevel = levels[0];
		while (topLevel.start.boundEnd != null)
		{
			topLevel = topLevel.start.boundEnd.level;
		}
		List<InfiniteLevel> currentLevels = new List<InfiniteLevel>();
        List<InfiniteLevel> nextLevels = new List<InfiniteLevel>();
		nextLevels.Add(topLevel);

		int depth = 0;
		while (nextLevels.Count != 0)
		{
			currentLevels.Clear();
			currentLevels.AddRange(nextLevels);
			nextLevels.Clear();
			foreach (InfiniteLevel level in currentLevels)
			{
				foreach (InfiniteLevelGoal end in level.ends)
				{
					if (end.boundStart == null)
					{
						return depth;
					}
					else
					{
						nextLevels.Add(end.boundStart.level);
					}
				}
			}
			++depth;
		}
		return depth;
	}

	public void FillToDepth()
	{
		while (DepthFilled() < Mathf.Max(Player.Instance.GetViewRange(), InfiniteGameManager.Instance.GetFreeHeartDistance(GetFirstLevelNumber())))
		{
			SpawnLevel();
		}
	}

	public void RemoveLevel(InfiniteLevel level, InfiniteLevel saveLevel = null)
	{
		levels.Remove(level);
		level.RemoveReferences();
        levelsObj.Remove(level.gameObject);
		Destroy(level.gameObject);
		levels[0].CloseLevel();

		FillToDepth();

		foreach (InfiniteLevelGoal currentLevelEnd in level.ends)
		{
			if (currentLevelEnd.boundStart != null && (saveLevel == null || currentLevelEnd.boundStart.level != saveLevel))
			{
				RemoveLevel(currentLevelEnd.boundStart.level);
            }
		}
	}

	public int GetFirstLevelNumber()
	{
		return currentLevel - levels.Count + 1;
	}

	public InfiniteLevel GetLevel(int levelNumber)
	{
		for (int lvlIdx = 0; lvlIdx < levels.Count; ++lvlIdx)
		{
			if (levels[lvlIdx].levelNumber == levelNumber)
			{
				return levels[lvlIdx];
            }
		}
		return null;
	}

	public void SetNextHeartLevel()
	{
		if (nextFreeHeartLevel < GetFirstLevelNumber() + 1)
		{
			nextFreeHeartLevel = GetFirstLevelNumber() + InfiniteGameManager.Instance.GetFreeHeartDistance(GetFirstLevelNumber());
			SetFreeHeartLevelPosition();
		}
	}

	public void SetFreeHeartLevelPosition()
	{
		InfiniteLevel levelCandidate = GetLevel(nextFreeHeartLevel);
		if (levelCandidate)
		{
			freeHeartLevelUI.transform.position = new Vector3(levelCandidate.start.transform.position.x, levelCandidate.start.transform.position.y, freeHeartLevelUI.transform.position.z);
			freeHeartLevelUI.SetActive(true);
		}
    }

	public Bounds GetGlobalBounds()
	{
		float xMin = float.MaxValue;
		float xMax = float.MinValue;
		float yMin = float.MaxValue;
		float yMax = float.MinValue;
		for (int levelIdx = 0; levelIdx < levels.Count; ++levelIdx)
		{
			Bounds currentBounds = levels[levelIdx].GetCurrentBounds();
			xMin = Mathf.Min(xMin, currentBounds.min.x);
			xMax = Mathf.Max(xMax, currentBounds.max.x);
            yMin = Mathf.Min(yMin, currentBounds.min.y);
			yMax = Mathf.Max(yMax, currentBounds.max.y);
		}

		return new Bounds(new Vector3((xMin + xMax) / 2.0f, (yMin + yMax) / 2.0f), new Vector3((xMax - xMin), (yMax - yMin)));
	}


	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Bounds globalBounds = GetGlobalBounds();
		Gizmos.DrawWireCube(globalBounds.center, globalBounds.size);
	}
}
