using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BJ_PatrolState : BJ_BaseState
{
	private BJ_SmartTank tank;
	private GameObject currentConsumable;
	private BJ_BTBaseNode patrolBehaviourTree;

	public BJ_PatrolState(BJ_SmartTank tank)
	{
		this.tank = tank;
		BuildPatrolBehaviourTree();
	}

	private void BuildPatrolBehaviourTree()
	{
		// Try to collect consumable first then random patrol
		patrolBehaviourTree = new BJ_BTSelector(new List<BJ_BTBaseNode>
		{
			new BJ_BTSequence(new List<BJ_BTBaseNode>
			{
				new BJ_BTAction(IsConsumableVisible),
				new BJ_BTAction(SelectNearestConsumable),
				new BJ_BTAction(MoveToConsumable)
			}),

			new BJ_BTAction(RandomPatrol)
		});
	}

	public override Type StateEnter()
	{
		tank.stats["patrolState"] = true;
		currentConsumable = null;
		return null;
	}

	public override Type StateExit()
	{
		tank.stats["patrolState"] = false;
		currentConsumable = null;
		return null;
	}

	public override Type StateUpdate()
	{
		// Check for flee conditions
		if (tank.stats["lowHealth"] || tank.stats["lowFuel"] || tank.stats["lowAmmo"])
		{
			return typeof(BJ_FleeState);
		}

		// If enemy is spotted check or base
		if (tank.enemyTank != null)
		{
			float distanceToEnemy = Vector3.Distance(tank.transform.position, tank.enemyTank.transform.position);

			if (distanceToEnemy < tank.pursueRange)
			{
				return typeof(BJ_PursueState);
			}
		}
		else if (tank.enemyBase != null)
		{
			float distanceToEnemyBase = Vector3.Distance(tank.transform.position, tank.enemyBase.transform.position);
			if (distanceToEnemyBase < tank.pursueRange)
			{
				return typeof(BJ_PursueState);
			}
		}

		patrolBehaviourTree.Evaluate();

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

	private BJ_BTNodeState IsConsumableVisible()
	{
		if (tank.VisibleConsumables.Count > 0)
		{
			return BJ_BTNodeState.SUCCESS;
		}
		else
		{
			return BJ_BTNodeState.FAILURE;
		}
	}

	private BJ_BTNodeState SelectNearestConsumable()
	{
		// Do not lave until that consumable is collected
		if (currentConsumable == null || !currentConsumable.activeInHierarchy)
		{
			float minDistance = float.MaxValue;

			// For each keyValuepair in the dictionary
			foreach (var kvp in tank.VisibleConsumables)
			{
				GameObject c = kvp.Key;

				float distance = Vector3.Distance(tank.transform.position, c.transform.position);

				if (distance < minDistance)
				{
					minDistance = distance;
					currentConsumable = c;
				}
			}

			if (currentConsumable != null)
			{
				return BJ_BTNodeState.SUCCESS;
			}
			else
			{
				return BJ_BTNodeState.FAILURE;
			}
		}

		return BJ_BTNodeState.SUCCESS;
	}

	private BJ_BTNodeState MoveToConsumable()
	{
		if (currentConsumable != null && currentConsumable.activeInHierarchy)
		{
			tank.FollowPathToWorldPoint(currentConsumable, 0.7f, tank.heuristicMode);

			return BJ_BTNodeState.SUCCESS;
		}

		return BJ_BTNodeState.FAILURE;
	}

	private BJ_BTNodeState RandomPatrol()
	{
		// Random patrol if no consumables or enemies
		tank.FollowPathToRandomWorldPoint(0.7f, tank.heuristicMode);

		return BJ_BTNodeState.SUCCESS;
	}
}