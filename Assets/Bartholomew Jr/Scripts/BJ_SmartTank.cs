using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BJ_SmartTank : DumbTank
{
	public float pursueRange = 52.0f; // from AITank viewRadius = 52.0f
	public float attackRange = 26.0f;
	public float bodyRotationSpeed = 7.0f; // from AITank body rotation speed = 7.0f

	public Dictionary<string, bool> stats = new Dictionary<string, bool>();
	public BJ_Rules rules = new BJ_Rules();
	public bool lowHealth;

	void InitialiseStats()
	{
		stats.Add("lowHealth", lowHealth);
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
	}

	private void Start()
	{
		InitialiseStats();
		InitialiseRules();
	}
} 