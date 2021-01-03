﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RSToolkit.Helpers;
using RSToolkit.AI.Helpers;
using RSToolkit.Space3D;
using RSToolkit.AI.FSM;

namespace RSToolkit.AI.Locomotion
{

    public class BotLogicNavMesh : BotLogicLocomotion
    {
        public float WalkSpeed { get; set; }
        public float WalkRotationSpeed { get; set; }

        public float RunSpeed { get; set; }
        public float RunRotationSpeed { get; set; }

        #region Components

        public NavMeshAgent NavMeshAgentComponent { get; private set; }
        public ProximityChecker JumpProximityChecker { get; private set; }

        #endregion Components

        public override float CurrentSpeed
        {
            get
            {
                return NavMeshHelpers.GetCurrentSpeed(NavMeshAgentComponent);
            }
        }

        public override void RotateTowardsPosition()
        {            
            // var rotation = Quaternion.LookRotation(BotLocomotiveComponent.FocusedOnPosition.Value - BotLocomotiveComponent.transform.position, Vector3.up);
            var lookTo = new Vector3(BotLocomotiveComponent.FocusedOnPosition.Value.x, BotLocomotiveComponent.transform.position.y, BotLocomotiveComponent.FocusedOnPosition.Value.z);
            var rotation = Quaternion.LookRotation(lookTo - BotLocomotiveComponent.transform.position, Vector3.up);
            BotLocomotiveComponent.transform.rotation = Quaternion.RotateTowards(BotLocomotiveComponent.transform.rotation, rotation, NavMeshAgentComponent.angularSpeed * Time.deltaTime);
        }

        public override void RotateAwayFromPosition()
        {
            var rotation = Quaternion.LookRotation(BotLocomotiveComponent.GetMoveAwayDestination() - BotLocomotiveComponent.transform.position, Vector3.up);
            BotLocomotiveComponent.transform.rotation = Quaternion.RotateTowards(BotLocomotiveComponent.transform.rotation, rotation, NavMeshAgentComponent.angularSpeed * Time.deltaTime);
        }

        private void MoveTo(Vector3 destination, float speed, float angularSpeed)
        {
            NavMeshAgentComponent.speed = speed;
            NavMeshAgentComponent.angularSpeed = angularSpeed;
            NavMeshAgentComponent.SetDestination(destination);
            /*
            NavMeshAgentComponent.stoppingDistance = 0f;
            switch (BotLocomotiveComponent.StopMovementCondition)
            {
                case BotLocomotive.StopMovementConditions.WITHIN_INTERACTION_DISTANCE:
                    NavMeshAgentComponent.stoppingDistance = BotLocomotiveComponent.SqrInteractionMagnitude * .75f;
                    break;
                case BotLocomotive.StopMovementConditions.WITHIN_PERSONAL_SPACE:
                    NavMeshAgentComponent.stoppingDistance = BotLocomotiveComponent.SqrPersonalSpaceMagnitude * .75f;
                    break;
            }
            */
            NavMeshAgentComponent.isStopped = false;
        }

        public override bool MoveTowardsPosition(bool fullspeed = true)
        {
            if (BotLocomotiveComponent.CurrentFState == BotLocomotive.FStatesLocomotion.MovingToPosition
                && BotLocomotiveComponent.IsWithinDistance(Bot.DistanceType.PERSONAL_SPACE)) // IsAtPosition())
            {
                return false;
            }
            if (fullspeed)
            {
                MoveTo(BotLocomotiveComponent.FocusedOnPosition.Value, RunSpeed, RunRotationSpeed);
            }
            else
            {
                MoveTo(BotLocomotiveComponent.FocusedOnPosition.Value, WalkSpeed, WalkRotationSpeed);
            }
            return true;
        }

        public override void MoveAway(bool fullspeed = true)
        {
            var destination = BotLocomotiveComponent.GetMoveAwayDestination();

            NavMeshHit _hit;
            if (NavMesh.SamplePosition(destination, out _hit, BotLocomotiveComponent.SqrAwarenessMagnitude, -1))
            {
                destination = _hit.position;
            }

            if (fullspeed)
            {
                MoveTo(destination, RunSpeed, RunRotationSpeed);
            }
            else
            {
                MoveTo(destination, WalkSpeed, WalkRotationSpeed);
            }
        }

        public void MoveToClosestEdge(bool fullspeed = true)
        {
            NavMeshHit hit;
            NavMeshAgentComponent.FindClosestEdge(out hit);
            BotLocomotiveComponent.UnFocus();
            BotLocomotiveComponent.FocusOnPosition(hit.position);
            MoveTowardsPosition(fullspeed);
            // BotLocomotiveComponent.MoveToPosition(BotLocomotive.StopMovementConditions.WITHIN_PERSONAL_SPACE, fullspeed);
            BotLocomotiveComponent.MoveToPosition(Bot.DistanceType.PERSONAL_SPACE, fullspeed);
        }

        public Vector3? JumpOffLedge(bool fullspeed = false)
        {
            RaycastHit rayhit;

            if (CanJumpDown(out rayhit))
            {
                BotLocomotiveComponent.UnFocus();
                BotLocomotiveComponent.FocusOnPosition(rayhit.point);
                // BotLocomotiveComponent.MoveToPosition(BotLocomotive.StopMovementConditions.AT_POSITION, fullspeed);
                BotLocomotiveComponent.MoveToPosition(Bot.DistanceType.AT_POSITION, fullspeed);
                return rayhit.point;
            }
            return null;
        }

