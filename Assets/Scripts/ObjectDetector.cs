using UnityEngine;
using Unity.InferenceEngine;
using System.Collections.Generic;

public class ObjectDetector : MonoBehaviour
{
    private Worker _worker;
    private RenderTexture _input;
    private Tensor<float> _tensor;
    private CameraApp _cameraApp;
    private int _poller = 0;
    private string[] _labels;
    private readonly List<DetectionType> _detections = new List<DetectionType>();
    public IReadOnlyList<DetectionType> Detections => _detections;
    private void Start()
    {
        // Allocate the inference buffers before anything that can fail, so Update
        // never hands nulls to TextureConverter.ToTensor.
        _input = new RenderTexture(640, 640, 0);
        _tensor = new Tensor<float>(new TensorShape(1, 3, 640, 640));

        // Resources.Load returns the imported ModelAsset, not the .onnx file. If the
        // ONNX importer failed on this editor the asset is missing and this is null.
        var model = Resources.Load<ModelAsset>("Models/yolo11n");
        if (model == null)
        {
            Debug.LogError("ObjectDetector: no ModelAsset at Resources/Models/yolo11n. Select yolo11n.onnx in the Project window and check the Inspector for import errors. Detection is disabled.");
            return;
        }

        var text = Resources.Load<TextAsset>("Models/coco-classes");
        if (text == null)
        {
            Debug.LogError("ObjectDetector: no label file at Resources/Models/coco-classes.txt. Detection is disabled.");
            return;
        }
        _labels = text.text.Split('\n');
        Debug.Log("Labels: " + _labels.Length);

        try
        {
            var r = ModelLoader.Load(model);
            _worker = new Worker(r, BackendType.GPUCompute);
        }
        catch (System.Exception e)
        {
            Debug.LogError("ObjectDetector: could not load " + model.name + ": " + e.Message + ". Detection is disabled.");
            _worker = null;
            return;
        }

        Debug.Log("Object detector initialized with model: " + model.name);
        Debug.Log("Mirrored: " + _cameraApp?.cam?.videoVerticallyMirrored);
    }
    private void Update()
    {
        // No worker means Start bailed out above; nothing to run.
        if (_worker == null)
            return;

        WebCamTexture cam = _cameraApp.cam;
        if (cam == null || !cam.isPlaying || cam.width <= 16)
            return;
        
        _poller++;
        if (_poller < 3)
            return;
        
        _poller = 0;
        if (cam.videoVerticallyMirrored)
            Graphics.Blit(cam, _input, new Vector2(1f, -1f), new Vector2(0f, 1f));
        else
            Graphics.Blit(cam, _input);

        TextureConverter.ToTensor(_input, _tensor, default);
        _worker.Schedule(_tensor);

        var output = _worker.PeekOutput() as Tensor<float>;

        _detections.Clear();
        float topScore = 0f; string topLabel = "";
        
        using (var cpu = output.ReadbackAndClone())
        {
            Debug.Log("Output shape: " + cpu.shape);
            for (int i = 0; i <= 8399; i++)
            {
                float colScore = 0f;
                int colRow = -1;
                for (int j = 4; j <= 83; j++)
                {
                    if (colScore > topScore) { topScore = colScore; topLabel = _labels[colRow - 4]; }
                    var read = cpu[0, j, i];
                    if (read > colScore)
                    {
                        colScore = read;
                        colRow = j;
                    }
                }
                if (colScore < 0.8f)
                    continue;
                float cx = cpu[0, 0, i];
                float cy = cpu[0, 1, i];
                float w = cpu[0, 2, i];
                float h = cpu[0, 3, i];

                Rect rect = new Rect( (cx - w / 2) / 640f, (cy - h / 2) / 640f, w / 640f, h / 640f);
                _detections.Add(new DetectionType 
                { 
                    label = _labels[colRow - 4], 
                    score = colScore, 
                    rect = rect 
                });
            }
        }
        int before = _detections.Count;
        var kept = BoxMath.Suppress(_detections);
        _detections.Clear();
        _detections.AddRange(kept);
        Debug.Log("Detections: " + before + " before suppression, " + _detections.Count + " after, top: " + topLabel + " " + topScore);
    }

    private void Awake()
    {
        _cameraApp = GetComponent<CameraApp>();
    }

    private void OnDestroy()
    {
        _worker?.Dispose();
        _input?.Release();
        _tensor?.Dispose();
    }
}

public struct DetectionType
{
    public string label;
    public float score;
    public Rect rect;
}