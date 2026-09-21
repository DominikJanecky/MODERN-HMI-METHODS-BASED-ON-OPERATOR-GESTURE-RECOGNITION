using Microsoft.MixedReality.Toolkit.UI;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

public class GestureActions : MonoBehaviour
{
    private string topicName = "/cmd_vel";
    [SerializeField]
    private RunPythonScript runPyThonScript;
    [SerializeField]
    private PinchSlider slider;
    [SerializeField]
    private TextMeshProUGUI gestureText;
    [SerializeField]
    private GameObject confirmIntensityButton;
    [SerializeField]
    private GameObject setMovementIntensityHeader;
    [SerializeField]
    private TextMeshPro movementIntensitySliderValue;
    [SerializeField]
    private GameObject startPositionVisual;
    [SerializeField]
    private GameObject emergencyStopVisual;
    [SerializeField]
    private GameObject movementIntensityVisual;
    [SerializeField]
    private GameObject moveForwardVisual;
    [SerializeField]
    private GameObject moveBackVisual;
    [SerializeField]
    private GameObject turnLeftVisual;
    [SerializeField]
    private GameObject turnRightVisual;
    [SerializeField]
    private GameObject scanningEnvironmentVisual;
    [SerializeField]
    private GameObject activationRoboticArmVisual;
    [SerializeField]
    private GameObject analysingSamplesVisual;
    [SerializeField]
    private GameObject transmitSamplesVisual;
    [SerializeField]
    private GameObject startFlightVisual;

    private List<GameObject> allVisuals;

    private ROSConnection ros;
    //public TextMeshPro ROSInfoText, ConnectionInfoText, LinearVelocityText, AngularVelocityText, ResponseText;
    private TwistMsg twistMessage;
    private bool isIntensityConfirmed = false;
    // Queue to store movement actions until intensity is confirmed
    private Queue<Action> movementActionsQueue = new Queue<Action>();
    private int maxMovementIntensity=10;
    private Animator animator;
    private bool canSetVelocityWithHand = false;

    private void Awake()
    {
        // Inicializ·cia kolekcie bez startPositionVisual, ktor˝ budeme spracov·vaù zvl·öù
        allVisuals = new List<GameObject>
        {
            startPositionVisual,
            emergencyStopVisual,
            movementIntensityVisual,
            moveForwardVisual,
            moveBackVisual,
            turnLeftVisual,
            turnRightVisual,
            scanningEnvironmentVisual,
            activationRoboticArmVisual,
            analysingSamplesVisual,
            transmitSamplesVisual,
            startFlightVisual
        };
    }

    private void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<StringMsg>("/unity_info");
        ros.RegisterPublisher<TwistMsg>(topicName);

        twistMessage = new TwistMsg();
        twistMessage.angular.x = 0;
        twistMessage.angular.y = 0;
        twistMessage.angular.z = 0;
        twistMessage.linear.x = 0;
        twistMessage.linear.y = 0;
        twistMessage.linear.z = 0;

