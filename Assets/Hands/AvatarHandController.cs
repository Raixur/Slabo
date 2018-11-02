using UnityEngine;
using System.Collections;

namespace VRTK
{
    /// <summary>
    /// Provides a custom controller hand model with psuedo finger functionality.
    /// </summary>
    /// <remarks>
    /// **Prefab Usage:**
    ///  * Place the `VRTK/Prefabs/AvatarHands/VRTK_BasicHand` prefab as a child of either the left or right script alias.
    ///  * If the prefab is being used in the left hand then check the `Mirror Model` parameter.
    ///  * By default, the avatar hand controller will detect which controller is connected and represent it accordingly.
    ///  * Optionally, use SDKTransformModify scripts to adjust the hand orientation based on different controller types.
    /// </remarks>
    /// <example>
    /// `032_Controller_CustomControllerModel` uses the `VRTK_BasicHand` prefab to display custom avatar hands for the left and right controller.
    /// </example>
    public class AvatarHandController : MonoBehaviour
    {
        [Header("Hand Settings")]

        [Tooltip("The controller type to use for default finger settings.")]
        public SDK_BaseController.ControllerType controllerType;
        [Tooltip("Determines whether the Finger and State settings are auto set based on the connected controller type.")]
        public bool setFingersForControllerType = true;
        [Tooltip("If this is checked then the model will be mirrored, tick this if the avatar hand is for the left hand controller.")]
        public bool mirrorModel = false;
        [Tooltip("The speed in which a finger will transition to it's destination position if the finger state is `Digital`.")]
        public float animationSnapSpeed = 0.1f;

        [Header("Digital Finger Settings")]
        [Tooltip("The button alias to control the thumb if the thumb state is `Digital`.")]
        public VRTK_ControllerEvents.ButtonAlias thumbButton = VRTK_ControllerEvents.ButtonAlias.TouchpadTouch;
        [Tooltip("The button alias to control the index finger if the index finger state is `Digital`.")]
        public VRTK_ControllerEvents.ButtonAlias indexButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
        [Tooltip("The button alias to control the middle, ring and pinky finger if the three finger state is `Digital`.")]
        public VRTK_ControllerEvents.ButtonAlias threeFingerButton = VRTK_ControllerEvents.ButtonAlias.GripPress;

        [Header("Axis Finger Settings")]
        [Tooltip("The button type to listen for axis changes to control the thumb.")]
        public SDK_BaseController.ButtonTypes thumbAxisButton = SDK_BaseController.ButtonTypes.Touchpad;
        [Tooltip("The button type to listen for axis changes to control the index finger.")]
        public SDK_BaseController.ButtonTypes indexAxisButton = SDK_BaseController.ButtonTypes.Trigger;
        [Tooltip("The button type to listen for axis changes to control the middle, ring and pinky finger.")]
        public SDK_BaseController.ButtonTypes threeFingerAxisButton = SDK_BaseController.ButtonTypes.Grip;

        [Header("Finger State Settings")]
        [Tooltip("The Axis Type to utilise when dealing with the thumb state. Not all controllers support all axis types on all of the available buttons.")]
        public VRTK_ControllerEvents.AxisType thumbState = VRTK_ControllerEvents.AxisType.Digital;
        public VRTK_ControllerEvents.AxisType indexState = VRTK_ControllerEvents.AxisType.Digital;
        public VRTK_ControllerEvents.AxisType middleState = VRTK_ControllerEvents.AxisType.Digital;
        public VRTK_ControllerEvents.AxisType ringState = VRTK_ControllerEvents.AxisType.Digital;
        public VRTK_ControllerEvents.AxisType pinkyState = VRTK_ControllerEvents.AxisType.Digital;
        public VRTK_ControllerEvents.AxisType threeFingerState = VRTK_ControllerEvents.AxisType.Digital;

        [Header("Custom Settings")]
        [Tooltip("The controller to listen for the events on. If this is left blank as it will be auto populated by finding the Controller Events script on the parent GameObject.")]
        public VRTK_ControllerEvents controllerEvents;

        #region Protected class variables
        protected Animator animator;

        // Index Finger Mapping: 0 = thumb, 1 = index, 2 = middle, 3 = ring, 4 = pinky
        protected bool[] fingerStates = new bool[5];
        protected bool[] fingerChangeStates = new bool[5];
        protected float[] fingerAxis = new float[5];
        protected float[] fingerUntouchedAxis = new float[5];
        protected float[] fingerSaveAxis = new float[5];
        protected float[] fingerForceAxis = new float[5];

