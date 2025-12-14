using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class BJ_BTAction : BJ_BTBaseNode
{
	public delegate BJ_BTNodeState ActionFunction();
	private ActionFunction btAction;

	public BJ_BTAction(ActionFunction btAction)
	{
		this.btAction = btAction;
	}

	public override BJ_BTNodeState Evaluate()
	{
		switch (btAction())
		{
			case BJ_BTNodeState.SUCCESS:
				btNodeState = BJ_BTNodeState.SUCCESS;
				return btNodeState;

			case BJ_BTNodeState.FAILURE:
				btNodeState = BJ_BTNodeState.FAILURE;
				return btNodeState;

			default:
				btNodeState = BJ_BTNodeState.FAILURE;
				return btNodeState;
		}
	}
}