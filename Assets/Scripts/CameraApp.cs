using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraApp: MonoBehaviour
{
    public WebCamTexture cam;
    public Texture btnTexture;
    public ObjectDetector _objectDetector;
    private IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("WebCam authorization denied");
            yield break;
        }

        var cams = ListCameras();
        int bestIndex = CameraPriority.SelectBest(cams);
        Debug.Log("Best camera index at start: " + bestIndex);
        if (bestIndex != -1)
            cam = new WebCamTexture(cams[bestIndex], 1920, 1080, 30);
        else
            cam = new WebCamTexture(1920, 1080, 30);
        
        cam.Play();
        Debug.Log("WebCam started: " + cam.deviceName);
    }
    private void Awake()
    {
        _objectDetector = GetComponent<ObjectDetector>();
    }

    private Rect VideoRect(Rect picture, WebCamTexture cam)
    {
        var camAspect = cam.width / (float)cam.height;
        float width = picture.width;
        float height = width / camAspect;

        if (height > picture.height)
        {
            height = picture.height;
            width = height * camAspect;
        }

        float x = (picture.width - width) / 2f;
        float y = (picture.height - height) / 2f;
        return new Rect(x, y, width, height);
    }

    private void OnGUI()
    {
        Rect picture = new Rect(0, 0, Screen.width, Screen.height);
        if (cam != null && cam.width > 16 && cam.isPlaying)
        {
            GUI.DrawTexture(picture, cam, ScaleMode.ScaleToFit);
            var videoRect = VideoRect(picture, cam);
            foreach (var detection in _objectDetector.Detections)
            {
                // Draw detection bounding boxes or other visualizations here
                Rect box = new Rect(
                    videoRect.x + detection.rect.x * videoRect.width,
                    videoRect.y + detection.rect.y * videoRect.height,
                    detection.rect.width * videoRect.width,
                    detection.rect.height * videoRect.height
                );

                const float thickness = 4f;
                GUI.color = Color.green;
                GUI.DrawTexture(new Rect(box.xMin, box.yMin, box.width, thickness), Texture2D.whiteTexture);   // top
                GUI.DrawTexture(new Rect(box.xMin, box.yMax - thickness, box.width, thickness), Texture2D.whiteTexture); // bottom
                GUI.DrawTexture(new Rect(box.xMin, box.yMin, thickness, box.height), Texture2D.whiteTexture);  // left
                GUI.DrawTexture(new Rect(box.xMax - thickness, box.yMin, thickness, box.height), Texture2D.whiteTexture); // right
                GUI.color = Color.white;

                GUIStyle caption = new GUIStyle(GUI.skin.label) { fontSize = 28 };
                caption.normal.textColor = Color.green;
                GUI.Label(new Rect(box.xMin, box.yMin - 36f, 600f, 36f), "I am a " + detection.label, caption);
            }
        }
        else
        {
            GUI.color = new Color(0.08f, 0.08f, 0.09f);
            GUI.DrawTexture(picture, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
            };
            style.normal.textColor = new Color(0.65f, 0.65f, 0.68f);
            GUI.Label(picture, "Camera off", style);
        }

        float scale = Screen.height / 720f;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

        // Starts an area to draw elements
        float frameWidth = Screen.width / scale;
        GUILayout.BeginArea(new Rect(20, 20, 192, 720));
        if (GUILayout.Button("Start or Stop Camera", GUILayout.Height(50)))
        {
            ToggleCamera();
        }
        if (GUILayout.Button("Next Camera", GUILayout.Height(50)))
        {
            NextCamera();
        }
        if (GUILayout.Button("Auto Select", GUILayout.Height(50)))
        {
            AutoSelectCamera();
        }
        if (GUILayout.Button("Refreash", GUILayout.Height(50)))
        {
            RefreshCamera();
        }
        if (GUILayout.Button("Detection on or off", GUILayout.Height(50)))
        {
            ToggleDetection();
        }
        GUILayout.EndArea();
    }

    private void ToggleCamera()
    {
        if (cam != null)
        {
            if (cam.isPlaying)
            {
                cam.Stop();
            }
            else
            {
                cam.Play();
            }
        }
    }

    private void NextCamera()
    {
        if (WebCamTexture.devices.Length > 1)
        {
            var devices = WebCamTexture.devices;

            cam.Stop();
            int currentIndex = System.Array.FindIndex(devices, d => d.name == cam.deviceName);
            int nextIndex = (currentIndex + 1) % devices.Length;
            cam.deviceName = devices[nextIndex].name;
            cam.Play();
        }
    }

    private void AutoSelectCamera()
    {
        if (WebCamTexture.devices.Length > 0)
        {
            var cams = ListCameras();
            int bestIndex = CameraPriority.SelectBest(cams);
            Debug.Log("Best camera index: " + bestIndex);
            if (bestIndex != -1)
            {
                cam.Stop();
                cam.deviceName = cams[bestIndex];
                cam.Play();
            }
        }
    }
    private void RefreshCamera()
    {
        if (cam != null)
        {
            cam.Stop();
            cam.Play();
        }
    }
    
    private void ToggleDetection()
    {
        // Implement your detection toggle logic here
    }
    private List<String> ListCameras()
    {
        List<String> cameraNames = new List<String>();
        foreach (var device in WebCamTexture.devices)
        {
            Debug.Log("WebCam device found: " + device.name);
            cameraNames.Add(device.name);
        }
        return cameraNames;
    }
}