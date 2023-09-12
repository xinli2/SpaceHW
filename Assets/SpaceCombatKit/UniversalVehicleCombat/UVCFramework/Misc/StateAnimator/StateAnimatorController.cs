using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.Utilities
{
    /// <summary>
    /// Control an animator that switches between multiple distinct states, such as a door (open/close) or a ramp (extend/retract)
    /// </summary>
    public class StateAnimatorController : MonoBehaviour
    {

        [Tooltip("The list of triggers that trigger different states via animation.")]
        [SerializeField]
        protected List<string> stateTriggers = new List<string>();

        [Tooltip("The starting state of this animated object.")]
        [SerializeField]
        protected int startingState = 0;

        /// <summary>
        /// The current state of this animated object.
        /// </summary>
        public int CurrentState { get { return currentState; } }
        protected int currentState = -1;

        [Tooltip("The animators that are being managed by this controller.")]
        [SerializeField]
        protected List<Animator> animators = new List<Animator>();

        [Tooltip("Whether the animators keep their state when they become disabled in the scene.")]
        [SerializeField]
        protected bool keepAnimatorStateOnDisable = true;



        protected virtual void Reset()
        {
            animators = new List<Animator>(GetComponentsInChildren<Animator>());
            
            stateTriggers.Add("Retract");
            stateTriggers.Add("Extend");
        }


        protected virtual void Awake()
        {
            foreach (Animator m_Animator in animators)
            {
                m_Animator.keepAnimatorStateOnDisable = keepAnimatorStateOnDisable;
            }

            SetState(startingState);
        }


        /// <summary>
        /// Set the state of this controller.
        /// </summary>
        /// <param name="index">The new state.</param>
        public virtual void SetState(int index)
        {
            if (currentState != index && index >= 0 && index < stateTriggers.Count)
            {
                currentState = index;
                foreach (Animator m_Animator in animators)
                {
                    m_Animator.SetTrigger(stateTriggers[index]);
                }
            }
        }

        
        /// <summary>
        /// Called via animation event when an animation has finished putting the object in a state.
        /// </summary>
        protected virtual void OnStateAnimationFinished()
        {
            ResetTriggers();
        }


        /// <summary>
        /// Reset all animator triggers.
        /// </summary>
        public virtual void ResetTriggers()
        {
            foreach (string stateTrigger in stateTriggers)
            {
                foreach (Animator m_Animator in animators)
                {
                    m_Animator.ResetTrigger(stateTrigger);
                }
            }
        }
    }
}


