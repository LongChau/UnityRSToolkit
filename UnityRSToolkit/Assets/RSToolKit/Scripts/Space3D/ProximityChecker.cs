﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using RSToolkit.Space3D.Helpers;

namespace RSToolkit.Space3D
{
    public class ProximityChecker : MonoBehaviour
    {

        public float MinRayDistance = 0f;
        public float MaxRayDistance = 0.5f;
        public float IsAlmostTouchingDistance = 0.05f;
        public AxisHelpers.Axis RayDirection = AxisHelpers.Axis.DOWN;
        public LayerMask LayerMask = 1 << 0; // default | ~0; // everything

        public bool IsTrigger = true;

        public bool DebugMode;
        public bool DebugNavMesh = false;

        public UnityEvent OnProximityEntered { get; private set; } = new UnityEvent();
        public UnityEvent OnTouchingEntered { get; private set; } = new UnityEvent();
        bool proximityEnteredTriggered = false;
        bool touchEnteredTriggered = false;
        Color _rayColor = Color.green;
        /*
        private Vector3 GetRayDirectionVector()
        {
            switch (RayDirection)
            {
                case RayDirectionEnum.UP:
                    return Vector3.up;
                case RayDirectionEnum.DOWN:
                    return Vector3.down;
                case RayDirectionEnum.LEFT:
                    return Vector3.left;
                case RayDirectionEnum.RIGHT:
                    return Vector3.right;
                case RayDirectionEnum.FORWARD:
                    return Vector3.forward;
                case RayDirectionEnum.BACK:
                    return Vector3.back;
            }
            return Vector3.zero;
        }
        */

        float? _currentRayDistance = null;
        public bool IsAlmostTouching(bool checkForNavMesh = true)
        {
            _currentRayDistance = IsWithinRayDistance(checkForNavMesh);
            bool result = _currentRayDistance != null && _currentRayDistance.Value <= IsAlmostTouchingDistance;
            if (result)
            {
                if (IsTrigger && !touchEnteredTriggered)
                {
                    OnTouchingEntered.Invoke();
                    touchEnteredTriggered = true;
                }
            }
            else
            {
                touchEnteredTriggered = false;
            }
            return result;
        }

        /*
        public bool IsBeyondRayDistance(float offsetDistance)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, GetRayDirectionVector(), out hit, MaxRayDistance + offsetDistance, LayerMask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }
            return true;
        }
        */

        // To Refactor
        public float? IsWithinRayDistance(bool checkForNavMesh = true)
        {
            if (checkForNavMesh)
            {
                NavMeshHit navHit;
                return IsWithinRayDistance(out navHit);
            }
            else
            {
                RaycastHit rayHit;
                return IsWithinRayDistance(out rayHit);
            }

        }

        public float? IsWithinRayDistance(out RaycastHit hit)
        {
            float? hitDistance = null;
            _rayColor = Color.green;
            if (Physics.Raycast(transform.position, RayDirection.ToVector3() /* GetRayDirectionVector() */, out hit, MaxRayDistance, LayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.distance >= MinRayDistance)
                {

                    hitDistance = hit.distance;
                    _rayColor = Color.red;

                    if (IsTrigger && !proximityEnteredTriggered)
                    {
                        proximityEnteredTriggered = true;
                        OnProximityEntered.Invoke();
                    }
                }
                else
                {
                    proximityEnteredTriggered = false;
                }
            }
            else
            {
                proximityEnteredTriggered = false;
            }
            return hitDistance;
        }

        public float? IsWithinRayDistance(out NavMeshHit hit)
        {
            float? hitDistance = null;
            _rayColor = Color.green;
            if (NavMesh.SamplePosition(transform.position, out hit, MaxRayDistance, NavMesh.AllAreas))
            {
                _rayColor = Color.cyan;
                hitDistance = hit.distance;

                if (IsTrigger && !proximityEnteredTriggered)
                {
                    proximityEnteredTriggered = true;
                    OnProximityEntered.Invoke();
                }
            }
            else
            {
                proximityEnteredTriggered = false;
            }
            return hitDistance;
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (DebugMode)
            {
                IsWithinRayDistance(DebugNavMesh);
                Debug.DrawLine(transform.position, transform.TransformPoint(/* GetRayDirectionVector() */ RayDirection.ToVector3() * MaxRayDistance), _rayColor);
            }
#endif
        }


    }
}