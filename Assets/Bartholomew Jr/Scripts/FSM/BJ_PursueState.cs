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

		if (tank.enemyTank == null && tank.enemyBase == null)
		{
			return typeof(BJ_PatrolState);
		}



		if (tank.enemyTank != null)
		{
			float distanceToEnemy = Vector3.Distance(tank.transform.position, tank.enemyTank.transform.position);

			if (distanceToEnemy < tank.attackRange)
			{
				return typeof(BJ_AttackState);

			}

			// If enemy tank is outside of range, switch back to PatrolState
			else if (distanceToEnemy > tank.pursueRange)
			{
				return typeof(BJ_PatrolState);
			}
		}
		if (tank.enemyBase != null)
		{
			float distanceToEnemyBase = Vector3.Distance(tank.transform.position, tank.enemyBase.transform.position);


			// If enemy tank is in range or base, switch to AttackState
			if (distanceToEnemyBase < tank.attackRange)
			{
				return typeof(BJ_AttackState);

			}

			// If enemy tank is outside of range, switch back to PatrolState
			else if (distanceToEnemyBase > tank.pursueRange)
			{
				return typeof(BJ_PatrolState);
			}
		}



		/* 
		* ------------
		* Pursue logic
		* ------------
		*/
		// Move forward using pathfinding
		if (tank.enemyTank != null)
		{
			tank.FollowPathToWorldPoint(tank.enemyTank, 1.0f, tank.heuristicMode);
			tank.TurretFaceWorldPoint(tank.enemyTank);
		}
		else if (tank.enemyBase != null)
		{
			tank.FollowPathToWorldPoint(tank.enemyBase, 1.0f, tank.heuristicMode);
			tank.TurretFaceWorldPoint(tank.enemyBase);
		}

		// Aim turret at enemy

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