        /// <summary>
        /// Use JumpProximityChecker to check if the bot is within jumping down distance
        /// </summary>
        public bool CanJumpDown(out RaycastHit rayhit, bool pathComplete = true)
        {
            if (JumpProximityChecker.IsWithinRayDistance(out rayhit) != null)
            {
                var jumpPath = new NavMeshPath();
                NavMesh.CalculatePath(BotLocomotiveComponent.transform.position, rayhit.point, NavMesh.AllAreas, jumpPath);
                if (pathComplete)
                {
                    return jumpPath.status == NavMeshPathStatus.PathComplete;
                }
                return jumpPath.status != NavMeshPathStatus.PathInvalid;
            }
            return false;
        }


        protected virtual void OffMeshLinkUpdate(NavMeshHelpers.OffMeshLinkPosition linkposition)
        {

        }

        private NavMeshHelpers.OffMeshLinkPosition m_linkposition;
        /// <summary>
        /// Update the current OffMeshLinkPosition
        /// </summary>
        private void CheckUpdateOffMeshLinkPosition()
        {
            m_linkposition = NavMeshAgentComponent.GetOffMeshLinkPosition();
            if (m_linkposition != NavMeshHelpers.OffMeshLinkPosition.Off)
            {
                OffMeshLinkUpdate(m_linkposition);
            }
        }

        private void CheckLinkArea()
        {

        }

        /// <summary>
        /// Check if Bot is able to move
        /// </summary>
        public override bool CanMove()
        {
            return (NavMeshAgentComponent.speed > 0
                && NavMeshAgentComponent.angularSpeed > 0
                && NavMeshAgentComponent.isActiveAndEnabled);
        }

        public bool IsAboveNavMeshSurface()
        {
            return NavMeshAgentComponent.IsAboveNavMeshSurface();
        }

        public bool IsAboveNavMeshSurface(out Vector3 navPosition)
        {
            return NavMeshAgentComponent.IsAboveNavMeshSurface(out navPosition);
        }


        #region States

        public override void OnStateChange(BotLocomotive.FStatesLocomotion locomotionState)
        {
            switch (locomotionState)
            {
                case BotLocomotive.FStatesLocomotion.NotMoving:
                    if (NavMeshAgentComponent.isOnNavMesh)
                    {
                        NavMeshAgentComponent.isStopped = true;
                    }
                    break;
            }
        }

        #endregion States

        private void Initialize(BotLocomotive botLocomotion, NavMeshAgent navMeshAgentComponent,
            ProximityChecker jumpProximityChecker,
            float walkSpeed = 0.75f, float walkRotationSpeed = 120f,
            float runSpeed = 5f, float runRotationSpeed = 120)
        {
            NavMeshAgentComponent = navMeshAgentComponent;
            JumpProximityChecker = jumpProximityChecker;
            WalkSpeed = walkSpeed;
            WalkRotationSpeed = walkRotationSpeed;
            RunSpeed = runSpeed;
            RunRotationSpeed = runRotationSpeed;

            NavMeshAgentComponent.speed = WalkSpeed;
            NavMeshAgentComponent.angularSpeed = WalkRotationSpeed;
        }

        public BotLogicNavMesh(BotLocomotive botLocomotion, NavMeshAgent navMeshAgentComponent,
            ProximityChecker jumpProximityChecker,
            float walkSpeed = 0.75f, float walkRotationSpeed = 120f,
            float runSpeed = 5f, float runRotationSpeed = 120f) : base(botLocomotion)
        {
            Initialize(botLocomotion, navMeshAgentComponent,
            jumpProximityChecker,
            walkSpeed, walkRotationSpeed, runSpeed, runRotationSpeed);
        }

        public BotLogicNavMesh(BotGround botGround,
            float walkSpeed = 0.75f, float walkRotationSpeed = 120f,
            float runSpeed = 5f, float runRotationSpeed = 120f) : base(botGround)
        {
            Initialize(botGround, botGround.NavMeshAgentComponent,
            botGround.JumpProximityChecker,
            walkSpeed, walkRotationSpeed, runSpeed, runRotationSpeed);
            botGround.BotLogicNavMeshRef = this;
        }

        public BotLogicNavMesh(BotFlyable botFlyable,
            float walkSpeed = 0.75f, float walkRotationSpeed = 120f,
            float runSpeed = 5f, float runRotationSpeed = 120f) : base(botFlyable)
        {
            Initialize(botFlyable, botFlyable.NavMeshAgentComponent,
            botFlyable.JumpProximityChecker,
            walkSpeed, walkRotationSpeed, runSpeed, runRotationSpeed);
            botFlyable.BotLogicNavMeshRef = this;
        }

        /*
        // To improve
        public override bool HasReachedDestination()
        {
            return (BotLocomotiveComponent.FocusedOnPosition != null && BotLocomotiveComponent.FocusedOnPosition == NavMeshAgentComponent.destination 
                    && ( NavMeshAgentComponent.path.status == NavMeshPathStatus.PathPartial //If Agent tried and failed to reach destination return FAILED
                            || NavMeshAgentComponent.path.status == NavMeshPathStatus.PathComplete)
                        ) || base.HasReachedDestination();
        }
        */
    }
}