        protected VRTK_ControllerEvents.AxisType[] axisTypes = new VRTK_ControllerEvents.AxisType[5];
        protected Coroutine[] fingerAnimationRoutine = new Coroutine[5];

        protected VRTK_ControllerEvents.ButtonAlias savedThumbButtonState = VRTK_ControllerEvents.ButtonAlias.Undefined;
        protected VRTK_ControllerEvents.ButtonAlias savedIndexButtonState = VRTK_ControllerEvents.ButtonAlias.Undefined;
        protected VRTK_ControllerEvents.ButtonAlias savedMiddleButtonState = VRTK_ControllerEvents.ButtonAlias.Undefined;
        protected VRTK_ControllerEvents.ButtonAlias savedRingButtonState = VRTK_ControllerEvents.ButtonAlias.Undefined;
        protected VRTK_ControllerEvents.ButtonAlias savedPinkyButtonState = VRTK_ControllerEvents.ButtonAlias.Undefined;
        protected VRTK_ControllerEvents.ButtonAlias savedThreeFingerButtonState = VRTK_ControllerEvents.ButtonAlias.Undefined;

        protected SDK_BaseController.ButtonTypes savedThumbAxisButtonState;
        protected SDK_BaseController.ButtonTypes savedIndexAxisButtonState;
        protected SDK_BaseController.ButtonTypes savedMiddleAxisButtonState;
        protected SDK_BaseController.ButtonTypes savedRingAxisButtonState;
        protected SDK_BaseController.ButtonTypes savedPinkyAxisButtonState;
        protected SDK_BaseController.ButtonTypes savedThreeFingerAxisButtonState;

        #endregion Protected class variables

        #region MonoBehaviour methods

        protected virtual void OnEnable()
        {
            animator = GetComponent<Animator>();
            controllerEvents = controllerEvents ?? GetComponentInParent<VRTK_ControllerEvents>();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeEvents();
            controllerType = SDK_BaseController.ControllerType.Undefined;
            for (int i = 0; i < fingerAnimationRoutine.Length; i++)
            {
                fingerAnimationRoutine[i] = null;
            }
        }

        protected virtual void Update()
        {
            if (controllerType == SDK_BaseController.ControllerType.Undefined)
            {
                DetectController();
            }

            if (animator != null)
            {
                ProcessFinger(thumbState, 0);
                ProcessFinger(indexState, 1);
                ProcessFinger(middleState, 2);
                ProcessFinger(ringState, 3);
                ProcessFinger(pinkyState, 4);
            }
        }
        #endregion MonoBehaviour methods

        #region Subscription Managers

        protected virtual void SubscribeEvents()
        {
            if (controllerEvents != null)
            {
                if (thumbState == VRTK_ControllerEvents.AxisType.Digital)
                    SubscribeButtonEvent(thumbButton, ref savedThumbButtonState, SetThumbEvent, ResetThumbEvent);
                else
                    SubscribeButtonAxisEvent(thumbAxisButton, ref savedThumbAxisButtonState, thumbState,
                        DoThumbAxisEvent);

                if (indexState == VRTK_ControllerEvents.AxisType.Digital)
                    SubscribeButtonEvent(indexButton, ref savedIndexButtonState, SetIndexEvent, ResetIndexEvent);
                else
                    SubscribeButtonAxisEvent(indexAxisButton, ref savedIndexAxisButtonState, indexState,
                        DoIndexAxisEvent);

                if (threeFingerState == VRTK_ControllerEvents.AxisType.Digital)
                    SubscribeButtonEvent(threeFingerButton, ref savedThreeFingerButtonState, SetThreeFingerEvent, ResetThreeFingerEvent);
                else
                    SubscribeButtonAxisEvent(threeFingerAxisButton, ref savedThreeFingerAxisButtonState, threeFingerState, DoThreeFingerAxisEvent);
            }
        }

        protected virtual void UnsubscribeEvents()
        {
            if (controllerEvents != null)
            {
                if (thumbState == VRTK_ControllerEvents.AxisType.Digital)
                    UnsubscribeButtonEvent(savedThumbButtonState, SetThumbEvent, ResetThumbEvent);
                else
                    UnsubscribeButtonAxisEvent(savedThumbAxisButtonState, thumbState, DoThumbAxisEvent);

                if (indexState == VRTK_ControllerEvents.AxisType.Digital)
                    UnsubscribeButtonEvent(savedIndexButtonState, SetIndexEvent, ResetIndexEvent);
                else
                    UnsubscribeButtonAxisEvent(savedIndexAxisButtonState, indexState, DoIndexAxisEvent);

                if (threeFingerState == VRTK_ControllerEvents.AxisType.Digital)
                    UnsubscribeButtonEvent(savedThreeFingerButtonState, SetThreeFingerEvent, ResetThreeFingerEvent);
                else
                    UnsubscribeButtonAxisEvent(savedThreeFingerAxisButtonState, threeFingerState, DoThreeFingerAxisEvent);
            }
        }

