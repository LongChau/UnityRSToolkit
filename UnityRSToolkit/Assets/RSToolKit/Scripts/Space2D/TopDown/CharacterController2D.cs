﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RSToolkit.Character;

namespace RSToolkit.Space2D.TopDown
{
    public class CharacterController2D : RSCharacterController
    {

        public Rigidbody2D Rigidbody2DComponent{get; private set;}
        protected override void InitRigidBody(){
            Rigidbody2DComponent = GetComponent<Rigidbody2D>();
        }
        // Start is called before the first frame update
        void Start()
        {
            
        }

        public override void Move(Vector2 directionAxis, float speed)
        {
            base.Move(directionAxis, speed);
            Rigidbody2DComponent.MovePosition(Rigidbody2DComponent.position + directionAxis * CurrentSpeed);
        }
    }
}
