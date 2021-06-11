// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using UnityEngine;

namespace Assets.Scripts.Utilities.Preferences
{
    /// <summary>
    /// A slider that can be moved by grabbing / pinching a slider thumb, modified to only take the required integer values.
    /// </summary>
    public class NumColsSlider : PinchSlider
    {
        private const float minSliderVal = 1;
        private const float maxSliderVal = 4;

        [Range(minSliderVal, maxSliderVal)]
        [SerializeField]
        private int displaySliderValue = 3;
        public new int SliderValue
        {
            get { return displaySliderValue; }
            set
            {
                var oldSliderValue = displaySliderValue;
                displaySliderValue = value;
                base.SliderValue = value;
                UpdateUI();
                OnValueUpdated.Invoke(new SliderEventData(oldSliderValue, value, ActivePointer, this));
            }
        }

        protected override void UpdateUI()
        {
            float normalisedSliderValue = (displaySliderValue - minSliderVal) / (maxSliderVal - minSliderVal);
            var newSliderPos = SliderStartPosition + sliderThumbOffset + SliderTrackDirection * normalisedSliderValue;
            ThumbRoot.transform.position = newSliderPos;
        }

        public override void OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            if (eventData.Pointer == ActivePointer && !eventData.used)
            {
                var delta = ActivePointer.Position - StartPointerPosition;
                var handDelta = Vector3.Dot(SliderTrackDirection.normalized, delta);

                SliderValue = Convert.ToInt32(Mathf.Clamp(StartSliderValue + handDelta / SliderTrackDirection.magnitude, minSliderVal, maxSliderVal));

                // Mark the pointer data as used to prevent other behaviors from handling input events
                eventData.Use();
            }
        }
    }
}
