using System.Collections.Generic;
using UnityEngine;

public class Draw : MonoBehaviour {
    [SerializeField] private GameObject drawEffectPrefab;

    private GameObject drawPointRight;
    private GameObject drawPointLeft;

    private bool isDrawingRight = false;
    private bool isDrawingLeft = false;

    private List<Vector3> drawingPointsRight = new List<Vector3>();
    private List<Vector3> drawingPointsLeft = new List<Vector3>();
    private float MIN_DISTANCE = 0.01f;
    private const int MAX_POINTS = 20;

    public System.Action<Vector3[], XRInputManager.Controller> OnDrawFinished { get; internal set; }

    // Start is called before the first frame update
    void Start() {
        drawPointRight = GameObject.Find("DrawPointRight");
        drawPointLeft = GameObject.Find("DrawPointLeft");
    }

    // Update is called once per frame
    void Update() {
        if (isDrawingRight) {
            // add current position to drawing points if distance to last point is greater than minDistance
            if (drawingPointsRight.Count == 0 || Vector3.Distance(drawingPointsRight[drawingPointsRight.Count - 1], drawPointRight.transform.position) > MIN_DISTANCE) {
                drawingPointsRight.Add(drawPointRight.transform.position);
            }
        }

        if (isDrawingLeft) {
            // add current position to drawing points if distance to last point is greater than minDistance
            if (drawingPointsLeft.Count == 0 || Vector3.Distance(drawingPointsLeft[drawingPointsLeft.Count - 1], drawPointLeft.transform.position) > MIN_DISTANCE) {
                drawingPointsLeft.Add(drawPointLeft.transform.position);
            }
        }
    }

    /// <summary>
    /// Starts drawing and attaches draw effect to draw point
    /// </summary>
    /// <param name="controller">Controller that is used to draw</param>
    public void StartDrawing(XRInputManager.Controller controller) {
        if (controller == XRInputManager.Controller.Left && !isDrawingLeft) {
            // attach draw effect to draw point
            GameObject drawEffect = Instantiate(drawEffectPrefab, drawPointLeft.transform);
            isDrawingLeft = true;
        } else if (controller == XRInputManager.Controller.Right && !isDrawingRight){
            // attach draw effect to draw point
            GameObject drawEffect = Instantiate(drawEffectPrefab, drawPointRight.transform);
            isDrawingRight = true;
        }
    }

    /// <summary>
    /// Stops drawing and emits OnDrawFinished event
    /// </summary>
    /// <param name="controller">Controller that was used to draw</param>
    public void StopDrawing(XRInputManager.Controller controller) {
        if (controller == XRInputManager.Controller.Left && isDrawingLeft) {
            isDrawingLeft = false;

            // add particlescript to draw effect (destroy particle after given time, default 1.5f)
            GameObject drawEffect = drawPointLeft.transform.GetChild(0).gameObject;
            drawEffect.AddComponent<ParticleScript>();

            // detach draw effect from draw point
            drawEffect.transform.parent = null;

            // emit event and reset drawing points
            Vector3[] normalizedPoints = GenerateNormalizedList(drawingPointsLeft);
            OnDrawFinished?.Invoke(normalizedPoints, XRInputManager.Controller.Left);
            drawingPointsLeft = new List<Vector3>();
        } else if (controller == XRInputManager.Controller.Right && isDrawingRight) {
            isDrawingRight = false;

            // add particlescript to draw effect (destroy particle after given time, default 1.5f)
            GameObject drawEffect = drawPointRight.transform.GetChild(0).gameObject;
            drawEffect.AddComponent<ParticleScript>();

            // detach draw effect from draw point
            drawEffect.transform.parent = null;

            // emit event and reset drawing points
            Vector3[] normalizedPoints = GenerateNormalizedList(drawingPointsRight);
            OnDrawFinished?.Invoke(normalizedPoints, XRInputManager.Controller.Right);
            drawingPointsRight = new List<Vector3>();
        }
    }

