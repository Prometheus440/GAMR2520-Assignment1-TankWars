using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BJ_FleeState : BJ_BaseState
{
	private BJ_SmartTank tank;
	private GameObject fleeTarget;

	public BJ_FleeState(BJ_SmartTank tank)
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
		/* 
		 * ------------
		 * Flee logic
		 * ------------
		*/
		// If tank is low on health
		if (tank.TankCurrentHealth <= 35.0f)
		{
			// Find the health item in the dictionary
			var healthItem = tank.VisibleConsumables.FirstOrDefault(c => c.Key.tag == "Health").Key;

			// Prevent crash if theres no heal found
			if (healthItem != null)
			{
				fleeTarget = healthItem;
				tank.FollowPathToWorldPoint(healthItem, 1.0f);

				if (LocationReached(healthItem))
				{
					tank.VisibleConsumables.Remove(healthItem);
					fleeTarget = null;
				}
			}
			// If there are no heals, just run to a random point
			else
			{
				FleeToARandomPoint();
			}

		}
		// If tank health is fine
		else
		{
			fleeTarget = null;
		}

		return null;
	}

	void FleeToARandomPoint()
	{
		// Make a flee point
		if (fleeTarget == null)
		{
			tank.GenerateNewRandomWorldPoint();
		}

		// Get to the flee point
		if (fleeTarget != null)
		{
			tank.FollowPathToWorldPoint(fleeTarget, 1.0f);

			// When the tank has reached the flee point, make a new one
			if (LocationReached(fleeTarget))
			{
				tank.GenerateNewRandomWorldPoint();
			}
		}
	}

	bool LocationReached(GameObject item)
	{
		return Vector3.Distance(tank.transform.position, item.transform.position) < 1.0f;
	}
}