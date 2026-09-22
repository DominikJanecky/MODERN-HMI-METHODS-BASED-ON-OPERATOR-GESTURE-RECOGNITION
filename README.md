# Modern HMI Methods Based on Operator Gesture Recognition

Source code developed for the dissertation **"Modern HMI Methods Based on Operator Gesture Recognition."**

This repository presents an experimental human–machine interface (HMI) pipeline in which an operator's movements are captured with Rokoko motion capture, classified by a neural-network model, and translated into commands for a Unity-based interface and ROS 2 robotic applications.

It combines motion-capture data collection and preprocessing, LSTM-based gesture recognition, a Unity / HoloLens-oriented interface, ROS 2 communication, and TurtleBot3-oriented robot-control examples.

> **Note:** This repository contains source code used during development and experimentation. Large training datasets and trained model files are not included.

## System overview

```text
Rokoko Studio motion data
          |
          v
Python data capture and preprocessing
          |
          v
LSTM gesture-recognition model
          |
          v
Recognised gesture printed by Python script
          |
          v
Unity application / HoloLens interface
          |
          v
ROS 2 topics, services and actions
          |
          v
Robot or simulation environment
```

## Repository structure

```text
.
├── Python/                 # Data collection, augmentation, training and recognition
├── C#/                     # Unity scripts, ROS message classes and Rokoko integration
└── ROS2/                   # ROS 2 packages, custom action and service interfaces
```

## Python

The `Python` directory contains the scripts used to create the gesture dataset, extend it through augmentation, train the neural-network model, and perform real-time recognition.

### `Rokoko_capture_to_array.py`

Captures body data received from **Rokoko Studio** through a UDP connection. It listens on `127.0.0.1:14043`, receives motion-capture frames in JSON format, extracts body-joint positions, keeps the first 69 coordinate values per frame, and saves each frame as a NumPy `.npy` file organised by gesture class and sequence.

The configured dataset contains 12 gesture classes: Starting position, Emergency stop, Movement intensity, Move forward, Move back, Turn left, Turn right, Scanning environment, Activation of robotic arm, Analysing samples, Transmit samples, and Start flight. Each recorded sequence contains 90 frames.

### `ExtendDatased.py`

Extends the captured dataset through simple data augmentation. It adds Gaussian noise to recorded joint-coordinate data and saves the generated sequences to a separate output directory. This increases variation in the training data and can improve model robustness.

### `LSTM_train.py`

The previous training approach, included to show an earlier stage of model development. It uses two standard LSTM layers with 128 and 64 units, dropout, and L2 regularisation to classify the 12 gestures from sequences of 90 frames × 69 values.

### `LSTM_train_bilstm.py`

A training script following the final (sixth) training configuration described in the article. It uses two bidirectional LSTM layers with 128 and 64 units, each followed by dropout of 0.5 and batch normalisation, then a Dense/ReLU layer and a 12-class softmax output. Training uses a stratified split, Adam, categorical cross-entropy, TensorBoard, `ReduceLROnPlateau`, and 50 epochs.

### `GestureRecognizer.py`

Performs real-time gesture recognition from live Rokoko Studio data. It receives UDP packets, extracts body coordinates, fills a buffer of 90 motion frames, loads a trained Keras `.h5` model, predicts the most likely gesture, and prints the result to standard output.

The Unity application can read this output and map the recognised gesture to robot-control or interface actions.

## C# / Unity

The `C#` directory contains Unity scripts used to integrate gesture recognition with a mixed-reality user interface and ROS communication.

### Main application scripts

#### `GestureActions.cs`

The main Unity gesture-action controller. It maps recognised gestures to visual interface feedback and robot-related actions. Movement commands are published as ROS `geometry_msgs/Twist` messages on the `/cmd_vel` topic.

Supported actions include start position, emergency stop, movement-intensity selection, forward and backward movement, left and right turns, environment scanning, robotic-arm activation, sample analysis, sample transmission, and flight start.

Before a movement command is applied, the user can set and confirm movement intensity using a Unity / MRTK slider.

#### `RunPythonScript.cs`

Starts the Python gesture-recognition process from Unity and reads its console output. When a line beginning with `Recognized gesture:` is received, it maps the recognised label to the relevant method in `GestureActions.cs`.

It also validates interaction: a sequence must begin with **Starting position**, non-emergency gestures must be recognised twice consecutively before execution, and emergency stop is executed immediately.

#### `UnityMainThreadDispatcher.cs`

Allows actions originating from background threads, such as Python process output handlers, to safely update Unity objects on the main Unity thread.

This file is based on the Apache-2.0-licensed UnityMainThreadDispatcher project by Pim de Witte.

#### `SetupRenderTexture.cs`

Assigns the `CharacterRenderTexture` resource to a Unity `RawImage` component.

#### `ButtonManagerTest.cs`

A minimal test component for a Unity button-click event.

### `RosMessages/`

Contains C# ROS message and service/action classes used by Unity.

- `CustomInterfaces` — generated classes for the custom patrol action and angle service;
- `ExampleInterfaces` — generated classes for the `GetWheels` service;
- `Turtlebot3` — message and service definitions used with TurtleBot3-related functionality.

### `Scripts/`

Contains the Rokoko Studio Unity integration used for receiving and visualising motion-capture data.

