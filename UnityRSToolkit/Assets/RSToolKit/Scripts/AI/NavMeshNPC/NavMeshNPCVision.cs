﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using RSToolkit.Helpers;
using System;
using UnityEngine.Events;

namespace RSToolkit.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshNPCVision : MonoBehaviour
    {
        public float ViewMagnitude = 10;
        public float SqrViewMagnitude
        {
            get
            {
                return ViewMagnitude * ViewMagnitude;
            }
        }

        private NavMeshNPC m_navMeshNPCComponent;
        public NavMeshNPC NavMeshNPCComponent
        {
            get
            {
                if (m_navMeshNPCComponent == null)
                {
                    m_navMeshNPCComponent = GetComponent<NavMeshNPC>();
                }
                return m_navMeshNPCComponent;
            }

        }

        public float FieldOfViewAngle = 45;

        public string[] LookOutForTags;
        public Transform[] LookOutForTransforms;

        public class OnTransformSeenEvent : UnityEvent<Transform> { }
        public OnTransformSeenEvent OnTransformSeen = new OnTransformSeenEvent();

        private List<Transform> m_tagLookOutForTransforms = new List<Transform>();
        public List<Transform> GetTagLookOutForTransforms(bool refresh = false)
        {
            if(!refresh && m_tagLookOutForTransforms.Any())
            {
                return m_tagLookOutForTransforms;
            }

            m_tagLookOutForTransforms = new List<Transform>();
            for(int i = 0; i < LookOutForTags.Length; i++)
            {
                m_tagLookOutForTransforms.AddRange(GameObject.FindGameObjectsWithTag(LookOutForTags[i])
                            .Select(go => go.transform));
            }

            return m_tagLookOutForTransforms;
        }

        public List<Transform > GetAllLookOutForTransforms(bool refresh = false)
        {
            return GetTagLookOutForTransforms(refresh).Union(LookOutForTransforms).ToList();
        }

        private Func<Transform, bool> isWithinSightLambda => (Transform t) => ProximityHelpers.IsWithinSight(transform, t, FieldOfViewAngle, SqrViewMagnitude);

        public virtual bool IsWithinSight()
        {
            return LookOutForTransforms.Any(isWithinSightLambda);
        }

        public Transform[] GetTrasnformsWithinSight(bool refreshList = false)
        {
            return GetAllLookOutForTransforms(refreshList).Where(isWithinSightLambda).ToArray();
        }

        public Transform[] DoLookoutFor(bool newTransformsOnly = true, bool refreshList = false)
        {
            var targets = GetTrasnformsWithinSight(refreshList);
            if (newTransformsOnly)
            {
                targets = targets.Where(t => !NavMeshNPCComponent.NoticedTransforms.Contains(t)).ToArray();
            }
                
            
            for (int i = 0; i < targets.Length; i++)
            {
                if (NavMeshNPCComponent.AttractAttention_FromTransform(targets[i]))
                {
                    OnTransformSeen.Invoke(targets[i]);       
                }
            }
            return targets;
        }

        // Draw the line of sight representation within the scene window
        public void OnDrawGizmos()
        {
            ProximityHelpers.DrawGizmoLineOfSight(transform, FieldOfViewAngle, ViewMagnitude, IsWithinSight());
        }
    }
}