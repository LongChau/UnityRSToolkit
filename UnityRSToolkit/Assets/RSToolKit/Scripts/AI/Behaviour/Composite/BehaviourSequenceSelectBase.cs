﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RSToolkit.Helpers;

namespace RSToolkit.AI.Behaviour.Composite
{
    public abstract class BehaviourSequenceSelectBase : BehaviourParentNode
    {
        private int _index = -1;
        //private NodeTimer m_processChildTimer;

        public BehaviourNode CurrentChild
        {
            get
            {
                if (_index >= 0 && _index < Children.Count)
                {
                    return Children[_index];
                }
                return null;
            }
        }

        public bool IsRandom { get; private set; }
        public BehaviourSequenceSelectBase(string name, bool isRandom) : base(name, NodeType.COMPOSITE)
        {
            IsRandom = isRandom;
            OnStarted.AddListener(OnStarted_Listener);
            OnStartedSilent.AddListener(OnStartedSilent_Listener);
            OnStopping.AddListener(OnStopping_Listener);
            OnChildNodeStopped.AddListener(OnChildNodeStopped_Listener);
        }

        protected void ProcessChildNodeSequence(bool result_on_stop)
        {
            if (++_index < Children.Count)
            {
                if (State == NodeState.STOPPING)
                {

                    // Stopped manually
                    //OnStopped.Invoke(false);
                    StopNode(false);
                    
                }
                else
                {
                    // Run next child in sequence
                    // Children[m_index].StartNode();
                    CurrentChild.StartNode();
                    
                }
            }
            else
            {
                // Finished running all children
                // OnStopped.Invoke(result_on_stop);
                StopNode(result_on_stop);
                
            }
        }

        protected abstract void ProcessChildNodeSequence();

        private void ResetIndex()
        {
            _index = -1;
        }

        private void OnStarted_Common()
        {
            ResetIndex();
            if (IsRandom)
            {
                ShuffleChildren();
            }
        }

        protected virtual void OnStarted_Listener()
        {
            OnStarted_Common();
            RunOnNextTick(ProcessChildNodeSequence);
        }

        protected virtual void OnStartedSilent_Listener()
        {
            OnStarted_Common();
        }

        private void OnStopping_Listener()
        {
            if(_index > 0)
            {
                Children[_index].RequestStopNode();
            }
            else
            {
                StopChildren();
            }
        }

        /*
        protected override void StoppingNodeLogic(bool silent = false)
        {
            if (!silent)
            {
                CurrentChild.RequestStopNode();
            }
        }
        */

        protected abstract void OnChildNodeStopped_Listener(BehaviourNode child, bool success);
        

        public virtual void StopNextChildInPriorityTo(BehaviourNode child, bool restart_child)
        {
            int next_child_index = Children.IndexOf(child) + 1;

            //while(next_child_index < Children.Count)
            for (int i = next_child_index; i < Children.Count; i++)
            {
                if(Children[i].State == NodeState.ACTIVE)
                {
                    Children[i].RequestStopNode();
                    _index = restart_child ? i - 1 : Children.Count;
                    return;
                }
                
            }
        }
    }
}
