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
		tank.stats["fleeState"] = true;
		return null;
	}

	public override Type StateExit()
	{
		tank.stats["fleeState"] = false;
		return null;
	}

	public override Type StateUpdate()
	{
		/* 
		 * ------------
		 * Flee logic
		 * ------------
		*/
		// In order of importance
		// If tank is low on health
		if (tank.TankCurrentHealth <= 35.0f)
		{
			LowHealthFlee();
		}
		// If tank health is fine but low on fuel
		else if (tank.TankCurrentFuel <= 25.0f)
		{
			LowFuelFlee();
		}
		// If tank health and fuel is fine but ammo is low
		else if (tank.TankCurrentAmmo <= 2.0f)
		{
			LowAmmoFlee();
		}

		// If tank is okay
		if (fleeTarget == null)
		{
			return typeof(BJ_PatrolState);
		}

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

	void LowHealthFlee()
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

	void LowAmmoFlee()
	{
		// Find the ammo item in the dictionary
		var ammoItem = tank.VisibleConsumables.FirstOrDefault(c => c.Key.tag == "Ammo").Key;

		// Prevent crash if theres no ammo found
		if (ammoItem != null)
		{
			fleeTarget = ammoItem;
			tank.FollowPathToWorldPoint(ammoItem, 1.0f);

			if (LocationReached(ammoItem))
			{
				tank.VisibleConsumables.Remove(ammoItem);
				fleeTarget = null;
			}
		}
		// If there are no ammo, just run to a random point
		else
		{
			FleeToARandomPoint();
		}
	}

	void LowFuelFlee()
	{
		// Find the fuel item in the dictionary
		var fuelItem = tank.VisibleConsumables.FirstOrDefault(c => c.Key.tag == "Fuel").Key;

		// Prevent crash if theres no fuel found
		if (fuelItem != null)
		{
			fleeTarget = fuelItem;
			tank.FollowPathToWorldPoint(fuelItem, 1.0f);

			if (LocationReached(fuelItem))
			{
				tank.VisibleConsumables.Remove(fuelItem);
				fleeTarget = null;
			}
		}
		// If there are no fuel, just run to a random point
		else
		{
			FleeToARandomPoint();
		}
	}

	void FleeToARandomPoint()
	{
		// Make a flee point
		if (fleeTarget == null)
		{
			tank.GenerateNewRandomWorldPoint();
		}

		tank.FollowPathToWorldPoint(fleeTarget, 1.0f);

		// When the tank has reached the flee point, make a new one
		if (LocationReached(fleeTarget))
		{
			tank.GenerateNewRandomWorldPoint();
		}
	}

	bool LocationReached(GameObject item)
	{
		return Vector3.Distance(tank.transform.position, item.transform.position) < 1.0f;
	}
}