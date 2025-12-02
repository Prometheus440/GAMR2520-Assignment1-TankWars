using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartTank : DumbTank
{
	public GameObject enemyTank;
	public float attackRange = 25.0f;
	public float pursueRange = 40.0f;
	public float turnSpeed = 2.0f;
}
