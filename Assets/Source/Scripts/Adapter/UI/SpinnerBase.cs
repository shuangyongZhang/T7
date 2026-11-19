using HT.Framework;
using DG.Tweening;
using System.Collections;
using TMPro;
using UIWidgets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LB
{
    /// <summary>
    /// Spinner base class.
    /// </summary>
    /// <typeparam name="T">Type of spinner value.</typeparam>
    public abstract class SpinnerBase<T> : HTBehaviour
    {
        //启用自动化
        protected override bool IsAutomate => false;

#pragma warning disable 0649
        [SerializeField]
        [FormerlySerializedAs("_text")]
        protected TMP_Text TextComponent;

        /// <summary>
        /// The min.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_min")]
        protected T ValueMin;

        /// <summary>
        /// The max.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_max")]
        protected T ValueMax;

        /// <summary>
        /// The step.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_step")]
        protected T ValueStep;

        /// <summary>
        /// The value.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_value")]
        protected T SpinnerValue;

        /// <summary>
        /// Allow changing value during hold.
        /// </summary>
        [SerializeField]
        public bool AllowHold = true;

        /// <summary>
        /// Delay of hold in seconds for permanent increase/decrease value.
        /// </summary>
        [SerializeField]
        public float HoldStartDelay = 0.5f;

        /// <summary>
        /// Delay of hold in seconds between increase/decrease value.
        /// </summary>
        [SerializeField]
        public float HoldChangeDelay = 0.1f;

        /// <summary>
        /// Gets or sets the minimum.
        /// </summary>
        /// <value>The minimum.</value>
        public T Min
        {
            get
            {
                return ValueMin;
            }

            set
            {
                ValueMin = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        /// <value>The maximum.</value>
        public T Max
        {
            get
            {
                return ValueMax;
            }

            set
            {
                ValueMax = value;
            }
        }

        /// <summary>
        /// Gets or sets the step.
        /// </summary>
        /// <value>The step.</value>
        public T Step
        {
            get
            {
                return ValueStep;
            }

            set
            {
                ValueStep = value;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value
        {
            get
            {
                return SpinnerValue;
            }

            set
            {
                SetValue(value);

            }
        }

        /// <summary>
        /// The plus button.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_plusButton")]
        protected ButtonAdvanced plusButton;

        /// <summary>
        /// The minus button.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_minusButton")]
        protected ButtonAdvanced minusButton;
        /// <summary>
        /// Use unscaled time.
        /// </summary>
        [SerializeField]
        public bool UnscaledTime;

        /// <summary>
        /// Gets or sets the plus button.
        /// </summary>
        /// <value>The plus button.</value>
        public ButtonAdvanced PlusButton
        {
            get
            {
                return plusButton;
            }

            set
            {
                if (plusButton != null)
                {
                    plusButton.onClick.RemoveListener(OnPlusClick);
                    plusButton.onPointerDown.RemoveListener(OnPlusButtonDown);
                    plusButton.onPointerUp.RemoveListener(OnPlusButtonUp);

                }

                plusButton = value;

                if (plusButton != null)
                {
                    plusButton.onClick.AddListener(OnPlusClick);
                    plusButton.onPointerDown.AddListener(OnPlusButtonDown);
                    plusButton.onPointerUp.AddListener(OnPlusButtonUp);
                }
            }
        }

        /// <summary>
        /// Gets or sets the minus button.
        /// </summary>
        /// <value>The minus button.</value>
        public ButtonAdvanced MinusButton
        {
            get
            {
                return minusButton;
            }

            set
            {
                if (minusButton != null)
                {
                    minusButton.onClick.RemoveListener(OnMinusClick);
                    minusButton.onPointerDown.RemoveListener(OnMinusButtonDown);
                    minusButton.onPointerUp.RemoveListener(OnMinusButtonUp);
                }

                minusButton = value;

                if (minusButton != null)
                {
                    minusButton.onClick.AddListener(OnMinusClick);
                    minusButton.onPointerDown.AddListener(OnMinusButtonDown);
                    minusButton.onPointerUp.AddListener(OnMinusButtonUp);
                }
            }
        }
        public UnityEvent<T> onValueChanged = new UnityEvent<T>();
        /// <summary>
        /// onPlusClick event.
        /// </summary>
        public UnityEvent onPlusClick = new UnityEvent();

        /// <summary>
        /// onMinusClick event.
        /// </summary>
        public UnityEvent onMinusClick = new UnityEvent();

        /// <summary>
        /// Increase value on step.
        /// </summary>
        public abstract void Plus();

        /// <summary>
        /// Decrease value on step.
        /// </summary>
        public abstract void Minus();

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="newValue">New value.</param>
        protected abstract void SetValue(T newValue);

        /// <summary>
        /// Start this instance.
        /// </summary>
        protected virtual void Start()
        {
            Init();
        }

        bool isInited;



        /// <summary>
        /// Init.
        /// </summary>
        public void Init()
        {
            if (isInited)
            {
                return;
            }

            isInited = true;

            PlusButton = plusButton;
            MinusButton = minusButton;
            Value = SpinnerValue;

            SetTextValue();
        }

        /// <summary>
        /// Set text value.
        /// </summary>
        protected virtual void SetTextValue()
        {
            TextComponent.text = SpinnerValue.ToString();
        }

        /// <summary>
        /// Hold Plus coroutine.
        /// </summary>
        /// <returns>IEnumerator.</returns>
        protected virtual IEnumerator HoldPlus()
        {
            if (AllowHold)
            {
                yield return UtilitiesTime.Wait(HoldStartDelay, UnscaledTime);
                while (AllowHold)
                {
                    OnPlusClick();
                    yield return UtilitiesTime.Wait(HoldChangeDelay, UnscaledTime);
                }
            }
        }

        /// <summary>
        /// Hold Minus coroutine.
        /// </summary>
        /// <returns>IEnumerator.</returns>
        protected virtual IEnumerator HoldMinus()
        {
            if (AllowHold)
            {
                yield return UtilitiesTime.Wait(HoldStartDelay, UnscaledTime);
                while (AllowHold)
                {
                    OnMinusClick();
                    yield return UtilitiesTime.Wait(HoldChangeDelay, UnscaledTime);
                }
            }
        }

        /// <summary>
        /// Process the minus click event.
        /// </summary>
        public void OnMinusClick()
        {
            Minus();
            onMinusClick.Invoke();
        }

        /// <summary>
        /// Process the plus click event.
        /// </summary>
        public void OnPlusClick()
        {
            Plus();
            onPlusClick.Invoke();
        }
        IEnumerator coroutine;

        /// <summary>
        /// Process the plus button down event.
        /// </summary>
        /// <param name="eventData">Current event data.</param>
        public void OnPlusButtonDown(PointerEventData eventData)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            coroutine = HoldPlus();
            StartCoroutine(coroutine);
        }

        /// <summary>
        /// Process the plus button up event.
        /// </summary>
        /// <param name="eventData">Current event data.</param>
        public void OnPlusButtonUp(PointerEventData eventData)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        /// <summary>
        /// Process the minus button down event.
        /// </summary>
        /// <param name="eventData">Current event data.</param>
        public void OnMinusButtonDown(PointerEventData eventData)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            coroutine = HoldMinus();
            StartCoroutine(coroutine);
        }

        /// <summary>
        /// Process the minus button up event.
        /// </summary>
        /// <param name="eventData">Current event data.</param>
        public void OnMinusButtonUp(PointerEventData eventData)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        /// <summary>
        /// Process the destroy event.
        /// </summary>
        protected virtual void onDestroy()
        {
            PlusButton = null;
            MinusButton = null;
        }

        /// <summary>
        /// Clamps a value between a minimum and maximum value.
        /// </summary>
        /// <returns>The bounds.</returns>
        /// <param name="value">Value.</param>
        protected abstract T InBounds(T value);


    }
}
