﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RSToolkit.Helpers;
using RSToolkit.AI.Helpers;
using RSToolkit.Space3D;

namespace RSToolkit.AI.Locomotion
{

    [RequireComponent(typeof(NavMeshAgent))]
    public class BotNavMesh : BotLocomotion
    {
        public float walkSpeed = 0.75f;
        public float walkRotationSpeed = 120f;

        public float runSpeed = 5f;
        public float runRotationSpeed = 120f;
        
        #region Components
        private NavMeshAgent m_navMeshAgentComponent;
        public NavMeshAgent NavMeshAgentComponent
        {
            get
            {
                if (m_navMeshAgentComponent == null)
                {
                    m_navMeshAgentComponent = GetComponent<NavMeshAgent>();
                }
                return m_navMeshAgentComponent;
            }

        }
        #endregion Components

        public ProximityChecker JumpProximityChecker;

        public override float CurrentSpeed
        {
            get
            {
                return NavMeshHelpers.GetCurrentSpeed(NavMeshAgentComponent);
            } 
        }

        public override void RotateTowardsPosition()
        {
            var rotation = Quaternion.LookRotation(BotComponent.FocusedOnPosition.Value - transform.position, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, NavMeshAgentComponent.angularSpeed* Time.deltaTime);
        }

        public override void RotateAwayFromPosition(){
            var rotation = Quaternion.LookRotation(GetMoveAwayDestination() - transform.position, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, NavMeshAgentComponent.angularSpeed* Time.deltaTime);
        }

        private void MoveTo(Vector3 destination, float speed, float angularSpeed)
        {
            try
            {


                NavMeshAgentComponent.speed = speed;
                NavMeshAgentComponent.angularSpeed = angularSpeed;
                NavMeshAgentComponent.destination = destination;
                NavMeshAgentComponent.stoppingDistance = 0f;
                switch (m_stopMovementCondition)
                {
                    case StopMovementConditions.WITHIN_INTERACTION_DISTANCE:
                        NavMeshAgentComponent.stoppingDistance = BotComponent.SqrInteractionMagnitude * .75f;
                        break;
                    case StopMovementConditions.WITHIN_PERSONAL_SPACE:
                        NavMeshAgentComponent.stoppingDistance = BotComponent.SqrPersonalSpaceMagnitude * .75f;
                        break;
                }
                NavMeshAgentComponent.isStopped = false;

            }
            catch (System.Exception ex)
            {
                if (DebugMode)
                {
                    Debug.LogError($"Locomotion Error: {ex.Message}");
                }
                m_FSM.ChangeState(LocomotionState.CannotMove);
            }
        }

        public override void MoveTowardsPosition(bool fullspeed = true)
        {
            if (fullspeed)
            {
                MoveTo(BotComponent.FocusedOnPosition.Value, runSpeed, runRotationSpeed);
            }
            else
            {
                MoveTo(BotComponent.FocusedOnPosition.Value, walkSpeed, walkRotationSpeed);
            }
        }

        public override void MoveAway(bool fullspeed = true){

            if (fullspeed)
            {
                MoveTo(GetMoveAwayDestination(), runSpeed, runRotationSpeed);
            }
            else
            {
                MoveTo(GetMoveAwayDestination(), walkSpeed, walkRotationSpeed);
            }
        }

        public void MoveToClosestEdge(bool fullspeed = true)
        {
            NavMeshHit hit;
            NavMeshAgentComponent.FindClosestEdge(out hit);
            BotComponent.UnFocus();
            BotComponent.FocusOnPosition(hit.position);
            MoveTowardsPosition(fullspeed);
            MoveToPosition(StopMovementConditions.WITHIN_PERSONAL_SPACE, fullspeed);
        }

        public Vector3? JumpOffLedge(bool fullspeed = false)
        {
            RaycastHit rayhit;

            if(CanJumpDown(out rayhit))
            {
                BotComponent.UnFocus();
                BotComponent.FocusOnPosition(rayhit.point);                
                MoveToPosition(StopMovementConditions.AT_POSITION, fullspeed);
                return rayhit.point;
            }
            return null;
        }

        public bool CanJumpDown(out RaycastHit rayhit)
        {
            if (JumpProximityChecker.IsWithinRayDistance(out rayhit) != null)
            {
                var jumpPath = new NavMeshPath();
                NavMesh.CalculatePath(transform.position, rayhit.point, NavMesh.AllAreas, jumpPath);
                return jumpPath.status == NavMeshPathStatus.PathComplete;
            }
            return false;
        }


        protected virtual void OffMeshLinkUpdate(NavMeshHelpers.OffMeshLinkPosition linkposition)
        {

        }

        private NavMeshHelpers.OffMeshLinkPosition m_linkposition;
        private void CheckUpdateOffMeshLinkPosition()
        {
            m_linkposition = NavMeshAgentComponent.GetOffMeshLinkPosition();
            if( m_linkposition != NavMeshHelpers.OffMeshLinkPosition.Off)
            {
                OffMeshLinkUpdate(m_linkposition);
            }
        }
        
        private void CheckLinkArea()
        {

        }

        protected override bool CanMove()
        {
            return (NavMeshAgentComponent.speed > 0
                && NavMeshAgentComponent.angularSpeed > 0
                && isActiveAndEnabled);
        }

        public bool IsAboveNavMeshSurface()
        {
            return NavMeshAgentComponent.IsAboveNavMeshSurface();
        }

        #region States

        protected override void NotMoving_Enter()
        {
            base.NotMoving_Enter();
            if (NavMeshAgentComponent.isOnNavMesh)
            {
                NavMeshAgentComponent.isStopped = true;
            }
        }

        protected override void MovingToPosition_Update()
        {
            base.MovingToPosition_Update();
            //NavMeshHelpers.
        }

        #endregion States

        #region MonoBehaviour Functions

        protected override void Awake()
        {
            base.Awake();
            NavMeshAgentComponent.speed = walkSpeed;
            NavMeshAgentComponent.angularSpeed = walkRotationSpeed;
            //NavMeshAgentComponent.radius = BotComponent.SqrPersonalSpaceMagnitude;
        }

        #endregion MonoBehaviour Functions
    }
}