        protected virtual void SubscribeButtonEvent(VRTK_ControllerEvents.ButtonAlias buttonType, ref VRTK_ControllerEvents.ButtonAlias saveType,
            ControllerInteractionEventHandler setEventHandler, ControllerInteractionEventHandler resetEventHandler)
        {
            if (buttonType != VRTK_ControllerEvents.ButtonAlias.Undefined)
            {
                saveType = buttonType;
                controllerEvents.SubscribeToButtonAliasEvent(buttonType, true, setEventHandler);
                controllerEvents.SubscribeToButtonAliasEvent(buttonType, false, resetEventHandler);
            }
        }

        protected virtual void UnsubscribeButtonEvent(VRTK_ControllerEvents.ButtonAlias buttonType, ControllerInteractionEventHandler setEventHandler,
            ControllerInteractionEventHandler resetEventHandler)
        {
            if (buttonType != VRTK_ControllerEvents.ButtonAlias.Undefined)
            {
                controllerEvents.UnsubscribeToButtonAliasEvent(buttonType, true, setEventHandler);
                controllerEvents.UnsubscribeToButtonAliasEvent(buttonType, false, resetEventHandler);
            }
        }

        protected virtual void SubscribeButtonAxisEvent(SDK_BaseController.ButtonTypes buttonType, ref SDK_BaseController.ButtonTypes saveType, VRTK_ControllerEvents.AxisType axisType, ControllerInteractionEventHandler eventHandler)
        {
            saveType = buttonType;
            controllerEvents.SubscribeToAxisAliasEvent(buttonType, axisType, eventHandler);
        }

        protected virtual void UnsubscribeButtonAxisEvent(SDK_BaseController.ButtonTypes buttonType, VRTK_ControllerEvents.AxisType axisType, ControllerInteractionEventHandler eventHandler)
        {
            controllerEvents.UnsubscribeToAxisAliasEvent(buttonType, axisType, eventHandler);
        }

        #endregion Subscription Managers

        #region Digital button events

        protected virtual void SetThumbEvent(object sender, ControllerInteractionEventArgs e)
        {
            SetFingerEvent(0, 1);
        }

        protected virtual void ResetThumbEvent(object sender, ControllerInteractionEventArgs e)
        {
            SetFingerEvent(0, 0);
        }

        protected virtual void SetIndexEvent(object sender, ControllerInteractionEventArgs e)
        {
            SetFingerEvent(1, 1);
        }

        protected virtual void ResetIndexEvent(object sender, ControllerInteractionEventArgs e)
        {
            SetFingerEvent(1, 0);
        }

        protected virtual void SetThreeFingerEvent(object sender, ControllerInteractionEventArgs e)
        {
            SetFingerEvent(2, 1);
            SetFingerEvent(3, 1);
            SetFingerEvent(4, 1);
        }

        protected virtual void ResetThreeFingerEvent(object sender, ControllerInteractionEventArgs e)
        {
            SetFingerEvent(2, 0);
            SetFingerEvent(3, 0);
            SetFingerEvent(4, 0);
        }

        #endregion

        #region Axis button events

        protected virtual void DoThumbAxisEvent(object sender, ControllerInteractionEventArgs e)
        {
            SetFingerAxisEvent(0, e);
        }

        protected virtual void DoIndexAxisEvent(object sender, ControllerInteractionEventArgs e)
        {
            SetFingerAxisEvent(1, e);
        }

        protected virtual void DoThreeFingerAxisEvent(object sender, ControllerInteractionEventArgs e)
        {
            SetFingerAxisEvent(2, e);
            SetFingerAxisEvent(3, e);
            SetFingerAxisEvent(4, e);
        }

        #endregion


        protected virtual void SetFingerEvent(int fingerIndex, float buttonPressure)
        {
            fingerChangeStates[fingerIndex] = true;
            fingerStates[fingerIndex] = buttonPressure > 0f;
        }

        protected virtual void SetFingerAxisEvent(int fingerIndex, ControllerInteractionEventArgs e)
        {
            fingerAxis[fingerIndex] = e.buttonPressure;
        }

        protected virtual bool IsButtonPressed(int arrayIndex)
        {
            return fingerStates[arrayIndex];
        }

