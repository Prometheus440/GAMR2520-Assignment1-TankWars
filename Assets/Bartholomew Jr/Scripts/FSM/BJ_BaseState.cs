using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class BJ_BaseState
{
	public abstract Type StateEnter();
	public abstract Type StateUpdate();
	public abstract Type StateExit();
}