    /// <summary>
    /// Generate a Normalized List of points, that are in camera space in a 1x1x1 cube
    /// </summary>
    /// <param name="originalPoints"><c>List<Vector3></c> with original point list (should be larger than the MAX_POINTS variable)</param>
    /// <returns></returns>
    Vector3[] GenerateNormalizedList(List<Vector3> originalPoints) {
        // make sure the list has at least 20 points
        List<Vector3> pointList = originalPoints.Count < MAX_POINTS ? InterpoalateMissingPoints(originalPoints) : originalPoints;

        // create Transform from first point of pointList
        Transform relativeWorldSpace = new GameObject().transform;
        relativeWorldSpace.position = pointList[0];

        List<Vector3> normalizedPoints = new List<Vector3>();
        
        // Always add the first point
        normalizedPoints.Add(pointList[0] - relativeWorldSpace.position); 

        // calculate the length of the complete path
        float totalLength = 0f;
        for (int i = 1; i < pointList.Count; i++) {
            totalLength += Vector3.Distance(pointList[i], pointList[i - 1]);
        }

        // divide the length by MAX_POINTS get an estimated length for each segment (distance between the new points)
        float segmentLength = totalLength / MAX_POINTS;
        Vector3 lastPoint = pointList[0];
        float distanceBetweenPoints = 0.0f;

        // loop through the original points until MAX_POINTS-1 Points are in the list
        for (int i = 1; i < pointList.Count - 1 && normalizedPoints.Count < MAX_POINTS; i++) {
            Vector3 currentPoint = pointList[i];

            // add the distance between this point and the previous point to distanceBetweenPoints (-> distance between last added point and current point) 
            distanceBetweenPoints += Vector3.Distance(pointList[i - 1], pointList[i]);

            // if distanceBetweenPoints is greater than the segmentLength, add a normalized point and reset distanceBetweenPoints
            if (distanceBetweenPoints >= segmentLength) {
                Vector3 pointOnSegment = lastPoint + segmentLength * (currentPoint - lastPoint).normalized;
                normalizedPoints.Add(pointOnSegment - relativeWorldSpace.position);
                
                distanceBetweenPoints = 0.0f;
                lastPoint = pointOnSegment;
            }
        }

        // add the last point to the list
        normalizedPoints.Add(pointList[pointList.Count - 1] - relativeWorldSpace.position);

        // this is just a fallback to make sure that there are always the correct number of points. This should never happen and if it happens the results are probably crappy
        while (normalizedPoints.Count < MAX_POINTS) {
            normalizedPoints.Add(pointList[pointList.Count - 1] - relativeWorldSpace.position);
        }

        // translate the generated pointlist into camera space
        Vector3[] cameraPoints = new Vector3[normalizedPoints.Count];
        for (int i = 0; i < normalizedPoints.Count; i++) {
            cameraPoints[i] = Camera.main.WorldToScreenPoint(relativeWorldSpace.TransformPoint(normalizedPoints[i]));
        }

        // destroy the relativeWorldSpace object
        Destroy(relativeWorldSpace.gameObject);

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
    /// Add points to a list of points by interpolating between the points (add 3 additional points in between the points)
    /// </summary>
    /// <param name="pointCloud"><c>List<Vector3></c> with original point list</param>
    /// <returns><c>List<Vector3></c></returns>
    private List<Vector3> InterpoalateMissingPoints(List<Vector3> pointCloud) {
        List<Vector3> interpolatedPoints = new List<Vector3>();

        for (int i = 0; i < pointCloud.Count - 1; i++) {
            interpolatedPoints.Add(pointCloud[i]);
            interpolatedPoints.Add(Vector3.Lerp(pointCloud[i], pointCloud[i + 1], 0.25f));
            interpolatedPoints.Add(Vector3.Lerp(pointCloud[i], pointCloud[i + 1], 0.5f));
            interpolatedPoints.Add(Vector3.Lerp(pointCloud[i], pointCloud[i + 1], 0.75f));
        }

        interpolatedPoints.Add(pointCloud[pointCloud.Count - 1]);

        return interpolatedPoints;
    }
}
