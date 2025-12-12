using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BJ_FleeState : BJ_BaseState
{
	private BJ_SmartTank tank;
	private string currentConsumableTag = "";

	// No consumables flee
	private float safeDistance = 20.0f;
	private GameObject fleeDestination;

	public BJ_FleeState(BJ_SmartTank tank)
	{
		this.tank = tank;
		fleeDestination = new GameObject("fleeDestination");
	}

	public override Type StateEnter()
	{
		tank.stats["fleeState"] = true;
		currentConsumableTag = "";
		return null;
	}

	public override Type StateExit()
	{
		tank.stats["fleeState"] = false;
		currentConsumableTag = "";
		return null;
	}

	public override Type StateUpdate()
	{

		string consumableTag = "";

		/* 
		 * ------------
		 * Flee logic
		 * ------------
		*/
		// In order of importance
		// If tank is low on health
		if (tank.TankCurrentHealth <= 35.0f)
		{
			consumableTag = "Health";
		}
		// If tank health is fine but low on fuel
		else if (tank.TankCurrentFuel <= 25.0f)
		{
			consumableTag = "Fuel";
		}
		// If tank health and fuel is fine but ammo is low
		else if (tank.TankCurrentAmmo <= 2.0f)
		{
			consumableTag = "Ammo";
		}
		// If tank is okay
		else
		{
			return typeof(BJ_PatrolState);
		}

		Flee(consumableTag);

		// Return to patrolling if no enemies are seen
		if (tank.VisibleEnemyTanks.Count == 0)
		{
			return typeof(BJ_PatrolState);
		}

		// Run tank rules
		foreach (var item in tank.rules.GetRules)
		{
			if (item.CheckRule(tank.stats) != null)
			{
				return item.CheckRule(tank.stats);
			}
		}

		return null;
	}

	void Flee(string tag)
	{
		// Find the needed item in the dictionary
		var neededItem = tank.VisibleConsumables.FirstOrDefault(c => c.Key != null && c.Key.tag == tag).Key;

		// Prevent crash if theres no needed item found
		if (neededItem != null)
		{
			tank.FollowPathToWorldPoint(neededItem, 1.0f, tank.heuristicMode);
		}
		// If there are no needed item, just run to the nearest consumable
		else
		{
			GameObject closestConsumable = GetClosestConsumable();

			if (closestConsumable != null)
			{
				tank.FollowPathToWorldPoint(closestConsumable, 1.0f, tank.heuristicMode);
			}
			else
			{
				if (tank.enemyTank != null)
				{
					// Calculate direction to enemy to move directly backwards
					Vector3 directionToEnemy = (tank.transform.position - tank.enemyTank.transform.position).normalized;

					// Add distance to move
					fleeDestination.transform.position = tank.transform.position + (directionToEnemy * safeDistance);
					tank.FollowPathToWorldPoint(fleeDestination, 1.0f);
				}
				else
				{
					tank.FollowPathToRandomWorldPoint(1.0f, tank.heuristicMode);
				}

			}
		}
	}

	GameObject GetClosestConsumable()
	{
		// Collect a power up that is close while patrolling
		if (tank.VisibleConsumables.Count == 0)
		{
			return null;
		}

		GameObject closestConsumable = null;
		float minDistance = float.MaxValue;

		// For each keyValuepair in the dictionary
		foreach (var kvp in tank.VisibleConsumables)
		{
			GameObject c = kvp.Key;

			if (c == null || !c.activeInHierarchy)
			{
				continue;
			}

			float distance = Vector3.Distance(tank.transform.position, c.transform.position);

			if (distance < minDistance)
			{
				minDistance = distance;
				closestConsumable = c;
			}
		}

		return closestConsumable;
	}
}