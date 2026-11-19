using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class GestureTrackerTap : MonoBehaviour
    {
        public const int TAP_COUNT_LAUNCH = 4;

        public const float TAPS_DELTA = 0.5f;

        private int _tapCount;

        private float _prevTapTimestemp;

        public Action OnTapsTracked = delegate{}; 

        private Button _button;

        // private void Start()
        // {
        //     _button = GetComponent<Button>();
        // }

void ButtonCount(){
    if (_tapCount == TAP_COUNT_LAUNCH)
    {
        OnTapsTracked();
        _tapCount = 0;
    }
}


        private void Update1()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _tapCount++;
                _prevTapTimestemp = Time.time;
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (Time.time - _prevTapTimestemp < TAPS_DELTA)
                {
                    ButtonCount();
                }
                else
                {
                    _tapCount = 0;
                }
            }
        }
    

        private void Update()
        {
            if (Input.touches.Length > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Ended && touch.position.y < Screen.height * 0.1f
                && touch.position.x<Screen.width*0.1f)
                {
                    float timeSincePreviousClick = Time.realtimeSinceStartup - _prevTapTimestemp;        
                    if (timeSincePreviousClick < TAPS_DELTA)
                    {
                        _tapCount++;
                        if (_tapCount == TAP_COUNT_LAUNCH)
                        {
                            OnTapsTracked();
                        }
                    }
                    else
                    {
                        _tapCount = 1;
                    }
                    _prevTapTimestemp = Time.realtimeSinceStartup;
                }
            }

        }
    }
}