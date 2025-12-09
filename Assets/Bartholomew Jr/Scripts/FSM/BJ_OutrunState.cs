using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BJ_OutrunState : BJ_BaseState
{
	private BJ_SmartTank tank;
	private GameObject enemyTank;
	private GameObject outrunPoint;

	public BJ_OutrunState(BJ_SmartTank tank, GameObject enemyTank)
	{
		this.tank = tank;
		this.enemyTank = enemyTank;
	}

	public override Type StateEnter()
	{
		tank.stats["outrunState"] = true;
		return null;
	}

	public override Type StateExit()
	{
		tank.stats["outrunState"] = false;
		return null;
	}

	public override Type StateUpdate()
	{
		// Check for flee conditions
		if (tank.stats["lowHealth"] || tank.stats["lowFuel"] || tank.stats["lowAmmo"])
		{
			return typeof(BJ_FleeState);
		}

		/* 
		 * ------------
		 * Outrun logic
		 * ------------
		*/
		// If SmartTank fuel > DumbTank fuel and health and ammo are low
		// Make the DumbTank chase the SmartTank to reduce fuel

		// Run tank rules
		foreach (var item in tank.rules.GetRules)
		{
			if (item.CheckRule(tank.stats) != null)
			{
				return item.CheckRule(tank.stats);
			}
		}

		// Check if enemy tank is alive
		if (tank.enemyTank != null)
		{
			// Find direction
			Vector3 directionAway = (tank.transform.position - enemyTank.transform.position).normalized;

			// Find safe distance
			Vector3 outrunPos = tank.transform.position + directionAway * 15.0f;

			outrunPoint.transform.position = outrunPos;

			tank.FollowPathToWorldPoint(outrunPoint, 1.0f);

			return null;
		}
		else
		{
			// Patrol if no enemy tank
			return typeof(BJ_PatrolState);
		}


		return null;
	}
}
