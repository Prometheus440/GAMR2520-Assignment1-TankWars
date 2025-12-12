using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BJ_FleeState : BJ_BaseState
{
	private BJ_SmartTank tank;

	// Timer
	private float fleeTimer = 0f;
	private float fleeDuration = 10.0f;
	private bool timerActive = false;

	public BJ_FleeState(BJ_SmartTank tank)
	{
		this.tank = tank;
	}

	public override Type StateEnter()
	{
		tank.stats["fleeState"] = true;
		RestartTimer();
		return null;
	}

	public override Type StateExit()
	{
		tank.stats["fleeState"] = false;
		RestartTimer();
		return null;
	}

	public override Type StateUpdate()
	{
		// End flee after 10 seconds
		if (timerActive)
		{
			// Count seconds
			fleeTimer += Time.deltaTime;

			if (fleeTimer >= fleeDuration)
			{
				RestartTimer();
				return typeof(BJ_PatrolState);
			}
		}

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
			StartTimer();
		}
		// If tank health is fine but low on fuel
		else if (tank.TankCurrentFuel <= 25.0f)
		{
			LowFuelFlee();
			StartTimer();
		}
		// If tank health and fuel is fine but ammo is low
		else if (tank.TankCurrentAmmo <= 2.0f)
		{
			LowAmmoFlee();
			StartTimer();
		}
		// If tank is okay
		else
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
		var healthItem = tank.VisibleConsumables.FirstOrDefault(c => c.Key != null && c.Key.tag == "Health").Key;

		// Prevent crash if theres no health found
		if (healthItem != null)
		{
			tank.FollowPathToWorldPoint(healthItem, 1.0f);
		}
		// If there are no health, just run to a random point
		else
		{
			tank.FollowPathToRandomWorldPoint(1.0f);
		}
	}

	void LowFuelFlee()
	{
		// Find the fuel item in the dictionary
		var fuelItem = tank.VisibleConsumables.FirstOrDefault(c => c.Key != null && c.Key.tag == "Fuel").Key;

		// Prevent crash if theres no fuel found
		// Move slower to reduce fuel consumption
		if (fuelItem != null)
		{
			tank.FollowPathToWorldPoint(fuelItem, 0.5f);
		}
		// If there are no fuel, just run to a random point
		else
		{
			tank.FollowPathToRandomWorldPoint(0.5f);
		}
	}

	void LowAmmoFlee()
	{
		// Find the ammo item in the dictionary
		var ammoItem = tank.VisibleConsumables.FirstOrDefault(c => c.Key != null && c.Key.tag == "Ammo").Key;

		// Prevent crash if theres no ammo found
		if (ammoItem != null)
		{
			tank.FollowPathToWorldPoint(ammoItem, 1.0f);
		}
		// If there are no ammo, just run to a random point
		else
		{
			tank.FollowPathToRandomWorldPoint(1.0f);
		}
	}
	void StartTimer()
	{
		if (!timerActive)
		{
			// Start timer
			timerActive = true;
			fleeTimer = 0.0f;
		}
	}

	void RestartTimer()
	{
		timerActive = false;
		fleeTimer = 0.0f;
	}
	
}