        protected virtual void SaveFingerAxis(int arrayIndex, float updateAxis)
        {
            fingerSaveAxis[arrayIndex] = (fingerSaveAxis[arrayIndex] != fingerForceAxis[arrayIndex] ? updateAxis : fingerSaveAxis[arrayIndex]);
        }

        protected virtual void DetectController()
        {
            controllerType = VRTK_DeviceFinder.GetCurrentControllerType();
            if (controllerType != SDK_BaseController.ControllerType.Undefined)
            {
                if (setFingersForControllerType)
                {
                    switch (controllerType)
                    {
                        case SDK_BaseController.ControllerType.SteamVR_ViveWand:
                            thumbState = VRTK_ControllerEvents.AxisType.Digital;
                            indexState = VRTK_ControllerEvents.AxisType.Axis;
                            middleState = VRTK_ControllerEvents.AxisType.Digital;
                            ringState = VRTK_ControllerEvents.AxisType.Digital;
                            pinkyState = VRTK_ControllerEvents.AxisType.Digital;
                            threeFingerState = VRTK_ControllerEvents.AxisType.Digital;
                            break;
                        case SDK_BaseController.ControllerType.Oculus_OculusTouch:
                        case SDK_BaseController.ControllerType.SteamVR_OculusTouch:
                            thumbState = VRTK_ControllerEvents.AxisType.Digital;
                            indexState = VRTK_ControllerEvents.AxisType.Axis;
                            middleState = VRTK_ControllerEvents.AxisType.Axis;
                            ringState = VRTK_ControllerEvents.AxisType.Axis;
                            pinkyState = VRTK_ControllerEvents.AxisType.Axis;
                            threeFingerState = VRTK_ControllerEvents.AxisType.Axis;
                            break;
                        default:
                            thumbState = VRTK_ControllerEvents.AxisType.Digital;
                            indexState = VRTK_ControllerEvents.AxisType.Digital;
                            middleState = VRTK_ControllerEvents.AxisType.Digital;
                            ringState = VRTK_ControllerEvents.AxisType.Digital;
                            pinkyState = VRTK_ControllerEvents.AxisType.Digital;
                            threeFingerState = VRTK_ControllerEvents.AxisType.Digital;
                            break;
                    }
                }
                UnsubscribeEvents();
                SubscribeEvents();
                if (mirrorModel)
                {
                    mirrorModel = false;
                    MirrorHand();
                }
            }
        }

        protected virtual void MirrorHand()
        {
            Transform modelTransform = transform.Find("Model");
            if (modelTransform != null)
            {
                modelTransform.localScale = new Vector3(modelTransform.localScale.x, modelTransform.localScale.y * -1f, modelTransform.localScale.z);
            }
        }

        protected virtual void ProcessFinger(VRTK_ControllerEvents.AxisType state, int arrayIndex)
        {
            axisTypes[arrayIndex] = state;
            if (state == VRTK_ControllerEvents.AxisType.Digital)
            {
                if (fingerChangeStates[arrayIndex])
                {
                    fingerChangeStates[arrayIndex] = false;
                    float startAxis = (fingerStates[arrayIndex] ? 0f : 1f);
                    float targetAxis = (fingerStates[arrayIndex] ? 1f : 0f);
                    LerpChangePosition(arrayIndex, startAxis, targetAxis, animationSnapSpeed);
                }
            }
            else
            {
                SetFingerPosition(arrayIndex, fingerAxis[arrayIndex]);
            }
        }

        protected virtual void LerpChangePosition(int arrayIndex, float startPosition, float targetPosition, float speed)
        {
            fingerAnimationRoutine[arrayIndex] = StartCoroutine(ChangePosition(arrayIndex, startPosition, targetPosition, speed));
        }

        /// <summary>
        /// Lerping finger position by time.
        /// </summary>
        /// <param name="arrayIndex"></param>
        /// <param name="startAxis"></param>
        /// <param name="targetAxis"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        protected virtual IEnumerator ChangePosition(int arrayIndex, float startAxis, float targetAxis, float time)
        {
            var elapsedTime = 0f;
            while (elapsedTime < time)
            {
                elapsedTime += Time.deltaTime;
                var currentAxis = Mathf.Lerp(startAxis, targetAxis, (elapsedTime / time));
                SetFingerPosition(arrayIndex, currentAxis);
                yield return null;
            }
            SetFingerPosition(arrayIndex, targetAxis);
            fingerAnimationRoutine[arrayIndex] = null;
        }

        protected virtual void SetFingerPosition(int arrayIndex, float axis)
        {
            var animationLayer = arrayIndex + 1;
            animator.SetLayerWeight(animationLayer, axis);
            fingerAxis[arrayIndex] = axis;
        }
    }
}
