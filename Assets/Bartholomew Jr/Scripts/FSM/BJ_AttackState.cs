using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System;

public class BJ_AttackState : BJ_BaseState
{
	private BJ_SmartTank tank;

	private float time = 0f;

	public BJ_AttackState(BJ_SmartTank tank)
	{
		this.tank = tank;
	}

	public override Type StateEnter()
	{
		tank.stats["attackState"] = true;
		return null;
	}

	public override Type StateExit()
	{
		tank.stats["attackState"] = false;
		return null;
	}

	public override Type StateUpdate()
	{
		// Return to patrol if no enemy spotted
		if (tank.enemyTank == null)
		{
			return typeof(BJ_PatrolState);
		}

		float distanceBetweenTanks = Vector3.Distance(tank.transform.position, tank.enemyTank.transform.position);

		// If enemy has moved away, pursue
		if (distanceBetweenTanks > tank.pursueRange)
		{
			return typeof(BJ_PursueState);
		}
		
		// Run tank rules
		foreach (var item in tank.rules.GetRules)
		{
			if (item.CheckRule(tank.stats) != null)
			{
				return item.CheckRule(tank.stats);
			}
		}

		/* 
		 * ------------
		 * Attack logic
		 * ------------
		*/

		// Turn turret at enemy and fire
		tank.TurretFaceWorldPoint(tank.enemyTank);
		tank.TurretFireAtPoint(tank.enemyTank);

		return null;
	}
}