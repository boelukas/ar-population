using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorBehaviourScript : MonoBehaviour
{
	Animator anim;
	AnimatorOverrideController animOverrideController;
	AnimationClip animationClip;

	// Use this for initialization
	void Start()
	{
		//create animation clip
		AnimationCurve translateX = AnimationCurve.Linear(0.0f, 0.0f, 2.0f, 2.0f);
		animationClip = new AnimationClip();
		animationClip.SetCurve("", typeof(Transform), "localPosition.x", translateX);

		anim = GetComponent<Animator>();
		AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController();
		animatorOverrideController.runtimeAnimatorController = anim.runtimeAnimatorController;
		animatorOverrideController["SMPLX-male|SMPLX-femaleAction"] = animationClip;
		anim.runtimeAnimatorController = animatorOverrideController;
	}
}