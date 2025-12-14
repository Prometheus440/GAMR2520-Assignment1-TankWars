using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BJ_BTBaseNode
{
	protected BJ_BTNodeState btNodeState;

	public BJ_BTNodeState BTNodeState
	{
		get { return btNodeState; }
	}

	public abstract BJ_BTNodeState Evaluate();
}