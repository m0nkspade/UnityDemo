# UnityDemo

A small Unity app that shows a live camera feed and labels the objects it sees.
Written on a MacBook Air and driven entirely from the shell, no Unity editor window.

## What it does

- Opens the best available camera and shows the feed. Dedicated cameras (Nikon, capture
  cards) are preferred, then an iPhone over Continuity Camera, then the built-in webcam.
  I use my iPhone for this: plug it in or set it nearby and the app picks it up.
- Runs YOLO11n on the frames through Unity's Inference Engine. Only detections at 80 percent
  confidence or higher are drawn, each as a box captioned "I am a <object>".
- Buttons for start/stop, next camera, auto select, and refresh.

## Building and running

Unity 6.3 (6000.3.26f1), installed through Unity Hub. From the project folder:

```
UNITY=/Applications/Unity/Hub/Editor/6000.3.26f1/Unity.app/Contents/MacOS/Unity

# generate the scene (once, or after changing CreateScene.cs)
$UNITY -batchmode -nographics -quit -projectPath . -executeMethod CreateScene.Run -logFile Logs/scene.log

# build the macOS app
$UNITY -batchmode -nographics -quit -projectPath . -buildOSXUniversalPlayer "$PWD/Builds/MyCameraApp.app" -logFile Logs/build.log

open Builds/MyCameraApp.app
```

The player log is at `~/Library/Logs/DefaultCompany/MyUnityProject/Player.log`.

## Layout

```
Assets/Scripts/CameraApp.cs        camera lifecycle, UI, drawing the boxes
Assets/Scripts/CameraPriority.cs   ranks camera names (pure C#)
Assets/Scripts/ObjectDetector.cs   YOLO inference and decoding
Assets/Scripts/BoxMath.cs          overlap and non-maximum suppression (pure C#)
Assets/Editor/CreateScene.cs       writes the scene from the shell
Assets/Resources/Models/           yolo11n.onnx and the COCO class list
```

## Model

YOLO11n from Unity's [inference-engine-yolo](https://huggingface.co/unity/inference-engine-yolo)
repository. The weights are published by Ultralytics under the AGPL-3.0 licence.
