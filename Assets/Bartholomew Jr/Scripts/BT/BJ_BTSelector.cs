using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class BJ_BTSelector : BJ_BTBaseNode
{
	protected List<BJ_BTBaseNode> btNodes = new List<BJ_BTBaseNode>();

	public BJ_BTSelector(List<BJ_BTBaseNode> btNodes)
	{
		this.btNodes = btNodes;
	}

	public override BJ_BTNodeState Evaluate()
	{
		foreach (BJ_BTBaseNode btNode in btNodes)
		{
			switch (btNode.Evaluate())
			{
				case BJ_BTNodeState.FAILURE:
					continue;

				case BJ_BTNodeState.SUCCESS:
					btNodeState = BJ_BTNodeState.SUCCESS;
					return btNodeState;

				default:
					continue;
			}
		}

		btNodeState = BJ_BTNodeState.FAILURE;
		return btNodeState;
	}
}
