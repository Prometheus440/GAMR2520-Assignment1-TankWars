using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BJ_AttackState : BJ_BaseState
{
	private BJ_SmartTank tank;
	private float attackClosest = 12.0f;
	private GameObject backupPoint;
	private GameObject currentTarget;

	private BJ_BTBaseNode attackBehaviourTree;

	public BJ_AttackState(BJ_SmartTank tank)
	{
		this.tank = tank;
		BuildAttackBehaviourTree();
	}

	private void BuildAttackBehaviourTree()
	{
		attackBehaviourTree = new BJ_BTSelector(new List<BJ_BTBaseNode>
		{
			new BJ_BTSequence(new List<BJ_BTBaseNode>
			{
				// Back up sequence
				new BJ_BTAction(TooClose),
				new BJ_BTAction(BackAway)
			}),
			new BJ_BTSequence(new List<BJ_BTBaseNode>
			{
				// Normal attack sequence
				new BJ_BTAction(HasValidTarget),
				new BJ_BTAction(IsInRange),
				new BJ_BTAction(StopAndFace),
				new BJ_BTAction(FireAtTarget)
			})
		});
	}

	public override Type StateEnter()
	{
		tank.stats["attackState"] = true;
		return null;
	}

	public override Type StateExit()
	{
		tank.stats["attackState"] = false;

		// Destroy back up object
		if (backupPoint != null)
		{
			GameObject.Destroy(backupPoint);
			backupPoint = null;
		}

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

		attackBehaviourTree.Evaluate();

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
	private BJ_BTNodeState HasValidTarget()
	{
		if (currentTarget != null)
		{
			return BJ_BTNodeState.SUCCESS;
		}
		else
		{
			return BJ_BTNodeState.FAILURE;
		}
	}

	private BJ_BTNodeState IsInRange()
	{
		if (currentTarget == null)
		{
			return BJ_BTNodeState.FAILURE;
		}

		float distance = Vector3.Distance(tank.transform.position, currentTarget.transform.position);

		if (distance <= tank.attackRange)
		{
			return BJ_BTNodeState.SUCCESS;
		}
		else
		{
			return BJ_BTNodeState.FAILURE;
		}
	}

	private BJ_BTNodeState TooClose()
	{
		if (currentTarget == null)
		{
			return BJ_BTNodeState.FAILURE;
		}

		float distance = Vector3.Distance(tank.transform.position, currentTarget.transform.position);

		if (distance <= attackClosest)
		{
			return BJ_BTNodeState.SUCCESS;
		}
		else
		{
			return BJ_BTNodeState.FAILURE;
		}
	}

	private BJ_BTNodeState BackAway()
	{
		if (backupPoint == null)
		{
			// Create target back up point
			backupPoint = new GameObject("BackupPoint");
		}

		float distanceToTarget = Vector3.Distance(tank.transform.position, currentTarget.transform.position);

		if (distanceToTarget < attackClosest)
		{
			float moveDistance = attackClosest - distanceToTarget + 2.0f;

			// Calculate direction to enemy to move directly backwards
			Vector3 directionToEnemy = (tank.transform.position - currentTarget.transform.position).normalized;
			// Add distance to move
			backupPoint.transform.position = tank.transform.position + (directionToEnemy * moveDistance);

			tank.FollowPathToWorldPoint(backupPoint, 0.8f);
			tank.TurretFaceWorldPoint(currentTarget);

			return BJ_BTNodeState.SUCCESS;
		}

		return BJ_BTNodeState.FAILURE;
	}

	private BJ_BTNodeState StopAndFace()
	{
		tank.TankStop();
		tank.TurretFaceWorldPoint(currentTarget);

		return BJ_BTNodeState.SUCCESS;
	}

	private BJ_BTNodeState FireAtTarget()
	{
		if (currentTarget == null) return BJ_BTNodeState.FAILURE;

		tank.TurretFireAtPoint(currentTarget);

		return BJ_BTNodeState.SUCCESS;
	}

	private GameObject GetTarget()
	{
		// Attack a tank first but then attack a base if theres no visible enemy
		if (tank.enemyTank != null) return tank.enemyTank;
		if (tank.enemyBase != null) return tank.enemyBase;

		return null;
	}
}