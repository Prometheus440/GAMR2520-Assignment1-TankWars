using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;

public class BJ_Rule
{
    public string antecedentA;
    public string antecedentB;
    public string antecedentC;
    public Type consequentState;

    public Predicate compare;
    public enum Predicate { And, Or, nAnd, notAAndB, AAndBNotC }


    public BJ_Rule(string antecedentA, string antecedentB, string antecedentC, Type consequentState, Predicate compare)
    {
        this.antecedentA = antecedentA;
        this.antecedentB = antecedentB;
        this.antecedentC = antecedentC;
        this.consequentState = consequentState;
        this.compare = compare;
    }

    public Type CheckRule(Dictionary<string, bool> stats)
    {
        bool antecedentABool = stats[antecedentA];
        bool antecedentBBool = stats[antecedentB];
        bool antecedentCBool = stats[antecedentC];

        switch (compare)
        {
            case Predicate.And:
                if (antecedentABool && antecedentBBool)
                {
                    return consequentState;
                }
                else
                {
                    return null;
                }
            case Predicate.Or:
                if (antecedentABool || antecedentBBool)
                {
                    return consequentState;
                }
                else
                {
                    return null;
                }
            case Predicate.nAnd:
                if (!antecedentABool && !antecedentBBool)
                {
                    return consequentState;
                }
                else
                {
                    return null;
                }
            case Predicate.notAAndB:
                if (!antecedentABool && antecedentBBool)
                {
                    return consequentState;
                }
                else
                {
                    return null;
                }
            case Predicate.AAndBNotC:
                if (antecedentABool && antecedentBBool && !antecedentCBool)
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