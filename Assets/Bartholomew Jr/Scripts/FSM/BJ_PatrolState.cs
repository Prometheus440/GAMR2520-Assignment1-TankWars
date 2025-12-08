using System;
using System.Collections;
using System.Collections.Generic;
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
		tank.FollowPathToRandomWorldPoint(1.0f);

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