- `Core/` — UDP receiving, JSON serialisation, LZ4 handling and Rokoko Studio command API;
- `Mono/` — Unity components for actors, characters, props, face data, blend shapes and UI;
- `Plugins/LZ4/` — native LZ4 libraries for Windows, Android, macOS and iOS.

## ROS 2

The `ROS2` directory contains ROS 2 packages used for custom robot communication.

### `custom_interfaces`

Defines the custom `Patrol` action:

```text
# Goal
float32 radius

# Result
bool success

# Feedback
float32 time_left
```

The action allows a client to request circular robot motion with a selected radius and receive progress feedback.

### `patrol_action_server`

Implements a ROS 2 action server for the custom `Patrol` action. The server accepts a requested patrol radius, calculates the required angular velocity, publishes `geometry_msgs/Twist` commands to `cmd_vel`, drives the robot in a circular trajectory, publishes remaining-time feedback, and supports action cancellation.

The implementation is intended for TurtleBot3-compatible control through the `cmd_vel` topic.

The action definition and server are adapted from The Construct's ROS 2 patrol action-server tutorial, which credits ROBOTIS CO., LTD.

### `example_interfaces`

Defines the `GetWheels` service:

```text
bool request_both_angles
---
float32 left_wheel_angle
float32 right_wheel_angle
```

The service is intended to provide the current angles of the left and right wheel joints.

### `py_srvcli`

Contains Python examples of a ROS 2 service client and service server.

- `client_member_function.py` requests wheel-angle data through the `get_wheels` service.
- `service_member_function.py` subscribes to `/joint_states` and exposes wheel-angle values through the service.

## Requirements

The exact configuration depends on the target Unity project and ROS 2 environment. The development workflow uses:

- Python 3;
- NumPy;
- TensorFlow / Keras;
- scikit-learn;
- Rokoko Studio;
- Unity;
- Unity Robotics ROS–TCP Connector;
- Microsoft Mixed Reality Toolkit (MRTK);
- ROS 2;
- TurtleBot3-compatible robot or simulation environment.

## Configuration before use

Several scripts include machine-specific absolute paths. Update these values before running the project:

- `DATA_PATH` in `Rokoko_capture_to_array.py`, `LSTM_train.py`, and `LSTM_train_bilstm.py`;
- input and output paths in `ExtendDatased.py`;
- `model_path` in `GestureRecognizer.py`;
- `pythonInterpreterPath` and `pythonScriptPath` in `RunPythonScript.cs`.

The following assets are required but are not included in this repository:

- recorded motion-capture dataset;
- trained `.h5` gesture-recognition model;
- configured Unity scene and prefabs;
- ROS 2 workspace dependencies and a robot or simulator setup.

## Typical workflow

1. Start Rokoko Studio and enable the UDP data stream.
2. Run `Rokoko_capture_to_array.py` to record training sequences.
3. Optionally run `ExtendDatased.py` to augment the dataset.
4. Run `LSTM_train.py` for the earlier experiment or `LSTM_train_bilstm.py` for the article-based BiLSTM architecture.
5. Update `model_path` in `GestureRecognizer.py` to the model produced by the selected training script.
6. Configure the Unity scene and assign required references in the Inspector.
7. Configure ROS 2 packages and start the required ROS nodes.
8. Run the Unity application and start the Python recogniser from Unity.

## Disclaimer

This repository is primarily intended as an academic and experimental reference implementation. It may require adaptation before use in another hardware setup, Unity project, ROS 2 distribution, or production environment.

## License and third-party components

The project-specific scripts in `Python/` and the application scripts `C#/GestureActions.cs`, `C#/RunPythonScript.cs`, `C#/SetupRenderTexture.cs`, and `C#/ButtonManagerTest.cs` are provided under the [MIT License](LICENSE). This license applies to these files only; it does not relicense the third-party or generated components below.

- `C#/Scripts/` contains the Rokoko Studio Live Unity integration and bundled LZ4 libraries. These are third-party components, not original code by the repository author. Their applicable original terms must be respected; the precise provenance and redistribution terms of the bundled version should be checked before reuse.
- `C#/UnityMainThreadDispatcher.cs` is based on [UnityMainThreadDispatcher by Pim de Witte](https://github.com/PimDeWitte/UnityMainThreadDispatcher), which is licensed under Apache License 2.0.
- `C#/RosMessages/` contains classes generated by Unity-ROS MessageGeneration from ROS interface definitions; generation does not make these classes independently authored source code.
- `ROS2/custom_interfaces/` and `ROS2/patrol_action_server/` include an action definition and server adapted from [The Construct's ROS 2 patrol action-server tutorial](https://www.theconstruct.ai/ros2-how-to-2-create-a-ros2-action-server/), which credits ROBOTIS CO., LTD. and uses Apache License 2.0 for the example code.
- `ROS2/example_interfaces/` and `ROS2/py_srvcli/` contain ROS 2 scaffolding and project-specific adaptations. Their existing Apache License 2.0 files are retained.

The full Apache License 2.0 text is included in [`ROS2/example_interfaces/LICENSE`](ROS2/example_interfaces/LICENSE). Third-party copyright notices and license terms take precedence for the respective components. The MIT notice above should not be interpreted as a claim of authorship over those components.
