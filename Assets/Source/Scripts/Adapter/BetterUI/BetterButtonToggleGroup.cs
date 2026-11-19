using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

namespace TheraBytes.BetterUi
{
    [AddComponentMenu("Better UI/Controls/Better Button Toggle Group", 30)]
    [DisallowMultipleComponent]
    /// <summary>
    /// A component that represents a group of BetterUI.Buttons.
    /// </summary>
    /// <remarks>
    /// When using a group reference the group from a BetterButtonToggle. Only one member of a group can be active at a time.
    /// </remarks>
    public class BetterButtonToggleGroup : UIBehaviour
    {
        [SerializeField] private bool m_AllowSwitchOff = false;

        /// <summary>
        /// Is it allowed that no button is switched on?
        /// </summary>
        /// <remarks>
        /// If this setting is enabled, pressing the button that is currently switched on will switch it off, so that no button is switched on. If this setting is disabled, pressing the button that is currently switched on will not change its state.
        /// Note that even if allowSwitchOff is false, the Better Button Group will not enforce its constraint right away if no buttons in the group are switched on when the scene is loaded or when the group is instantiated. It will only prevent the user from switching a button off.
        /// </remarks>
        public bool allowSwitchOff { get { return m_AllowSwitchOff; } set { m_AllowSwitchOff = value; } }

        protected List<BetterButtonToggle> m_Buttons = new List<BetterButtonToggle>();

        protected BetterButtonToggleGroup()
        { }

        /// <summary>
        /// Because all the Buttons have registered themselves in the OnEnabled, Start should check to
        /// make sure at least one BetterButtonToggle is active in groups that do not AllowSwitchOff
        /// </summary>
        protected override void Start()
        {
            // EnsureValidState();
            base.Start();
        }
        protected override void OnDisable()
        {
            // SetAllButtonsOff(false);
            base.OnDisable();
        }
        private void ValidateButtonIsInGroup(BetterButtonToggle button)
        {
            if (button == null || !m_Buttons.Contains(button))
                throw new ArgumentException(string.Format("BetterButtonToggle {0} is not part of BetterButtonToggleGroup {1}", new object[] { button, this }));
        }

        /// <summary>
        /// Notify the group that the given button is enabled.
        /// </summary>
        /// <param name="button">The button that got triggered on.</param>
        /// <param name="sendCallback">If other buttons should send onValueChanged.</param>
        public void NotifyButtonOn(BetterButtonToggle button, bool sendCallback = true)
        {
            ValidateButtonIsInGroup(button);
            // disable all buttons in the group
            for (var i = 0; i < m_Buttons.Count; i++)
            {
                if (m_Buttons[i] == button)
                {
                    continue;
                }


                if (sendCallback)
                    m_Buttons[i].IsClick = false;
                else
                    m_Buttons[i].SetIsClickWithoutNotify(false);
            }
            if (sendCallback)
                button.IsClick = true;
            else
                button.SetIsClickWithoutNotify(true);
        }

        /// <summary>
        /// Unregister a button from the group.
        /// </summary>
        /// <param name="button">The button to remove.</param>
        public void UnregisterButton(BetterButtonToggle button)
        {
            if (m_Buttons.Contains(button))
                m_Buttons.Remove(button);
        }

        /// <summary>
        /// Register a button with the button group so it is watched for changes and notified if another button in the group changes.
        /// </summary>
        /// <param name="button">The button to register with the group.</param>
        public void RegisterButton(BetterButtonToggle button)
        {

            if (!m_Buttons.Contains(button))
                m_Buttons.Add(button);
        }

        /// <summary>
        /// Ensure that the button group still has a valid state. This is only relevant when a BetterButtonToggleGroup is Started
        /// or a BetterButtonToggle has been deleted from the group.
        /// </summary>
        public void EnsureValidState()
        {
            if (!allowSwitchOff && !AnyButtonsOn() && m_Buttons.Count != 0)
            {
                m_Buttons[0].IsClick = true;
                NotifyButtonOn(m_Buttons[0]);
            }
        }

        /// <summary>
        /// Are any of the buttons on?
        /// </summary>
        /// <returns>Are and of the buttons on?</returns>
        public bool AnyButtonsOn()
        {
            return m_Buttons.Find(x => x.IsClick) != null;
        }

        /// <summary>
        /// Returns the buttons in this group that are active.
        /// </summary>
        /// <returns>The active buttons in the group.</returns>
        /// <remarks>
        /// Buttons belonging to this group but are not active either because their GameObject is inactive or because the BetterButtonToggle component is disabled, are not returned as part of the list.
        /// </remarks>
        public IEnumerable<BetterButtonToggle> ActiveButtons()
        {
            return m_Buttons.Where(x => x.IsClick);
        }

        /// <summary>
        /// Switch all buttons off.
        /// </summary>
        /// <remarks>
        /// This method can be used to switch all buttons off, regardless of whether the allowSwitchOff property is enabled or not.
        /// </remarks>
        public void SetAllButtonsOff(bool sendCallback = true)
        {
            bool oldAllowSwitchOff = m_AllowSwitchOff;
            m_AllowSwitchOff = true;

            if (sendCallback)
            {
                for (var i = 0; i < m_Buttons.Count; i++)
                    m_Buttons[i].IsClick = false;
            }
            else
            {
                for (var i = 0; i < m_Buttons.Count; i++)
                    m_Buttons[i].SetIsClickWithoutNotify(false);
            }

            m_AllowSwitchOff = oldAllowSwitchOff;
        }

    }
}
