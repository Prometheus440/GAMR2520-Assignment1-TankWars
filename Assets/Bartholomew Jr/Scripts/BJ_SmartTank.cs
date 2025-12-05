using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BJ_SmartTank : DumbTank
{
	public float pursueRange = 52.0f; // from AITank viewRadius = 52.0f
	public float attackRange = 26.0f;
	public float bodyRotationSpeed = 7.0f; // from AITank body rotation speed = 7.0f
} 