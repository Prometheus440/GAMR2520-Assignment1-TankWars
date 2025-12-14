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
	private BJ_BTBaseNode outrunBehaviourTree;

	public BJ_OutrunState(BJ_SmartTank tank, GameObject enemyTank)
	{
		this.tank = tank;
		this.enemyTank = enemyTank;
		outrunPoint = new GameObject("OutrunPoint");
		BuildOutrunBehaviourTree();
	}

	private void BuildOutrunBehaviourTree()
	{
		outrunBehaviourTree = new BJ_BTSelector(new List<BJ_BTBaseNode>
		{
			new BJ_BTSequence(new List<BJ_BTBaseNode>
			{
				// Collect consumables while outrunning if safe
				new BJ_BTAction(HasSafeConsumable),
				new BJ_BTAction(CollectWhileOutrunning)
			}),
			new BJ_BTSequence(new List<BJ_BTBaseNode>
			{
				// Just stay out of range
				new BJ_BTAction(HasEnemy),
				new BJ_BTAction(MaintainDistance)
			})
		});
	}

	public override Type StateEnter()
	{
		tank.stats["outrunState"] = true;
		return null;
	}

	public override Type StateExit()
	{
		tank.stats["outrunState"] = false;

		// Destroy object
		if (outrunPoint != null)
		{
			GameObject.Destroy(outrunPoint);
			outrunPoint = null;
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

		enemyTank = tank.enemyTank;

		// Return to patrol
		if (enemyTank == null)
		{
			return typeof(BJ_PatrolState);
		}

		// If tank is too far away to be followed
		float distanceToEnemy = Vector3.Distance(tank.transform.position, enemyTank.transform.position);

		if (distanceToEnemy > tank.pursueRange * 2.0f)
		{
			return typeof(BJ_PatrolState);
		}

		outrunBehaviourTree.Evaluate();

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

	private BJ_BTNodeState HasEnemy()
	{
		if (enemyTank != null)
		{
			return BJ_BTNodeState.SUCCESS;
		}
		else
		{
			return BJ_BTNodeState.FAILURE;
		}
	}

	private BJ_BTNodeState MaintainDistance()
	{
		if (enemyTank == null)
		{
			return BJ_BTNodeState.FAILURE;
		}

		// Calculate distances
		float currentDistance = Vector3.Distance(tank.transform.position, enemyTank.transform.position);
		float safeDistance = tank.attackRange * 1.5f;

		Vector3 directionAway = (tank.transform.position - enemyTank.transform.position).normalized;
		Vector3 targetPos;

		if (currentDistance < safeDistance)
		{
			// Too close so move away
			targetPos = tank.transform.position + (directionAway * 15.0f);
		}
		else
		{
			// Distance is safe so maintain parallel movement
			Vector3 perpendicular = new Vector3(-directionAway.z, directionAway.y, directionAway.x);
			targetPos = tank.transform.position + (perpendicular * 10.0f);
		}

		outrunPoint.transform.position = targetPos;
		tank.FollowPathToWorldPoint(outrunPoint, 1.0f, tank.heuristicMode);

		return BJ_BTNodeState.SUCCESS;
	}

	private BJ_BTNodeState HasSafeConsumable()
	{
		if (tank.VisibleConsumables.Count == 0)
		{
			return BJ_BTNodeState.FAILURE;
		}

		// For each keyValuepair in the dictionary
		foreach (var consumable in tank.VisibleConsumables)
		{
			Vector3 distanceToConsumable = consumable.Key.transform.position - tank.transform.position;
			Vector3 distanceToEnemy = enemyTank.transform.position - tank.transform.position;

			// If consumable is in opposite direction from enemy
			if (Vector3.Dot(distanceToConsumable.normalized, distanceToEnemy.normalized) < 0)
			{
				return BJ_BTNodeState.SUCCESS;	
			}
		}

		return BJ_BTNodeState.FAILURE;
	}

	private BJ_BTNodeState CollectWhileOutrunning()
	{
		// Find consumable object in opposite distance to enemy
		GameObject safeConsumable = null;
		float maxSafety = -1.0f;

		foreach(var consumable in tank.VisibleConsumables)
		{
			Vector3 distanceToConsumable = consumable.Key.transform.position - tank.transform.position;
			Vector3 distanceToEnemy = enemyTank.transform.position - tank.transform.position;

			// Negative dot product for opposite direction
			float safety = -Vector3.Dot(distanceToConsumable, distanceToEnemy);

			if (safety > maxSafety)
			{
				maxSafety = safety;
				safeConsumable = consumable.Key;
			}
		}

		if (safeConsumable != null)
		{
			tank.FollowPathToWorldPoint(safeConsumable, 1.0f, tank.heuristicMode);
			return BJ_BTNodeState.SUCCESS;
		}

		return BJ_BTNodeState.FAILURE;
	}
}