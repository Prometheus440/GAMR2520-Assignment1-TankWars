using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BJ_PursueState : BJ_BaseState
{
	private BJ_SmartTank tank;

	public BJ_PursueState(BJ_SmartTank tank)
	{
		this.tank = tank;
	}

	public override Type StateEnter()
	{
		tank.stats["pursueState"] = true;
		return null;
	}

	public override Type StateExit()
	{
		tank.stats["pursueState"] = false;
		return null;
	}

	public override Type StateUpdate()
	{
		// Check for flee conditions
		if (tank.stats["lowHealth"] || tank.stats["lowFuel"] || tank.stats["lowAmmo"])
		{
			return typeof(BJ_FleeState);
		}

		// Check if enemy tank is alive or bases
		if (tank.enemyTank == null && tank.EnemyBases == null)
		{
			return typeof(BJ_PatrolState);
		}
		

		float distanceToEnemy = Vector3.Distance(tank.transform.position, tank.enemyTank.transform.position);
		float distanceToTarget = Vector3.Distance(tank.transform.position, tank.EnemyBases.transform.position);

		// If enemy tank is in range or base, switch to AttackState
		if (distanceToEnemy < tank.attackRange || distanceToTarget < tank.attackRange) 
		{
			return typeof(BJ_AttackState);
		}
		// If enemy tank is outside of range, switch back to PatrolState
		else if (distanceToEnemy > tank.pursueRange )
		{
			return typeof(BJ_PatrolState);
		}

		/* 
		* ------------
		* Pursue logic
		* ------------
		*/
		// Move forward using pathfinding
		tank.FollowPathToWorldPoint(tank.enemyTank, 1.0f);

		// Aim turret at enemy
		tank.TurretFaceWorldPoint(tank.enemyTank);

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