        StartCoroutine(WaitForCharacter());
    }

    private void Update()
    {
        if (animator != null && canSetVelocityWithHand)
        {
            Transform rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            if (rightUpperArm != null)
            {
                float dot = Vector3.Dot(rightUpperArm.up.normalized, Vector3.up);
                float normalized = Mathf.InverseLerp(1f, -1f, dot);
                int intensity = Mathf.RoundToInt(normalized * 100f);
                slider.SliderValue = normalized;
                Debug.Log($"Intenzita zdvihu ruky: {intensity}%");
            }
        }
    }

    IEnumerator WaitForCharacter()
    {
        while (animator == null)
        {
            GameObject character = GameObject.Find("Newton");
            if (character != null)
            {
                animator = character.GetComponent<Animator>();
            }
            yield return null; // poËkaj 1 frame
        }

        Debug.Log("Animator n·jden˝!");
    }

    public void ChangeMovementIntensity()
    {
        movementIntensitySliderValue.text = "Movement Intensity: " + (slider.SliderValue * 100).ToString("F0") + "%";
    }

    public void UpdateGestureText(string gesture)
    {
        //gestureText.text = gesture;
        // Ensure this runs on the main thread
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            gestureText.text = gesture;
        });
    }

    public void StartPosition()
    {
        runPyThonScript.cantStartAnotherRecognize = false;
        ShowMovementIntensitySlider(false); 
        UpdateGestureText("Starting Position");
        Debug.Log("Starting position action executed");

        foreach (GameObject visual in allVisuals)
            visual.SetActive(false);

        startPositionVisual.SetActive(true);
    }

    public void EmergencyStop()
    {
        runPyThonScript.cantStartAnotherRecognize = false;
        ShowMovementIntensitySlider(false);
        UpdateGestureText("Emergency Stop");
        Debug.Log("Emergency stop action executed");

        foreach (GameObject visual in allVisuals)
            visual.SetActive(false);

        emergencyStopVisual.SetActive(true);
        StopMoving();
    }

    public void MovementIntensity()
    {
        UpdateGestureText("Movement Intensity");
        Debug.Log("Movement intensity action executed");

        foreach (GameObject visual in allVisuals)
            visual.SetActive(false);

        movementIntensityVisual.SetActive(true);
    }

    public void MoveForward()
    {
        foreach (GameObject visual in allVisuals)
            visual.SetActive(false);
        HandleMovementAction(() =>
        {
            ShowMovementIntensitySlider(false);
            UpdateGestureText("Move Forward");
            Debug.Log("Move forward action executed");

            foreach (GameObject visual in allVisuals)
                visual.SetActive(false);

            moveForwardVisual.SetActive(true);

            twistMessage.linear.x = maxMovementIntensity * (slider.SliderValue);
            twistMessage.angular.z = 0f;
            //ROSInfoText.SetText("Command: Move forward. Linear Velocity: " + (maxMovementIntensity * (slider.SliderValue / 100.0f)).ToString("F3") + " Angular Velocity: " + (maxMovementIntensity * (slider.SliderValue / 100.0f)).ToString("F3"));
            Debug.Log("Command: Move forward. Linear Velocity: " + (maxMovementIntensity * (slider.SliderValue / 100.0f)).ToString("F3") + " Angular Velocity: " + (maxMovementIntensity * (slider.SliderValue / 100.0f)).ToString("F3"));
            PublishMessage();
            isIntensityConfirmed = false;
        });

        //ShowMovementIntensitySlider(true);
        //Debug.Log("Move forward action executed");

        //foreach (GameObject visual in allVisuals)
        //    visual.SetActive(false);

        //moveForwardVisual.SetActive(true);
    }

    public void MoveBack()
    {
        foreach (GameObject visual in allVisuals)
            visual.SetActive(false);
        HandleMovementAction(() =>
        {
            ShowMovementIntensitySlider(false);
            UpdateGestureText("Move Backgward");
            Debug.Log("Move back action executed");

            foreach (GameObject visual in allVisuals)
                visual.SetActive(false);

            moveBackVisual.SetActive(true);


            twistMessage.linear.x = -(maxMovementIntensity * (slider.SliderValue));
            twistMessage.angular.z = 0f;
            //ROSInfoText.SetText("Command: Move backward. Linear Velocity: " + (maxMovementIntensity * (slider.SliderValue / 100.0f)).ToString("F3") + " Angular Velocity: " + (maxMovementIntensity * (slider.SliderValue / 100.0f)).ToString("F3"));
            Debug.Log("Command: Move backward. Linear Velocity: " + (maxMovementIntensity * (slider.SliderValue)).ToString("F3") + " Angular Velocity: " + (maxMovementIntensity * (slider.SliderValue)).ToString("F3"));
            PublishMessage();
            isIntensityConfirmed = false;
        });

        //ShowMovementIntensitySlider(true);
        //Debug.Log("Move back action executed");

        //foreach (GameObject visual in allVisuals)
        //    visual.SetActive(false);

        //moveBackVisual.SetActive(true);
    }

    public void TurnLeft()
    {
        foreach (GameObject visual in allVisuals)
            visual.SetActive(false);
        HandleMovementAction(() =>
        {
            ShowMovementIntensitySlider(false);
            UpdateGestureText("Turn Left");
            Debug.Log("Turn left action executed");

            foreach (GameObject visual in allVisuals)
                visual.SetActive(false);

            turnLeftVisual.SetActive(true);


            twistMessage.linear.x = 0f;
            twistMessage.angular.z = maxMovementIntensity * (slider.SliderValue);
            //ROSInfoText.SetText("Command: Turn left. Linear Velocity: " + (maxMovementIntensity * (slider.SliderValue / 100.0f)).ToString("F3") + " Angular Velocity: " + (maxMovementIntensity * (slider.SliderValue / 100.0f)).ToString("F3"));
            Debug.Log("Command: Turn left. Linear Velocity: " + (maxMovementIntensity * (slider.SliderValue)).ToString("F3") + " Angular Velocity: " + (maxMovementIntensity * (slider.SliderValue)).ToString("F3"));
            PublishMessage();
            isIntensityConfirmed = false;
        });

        //ShowMovementIntensitySlider(true);
        //Debug.Log("Turn left action executed");

        //foreach (GameObject visual in allVisuals)
        //    visual.SetActive(false);

        //turnLeftVisual.SetActive(true);
    }

    public void TurnRight()
    {
        foreach (GameObject visual in allVisuals)
            visual.SetActive(false);
        HandleMovementAction(() =>
        {
            ShowMovementIntensitySlider(false);
            UpdateGestureText("Turn Right");
            Debug.Log("Turn right action executed");

            foreach (GameObject visual in allVisuals)
                visual.SetActive(false);

            turnRightVisual.SetActive(true);


            twistMessage.linear.x = 0f;
            twistMessage.angular.z = -(maxMovementIntensity * (slider.SliderValue));
            //ROSInfoText.SetText("Command: Turn right. Linear Velocity: " + (maxMovementIntensity * (slider.SliderValue / 100.0f)).ToString("F3") + " Angular Velocity: " + (maxMovementIntensity * (slider.SliderValue / 100.0f)).ToString("F3"));
            Debug.Log("Command: Turn right. Linear Velocity: " + (maxMovementIntensity * (slider.SliderValue)).ToString("F3") + " Angular Velocity: " + (maxMovementIntensity * (slider.SliderValue)).ToString("F3"));
            PublishMessage();
            isIntensityConfirmed = false;
        });


        //ShowMovementIntensitySlider(true);
        //Debug.Log("Turn right action executed");

        //foreach (GameObject visual in allVisuals)
        //    visual.SetActive(false);

        //turnRightVisual.SetActive(true);
    }

    public void ScanningEnvironment()
    {
        UpdateGestureText("Scanning Environment");
        SendTextMessage("Scanning environment action executed");
        Debug.Log("Scanning environment action executed");

        foreach (GameObject visual in allVisuals)
            visual.SetActive(false);

        scanningEnvironmentVisual.SetActive(true);
    }

    public void ActivationOfRoboticArm()
    {
        ShowMovementIntensitySlider(false);
        UpdateGestureText("Activation of Robotic Ardm");
        SendTextMessage("Activation of robotic arm action executed");
        Debug.Log("Activation of robotic arm action executed");
        foreach (GameObject visual in allVisuals)
            visual.SetActive(false);

        activationRoboticArmVisual.SetActive(true);
    }

    public void AnalysingSamples()
    {
        ShowMovementIntensitySlider(false);
        //SendTextMessage("Analysing samples action executed");
        UpdateGestureText("Analysing of Samples");
        Debug.Log("Analysing samples action executed");

        foreach (GameObject visual in allVisuals)
            visual.SetActive(false);

        analysingSamplesVisual.SetActive(true);
    }

    public void TransmitSamples()
    {
        ShowMovementIntensitySlider(false);
        SendTextMessage("Transmit Samples action executed");
        UpdateGestureText("Transmit Samples");
        Debug.Log("Transmit samples action executed");

        foreach (GameObject visual in allVisuals)
            visual.SetActive(false);

        transmitSamplesVisual.SetActive(true);
    }

    public void StartFlight()
    {
        ShowMovementIntensitySlider(true);
        UpdateGestureText("Start Flight");
        Debug.Log("Start flight action executed");
        SendTextMessage("Start flight action executed");

        foreach (GameObject visual in allVisuals)
            visual.SetActive(false);

        startFlightVisual.SetActive(true);
    }
    private void ShowMovementIntensitySlider(bool value)
    {
        setMovementIntensityHeader.SetActive(value);
        slider.gameObject.SetActive(value);
        confirmIntensityButton.gameObject.SetActive(value);
    }

    private void SendTextMessage(string message)
    {
        runPyThonScript.cantStartAnotherRecognize = false;
        runPyThonScript.newSequenceIsStarted = false;
        StringMsg msg = new StringMsg(message);
        ros.Publish("/unity_info", msg);
        //ResponseText.SetText("Sent message to ROS: " + message);
    }
    void QueueMovementAction(Action movementAction)
    {
        // Add the movement action to the queue
        movementActionsQueue.Enqueue(movementAction);
        UnityEngine.Debug.Log("Movement action queued. Please confirm intensity.");
    }

    // Method to confirm intensity when button is clicked
    public void ConfirmIntensity()
    {
        isIntensityConfirmed = true;
        UnityEngine.Debug.Log("Intensity confirmed.");

        // Execute all queued movement actions
        while (movementActionsQueue.Count > 0)
        {
            Action action = movementActionsQueue.Dequeue();
            action.Invoke();  // Execute the movement action
            canSetVelocityWithHand = false;
        }
    }


    void HandleMovementAction(Action movementAction)
    {
        if (!isIntensityConfirmed) // Check if intensity is confirmed
        {
            canSetVelocityWithHand = true;
            ShowMovementIntensitySlider(true);
            UnityEngine.Debug.LogWarning("Please confirm intensity first.");
            QueueMovementAction(movementAction);
            //gestureActions.UpdateGestureText("Please confirm intensity before performing the action.");
            return; // Don't execute the movement action if intensity is not confirmed
        }

        movementAction.Invoke(); // If intensity is confirmed, execute the action
        
    }


    public void StopMoving()
    {
        twistMessage.linear.x = 0f;
        twistMessage.angular.z = 0f;
        //ROSInfoText.SetText("Command: Stop movement. Linear Velocity: " + 
            //(maxMovementIntensity*(slider.SliderValue/100.0f)).ToString("F3") + " Angular Velocity: " + (maxMovementIntensity * (slider.SliderValue / 100.0f)).ToString("F3"));
        //LinearVelocityText.SetText("Linear velocity: 0");
        //AngularVelocityText.SetText("Angular velocity: 0");
        slider.SliderValue = 0f;
        PublishMessage();
    }

    private void PublishMessage()
    {
        runPyThonScript.cantStartAnotherRecognize = false;
        runPyThonScript.newSequenceIsStarted = false;
        Debug.Log("PUBLISHMESSAGE:   "+ twistMessage);
        ros.Publish(topicName, twistMessage);
    }
}
