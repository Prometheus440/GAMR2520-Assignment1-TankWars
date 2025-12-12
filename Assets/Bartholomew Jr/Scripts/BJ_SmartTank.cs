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

	public override void AIOnCollisionEnter(Collision collision)
	{
		base.AIOnCollisionEnter(collision);
	}

	public override void AITankStart()
	{
		if (!isInitialised)
		{
			InitialiseStats();
			InitialiseRules();

			// Start with patrolling
			currentState = new BJ_PatrolState(this);
			currentState.StateEnter();

			isInitialised = true;
		}
	}

	public override void AITankUpdate()
	{
		Debug.Log(currentState);

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
		}

		stats["lowHealth"] = TankCurrentHealth <= 35.0f;
		stats["lowFuel"] = TankCurrentFuel <= 25.0f;
		stats["lowAmmo"] = TankCurrentAmmo <= 2.0f;

		CheckTargetSpotted();
		CheckTargetReached();
		CheckFuel();


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

	public void CheckTargetSpotted()
	{
		if (enemyTank != null)
		{
			float checkPursueDistance = Vector3.Distance(transform.position, enemyTank.transform.position);
			stats["targetSpotted"] = checkPursueDistance < pursueRange;
		}
		else if (EnemyBases != null)
		{
			float checkPursueDistance = Vector3.Distance(transform.position, enemyBase.transform.position);
			stats["targetSpotted"] = checkPursueDistance < pursueRange;
		}
		else
		{
			stats["targetSpotted"] = false;
		}
	}

	public void CheckTargetReached()
	{
		if (enemyTank != null)
		{
			float checkAttackDistance = Vector3.Distance(transform.position, enemyTank.transform.position);
			stats["targetReached"] = checkAttackDistance < attackRange;
		}
		else if (EnemyBases != null)
		{
			float checkAttackDistance = Vector3.Distance(transform.position, enemyBase.transform.position);
			stats["targetReached"] = checkAttackDistance < attackRange;
		}
		else
		{
			stats["targetReached"] = false;
		}
	}

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
	}

	public void GetEnemyState()
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
}