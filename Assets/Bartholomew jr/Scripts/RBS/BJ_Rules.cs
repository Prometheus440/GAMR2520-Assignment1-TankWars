using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BJ_Rules
{
    public void AddRule(BJ_Rule rule)
    {
        GetRules.Add(rule);
    }

    public List<BJ_Rule> GetRules { get; } = new List<BJ_Rule>();
}