using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


namespace TheraBytes.BetterUi
{

    [RequireComponent(typeof(BetterSlider))]
    public class BetterSliderLabel : MonoBehaviour
    {


        [SerializeField]
        private TMP_Text Label;

        private BetterSlider slider;


        protected void Start()
        {
            slider = GetComponent<BetterSlider>();

            if (slider != null)
            {
                slider.onValueChanged.AddListener(ValueChanged);
                ValueChanged(slider.value);
            }


        }

        private void ValueChanged(float value)
        {
            Label.text = value.ToString();
            // if (slider.wholeNumbers) Label.text = value.ToString();
            // else Label.text = value.ToString("0.0");
        }
    }
}
