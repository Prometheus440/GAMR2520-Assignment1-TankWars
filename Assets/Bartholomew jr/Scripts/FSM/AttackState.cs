using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : BaseState
{
    private SmartTank tank;

    public AttackState(SmartTank tank)
    {
        this.tank = tank;
    }

    public override Type StateEnter()
    {
        return null;
    }

    public override Type StateExit()
    {
        return null;
    }

    public override Type StateUpdate()
    {
        time += Time.deltaTime;
        if (time > 2)
        {
            time = 0;
            return typeof(RoamState);
        }
        else
        {
            tank.AttackTarget();
            return null;
        }
    }
}
