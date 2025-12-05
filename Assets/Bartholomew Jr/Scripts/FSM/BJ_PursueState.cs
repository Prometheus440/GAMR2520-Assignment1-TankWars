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
		return null;
	}

	public override Type StateExit()
	{
		return null;
	}

	public override Type StateUpdate()
	{
		// Check if tank is alive
		if (tank.enemyTank == null)
		{
			return typeof(PatrolState);
		}

		float distanceToEnemy = Vector3.Distance(tank.transform.position, tank.enemyTank.transform.position);

		// If enemy tank is in range, switch to AttackState
		if (distanceToEnemy < (tank.pursueRange / 2))
		{
			return typeof(AttackState);
		}
		// If enemy tank is outside of range, switch back to PatrolState
		else if (distanceToEnemy > tank.pursueRange)
		{
			return typeof(PatrolState);
		}
		/* 
		 * ------------
		 * Pursue logic
		 * ------------
		*/
		else
		{
			Vector3 targetPos = tank.enemyTank.transform.position;
			Vector3 direction = (targetPos - tank.transform.position).normalized;

			// Rotate in the direction of enemy tank
			Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
			tank.transform.rotation = Quaternion.Slerp(tank.transform.rotation, targetRotation, tank.bodyRotationSpeed * Time.deltaTime);

			// Move forward using pathfinding
			tank.FollowPathToWorldPoint(tank.enemyTank, 1.0f);

			// Aim turret at enemy
			tank.TurretFaceWorldPoint(tank.enemyTank);

			// Stay pursuing
			return null;
		}
	}
}