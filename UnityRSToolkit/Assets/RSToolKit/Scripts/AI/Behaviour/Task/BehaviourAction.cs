﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RSToolkit.AI.Behaviour.Task
{

    public class BehaviourAction : BehaviourNode
    {
        private const string NODE_NAME = "Action";

        public enum ActionResult
        {
            SUCCESS,
            FAILED,
            BLOCKED,
            PROGRESS
        }

        public enum ActionRequest
        {
            START,
            UPDATE,
            CANCEL,
        }
        private System.Func<bool> m_singleFrameFunc = null;
        private System.Func<bool, ActionResult> m_multiFrameFunc = null;
        private System.Func<ActionRequest, ActionResult> m_multiFrameRequestFunc = null;
        private System.Action m_singleFrameAction = null;
        private bool m_bWasBlocked = false;

        private ActionResult m_actionResult = ActionResult.PROGRESS;
        private ActionRequest m_actionRequest = ActionRequest.START;
        private void Init()
        {
            OnStarted.AddListener(OnStarted_Listener);
            OnStopping.AddListener(OnStopping_Listener);
            m_actionResult = ActionResult.PROGRESS;
        }
        #region Constructors
        public BehaviourAction(System.Func<bool> singleFrameFunc, string name = NODE_NAME) : base(name, NodeType.TASK)
        {
            m_singleFrameFunc = singleFrameFunc;
            Init();
        }
        public BehaviourAction(System.Func<bool, ActionResult> multiFrameFunc, string name = NODE_NAME) : base(name, NodeType.TASK)
        {
            m_multiFrameFunc = multiFrameFunc;
            Init();
        }
        public BehaviourAction(System.Func<ActionRequest, ActionResult> multiFrameRequestFunc, string name = NODE_NAME) : base(name, NodeType.TASK)
        {
            m_multiFrameRequestFunc = multiFrameRequestFunc;
            Init();
        }

        public BehaviourAction(System.Action action, string name = NODE_NAME) : base(name, NodeType.TASK)
        {
            m_singleFrameAction = action;
            Init();
        }
        #endregion Constructors

        private void OnStarted_Listener()
        {
            m_bWasBlocked = false;

            if (m_multiFrameRequestFunc != null)
            {
                m_actionRequest = ActionRequest.START;
            }
            
            /*
            if (m_singleFrameAction != null)
            {
                m_singleFrameAction .Invoke();
                OnStopped.Invoke(true);
            }
            else if(m_multiFrameFunc != null || m_multiFrameRequestFunc != null)
            {
                if (m_multiFrameFunc != null)
                {
                    m_actionResult = m_multiFrameFunc.Invoke(false);
                }
                else if (m_multiFrameRequestFunc != null)
                {
                    m_actionResult  = m_multiFrameRequestFunc.Invoke(ActionRequest.START);
                }

                if (m_actionResult == ActionResult.BLOCKED)
                {
                    m_bWasBlocked = true;
                }
                else if (m_actionResult != ActionResult.PROGRESS)
                {
                    OnStopped.Invoke(m_actionResult == ActionResult.SUCCESS);
                }
            }
            else if (m_singleFrameFunc != null)
            {
                OnStopped.Invoke(m_singleFrameFunc.Invoke());
            }
            */
        }
        /*
        public override void Update()
        {
            base.Update();
            if (m_actionResult != ActionResult.FAILED && m_actionResult != ActionResult.SUCCESS)
            {
                if (m_multiFrameFunc != null)
                {
                    m_actionResult = m_multiFrameFunc.Invoke(false);
                    if (m_actionResult != ActionResult.PROGRESS && m_actionResult != ActionResult.BLOCKED)
                    {
                        OnStopped.Invoke(m_actionResult == ActionResult.SUCCESS);
                    }
                }
                else if (m_multiFrameRequestFunc != null)
                {
                    if (m_actionResult == ActionResult.BLOCKED)
                    {
                        m_bWasBlocked = true;
                    }
                    else if (m_actionResult == ActionResult.PROGRESS)
                    {
                        m_bWasBlocked = false;
                    }
                    else
                    {
                        OnStopped.Invoke(m_actionResult == ActionResult.SUCCESS);
                    }
                }
            }
        }
        */

        public override void Update()
        {
            base.Update();
            if (m_singleFrameAction != null)
            {
                m_singleFrameAction.Invoke();
                OnStopped.Invoke(true);
            }
            else if (m_multiFrameFunc != null || m_multiFrameRequestFunc != null)
            {
                if (m_multiFrameFunc != null)
                {
                    m_actionResult = m_multiFrameFunc.Invoke(false);
                }
                else if (m_multiFrameRequestFunc != null)
                {
                    m_actionResult = m_multiFrameRequestFunc.Invoke(m_actionRequest);
                    m_actionRequest = m_bWasBlocked ? ActionRequest.START : ActionRequest.UPDATE;
                }

                if (m_actionResult == ActionResult.BLOCKED)
                {
                    m_bWasBlocked = true;
                }
                else if (m_actionResult != ActionResult.PROGRESS)
                {
                    OnStopped.Invoke(m_actionResult == ActionResult.SUCCESS);
                }
            }
            else if (m_singleFrameFunc != null)
            {
                OnStopped.Invoke(m_singleFrameFunc.Invoke());
            }
        }

        private void OnStopping_Listener()
        {
            if (m_multiFrameFunc != null)
            {
                m_actionResult = m_multiFrameFunc.Invoke(true);
            }
            else if (m_multiFrameRequestFunc != null)
            {
                m_actionResult = m_multiFrameRequestFunc.Invoke(ActionRequest.CANCEL);
            }
            OnStopped.Invoke(m_actionResult == ActionResult.SUCCESS);
        }

    }

}
