using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using System;

public class BJ_SmartTank : DumbTank
{
	private BJ_BaseState currentState;

	public float pursueRange = 52.0f; // from AITank viewRadius = 52.0f
	public float smartTankBodyRotationSpeed = 7.0f; // from AITank bodyRotationSpeed = 7.0f

	public Dictionary<string, bool> stats = new Dictionary<string, bool>();
	public BJ_Rules rules = new BJ_Rules();
	public bool lowHealth;
	public bool lowFuel;
	public bool lowAmmo;

	void InitialiseStats()
	{
		stats.Add("lowHealth", lowHealth);
		stats.Add("lowFuel", lowFuel);
		stats.Add("lowAmmo", lowAmmo);
		stats.Add("targetSpotted", false);
		stats.Add("targetReached", false);
		stats.Add("fleeState", false);
		stats.Add("pursueState", false);
		stats.Add("patrolState", false);
		stats.Add("attackState", false);
	}

	void InitialiseRules()
	{
		// Rules

		// Low health so flee
		rules.AddRule(new BJ_Rule("lowHealth", "attackState", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
		rules.AddRule(new BJ_Rule("lowHealth", "pursueState", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
		rules.AddRule(new BJ_Rule("lowHealth", "patrolState", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
		// Low fuel so flee
		rules.AddRule(new BJ_Rule("lowFuel", "attackState", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
		rules.AddRule(new BJ_Rule("lowFuel", "pursueState", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
		rules.AddRule(new BJ_Rule("lowFuel", "patrolState", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
		// Low Ammo so flee
		rules.AddRule(new BJ_Rule("lowAmmo", "attackState", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
		rules.AddRule(new BJ_Rule("lowAmmo", "pursueState", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
		rules.AddRule(new BJ_Rule("lowAmmo", "patrolState", typeof(BJ_FleeState), BJ_Rule.Predicate.And));
		// Found target so pursue
		rules.AddRule(new BJ_Rule("patrolState", "targetSpotted", typeof(BJ_PursueState), BJ_Rule.Predicate.And));
		// Lost target so patrol
		rules.AddRule(new BJ_Rule("targetSpotted", "patrolState", typeof(BJ_PatrolState), BJ_Rule.Predicate.notAAndB));
		rules.AddRule(new BJ_Rule("targetSpotted", "pursueState", typeof(BJ_PatrolState), BJ_Rule.Predicate.notAAndB));
		rules.AddRule(new BJ_Rule("targetSpotted", "attackState", typeof(BJ_PatrolState), BJ_Rule.Predicate.notAAndB));
		// Reached target so attack
		rules.AddRule(new BJ_Rule("pursueState", "targetReached", typeof(BJ_AttackState), BJ_Rule.Predicate.And));
	}

	private void Start()
	{
		InitialiseStats();
		InitialiseRules();

		// Start with patrolling
		currentState = new BJ_PatrolState(this);
		currentState.StateEnter();
	}

	private void Update()
	{
		// Assign enemy tank
		if (enemyTank == null)
		{
			DumbTank newEnemy = FindObjectOfType<DumbTank>();

			if (newEnemy != null && newEnemy.gameObject != this.gameObject)
			{
				enemyTank = newEnemy.gameObject;
			}
		}

		stats["lowHealth"] = TankCurrentHealth <= 35.0f;
		stats["lowFuel"] = TankCurrentFuel <= 25.0f;
		stats["lowAmmo"] = TankCurrentAmmo <= 2.0f;

		if (enemyTank != null)
		{
			CheckTargetSpotted();
			CheckTargetReached();
			// Update FSM
			if (currentState != null)
			{
				Type nextStateType = currentState.StateUpdate();

				if (nextStateType != null)
				{
					currentState.StateExit();
					currentState = (BJ_BaseState)Activator.CreateInstance(nextStateType, this);
					currentState.StateEnter();
				}
			}
		}
	}

	public void CheckTargetSpotted()
	{
		if (Vector3.Distance(transform.position, enemyTank.transform.position) < pursueRange)
		{
			stats["targetSpotted"] = true;
		}
		else
		{
			stats["targetSpotted"] = false;
		}
	}

	public void CheckTargetReached()
	{
		if (Vector3.Distance(transform.position, enemyTank.transform.position) < 1.5f)
		{
			stats["targetReached"] = true;
		}
		else
		{
			stats["targetReached"] = false;
		}
	}
} 