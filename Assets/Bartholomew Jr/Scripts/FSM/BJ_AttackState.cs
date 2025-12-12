using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System;
using System.Linq;

public class BJ_AttackState : BJ_BaseState
{
	private BJ_SmartTank tank;
	private float attackClosest = 12.0f;
	private GameObject backupPoint;
	private GameObject currentTarget;

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
		// Check for flee conditions
		if (tank.stats["lowHealth"] || tank.stats["lowFuel"] || tank.stats["lowAmmo"])
		{
			return typeof(BJ_FleeState);
		}

		// If no target, make one
		if (currentTarget == null)
		{
			currentTarget = GetTarget();
		}

		// Return to patrol if no enemy spotted
		if (currentTarget == null)
		{
			return typeof(BJ_PatrolState);
		}

		float distanceBetweenTanks = Vector3.Distance(tank.transform.position, currentTarget.transform.position);


		// If enemy has moved away, pursue
		if (distanceBetweenTanks > tank.pursueRange)
		{
			return typeof(BJ_PursueState);
		}

		// Tanks can get too close and projectiles can clip through tanks and cause endless loops
		if (distanceBetweenTanks < attackClosest)
		{
			CreateDistance(currentTarget);

			// Stay facing enemy
			tank.TurretFaceWorldPoint(currentTarget);
		}
		// Normal attack
		else
		{
			tank.TankStop();
			tank.TurretFaceWorldPoint(currentTarget);
			tank.TurretFireAtPoint(currentTarget);
		}

		/* 
			* ------------
			* Attack logic
			* ------------
		*/

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

	void CreateDistance(GameObject target)
	{
		if (backupPoint == null)
		{
			// Create target back up point
			backupPoint = new GameObject("BackupPoint");
		}

		float distanceToTarget = Vector3.Distance(tank.transform.position, target.transform.position);

		if (distanceToTarget < attackClosest)
		{
			float moveDistance = attackClosest - distanceToTarget;

			// Calculate direction to enemy to move directly backwards
			Vector3 directionToEnemy = (tank.transform.position - target.transform.position).normalized;
			// Add distance to move
			backupPoint.transform.position = tank.transform.position + (directionToEnemy * moveDistance);

			tank.FollowPathToWorldPoint(backupPoint, 0.8f);
		}
		else
		{
			tank.TankStop();
		}
	}

	private GameObject GetTarget()
	{
		// Attack a tank first but then attack a base if theres no visible enemy
		if (tank.VisibleEnemyTanks.Count > 0)
		{
			return tank.VisibleEnemyTanks.First().Key;
		}
		if (tank.VisibleEnemyBases.Count > 0)
		{
			return tank.VisibleEnemyBases.First().Key;
		}

		return null;
	}
}