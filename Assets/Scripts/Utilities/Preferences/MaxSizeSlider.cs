// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using UnityEngine;

namespace Assets.Scripts.Utilities.Preferences
{
    /// <summary>
    /// A slider that can be moved by grabbing / pinching a slider thumb, modified to only take the required float values.
    /// </summary>
    public class MaxSizeSlider : PinchSlider
    {
        private const float minSliderVal = 0.5f;
        private const float maxSliderVal = 1.5f;

        [Range(minSliderVal, maxSliderVal)]
        [SerializeField]
        private float displaySliderValue = 1;
        public new float SliderValue
        {
            get { return displaySliderValue; }
            set
            {
                var oldSliderValue = displaySliderValue;
                displaySliderValue = value;
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

                SliderValue = Mathf.Clamp(StartSliderValue + handDelta / SliderTrackDirection.magnitude, minSliderVal, maxSliderVal);

                // Mark the pointer data as used to prevent other behaviors from handling input events
                eventData.Use();
            }
        }
    }
}
