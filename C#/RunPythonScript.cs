using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class RunPythonScript : MonoBehaviour
{
    [SerializeField]
    private GestureActions gestureActions;
    // Path to the Python interpreter
    public string pythonInterpreterPath = @"C:\Users\domin\AppData\Local\Programs\Python\Python311\python.exe";
    // Path to the Python script
    public string pythonScriptPath = @"C:\Users\domin\Downloads\DizertackaZDiplomovky\GestureRecognizer\test2.py";

    private Process pythonProcess;
    private Dictionary<string, Action> gestureActionsDict;
    private string lastRecognizedGesture = "";
    public bool newSequenceIsStarted = false;
    public bool cantStartAnotherRecognize = false;
    void Start()
    {
        InitializeGestureActions();
        //RunPythonScriptAndCaptureOutput();
    }

    void OnApplicationQuit()
    {
        StopPythonProcess();
    }

    void OnDisable()
    {
        StopPythonProcess();
    }

    void InitializeGestureActions()
    {
        gestureActionsDict = new Dictionary<string, Action>
        {
            { "Starting position", () => gestureActions.StartPosition() },
            { "Emergency stop", () => gestureActions.EmergencyStop() },
            //{ "Movement intensity", () => gestureActions.MovementIntensity() },
            { "Move forward", () => gestureActions.MoveForward() },
            { "Move back", () => gestureActions.MoveBack() },
            { "Turn left", () => gestureActions.TurnLeft() },
            { "Turn right", () => gestureActions.TurnRight() },
            { "Scanning environment", () => gestureActions.ScanningEnvironment() },
            { "Activation of robotic arm", () => gestureActions.ActivationOfRoboticArm() },
            { "Analysing samples", () => gestureActions.AnalysingSamples() },
            { "Transmit samples", () => gestureActions.TransmitSamples() },
            { "Start flight", () =>  gestureActions.StartFlight() }
        };
    }

    public async void RunPythonScriptAndCaptureOutput()
    {
         UnityEngine.Debug.Log("tu");
        ProcessStartInfo start = new ProcessStartInfo
        {
            FileName = pythonInterpreterPath,
            Arguments = $"\"{pythonScriptPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        pythonProcess = new Process
        {
            StartInfo = start,
            EnableRaisingEvents = true
        };

        pythonProcess.OutputDataReceived += new DataReceivedEventHandler((sender, e) => {
            if (!string.IsNullOrEmpty(e.Data))
            {
                // Filter and only log recognized gestures
                if (e.Data.StartsWith("Recognized gesture:"))
                {
                    string recognizedGesture = e.Data.Replace("Recognized gesture:", "").Trim();
                    UnityEngine.Debug.Log(recognizedGesture);
                    gestureActions.UpdateGestureText(recognizedGesture);
                    ExecuteGestureAction(recognizedGesture);
                }
            }
        });
        pythonProcess.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => {
            if (!string.IsNullOrEmpty(e.Data))
            {
                UnityEngine.Debug.LogError(e.Data);
            }
        });

        pythonProcess.Start();
        pythonProcess.BeginOutputReadLine();
        pythonProcess.BeginErrorReadLine();

        await Task.Run(() => pythonProcess.WaitForExit());
    }

    void ExecuteGestureAction(string gesture)
    {
        
        if (cantStartAnotherRecognize) return;
        if (gesture == "Emergency stop")
        {
            newSequenceIsStarted = false;
            UnityMainThreadDispatcher.Instance().Enqueue(() => gestureActionsDict[gesture].Invoke());
            return;
        }
        if (!newSequenceIsStarted)
        {
            if (gesture != "Starting position")
            {
                gestureActions.UpdateGestureText(gesture + "   Please start with 'Starting position'");
                return;
            }
            UnityMainThreadDispatcher.Instance().Enqueue(() => gestureActionsDict[gesture].Invoke());
            newSequenceIsStarted = true;
            return;
        }

        if (gesture == "Starting position")
        {
            return;
        }

        if (lastRecognizedGesture != gesture)
        {
            lastRecognizedGesture = gesture;
            gestureActions.UpdateGestureText(gesture + "  Please repeat the gesture twice.");
            return;
        }

        // Ak všetko sedí, vykonaj akciu
        if (gestureActionsDict.ContainsKey(gesture))
        {
            cantStartAnotherRecognize = true;
            newSequenceIsStarted = false;
            UnityMainThreadDispatcher.Instance().Enqueue(() => gestureActionsDict[gesture].Invoke());
        }
        else
        {
            newSequenceIsStarted = false;
            UnityEngine.Debug.LogWarning($"No action defined for gesture: {gesture}");
        }

        // Posuò históriu gest
        lastRecognizedGesture = gesture;
        //newSequenceIsStarted = false;
    }

    public void StopPythonProcess()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            pythonProcess.Kill();
            pythonProcess.Dispose();
            pythonProcess = null;
        }
    }

}