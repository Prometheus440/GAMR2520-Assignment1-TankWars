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
			Debug.Log("AttackState: Resources low, switching to Flee");
			return typeof(BJ_FleeState);
		}

		// Select target
		GameObject target = GetTarget();

		// Return to patrol if no enemy spotted
		if (target == null)
		{
			return typeof(BJ_PatrolState);
		}

		float distanceBetweenTanks = Vector3.Distance(tank.transform.position, target.transform.position);

		// If enemy has moved away, pursue
		if (distanceBetweenTanks > tank.pursueRange)
		{
			return typeof(BJ_PursueState);
		}

		// Tanks can get too close and projectiles can clip through tanks and cause endless loops
		if (distanceBetweenTanks < attackClosest)
		{
			CreateDistance(target);
		}
		// Normal attack
		else
		{
			tank.TankStop();
			tank.TurretFaceWorldPoint(target);
			tank.TurretFireAtPoint(target);
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
		// Calculate direction to enemy to move directly backwards
		Vector3 directionToEnemy = (tank.transform.position - target.transform.position).normalized;
		// Add distance to move
		Vector3 backupPosition = tank.transform.position + (directionToEnemy * 15);

		// Create target back up point
		GameObject backupPoint = new GameObject("BackupPoint");
		// Set the gameobject to the vector
		backupPoint.transform.position = backupPosition;

		tank.FollowPathToWorldPoint(backupPoint, 0.8f);

		// Destroy game object once reached
		GameObject.Destroy(backupPoint, 5f);
	}

	private GameObject GetTarget()
	{
		// Attack a tank first but then attack a base if theres no visible enemy
		if (tank.VisibleEnemyTanks.Count > 0)
		{
			return tank.VisibleEnemyTanks.First().Key;
		}
		else if (tank.VisibleEnemyBases.Count > 0)
		{
			return tank.VisibleEnemyBases.First().Key;
		}

		return null;
	}
}