using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BJ_PatrolState : BJ_BaseState
{
	private BJ_SmartTank tank;

	public BJ_PatrolState(BJ_SmartTank tank)
	{
		this.tank = tank;
	}

	public override Type StateEnter()
	{
		tank.stats["patrolState"] = true;
		return null;
	}

	public override Type StateExit()
	{
		tank.stats["patrolState"] = false;
		return null;
	}

	public override Type StateUpdate()
	{
		// Check for flee conditions
		if (tank.stats["lowHealth"] || tank.stats["lowFuel"] || tank.stats["lowAmmo"])
		{
			return typeof(BJ_FleeState);
		}

		// If enemy is spotted check
		if (tank.enemyTank != null)
		{
			float distanceToEnemy = Vector3.Distance(tank.transform.position, tank.enemyTank.transform.position);

			if (distanceToEnemy < tank.pursueRange)
			{
				return typeof(BJ_PursueState);
			}
		}

		/* 
		* ------------
		* Patrol logic
		* ------------
		*/
		tank.FollowPathToRandomWorldPoint(0.7f);

		// Collect a power up that is close while patrolling
		if (tank.VisibleConsumables.Count > 0)
		{
			// Dont waste fuel if health or ammo is full
			if (tank.TankCurrentHealth < 125f || tank.TankCurrentAmmo < 20f)
			{
				GameObject closestConsumable = null;
				float consumableDistance = float.MaxValue;
					
				// For each keyValuepair in the dictionary
				foreach (var kvp in tank.VisibleConsumables)
				{
					GameObject consumable = kvp.Key;

					float distance = Vector3.Distance(tank.transform.position, consumable.transform.position);

					if (distance < consumableDistance)
					{
						consumableDistance = distance;
						closestConsumable = consumable;

					}
				}

				if (closestConsumable != null)
				{
					tank.FollowPathToWorldPoint(closestConsumable, 0.7f);
				}
			}
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
}