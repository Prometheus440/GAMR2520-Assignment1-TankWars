using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class BJ_BTSequence : BJ_BTBaseNode
{
	protected List<BJ_BTBaseNode> btNodes = new List<BJ_BTBaseNode>();

	public BJ_BTSequence(List<BJ_BTBaseNode> btNodes)
	{
		this.btNodes = btNodes;
	}

	public override BJ_BTNodeState Evaluate()
	{
		bool failed = false;
		foreach (BJ_BTBaseNode btNode in btNodes)
		{
			if (failed == true)
			{
				break;
			}

			switch (btNode.Evaluate())
			{
				case BJ_BTNodeState.FAILURE:
					btNodeState = BJ_BTNodeState.FAILURE;
					failed = true;
					break;

				case BJ_BTNodeState.SUCCESS:
					btNodeState = BJ_BTNodeState.SUCCESS;
					continue;

				default:
					btNodeState = BJ_BTNodeState.FAILURE;
					failed = true;
					break;
			}
		}

		return btNodeState;
	}
}