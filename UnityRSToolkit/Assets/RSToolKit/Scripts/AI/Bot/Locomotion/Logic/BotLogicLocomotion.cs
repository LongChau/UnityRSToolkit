using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RSToolkit.Animation;
using RSToolkit.Space3D;
using RSToolkit.Helpers;
using RSToolkit.AI.FSM;

namespace RSToolkit.AI.Locomotion
{

    public abstract class BotLogicLocomotion
    {
        public BotLocomotive BotLocomotiveComponent { get; private set;}
        
        public abstract float CurrentSpeed { get; }

        public abstract bool MoveTowardsPosition(bool fullspeed = true);

        public abstract void MoveAway(bool fullspeed = true);

        public abstract void RotateTowardsPosition();

        public abstract void RotateAwayFromPosition();

        public virtual bool CanMove()
        {
            return true;
        }

        public abstract void OnStateChange(BotLocomotive.FStatesLocomotion locomotionState);

        public BotLogicLocomotion(BotLocomotive botLocomotion)
        {
            BotLocomotiveComponent = botLocomotion;
        }

        public bool HasReachedDestinationApprox(Bot.DistanceType StopMovementCondition )
        {
            return (BotLocomotiveComponent.StopMovementCondition == StopMovementCondition
                    && BotLocomotiveComponent.IsWithinDistance(StopMovementCondition, ProximityHelpers.DistanceDirection.VERTICAL)
                    && BotLocomotiveComponent.IsWithinDistance(StopMovementCondition, ProximityHelpers.DistanceDirection.HORIZONTAL)
                    );
        }

        public bool HasReachedDestination(Bot.DistanceType StopMovementCondition)
        {
            return (BotLocomotiveComponent.StopMovementCondition == StopMovementCondition
                    && BotLocomotiveComponent.IsWithinDistance(StopMovementCondition));
        }

    }
}