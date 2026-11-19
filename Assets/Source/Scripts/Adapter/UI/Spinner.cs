using UnityEngine;

namespace LB
{
    /// <summary>

    /// </summary>
    public class Spinner : SpinnerBase<int>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Spinner"/> class.
        /// </summary>
        public Spinner()
        {
            ValueMax = 100;
            ValueStep = 1;
        }

        /// <summary>
        /// Increase value on step.
        /// </summary>
        public override void Plus()
        {
            if ((Value <= 0) || (int.MaxValue - Value) >= Step)
            {
                Value += Step;
            }
            else
            {
                Value = int.MaxValue;
            }
        }

        /// <summary>
        /// Decrease value on step.
        /// </summary>
        public override void Minus()
        {
            if ((Value >= 0) || (Mathf.Abs(int.MinValue - Value) >= Step))
            {
                Value -= Step;
            }
            else
            {
                Value = int.MinValue;
            }
        }



        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="newValue">New value.</param>
        protected override void SetValue(int newValue)
        {
            if (SpinnerValue == InBounds(newValue))
            {
                SetTextValue();

                return;
            }

            SpinnerValue = InBounds(newValue);

            SetTextValue();


        }



        /// <summary>
        /// Clamps a value between a minimum and maximum value.
        /// </summary>
        /// <returns>The bounds.</returns>
        /// <param name="value">Value.</param>
        protected override int InBounds(int value)
        {
            return Mathf.Clamp(value, ValueMin, ValueMax);
        }
    }
}