using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{

    [AddComponentMenu("Better UI/Controls/Better Button Toggle", 30)]
    public class BetterButtonToggle : Button, IBetterTransitionUiElement
    {
        public List<Transitions> BetterTransitions { get { return betterTransitions; } }
        public List<Transitions> BetterToggleTransitions { get { return betterToggleTransitions; } }
        public Toggle.ToggleEvent onValueChanged = new Toggle.ToggleEvent();
        public bool IsClick
        {
            get { return isClick; }
            set
            {
                Set(value);
            }
        }

        [SerializeField, DefaultTransitionStates]
        List<Transitions> betterTransitions = new List<Transitions>();

        [SerializeField, TransitionStates("On", "Off")]
        List<Transitions> betterToggleTransitions = new List<Transitions>();
        [SerializeField]
        private BetterButtonToggleGroup buttonGroup;
        [SerializeField]
        private bool isClick;
        /// <summary>
        /// Group the button belongs to.
        /// </summary>
        public BetterButtonToggleGroup group
        {
            get { return buttonGroup; }
            set
            {
                SetButtonGroup(value, true);
            }
        }


        protected override void OnEnable()
        {
            onValueChanged.AddListener(ValueChanged);
            SetButtonGroup(buttonGroup, false);
            base.OnEnable();
            ValueChanged(IsClick, true);
            DoStateTransition(SelectionState.Normal, true);
        }

        protected override void OnDisable()
        {
            onValueChanged.RemoveListener(ValueChanged);
            // SetButtonGroup(null, false);
            // DoStateTransition(SelectionState.Normal, true);
            base.OnDisable();
        }

        private void SetButtonGroup(BetterButtonToggleGroup newGroup, bool setMemberValue)
        {
            // Sometimes IsActive returns false in OnDisable so don't check for it.
            // Rather remove the button too often than too little.
            if (buttonGroup != null)
                buttonGroup.UnregisterButton(this);

            // At runtime the group variable should be set but not when calling this method from OnEnable or OnDisable.
            // That's why we use the setMemberValue parameter.
            if (setMemberValue)
                buttonGroup = newGroup;

            // Only register to the new group if this Button is active.
            if (newGroup != null && IsActive() && IsInteractable())
                newGroup.RegisterButton(this);

            // If we are in a new group, and this button is on, notify group.
            // Note: Don't refer to buttonGroup here as it's not guaranteed to have been set.
            if (newGroup != null && IsClick && IsActive() && IsInteractable())
                newGroup.NotifyButtonOn(this);
        }
        /// <summary>
        /// Set IsClick without invoking onValueChanged callback.
        /// </summary>
        /// <param name="value">New Value for IsClick.</param>
        public void SetIsClickWithoutNotify(bool value)
        {
            Set(value, false);
        }

        void Set(bool value, bool sendCallback = true)
        {
            if (isClick == value)
                return;

            // if we are in a group and set to true, do group logic
            isClick = value;
            if (buttonGroup != null && IsActive() && IsInteractable())
            {
                if (isClick || (!buttonGroup.AnyButtonsOn() && !buttonGroup.allowSwitchOff))
                {
                    isClick = true;
                    buttonGroup.NotifyButtonOn(this, sendCallback);
                }
            }

            onValueChanged.Invoke(isClick);
            if (sendCallback)
            {
                if (isClick) onClick.Invoke();
            }
        }

        private void ValueChanged(bool on)
        {
            ValueChanged(on, false);
        }

        private void ValueChanged(bool on, bool immediate)
        {
            foreach (var state in betterToggleTransitions)
            {
                state.SetState((on) ? "On" : "Off", immediate);
            }
        }

        private void InternalToggle()
        {
            if (!IsActive() || !IsInteractable())
                return;

            IsClick = !IsClick;
        }

        /// <summary>
        /// React to clicks.
        /// </summary>
        public override void OnPointerClick(PointerEventData eventData)
        {

            if (buttonGroup == null)
            {
                base.OnPointerClick(eventData);
                return;
            }
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            InternalToggle();


        }

        public override void OnSubmit(BaseEventData eventData)
        {
            if (buttonGroup == null)
            {
                base.OnSubmit(eventData);
                return;
            }
            InternalToggle();

        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (!(base.gameObject.activeInHierarchy))
                return;

            foreach (var info in betterTransitions)
            {
                if (state != SelectionState.Disabled && IsClick)
                {
                    var tglTr = betterToggleTransitions.FirstOrDefault(
                        (o) => o.TransitionStates != null && info.TransitionStates != null
                            && o.TransitionStates.Target == info.TransitionStates.Target
                            && o.Mode == info.Mode);

                    if (tglTr != null)
                    {
                        continue;
                    }
                }
                info.SetState(state.ToString(), instant);
            }
        }

    }
}
