﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RSToolkit.AI.Behaviour.Composite;

namespace RSToolkit.AI.Behaviour.Decorator
{
    public abstract class BehaviourObserver : BehaviourParentNode
    {
        public enum AbortRule
        {
            /// <summary>
            /// Check it's condition once it is started and will never stop any running nodes
            /// </summary>
            NONE,
            /// <summary>
            /// Check it's condition once it is started and if it is met, it will observe the 
            /// blackboard for changes. Once the condition is no longer met, it will stop itself 
            /// allowing the parent composite to proceed with it's next node.
            /// </summary>
            SELF,
            /// <summary>
            /// Check it's condition once it is started and if it's not met, it will observe 
            /// the blackboard for changes. Once the condition is met, it will stop the 
            /// lower priority node allowing the parent composite to proceed with it's next node.
            /// </summary>
            LOWER_PRIORITY,
            /// <summary>
            /// Will stop both: self and lower priority nodes
            /// </summary>
            BOTH,
            /// <summary>
            /// Will check it's condition once it is started and if it's not met, it will observe 
            /// the blackboard for changes. Once the condition is met, it will stop the 
            /// lower priority node and order the parent composite to restart the Decorator 
            /// immediately.
            /// </summary>
            LOWER_PRIORITY_RESTART,
            /// <summary>
            /// Will check it's condition once it is started and if it's not met, it will observe 
            /// the blackboard for changes. Once the condition is met, it will stop the lower priority 
            /// node and order the parent composite to restart the Decorator immediately. 
            /// As in BOTH it will also stop itself as soon as the condition is no longer met.
            /// </summary>
            RESTART
        }

        private bool m_isObserving;
        private AbortRule m_abortRule;
        private bool m_initParent = false;
        public BehaviourObserver(string name, BehaviourNode decoratee, AbortRule abortRule) : base(name, NodeType.DECORATOR)
        {
            OnStarted.AddListener(OnStarted_Listener);
            OnStopping.AddListener(OnStopping_Listener);
            OnChildNodeStopped.AddListener(OnChildNodeStopped_Listener);

            m_abortRule = abortRule;
            m_isObserving = false;
            AddChild(decoratee);
        }
        protected abstract void StartObserving();

        protected abstract void StopObserving();

        protected abstract bool IsConditionMet();
        private void OnStarted_Listener()
        {
            // To refactor
            if (!m_initParent)
            {
                if (Parent.Type == NodeType.COMPOSITE)
                {
                    Parent.OnStopped.AddListener(OnCompositeParentStopped_Listener);
                }
                m_initParent = true;
            }

            if(m_abortRule != AbortRule.NONE)
            {
                if (!m_isObserving)
                {
                    m_isObserving = true;
                    StartObserving();
                }
            }
            if (!IsConditionMet())
            {
                OnStopped.Invoke(false);
            }
            else
            {
                Children[0].StartNode();
            }
        }

        private void OnStopping_Listener()
        {
            Children[0].StartNode();
        }
        private void OnChildNodeStopped_Listener(BehaviourNode child, bool success)
        {
            if(m_abortRule == AbortRule.NONE || m_abortRule == AbortRule.SELF)
            {
                if (m_isObserving)
                {
                    m_isObserving = false;
                    StopObserving();
                }
            }
            OnStopped.Invoke(success);
        }
        private void OnCompositeParentStopped_Listener(bool success)
        {
            if (m_isObserving)
            {
                m_isObserving = false;
                StopObserving();
            }
        }

        protected void Evaluate()
        {
            if (State == NodeState.ACTIVE && !IsConditionMet())
            {
                if (m_abortRule == AbortRule.SELF || m_abortRule == AbortRule.BOTH || m_abortRule == AbortRule.RESTART)
                {
                    this.RequestStopNode();
                }
            }
            else if (State != NodeState.ACTIVE && IsConditionMet())
            {
                if (m_abortRule == AbortRule.LOWER_PRIORITY || m_abortRule == AbortRule.BOTH || m_abortRule == AbortRule.RESTART || m_abortRule == AbortRule.LOWER_PRIORITY_RESTART)
                {
                    BehaviourNode parentNode = this.Parent;
                    BehaviourNode childNode = this;
                    while(parentNode != null && parentNode.Type != NodeType.COMPOSITE)
                    {
                        childNode = parentNode;
                        parentNode = parentNode.Parent;
                    }
                    if (m_abortRule == AbortRule.RESTART || m_abortRule == AbortRule.LOWER_PRIORITY_RESTART)
                    {
                        if (m_isObserving)
                        {
                            m_isObserving = false;
                            StopObserving();
                        }
                    }
                    if (parentNode is BehaviourSequenceSelectBase)
                    {
                        ((BehaviourSequenceSelectBase)parentNode).StopNextChildInPriorityTo(childNode, m_abortRule == AbortRule.RESTART || m_abortRule == AbortRule.LOWER_PRIORITY_RESTART);
                    }
                    else if (parentNode is BehaviourParallel)
                    {
                        ((BehaviourParallel)parentNode).RestartChild(childNode);
                    }
                }
            }
        }
    }
}
