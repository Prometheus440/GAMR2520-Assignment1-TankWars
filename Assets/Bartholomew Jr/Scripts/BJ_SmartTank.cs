using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class BJ_SmartTank : DumbTank
{
	private BJ_BaseState currentState;

	public float pursueRange = 37.0f;
	public float attackRange = 25.0f;
	public float smartTankBodyRotationSpeed = 7.0f;

	public List<GameObject> EnemyBases;
	public GameObject targetBase;

	public Dictionary<string, bool> stats = new Dictionary<string, bool>();
	public BJ_Rules rules = new BJ_Rules();

	// Behaviour tree
	public BJ_BTAction healthCheck;
	public BJ_BTAction fuelCheck;
	public BJ_BTAction ammoCheck;
	public BJ_BTAction targetSpottedCheck;
	public BJ_BTAction targetReachedCheck;
	public BJ_BTSequence regenSequence;

	private bool isInitialised = false;

	void InitialiseStats()
	{
		// Only initialise stats if empty
		if (stats.Count == 0)
		{
			stats.Add("lowHealth", false);
			stats.Add("lowFuel", false);
			stats.Add("lowAmmo", false);
			stats.Add("targetSpotted", false);
			stats.Add("targetReached", false);
			stats.Add("fleeState", false);
			stats.Add("pursueState", false);
			stats.Add("patrolState", false);
			stats.Add("attackState", false);
			stats.Add("outrunState", false);
			stats.Add("enemyLowFuel", false);
			stats.Add("enemyFleeing", false);
		}
	}

	void InitialiseRules()
	{
		if (stats.Count == 0)
		{
			// Rules

			// Low health so flee
			rules.AddRule(new BJ_Rule("lowHealth", "attackState", "", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
			rules.AddRule(new BJ_Rule("lowHealth", "pursueState", "", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
			rules.AddRule(new BJ_Rule("lowHealth", "patrolState", "", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
			// Low fuel so flee
			rules.AddRule(new BJ_Rule("lowFuel", "attackState", "", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
			rules.AddRule(new BJ_Rule("lowFuel", "pursueState", "", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
			rules.AddRule(new BJ_Rule("lowFuel", "patrolState", "", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
			// Low Ammo so flee
			rules.AddRule(new BJ_Rule("lowAmmo", "attackState", "", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
			rules.AddRule(new BJ_Rule("lowAmmo", "pursueState", "", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
			rules.AddRule(new BJ_Rule("lowAmmo", "patrolState", "", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
			// Found target so pursue
			rules.AddRule(new BJ_Rule("patrolState", "targetSpotted", "", typeof(BJ_PursueState), BJ_Rule.Predicate.And));
			// Lost target so patrol
			rules.AddRule(new BJ_Rule("targetSpotted", "patrolState", "", typeof(BJ_PatrolState), BJ_Rule.Predicate.notAAndB));
			rules.AddRule(new BJ_Rule("targetSpotted", "pursueState", "", typeof(BJ_PatrolState), BJ_Rule.Predicate.notAAndB));
			rules.AddRule(new BJ_Rule("targetSpotted", "attackState", "", typeof(BJ_PatrolState), BJ_Rule.Predicate.notAAndB));
			// Reached target so attack
			rules.AddRule(new BJ_Rule("pursueState", "targetReached", "", typeof(BJ_AttackState), BJ_Rule.Predicate.And));
			// If fighting is risky outrun tank
			rules.AddRule(new BJ_Rule("lowAmmo", "enemyLowFuel", "enemyFleeing", typeof(BJ_OutrunState), BJ_Rule.Predicate.AAndBNotC));
			rules.AddRule(new BJ_Rule("lowHealth", "enemyLowFuel", "enemyFleeing", typeof(BJ_OutrunState), BJ_Rule.Predicate.AAndBNotC));
		}
	}

	//Checks enemy health, used for the outrun state
	public void CheckEnemyState()
	{
		if (enemyTank != null)
		{
			DumbTank enemy = enemyTank.GetComponent<DumbTank>();

			if (enemyTank != null)
			{
				stats["enemyFleeing"] = enemy.TankCurrentHealth < 30;
			}
			else
			{
				stats["enemyFleeing"] = false;
			}
		}
		else
		{
			stats["enemyFleeing"] = false;
		}
	}

	public void InitialiseBT()
	{
		healthCheck = new BJ_BTAction(HealthCheck);
		fuelCheck = new BJ_BTAction(FuelCheck);
		ammoCheck = new BJ_BTAction(AmmoCheck);
		targetSpottedCheck = new BJ_BTAction(TargetSpottedCheck);
		targetReachedCheck = new BJ_BTAction(TargetReachedCheck);

		// Sequence - regenerate health then fuel then ammo
		regenSequence = new BJ_BTSequence(new List<BJ_BTBaseNode> {
			healthCheck,
			fuelCheck,
			ammoCheck
		});
	}

	//Checks if low on any consumable 
	public BJ_BTNodeState HealthCheck()
	{
		if (stats["lowHealth"])
		{
			return BJ_BTNodeState.SUCCESS;
		}
		else
		{
			return BJ_BTNodeState.FAILURE;
		}
	}

	public BJ_BTNodeState FuelCheck()
	{
		if (stats["lowFuel"])
		{
			return BJ_BTNodeState.SUCCESS;
		}
		else
		{
			return BJ_BTNodeState.FAILURE;
		}
	}

	public BJ_BTNodeState AmmoCheck()
	{
		if (stats["lowAmmo"])
		{
			return BJ_BTNodeState.SUCCESS;
		}
		else
		{
			return BJ_BTNodeState.FAILURE;
		}
	}

	//Checks if a base or enemy is spotted
	public BJ_BTNodeState TargetSpottedCheck()
	{
		if (stats["targetSpotted"])
		{
			return BJ_BTNodeState.SUCCESS;
		}
		else
		{
			return BJ_BTNodeState.FAILURE;
		}
	}

	//Checks if a base or enemy is reached
	public BJ_BTNodeState TargetReachedCheck()
	{
		if (stats["targetReached"])
		{
			return BJ_BTNodeState.SUCCESS;
		}
		else
		{
			return BJ_BTNodeState.FAILURE;
		}
	}


	//Finds the nearest needed consumable
	void FindhealthConsumable()
	{
		GameObject nearestHealth = FindNearestConsumableOfType("Health");

		if (nearestHealth != null)
		{
			FollowPathToWorldPoint(nearestHealth, 0.8f, heuristicMode);
		}
	}

	void FindFuelConsumable()
	{
		GameObject nearestFuel = FindNearestConsumableOfType("Fuel");

		if (nearestFuel != null)
		{
			FollowPathToWorldPoint(nearestFuel, 0.8f, heuristicMode);
		}
	}

	void FindAmmoConsumable()
	{
		GameObject nearestAmmo = FindNearestConsumableOfType("Ammo");

		if (nearestAmmo != null)
		{
			FollowPathToWorldPoint(nearestAmmo, 0.8f, heuristicMode);
		}
	}

	GameObject FindNearestConsumableOfType(string type)
	{
		GameObject nearest = null;
		float minDistance = float.MaxValue;

		// For each keyValuepair in the dictionary
		foreach (var consumable in VisibleConsumables)
		{
			if (consumable.Key.name.Contains(type))
			{
				float distance = Vector3.Distance(transform.position, consumable.Key.transform.position);

				if (distance < minDistance)
				{
					minDistance = distance;
					nearest = consumable.Key;
				}
			}
		}

		return nearest;
	}

	public override void AIOnCollisionEnter(Collision collision)
	{
		base.AIOnCollisionEnter(collision);
	}

	//Initialises tank
	public override void AITankStart()
	{
		if (!isInitialised)
		{
			InitialiseStats();
			InitialiseRules();
			// BT
			InitialiseBT();

			// Start with patrolling
			currentState = new BJ_PatrolState(this);
			currentState.StateEnter();

			isInitialised = true;
		}
	}

	public override void AITankUpdate()
	{
		// Debug.Log(currentState);

		if (!isInitialised)
		{
			AITankStart();
			return;
		}

		// Detect enemies and bases
		if (VisibleEnemyTanks.Count > 0)
		{
			enemyTank = VisibleEnemyTanks.First().Key;
		}
		else if (VisibleEnemyBases.Count > 0)
		{
			EnemyBases = VisibleEnemyBases.Keys.ToList();
			enemyBase = EnemyBases.First();
		}
		else
		{
			enemyTank = null;
			EnemyBases = null;
			enemyBase = null;
		}

		stats["lowHealth"] = TankCurrentHealth <= 35.0f;
		stats["lowFuel"] = TankCurrentFuel <= 25.0f;
		stats["lowAmmo"] = TankCurrentAmmo <= 2.0f;

		CheckTargetSpotted();
		CheckTargetReached();
		CheckFuel();
		CheckEnemyState();


		// Update FSM
		if (currentState != null)
		{
			Type nextStateType = currentState.StateUpdate();

			if (nextStateType != null)
			{
				currentState.StateExit();
				currentState = (BJ_BaseState)Activator.CreateInstance(nextStateType, new object[] { this });
				currentState.StateEnter();
			}
		}
	}

	//Function to check if a tank or base is spotted
	public void CheckTargetSpotted()
	{
		if (enemyTank != null)
		{
			float checkPursueDistance = Vector3.Distance(transform.position, enemyTank.transform.position);
			stats["targetSpotted"] = checkPursueDistance < pursueRange;
			return;
		}
		else if (enemyBase != null)
		{
			float checkPursueDistance = Vector3.Distance(transform.position, enemyBase.transform.position);
			stats["targetSpotted"] = checkPursueDistance < pursueRange;
			return;
		}
		else
		{
			stats["targetSpotted"] = false;
		}
	}

	//Function to determine if a tank or base is reached
	public void CheckTargetReached()
	{
		if (enemyTank != null)
		{
			float checkAttackDistance = Vector3.Distance(transform.position, enemyTank.transform.position);
			stats["targetReached"] = checkAttackDistance < attackRange;
			return;
		}
		else if (enemyBase != null)
		{
			float checkAttackDistance = Vector3.Distance(transform.position, enemyBase.transform.position);
			stats["targetReached"] = checkAttackDistance < attackRange;
			return;
		}
		else
		{
			stats["targetReached"] = false;
		}
	}

	//Function to check if the enemy is low on fuel, used in the outrun state
	public void CheckFuel()
	{
		if (enemyTank != null)
		{
			DumbTank enemy = enemyTank.GetComponent<DumbTank>();

			if (enemy != null)
			{
				stats["enemyLowFuel"] = enemy.TankCurrentFuel < TankCurrentFuel;
			}
			else
			{
				stats["enemyLowFuel"] = false;
			}
		}
		else
		{
			stats["enemyLowFuel"] = false;
		}
	}
}