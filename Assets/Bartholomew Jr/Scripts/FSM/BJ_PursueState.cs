using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BJ_PursueState : BJ_BaseState
{

	private BJ_SmartTank tank;
	private BJ_BTBaseNode pursueBehaviourTree;
	private GameObject currentTarget;

	public BJ_PursueState(BJ_SmartTank tank)
	{
		this.tank = tank;
		BuildPursueBehaviourTree();
	}

	private void BuildPursueBehaviourTree()
	{
		pursueBehaviourTree = new BJ_BTSelector(new List<BJ_BTBaseNode>
		{
			new BJ_BTSequence(new List<BJ_BTBaseNode>
			{
				// Chase enemy if its moving
				new BJ_BTAction(HasTarget),
				new BJ_BTAction(IsTargetMoving),
				new BJ_BTAction(CatchTarget)
			}),
			new BJ_BTSequence(new List<BJ_BTBaseNode>
			{
				// Chasing
				new BJ_BTAction(HasTarget),
				new BJ_BTAction(ChaseTarget)
			}),
			new BJ_BTSequence(new List<BJ_BTBaseNode>
			{
				// Search
				new BJ_BTAction(SearchForTarget)
			})
		});
	}

	public override Type StateEnter()
	{
		tank.stats["pursueState"] = true;
		currentTarget = GetTarget();
		return null;
	}

	public override Type StateExit()
	{
		tank.stats["pursueState"] = false;
		currentTarget = null;
		return null;
	}

	public override Type StateUpdate()
	{
		// Check for flee conditions
		if (tank.stats["lowHealth"] || tank.stats["lowFuel"] || tank.stats["lowAmmo"])
		{
			return typeof(BJ_FleeState);
		}

		// Get target
		if (currentTarget == null)
		{
			currentTarget = GetTarget();
		}

		// Work out next state from distance
		if (currentTarget != null)
		{
			float distanceToTarget = Vector3.Distance(tank.transform.position, currentTarget.transform.position);

			if (distanceToTarget < tank.attackRange)
			{
				return typeof(BJ_AttackState);
			}
			else if (distanceToTarget > tank.pursueRange)
			{
				return typeof(BJ_PatrolState);
			}
		}
		else
		{
			return typeof(BJ_PatrolState);
		}

		pursueBehaviourTree.Evaluate();

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

	private BJ_BTNodeState HasTarget()
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

	private BJ_BTNodeState IsTargetMoving()
	{
		if (currentTarget == null)
		{
			return BJ_BTNodeState.FAILURE;
		}

		Rigidbody targetRB = currentTarget.GetComponent<Rigidbody>();

		if (targetRB != null && targetRB.velocity.magnitude > 0.5f)
		{
			return BJ_BTNodeState.SUCCESS;
		}

		return BJ_BTNodeState.FAILURE;
	}

	private BJ_BTNodeState CatchTarget()
	{
		if (currentTarget == null)
		{
			return BJ_BTNodeState.FAILURE;
		}

		// Predict where enemy will be
		Rigidbody targetRB = currentTarget.GetComponent<Rigidbody>();

		if (targetRB != null)
		{
			// Get speed and direction of the target
			Vector3 targetVelocity = targetRB.velocity;

			// Get distance to target
			float distanceToTarget = Vector3.Distance(tank.transform.position, currentTarget.transform.position);

			// Create Game Object
			Vector3 catchPoint = currentTarget.transform.position + (targetVelocity * distanceToTarget);
			GameObject catchPointObject = new GameObject("CatchPoint");
			catchPointObject.transform.position = catchPoint;

			// Move
			tank.FollowPathToWorldPoint(catchPointObject, 1.0f, tank.heuristicMode);

			// Destroy object
			GameObject.Destroy(catchPointObject, 0.1f);

			return BJ_BTNodeState.SUCCESS;
		}

		return BJ_BTNodeState.FAILURE;
	}

	private BJ_BTNodeState ChaseTarget()
	{
		if (currentTarget != null)
		{
			// Move forward using pathfinding
			tank.FollowPathToWorldPoint(currentTarget, 1.0f, tank.heuristicMode);
			return BJ_BTNodeState.SUCCESS;
		}

		return BJ_BTNodeState.FAILURE;
	}

	private BJ_BTNodeState SearchForTarget()
	{
		// Look
		tank.FollowPathToRandomWorldPoint(0.7f, tank.heuristicMode);
		return BJ_BTNodeState.SUCCESS;
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