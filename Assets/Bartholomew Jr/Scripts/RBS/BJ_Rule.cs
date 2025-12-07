using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;

public class BJ_Rule
{
	public string antedecentA;
	public string antedecentB;
	public Type consequentState;

	public Predicate compare;
	public enum Predicate { And, Or, nAnd, notAAndB}


	public BJ_Rule(string antedecentA, string antedecentB, Type consequentState, Predicate compare)
	{
		this.antedecentA = antedecentA;
		this.antedecentB = antedecentB;
		this.consequentState = consequentState;
		this.compare = compare;
	}

	public Type CheckRule(Dictionary<string, bool> stats)
	{
		bool antedecentABool = stats[antedecentA];
		bool antedecentBBool = stats[antedecentB];

		switch (compare)
		{
			case Predicate.And:
				if (antedecentABool && antedecentBBool)
				{
					return consequentState;
				}
				else
				{
					return null;
				}
			case Predicate.Or:
				if (antedecentABool || antedecentBBool)
				{
					return consequentState;
				}
				else
				{
					return null;
				}
			case Predicate.nAnd:
				if (!antedecentABool && !antedecentBBool)
				{
					return consequentState;
				}
				else
				{
					return null;
				}
			case Predicate.notAAndB:
				if (!antedecentABool && antedecentBBool)
				{
					return consequentState;
				}
				else
				{
					return null;
				}
			default:
				return null;
		}
	}
}