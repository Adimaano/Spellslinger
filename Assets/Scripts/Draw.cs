using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class Draw : MonoBehaviour {

    [SerializeField]
    private XRNode xrNode = XRNode.RightHand;

    [SerializeField]
    private bool saveRunes = true;

    private List<InputDevice> devices = new List<InputDevice>();
    private InputDevice device;

    private GameObject drawPoint;

    public Material drawMaterial;

    private LineRenderer lineRenderer;
    private GameObject lineRendererParent;

    private bool isDrawing = false;
    private float lineWidth = 0.05f;

    private const int MAX_POINTS = 20;
    private const string FILEPATH = "Runes.txt";
    private int numberOfRunesSaved = 0;

    private ONNXModelRunner modelRunner;

    void GetDevice() {
        InputDevices.GetDevicesAtXRNode(xrNode, devices);
        device = devices.FirstOrDefault();
    }

    private void OnEnable() {
        if (!device.isValid) {
            GetDevice();
        }
    }

    private void Start() {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = drawMaterial;

        // For Testing Purposes only RightHandController 
        // TODO: implement for both controllers
        drawPoint = GameObject.Find("DrawPointRight");

        // get how many lines are currently in the file
        numberOfRunesSaved = System.IO.File.ReadAllLines(FILEPATH).Length;

        modelRunner = GameObject.Find("ModelRunner").GetComponent<ONNXModelRunner>();
    }

    void Update() {
        if (!device.isValid) {
            GetDevice();
        }

        // Capture TriggerButton
        bool triggerButtonAction = false;
        if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonAction) && triggerButtonAction) {
            if (!isDrawing) {
                Drawing();
            }
        } else if (isDrawing) {
            isDrawing = false;
            if (lineRendererParent != null) {
                lineRendererParent.AddComponent<Rune>();
                if (saveRunes) {
                    SaveRuneToFile();
                    
                } else {
                    List<Vector3> globalPoints = new List<Vector3>();
                    for (int i = 0; i < lineRenderer.positionCount; i++) {
                        globalPoints.Add(lineRenderer.GetPosition(i));
                    }

                    Vector3[] savedPoints = GenerateNormalizedList(globalPoints, lineRendererParent.transform);

                    modelRunner.IdentifyRune(savedPoints);
                }
            }
        }

        // Capture Primary ActionButton
        // NOTE: (Vive: No idea wihch button this is supposed to be?)
        bool primaryButtonAction = false;
        InputFeatureUsage<bool> primaryButtonUsage = CommonUsages.primaryButton;
        if (device.TryGetFeatureValue(primaryButtonUsage, out primaryButtonAction) && primaryButtonAction) {
            Debug.Log($"PrimaryButton activated {primaryButtonAction}");
        }

        // Capture Primary 2D Axis
        Vector2 primary2DAxisValue = Vector2.zero;
        InputFeatureUsage<Vector2> primary2DAxisUsage = CommonUsages.primary2DAxis;
        if (device.TryGetFeatureValue(primary2DAxisUsage, out primary2DAxisValue) && primary2DAxisValue != Vector2.zero) {
            Debug.Log($"primary2DAxis value {primary2DAxisValue}");
        }

        // Capture Grip Value 
        // NOTE: (Vive: 0 or 1)
        float gripActionValue = 0;
        InputFeatureUsage<float> gripUsage = CommonUsages.grip;
        if (device.TryGetFeatureValue(gripUsage, out gripActionValue)) {
            Debug.Log($"Grip value {gripActionValue}");
        }

        if (isDrawing) {
            lineRenderer.SetPosition(lineRenderer.positionCount++, drawPoint.transform.position);

            GameObject spell = new GameObject();
            spell.transform.position = drawPoint.transform.position;
            spell.transform.parent = lineRendererParent.transform;
            SphereCollider sc = spell.AddComponent(typeof(SphereCollider)) as SphereCollider;
            sc.radius = lineWidth / 5;
        }
    }

    private void Drawing() {

        isDrawing = true;

        lineRendererParent = new GameObject("Original Rune " + ++numberOfRunesSaved);
        
        // Add Collider
        SphereCollider sc = lineRendererParent.AddComponent(typeof(SphereCollider)) as SphereCollider;
        sc.radius = lineWidth / 5;
        
        // Draw Line
        lineRendererParent.transform.position = transform.position;
        lineRenderer = lineRendererParent.AddComponent<LineRenderer>();
        lineRenderer.material = drawMaterial;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.SetPosition(0, drawPoint.transform.position);
        lineRenderer.SetPosition(1, drawPoint.transform.position);
    }

    private void GetRuneEffect() {
        
    }

    private void SaveRuneToFile() {
        // generate a list from the linerenderer points
        List<Vector3> globalPoints = new List<Vector3>();
        for (int i = 0; i < lineRenderer.positionCount; i++) {
            globalPoints.Add(lineRenderer.GetPosition(i));
        }

        Vector3[] savedPoints = GenerateNormalizedList(globalPoints, lineRendererParent.transform);


        // save to file if length condition is met
        if (savedPoints.Length == MAX_POINTS) {
            //string shapeName = "hourglass";
            
            //string line = ID + ";" + shapeName + ";";
            string line = "";
            foreach (Vector3 point in savedPoints) {
                line += point.x + "," + point.y + "," + point.z + ";";
            }
            line += "1;\n";

            System.IO.File.AppendAllText(FILEPATH, line);
        } else {
            Debug.Log("Length Condition not met: " + savedPoints.Length + " Points");
        }

        // NOTE: only for debugging to check if the created pointcloud matches the expectations
        // create new gameobject with linerenderer and draw the points in savedPoints
        GameObject newLineRendererParent = new GameObject("Normalized Rune " + numberOfRunesSaved);
        LineRenderer newLineRenderer = newLineRendererParent.AddComponent<LineRenderer>();
        newLineRenderer.material = drawMaterial;
        newLineRenderer.startWidth = 0.05f;
        newLineRenderer.endWidth = lineWidth;
        newLineRenderer.positionCount = savedPoints.Length;

        for (int i = 0; i < savedPoints.Length; i++) {
            // Debug.Log("Point " + i + ": " + savedPoints[i]);
            newLineRenderer.SetPosition(i, savedPoints[i]);
        }
    }

    /// <summary>
    /// Generate a Normalized List of points, that are in camera space in a 1x1x1 cube
    /// </summary>
    /// <param name="originalPoints"><c>List<Vector3></c> with original point list (should be larger than the MAX_POINTS variable)</param>
    /// <param name="transform">Transform of the parent. This is necessary to sutract position to move the points from global to local space</param>
    /// <returns></returns>
    Vector3[] GenerateNormalizedList(List<Vector3> originalPoints, Transform transform) {
        List<Vector3> normalizedPoints = new List<Vector3>();
        normalizedPoints.Add(originalPoints[0] - transform.position); // Always add the first point

        // calculate the length of the complete path
        float totalLength = 0f;
        for (int i = 1; i < originalPoints.Count; i++) {
            totalLength += Vector3.Distance(originalPoints[i], originalPoints[i - 1]);
        }

        // divide the length by MAX_POINTS get an estimated length for each segment (distance between the new points)
        float segmentLength = totalLength / MAX_POINTS;
        Vector3 lastPoint = originalPoints[0];
        float distanceBetweenPoints = 0.0f;

        // loop through the original points until MAX_POINTS-1 Points are in the list
        for (int i = 1; i < originalPoints.Count - 1 && normalizedPoints.Count < MAX_POINTS; i++) {
            Vector3 currentPoint = originalPoints[i];

            // add the distance between this point and the previous point to distanceBetweenPoints (-> distance between last added point and current point) 
            distanceBetweenPoints += Vector3.Distance(originalPoints[i - 1], originalPoints[i]);

            // if distanceBetweenPoints is greater than the segmentLength, add a normalized point and reset distanceBetweenPoints
            if (distanceBetweenPoints >= segmentLength) {
                Vector3 pointOnSegment = lastPoint + segmentLength * (currentPoint - lastPoint).normalized;
                normalizedPoints.Add(pointOnSegment - transform.position);
                
                distanceBetweenPoints = 0.0f;
                lastPoint = pointOnSegment;
            }
        }

        // add the last point to the list
        normalizedPoints.Add(originalPoints[originalPoints.Count - 1] - transform.position);

        // this is just a fallback to make sure that there are always the correct number of points. This should never happen and if it happens the results are probably crappy
        while (normalizedPoints.Count < MAX_POINTS) {
            normalizedPoints.Add(originalPoints[originalPoints.Count - 1] - transform.position);
        }

        // translate the generated pointlist into camera space
        Vector3[] cameraPoints = new Vector3[normalizedPoints.Count];
        for (int i = 0; i < normalizedPoints.Count; i++) {
            cameraPoints[i] = Camera.main.WorldToScreenPoint(transform.TransformPoint(normalizedPoints[i]));
        }

        return NormalizeList(cameraPoints);
    }

    /// <summary>
    /// Method <c>NormalizeList</c> takes a Vector3[] and normalizes it to a range between -1 and 1.
    /// First the displacement of the positions are corrected and then the data is normalized to a 1x1x1 cube
    /// </summary>
    /// <param name="standarizedPositions">Vector3[]</param>
    /// <returns></returns>
    private Vector3[] NormalizeList(Vector3[] standarizedPositions) {
        float smallestX = 10000;
        float smallestY = 10000;
        float smallestZ = 10000;
        foreach (Vector3 pos in standarizedPositions) {
            if (pos.x < smallestX) {
                smallestX = pos.x;
            }
            if (pos.y < smallestY) {
                smallestY = pos.y;
            }
            if (pos.z < smallestZ) {
                smallestZ = pos.z;
            }
        }
        Vector3 correction = new Vector3(smallestX, smallestY, smallestZ);
        for (int i = 0; i < standarizedPositions.Length; i++) {
            standarizedPositions[i] -= correction;
        }

        float biggestComponent = -1000;
        foreach (Vector3 pos in standarizedPositions) {
            if (Mathf.Abs(pos.x) > biggestComponent)
                biggestComponent = Mathf.Abs(pos.x);
            if (Mathf.Abs(pos.y) > biggestComponent)
                biggestComponent = Mathf.Abs(pos.y);
            if (Mathf.Abs(pos.z) > biggestComponent)
                biggestComponent = Mathf.Abs(pos.z);
        }

        for (int i = 0; i < standarizedPositions.Length; i++) {
            standarizedPositions[i] /= biggestComponent;
        }

        return standarizedPositions;
    }


    /// <summary>
    /// This is a helper functionb for retrieving and logging Actions/Usages of various VR Setups. This might be helpuful as we
    /// don't know what device the player might use (oculus/vive/index...)
    /// <param name="type"><c>string</c> optional parameter. Define which Actions/Usages should be logged. possible parameters: "bool", "byte[]", "uint", "float", "Vector2", "Vector3". Default: Log Everything</param>
    /// </summary>
    void LogFeatures(string type = "") {
        List<InputFeatureUsage> features = new List<InputFeatureUsage>();
        device.TryGetFeatureUsages(features);

        foreach (var feature in features) {

            switch(type) {
                case "bool":
                    if (feature.type == typeof(bool)) {
                        Debug.Log($"feature {feature.name} type {feature.type}");
                    }
                    break;
                    
                case "byte[]":
                    if (feature.type == typeof(byte[])) {
                        Debug.Log($"feature {feature.name} type {feature.type}");
                    }
                    break;
                    
                case "uint":
                    if (feature.type == typeof(uint)) {
                        Debug.Log($"feature {feature.name} type {feature.type}");
                    }
                    break;
                    
                case "float":
                    if (feature.type == typeof(float)) {
                        Debug.Log($"feature {feature.name} type {feature.type}");
                    }
                    break;
                    
                case "Vector2":
                    if (feature.type == typeof(Vector2)) {
                        Debug.Log($"feature {feature.name} type {feature.type}");
                    }
                    break;
                    
                case "Vector3":
                    if (feature.type == typeof(Vector3)) {
                        Debug.Log($"feature {feature.name} type {feature.type}");
                    }
                    break;
                    
                default:
                    Debug.Log($"feature {feature.name} type {feature.type}");
                    break;
            }
        }
    }
}
