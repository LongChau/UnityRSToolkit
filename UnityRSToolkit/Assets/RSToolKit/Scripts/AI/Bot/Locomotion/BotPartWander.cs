﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RSToolkit.Helpers;
using RSToolkit.AI.FSM;

namespace RSToolkit.AI.Locomotion
{
    [RequireComponent(typeof(BotPartWanderManager))]
    public abstract class BotPartWander : MonoBehaviour
    {

        // public bool WanderOnAwake = false;
        public float WaitTime = 5f;
        public bool randomizeWait = false;
        private const string DEBUG_TAG = "BotWander";
        public bool DebugMode = false;
        public float MovementTimeout = 5f;
        public bool AutoWander = true;

        private float _wanderRadius = 20f;

        private BotLocomotive m_botLocomotiveComponent;
        public BotLocomotive BotLocomotiveComponent
        {
            get
            {
                if (m_botLocomotiveComponent == null)
                {
                    m_botLocomotiveComponent = GetComponent<BotLocomotive>();
                }
                return m_botLocomotiveComponent;
            }

        }

        public virtual Bot.DistanceType StopMovementCondition
        {
            get
            {
                return Bot.DistanceType.AT_POSITION;
            }
        }

        /// <summary>
        /// Check if Bot is able to wander
        /// </summary>
        public abstract bool CanWander();

        /// <summary>
        /// Set the radius of which the bot can wander from current position
        /// </summary>
        public void SetWanderRadius(float radius)
        {
            _wanderRadius = radius;
        }

        /// <summary>
        /// Get a valid random position within the wander radius of the provided wanderCenter
        /// </summary>
        public Vector3? GetNewWanderPosition(Transform wanderCenter){
            return GetNewWanderPosition(wanderCenter, _wanderRadius);
        }
        protected abstract Vector3? GetNewWanderPosition(Transform wanderCenter, float radius);

        protected virtual void Awake()
        {
        }

        /*
        protected virtual void OnDrawGizmos()
        {
#if UNITY_EDITOR
            var oldColor = UnityEditor.Handles.color;
            UnityEditor.Handles.color = new Color(1f, 0.45f, 0f); //, .75f);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, defaultWanderRadius);
            UnityEditor.Handles.color = oldColor;
#endif
        }
        */

    }
}
