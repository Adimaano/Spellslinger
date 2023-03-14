using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class Draw : MonoBehaviour {

    [SerializeField]
    private XRNode xrNode = XRNode.RightHand;

    private List<InputDevice> devices = new List<InputDevice>();
    private InputDevice device;

    private GameObject drawPoint;
    private GameObject drawEffect;

    public Material drawMaterial;
    //public Material runeMaterial;

    private Animator animator;
    private LineRenderer lineRenderer;
    private GameObject lineRendererParent;

    private bool isDrawing = false;
    private float lineWidth = 0.05f;

    private static int ID = 0;

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
        //drawEffect.SetActive(false);
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
                SaveRuneToFile();
                //lineRendererParent.AddComponent<Rune>();
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
            // spell.tag = "Spell Rune";
            spell.transform.position = drawPoint.transform.position;
            spell.transform.parent = lineRendererParent.transform;
            SphereCollider sc = spell.AddComponent(typeof(SphereCollider)) as SphereCollider;
            sc.radius = lineWidth / 5;
        }
    }

    private void Drawing() {

        isDrawing = true;

        lineRendererParent = new GameObject();
        //lineRendererParent.tag = "Spell Rune";
        
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

    private void SaveRuneToFile() {
        // generate a list from the linerenderer points
        List<Vector3> globalPoints = new List<Vector3>();
        for (int i = 0; i < lineRenderer.positionCount; i++) {
            globalPoints.Add(lineRenderer.GetPosition(i));
        }
        Debug.Log(globalPoints.Count + "global Points");

        Vector3[] savedPoints = GenerateNormalizedList(globalPoints, lineRendererParent.transform);

        Debug.Log(savedPoints.Length + " normalized points");

        // save to file if length condition is met
        if (savedPoints.Length == 20) {
            string filepath = "Assets/Ressources/Runes.txt";
            //string shapeName = "hourglass";
            
            //string line = ID + ";" + shapeName + ";";
            string line = "";
            foreach (Vector3 point in savedPoints) {
                line += point.x + "," + point.y + "," + point.z + ";";
            }
            line += "\n";

            System.IO.File.AppendAllText(filepath, line);
            ID++;

        } else {
            Debug.Log("Length Condition not met: " + savedPoints.Length + " Points");
        }

        // NOTE: only for debugging to check if the created pointcloud matches the expectations
        // create new gameobject with linerenderer and draw the points in savedPoints
        GameObject newLineRendererParent = new GameObject();
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


    Vector3[] GenerateNormalizedList(List<Vector3> originalPoints, Transform transform) {
        List<Vector3> normalizedPoints = new List<Vector3>();
        normalizedPoints.Add(originalPoints[0] - transform.position); // Always add the first point

        float totalLength = 0f;
        for (int i = 1; i < originalPoints.Count; i++) {
            totalLength += Vector3.Distance(originalPoints[i], originalPoints[i - 1]);
        }

        float segmentLength = totalLength / (20f);
        float distanceToNextPoint = segmentLength;
        Vector3 lastPoint = originalPoints[0];
        float distanceBetweenPoints = 0.0f;

        for (int i = 1; i < originalPoints.Count - 1 && normalizedPoints.Count < 20; i++) {
            Vector3 currentPoint = originalPoints[i];

            distanceBetweenPoints += Vector3.Distance(originalPoints[i - 1], originalPoints[i]);

            if (distanceBetweenPoints >= distanceToNextPoint) {
                Vector3 pointOnSegment = lastPoint + distanceToNextPoint * (currentPoint - lastPoint).normalized;
                normalizedPoints.Add(pointOnSegment - transform.position);
                
                distanceBetweenPoints = 0.0f;
                lastPoint = pointOnSegment;
            }
        }

        normalizedPoints.Add(originalPoints[originalPoints.Count - 1] - transform.position);

        while (normalizedPoints.Count < 20) {
            normalizedPoints.Add(originalPoints[originalPoints.Count - 1] - transform.position);
        }

        Vector3[] cameraPoints = new Vector3[normalizedPoints.Count];
        for (int i = 0; i < normalizedPoints.Count; i++) {
            cameraPoints[i] = Camera.main.WorldToScreenPoint(transform.TransformPoint(normalizedPoints[i]));
        }

        return NormalizeList(cameraPoints);
    }

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

    /* This is a Helper Function for retrieving Actions/Usages. This is helpful because we don't know what
     * device the player is using (oculus/vive/index...) */
    void LogFeatures() {
        List<InputFeatureUsage> features = new List<InputFeatureUsage>();
        device.TryGetFeatureUsages(features);

        // Loop for Logging Feature types
        foreach (var feature in features) {
            //Debug.Log($"feature {feature.name} type {feature.type}");

            // only log bool types (e.g. want to get trigger/button press)
            if (feature.type == typeof(bool)) {
                Debug.Log($"feature {feature.name} type {feature.type}");
            }
        }
    }
}
