﻿using UnityEngine;
using RSToolkit.AI.Locomotion;
using UnityEngine.AI;

namespace RSToolkit.AI
{
    public class BotGround : BotLocomotive
    {
        public enum StatesGround{
            NOTGROUNDED,
            GROUNDED_NAVMESH_SUCCESS,
            GROUNDED_NAVMESH_FAIL
        }

        public StatesGround CurrentStatesGround {get; private set;} = StatesGround.NOTGROUNDED; // Assume it is starting in the air

        public BotLogicNavMesh BotLogicNavMeshRef { get; set; }

        NavMeshHit navGroundHit;

        #region Components

        private NavMeshAgent _navMeshAgentComponent;
        public NavMeshAgent NavMeshAgentComponent
        {
            get
            {
                if (_navMeshAgentComponent == null)
                {
                    _navMeshAgentComponent = GetComponent<NavMeshAgent>();
                }

                return _navMeshAgentComponent;
            }

        }

        protected BotPartWanderNavMesh BotWanderNavMeshComponent {get; private set;}
        #endregion Components

        private void HandleFailling()
        {
            if (_IsNetworkPeer)
            {
                return;
            }

            if (CurrentStatesGround != StatesGround.GROUNDED_NAVMESH_SUCCESS)
            {
                if(CheckForGround()){
                    Land();
                }else if(GroundProximityCheckerComponent.IsAlmostTouching(false)){
                    CurrentStatesGround = StatesGround.GROUNDED_NAVMESH_FAIL;
                }
            }else if(!CheckForGround() && !GroundProximityCheckerComponent.IsAlmostTouching(false)){

                CurrentStatesGround = StatesGround.NOTGROUNDED;
            }
        }

        private void Land(){
            // if(NavMesh.SamplePosition())
            // IsFreefall = false;

            NavMeshAgentComponent.enabled = true;
            if (!NavMeshAgentComponent.isOnNavMesh)
            {
                NavMeshAgentComponent.enabled = false;
                CurrentStatesGround = StatesGround.GROUNDED_NAVMESH_FAIL;
                
                return;
            }

            RigidBodyComponent.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            RigidBodyComponent.velocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            
            if(NavMeshAgentComponent.isActiveAndEnabled){
                CurrentStatesGround = StatesGround.GROUNDED_NAVMESH_SUCCESS;
            }else{
                CurrentStatesGround = StatesGround.GROUNDED_NAVMESH_FAIL;
            }
        }

        protected override void ToggleComponentsForNetwork(bool toggleKinematic = true)
        {
            base.ToggleComponentsForNetwork(toggleKinematic);

            if (_IsNetworkPeer)
            {
                NavMeshAgentComponent.enabled = false;
                if (CurrentStatesGround != StatesGround.NOTGROUNDED)
                {
                    // IsFreefall = false;
                    CurrentStatesGround = StatesGround.GROUNDED_NAVMESH_SUCCESS;
                }
            }
            else
            {
                HandleFailling();
            }

        }

        protected override void InitLocomotionTypes(){
            BotLogicNavMeshRef = new BotLogicNavMesh(this, NavMeshAgentComponent, JumpProximityChecker);
            CurrentLocomotionType = BotLogicNavMeshRef;
        }
        protected override bool InitBotWander(){
            if(!base.InitBotWander()){
                return false;
            }
            BotWanderNavMeshComponent = GetComponent<BotPartWanderNavMesh>();
            BotWanderManagerComponent.Initialize(BotWanderNavMeshComponent);
            return true;
        }

        #region MonoBehaviour Functions

        public override bool Initialize(bool force = false)
        {
            if (!base.Initialize(force))
            {
                return false;
            }
            HandleFailling();
            return true;
        }

        protected override void Awake()
        {
            base.Awake();
            
        }

        protected override void Update()
        {
            base.Update();
            // There must be a better way to do this
            HandleFailling();
        }
        protected override void OnCollisionEnter(Collision collision)
        {
            if (CurrentStatesGround != StatesGround.GROUNDED_NAVMESH_SUCCESS)
            {
                for (int i = 0; i < collision.contacts.Length; i++)
                {
                    HandleFailling();
                    if (CurrentStatesGround == StatesGround.GROUNDED_NAVMESH_SUCCESS){
                        break;
                    }
                }
            }
        }

        private bool CheckForGround(Vector3 point){
            return NavMesh.SamplePosition(point, out navGroundHit, GroundProximityCheckerComponent.IsAlmostTouchingDistance, NavMesh.AllAreas);
        }

        private bool CheckForGround(){
            return CheckForGround(transform.position);
        }

        #endregion MonoBehaviour Functions
    }
}
