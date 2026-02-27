using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    /// <summary>
    ///     Responsible for allowing a spawned GameObjects position to be altered
    ///     during the experiment through one of the settings panels. Primary use case
    ///     is for adjusting the position of the drumming partner in the scene.
    /// </summary>
    public class PositionOffset : MonoBehaviour
    {

        [SerializeField] private GameObject objectContainer;  // parent object to the spawned objects
        [SerializeField] private Vector3 initialPosition;
        [SerializeField] private Vector3 initialFrontBackPosition;

        [SerializeField] private Slider leftRightSlider;
        [SerializeField] private Slider frontBackSlider;
        [SerializeField] private Slider upDownSlider;
        [SerializeField] private TextMeshProUGUI leftRightSliderLabel;
        [SerializeField] private TextMeshProUGUI frontBackSliderLabel;
        [SerializeField] private TextMeshProUGUI upDownSliderLabel;

        private float offsetInit = 0;

        public enum Axis
        {
            LeftRight,
            FrontBack,
            UpDown
        }

        /// <summary>
        ///     Sets initial values
        /// </summary>
        private void Awake()
        {
            
            initialPosition = objectContainer.transform.localPosition;
            offsetInit = initialFrontBackPosition.z;
            frontBackSlider.value = initialFrontBackPosition.z;
            OnOffset(offsetInit, Axis.FrontBack);
        }

        /// <summary>
        ///     Resets position to the default position
        /// </summary>
        public void Reset()
        {
            if (leftRightSlider != null)
                leftRightSlider.value = 0;
            if (frontBackSlider != null)
                frontBackSlider.value = 0;
            if (upDownSlider != null)
                upDownSlider.value = 0;
        }

        /// <summary>
        /// Used to offset the position
        /// </summary>
        /// <param name="offset">The amount to offset the object by</param>
        /// <param name="leftRightAxis">Offset in the left/right axis? If false,
        ///     will offset in the front/back axis</param>
        public void OnOffset(float offset, Axis axis)
        {
            // define values used for slider labels
            string positiveTextValue;
            string negativeTextValue;
            TextMeshProUGUI sliderText;

            // assign values based on left/right or front/back
            switch (axis)
            {
                case Axis.LeftRight:
                    positiveTextValue = "R+";
                    negativeTextValue = "L+";
                    sliderText = leftRightSliderLabel;
                    break;
                case Axis.FrontBack:
                    positiveTextValue = "B+";
                    negativeTextValue = "F+";
                    sliderText = frontBackSliderLabel;
                    break;
                default:
                    positiveTextValue = "U+";
                    negativeTextValue = "D+";
                    sliderText = upDownSliderLabel;
                    break;
            }
            // set slider text
            if (offset > 0)
            {
                sliderText.SetText(positiveTextValue + offset.ToString("F2"));
            }
            else if (offset < 0)
            {
                sliderText.SetText(negativeTextValue + Mathf.Abs(offset).ToString("F2"));

            }
            else
            {
                sliderText.SetText("0");
            }

            Vector3 newPosition = objectContainer.transform.localPosition; // get current position

            // apply offset to the appropriate axis
            switch (axis)
            {
                case Axis.LeftRight:
                    newPosition.x = initialPosition.x + offset;
                    break;
                case Axis.FrontBack:
                    newPosition.z = initialFrontBackPosition.z + offset;
                    break;
                default:
                    newPosition.y = initialPosition.y + offset;
                    break;
            }
            // update position in game
            objectContainer.transform.localPosition = newPosition;
        }

        /// <summary>
        ///     Used for offsetting the position of the object to the
        ///     left/right
        /// </summary>
        /// <param name = "offset" > Value by which to offset the position</param>
        public void OnLeftRightOffset(float offset)
        {
            OnOffset(offset, Axis.LeftRight);
        }

        /// <summary>
        ///     Used for offsetting the position of the object to the
        ///     front/back
        /// </summary>
        /// <param name = "offset" > Value by which to offset the position</param>
        public void OnFrontBackOffset(float offset)
        {
            OnOffset(offset, Axis.FrontBack);
        }

        /// <summary>
        ///    Used for offsetting the position of the object up/down
        /// </summary>
        /// <param name="offset">Value by which to offset the position</param>
        public void OnUpDownOffset(float offset)
        {
            OnOffset(offset, Axis.UpDown);
        }
    }
}