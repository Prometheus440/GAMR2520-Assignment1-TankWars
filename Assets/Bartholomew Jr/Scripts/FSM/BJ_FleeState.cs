using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BJ_FleeState : BJ_BaseState
{
	private BJ_SmartTank tank;
	private BJ_BTBaseNode fleeBehaviourTree;
	private GameObject fleePoint;

	public BJ_FleeState(BJ_SmartTank tank)
	{
		this.tank = tank;
		BuildFleeBehaviourTree();
	}

	private void BuildFleeBehaviourTree()
	{
		fleeBehaviourTree = new BJ_BTSelector(new List<BJ_BTBaseNode>
		{
			new BJ_BTSequence(new List<BJ_BTBaseNode>
			{
				// Needs a resource
				new BJ_BTAction(NeedsResource),
				new BJ_BTAction(MoveToNeededResource)
			}),
			new BJ_BTAction(MoveToSafeLocation)
		});
	}

	public override Type StateEnter()
	{
		tank.stats["fleeState"] = true;
		fleePoint = new GameObject("FleePoint");
		return null;
	}

	public override Type StateExit()
	{
		tank.stats["fleeState"] = false;

		// Destroy fleePoint object
		if (fleePoint != null)
		{
			GameObject.Destroy(fleePoint);
			fleePoint = null;
		}

		return null;
	}

	public override Type StateUpdate()
	{
		fleeBehaviourTree.Evaluate();

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

	private BJ_BTNodeState NeedsResource()
	{
		if (tank.stats["lowHealth"] || tank.stats["lowFuel"] || tank.stats["lowAmmo"])
		{
			return BJ_BTNodeState.SUCCESS;
		}
		else
		{
			return BJ_BTNodeState.FAILURE;
		}
	}

	private BJ_BTNodeState MoveToNeededResource()
	{
		GameObject consumable = GetNeededResource();

		if (consumable == null)
		{
			return BJ_BTNodeState.FAILURE;
		}

		tank.FollowPathToWorldPoint(consumable, 1.0f, tank.heuristicMode);

		return BJ_BTNodeState.SUCCESS;
	}

	private GameObject GetNeededResource()
	{
		if (tank.stats["lowHealth"]) return FindNearest("Health");
		if (tank.stats["lowFuel"]) return FindNearest("Fuel");
		if (tank.stats["lowAmmo"]) return FindNearest("Ammo");

		return null;
	}

	private GameObject FindNearest(string tag)
	{
		GameObject nearest = null;
		float minDistance = float.MaxValue;

		// For each keyValuepair in the dictionary
		foreach (var kvp in tank.VisibleConsumables)
		{
			if (!kvp.Key.name.Contains(tag))
			{
				continue;
			}

			float distance = Vector3.Distance(tank.transform.position, kvp.Key.transform.position);

			if (distance < minDistance)
			{
				minDistance = distance;
				nearest = kvp.Key;
			}
		}

		return nearest;
	}

	private BJ_BTNodeState MoveToSafeLocation()
	{
		tank.FollowPathToRandomWorldPoint(0.8f, tank.heuristicMode);
		return BJ_BTNodeState.SUCCESS;
	}
}