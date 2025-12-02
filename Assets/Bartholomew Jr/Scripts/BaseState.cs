using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class BaseState : MonoBehaviour
{
	public abstract Type StateEnter();
	public abstract Type StateUpdate();
	public abstract Type StateExit();
}
