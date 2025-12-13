using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BJ_StateMachine : MonoBehaviour
{
    private Dictionary<Type, BJ_BaseState> states;

    public BJ_BaseState currentState;

    public BJ_BaseState CurrentState
    {
        get
        {
            return currentState;
        }
        private set
        {
            currentState = value;
        }
    }

    public void SetStates(Dictionary<Type, BJ_BaseState> states)
    {
        this.states = states;
    }

    void Update()
    {
        if (CurrentState == null)
        {
            CurrentState = states.Values.First();
            CurrentState.StateEnter();
        }
        else
        {
            var nextState = CurrentState.StateUpdate();

            if (nextState != null && nextState != CurrentState.GetType())
            {
                SwitchToState(nextState);
            }
        }
    }

    void SwitchToState(Type nextState)
    {
        CurrentState.StateExit();
        CurrentState = states[nextState];
        CurrentState.StateEnter();
    }
}