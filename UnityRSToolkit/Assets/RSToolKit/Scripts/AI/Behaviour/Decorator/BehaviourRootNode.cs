﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace RSToolkit.AI.Behaviour
{

    /// <summary>
    /// The root node of a tree
    /// </summary>
    public class BehaviourRootNode : BehaviourParentNode
    {
        private NodeTimer m_rootTimer;
        public bool IsSilent { get; private set; } = false;

        private Dictionary<string, BehaviourNode> _nodeDictionary = new Dictionary<string, BehaviourNode>();

        public string[] LastResyncNodeIDs = null;

        /// <summary>
        /// The root node of a tree
        /// </summary>
        /// <param name="name"></param>
        public BehaviourRootNode(string name = "Root") : base(name, NodeType.DECORATOR)
        {
            OnChildNodeStopped.AddListener(OnChildNodeStopped_Listener);
            OnStopping.AddListener(OnStopping_Listener);
            OnStoppingSilent.AddListener(OnStoppingSilent_Listener);
            OnStarted.AddListener(OnStarted_Listener);
            OnStartedSilent.AddListener(OnStartedSilent_Listener);
        }

        #region Events

        private void OnStarted_Listener()
        {
            IsSilent = false;
            StartFirstChildNodeOnNextTick();
        }

        private void OnStartedSilent_Listener()
        {
            IsSilent = true;
        }

        private void OnChildNodeStopped_Listener(BehaviourNode child, bool success)
        {
            if (State != NodeState.STOPPING)
            {
                // wait one tick, to prevent endless recursions
                m_rootTimer = StartFirstChildNodeOnNextTick();
            }
            else
            {
                StopNodeOnNextTick(success);
            }
        }

        private void OnStopping_Listener()
        {
            if (this.Children[0].State == NodeState.ACTIVE)
            {
                StopChildren();

            }
            else
            {
                RemoveTimer(m_rootTimer);
                StopNodeOnNextTick(true);
            }
        }

        private void OnStoppingSilent_Listener()
        {

            if (this.Children[0].State != NodeState.ACTIVE)
            {
                RemoveTimer(m_rootTimer);
            }
        }

        #endregion Events

        // To Refactor
        public override void SetParent(BehaviourParentNode parent)
        {
            throw new System.Exception("Root nodes cannot have parents");
        }

        /// <summary>
        /// Update descendants and then update self (including timers)
        /// </summary>
        public override bool UpdateRecursively(UpdateType updateType = UpdateType.DEFAULT)
        {
            if (IsSilent)
            {
                return false;
            }
            return base.UpdateRecursively(updateType);
        }

        /// <summary>
        /// Set self to not Silent
        /// </summary>
        public void Wake()
        {
            IsSilent = false;
        }
        /// <summary>
        /// Set self to Silent (do not trigger BehaviourNode related events)
        /// </summary>
        public void Sleep()
        {
            IsSilent = true;
        }

        public void PopulateDictionary()
        {
            _nodeDictionary.Clear();
            _nodeDictionary.Add(GetUniqueID(), this);
            PopulateDictionaryFromChildren(Children);
        }

        /// <summary>
        /// Populate the dicitonary _nodeDictionary with all nodes in this
        /// BehaviourTree
        /// </summary>
        private void PopulateDictionaryFromChildren(ReadOnlyCollection<BehaviourNode> children)
        {
            BehaviourParentNode parentNode;
            for (int i = 0; i < children.Count; i++)
            {
                _nodeDictionary.Add(children[i].GetUniqueID(), children[i]);
                parentNode = children[i] as BehaviourParentNode;
                if (parentNode != null)
                {
                    PopulateDictionaryFromChildren(parentNode.Children);
                }
            }
        }

        public BehaviourNode GetNodeByID(string id)
        {
            return _nodeDictionary[id];
        }

        #region SyncLeaves
        // This is used to sync behaviour trees (for example when it comes to Network play)

        /// <summary>
        /// This is used for network peers, it tries to
        /// turn on/off behaviour nodes to match the BehaviourNodes
        /// of the host
        /// </summary>
        public bool SyncActiveLeaves(BehaviourNode[] activeLeaves, bool silent = true)
        {
            var myLeaves = GetLeaves(NodeState.ACTIVE).ToList();

            // Recursively stop all leaves that should not be running
            while (myLeaves.Count() > 0)
            {
                var ml = myLeaves[0];

                if (!activeLeaves.Contains(ml))
                {
                    var nodeParent = ml as BehaviourParentNode;
                    if (nodeParent == null || !nodeParent.IsAncestorOfOneOrMore(activeLeaves))
                    {
                        ml.RequestStopNode(true);
                        ml.StopNode(silent);
                    }
                }
                myLeaves.Remove(ml);

                if (ml.Parent != null && 
                        (ml.Parent.Type == NodeType.DECORATOR 
                            || (ml.Parent.Type == NodeType.COMPOSITE && !ml.Parent.IsAncestorOfOneOrMore(activeLeaves))
                        )
                    ) //!ml.Parent.HasChildren(activeLeaves))
                {
                    myLeaves.Add(ml.Parent);
                }
            }

            // Recursivly start all nodes that should be running
            for (int i = 0; i < activeLeaves.Length; i++)
            {
                if (activeLeaves[i].State != NodeState.ACTIVE)
                {
                    if (!activeLeaves[i].StartNodePath(silent))
                    {
                        return false;
                    }
                }

            }

            return true;
        }

        /// <summary>
        /// This is used for network peers, it tries to
        /// turn on/off behaviour nodes to match the BehaviourNodes
        /// of the host
        /// </summary>
        /// <param name="silent">If true will not invoke the OnStarted event</param>
        public bool SyncActiveLeaves(string[] nodeIDs, bool silent = true)
        {
            LastResyncNodeIDs = nodeIDs;
            var activeLeaves = new BehaviourNode[nodeIDs.Length];
            for(int i = 0; i < nodeIDs.Length; i++)
            {
                activeLeaves[i] = GetNodeByID(nodeIDs[i]);
            }

            return SyncActiveLeaves(activeLeaves, silent);
        }

        /// <summary>
        /// This is used for network peers, it tries to
        /// turn on/off behaviour nodes to match the BehaviourNodes
        /// of the host
        /// </summary>
        /// <param name="silent">If true will not invoke the OnStarted event</param>
        public bool SyncActiveLeaves(string nodeIDs, char seperator = '|', bool silent = true)
        {
            var nodeIDArray = nodeIDs.Split(seperator);
            return SyncActiveLeaves(nodeIDArray, silent);
        }

        #endregion
    }
}
