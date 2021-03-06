﻿namespace RSToolkit.UI.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using RSToolkit.Helpers;
    using UnityEngine.EventSystems;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect))]
    [RequireComponent(typeof(EventTrigger))]
    public class UISlideBackOnTimeout : MonoBehaviour
    {
        private bool m_paused = false;
        public float TimeoutSeconds = 10;
        DateTime timeOut = DateTime.Now;
        public float springSpeed = 0.5f;
        EventTrigger m_scrollRectEventTrigger;
        EventTrigger ScrollRectEventTrigger{
            get{
                if(m_scrollRectEventTrigger == null){
                    m_scrollRectEventTrigger = this.GetComponent<EventTrigger>();
                }
                return m_scrollRectEventTrigger;
            }
        }
        ScrollRect m_scrollRectComponent;

        ScrollRect ScrollRectComponent
        {
            get
            {
                if (m_scrollRectComponent == null)
                {
                    m_scrollRectComponent = this.GetComponent<ScrollRect>();
                }
                return m_scrollRectComponent;
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            var onDragEntry = new EventTrigger.Entry();
            onDragEntry.eventID = EventTriggerType.Drag;
            onDragEntry.callback.AddListener(onDrag);
            ScrollRectEventTrigger.triggers.Add(onDragEntry);
        }

        void onDrag(BaseEventData data){
            ResetTimeout();
        }
        public void ResetTimeout(){
            timeOut = DateTime.Now.AddSeconds(TimeoutSeconds);
        }
        public void Pause(){
            m_paused = true;
        }

        public void UnPause(bool resetTimeout = true){
            m_paused = false;
            if(resetTimeout){
                ResetTimeout();
            }
        }
        public bool IsPaused(){
            return m_paused;
        }
        // Update is called once per frame
        void Update()
        {
            if(m_paused){
                return;
            }
            
            if (!Input.GetMouseButton(0))
            {
                if( timeOut <= DateTime.Now &&
                    (int)Vector2.Distance(ScrollRectComponent.content.anchoredPosition, Vector2.zero) > 0){
                    ScrollRectComponent.content.anchoredPosition = Vector2.Lerp(ScrollRectComponent.content.anchoredPosition, Vector2.zero, Time.deltaTime * springSpeed);
                }
            }else{
                ResetTimeout();
            }
        }

    }
}