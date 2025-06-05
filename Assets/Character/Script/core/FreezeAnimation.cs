using UnityEngine;
using UnityEngine.Animations;

public class FreezeAnimation : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.speed = 0f;
    }
}
