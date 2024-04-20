namespace Spellslinger.Game.Control
{
    using System.Collections.Generic;
    using Spellslinger.Game.XR;
    using UnityEngine;

    public class Draw : MonoBehaviour {
        private const float MIN_DISTANCE = 0.01f;
        private const int MAX_POINTS = 20;
        private const int MIN_POINTS = 8;

        [SerializeField] private GameObject drawEffectPrefab;

        private GameObject drawPointRight;
        private GameObject drawPointLeft;
        
        private GameObject xrRig;
        private Camera playerCamera;

        private bool isDrawingRight = false;
        private bool isDrawingLeft = false;

        private List<Vector3> drawingPointsRight = new List<Vector3>();
        private List<Vector3> drawingPointsLeft = new List<Vector3>();

        public System.Action<Vector3[], XRInputManager.Controller> OnDrawFinished { get; internal set; }

        // Start is called before the first frame update
        private void Start() {
            this.drawPointRight = GameObject.Find("DrawPointRight");
            this.drawPointLeft = GameObject.Find("DrawPointLeft");
            this.xrRig = GameObject.Find("XRRig");
        }

        // Update is called once per frame
        private void Update() {
            if (this.isDrawingRight) {
                var point = drawPointRight.transform.position - xrRig.transform.position;
                // add current position to drawing points if distance to last point is greater than minDistance
                if (this.drawingPointsRight.Count == 0 || Vector3.Distance(this.drawingPointsRight[this.drawingPointsRight.Count - 1], point) > MIN_DISTANCE) {
                    this.drawingPointsRight.Add(point);
                }
            }

            if (this.isDrawingLeft) {
                
                var point = drawPointLeft.transform.position - xrRig.transform.position;
                // add current position to drawing points if distance to last point is greater than minDistance
                if (this.drawingPointsLeft.Count == 0 || Vector3.Distance(this.drawingPointsLeft[this.drawingPointsLeft.Count - 1], point) > MIN_DISTANCE) {
                    this.drawingPointsLeft.Add(point);
                }
            }
        }

        /// <summary>
        /// Starts drawing and attaches draw effect to draw point.
        /// </summary>
        /// <param name="controller">Controller that is used to draw.</param>
        public void StartDrawing(XRInputManager.Controller controller) {
            if (controller == XRInputManager.Controller.Left && !this.isDrawingLeft) {
                // attach draw effect to draw point
                GameObject drawEffect = Instantiate(this.drawEffectPrefab, this.drawPointLeft.transform);
                // get all particle systems
                ParticleSystem[] particleSystems = drawEffect.GetComponentsInChildren<ParticleSystem>();
                // set simulation space of all to current game object
                foreach (ParticleSystem ps in particleSystems)
                {
                    var main = ps.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.Custom;
                    main.customSimulationSpace = xrRig.transform;
                }
                this.isDrawingLeft = true;
            } else if (controller == XRInputManager.Controller.Right && !this.isDrawingRight) {
                // attach draw effect to draw point
                GameObject drawEffect = Instantiate(this.drawEffectPrefab, this.drawPointRight.transform);
                // get all particle systems
                ParticleSystem[] particleSystems = drawEffect.GetComponentsInChildren<ParticleSystem>();
                // set simulation space of all to current game object
                foreach (ParticleSystem ps in particleSystems)
                {
                    var main = ps.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.Custom;
                    main.customSimulationSpace = xrRig.transform;
                }
                this.isDrawingRight = true;
            }
        }

        /// <summary>
        /// Stops drawing and emits OnDrawFinished event.
        /// </summary>
        /// <param name="controller">Controller that was used to draw.</param>
        public void StopDrawing(XRInputManager.Controller controller) {
            if (controller == XRInputManager.Controller.Left && this.isDrawingLeft) {
                this.isDrawingLeft = false;

                // destroy drawing particles
                Destroy(this.drawPointLeft.transform.GetChild(0).gameObject);

                // emit event and reset drawing points
                Vector3[] normalizedPoints = this.GenerateNormalizedList(this.drawingPointsLeft);
                this.OnDrawFinished?.Invoke(normalizedPoints, XRInputManager.Controller.Left);
                this.drawingPointsLeft = new List<Vector3>();
            } else if (controller == XRInputManager.Controller.Right && this.isDrawingRight) {
                this.isDrawingRight = false;

                // destroy drawing particles
                Destroy(this.drawPointRight.transform.GetChild(0).gameObject);

                // emit event and reset drawing points
                Vector3[] normalizedPoints = this.GenerateNormalizedList(this.drawingPointsRight);
                this.OnDrawFinished?.Invoke(normalizedPoints, XRInputManager.Controller.Right);
                this.drawingPointsRight = new List<Vector3>();
            }
        }

        /// <summary>
        /// Generate a Normalized List of points, that are in camera space in a 1x1x1 cube.
        /// </summary>
        /// <param name="originalPoints"><c>List<Vector3></c> with original point list (should be larger than the MAX_POINTS variable)</param>.
        /// <returns></returns>
        private Vector3[] GenerateNormalizedList(List<Vector3> originalPoints) {
            // return empty list if originalPoints is smaller than MAX_POINTS/2. If the list is too small, the interpolation will not work properly and the result will be very inaccurate/random.
            if (originalPoints.Count < MIN_POINTS) {
                return new Vector3[0];
            }

            // interpolate additional points if originalPoints is smaller than MAX_POINTS
            List<Vector3> pointList = originalPoints.Count < MAX_POINTS ? this.InterpoalateMissingPoints(originalPoints) : originalPoints;

            // all points are relative to xrRig, convert back to world space we are currently in
            for (int i = 0; i < pointList.Count; i++) {
                pointList[i] += xrRig.transform.position;
            }
            
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
                    Vector3 pointOnSegment = lastPoint + (segmentLength * (currentPoint - lastPoint).normalized);
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
            Vector3[] outPoints = new Vector3[normalizedPoints.Count];
            Debug.Log("INFO: " + "Camera Rotation: " + Camera.main.transform.rotation);
            for (int i = 0; i < normalizedPoints.Count; i++) {
                // Subtract the player's position from the point
                // Rotate the point by the inverse of the player's rotation
                outPoints[i] = Quaternion.Inverse(Camera.main.transform.rotation) * (relativeWorldSpace.TransformPoint(normalizedPoints[i]) + -Camera.main.transform.position);
                //Debug.Log("INFO: " + "RotatedQuart:"+ outPoints[i] +" ScreenPoint: " + Camera.main.WorldToScreenPoint(relativeWorldSpace.TransformPoint(normalizedPoints[i])) + " Normalized Point points in local space: " + normalizedPoints[i] + " Relative World Space (DOES NOT TURN): " + relativeWorldSpace.TransformPoint(normalizedPoints[i]));
            }
            // destroy the relativeWorldSpace object
            Destroy(relativeWorldSpace.gameObject);

            return this.NormalizeList(outPoints);
        }

        /// <summary>
        /// Method <c>NormalizeList</c> takes a Vector3[] and normalizes it to a range between -1 and 1.
        /// First the displacement of the positions are corrected and then the data is normalized to a 1x1x1 cube.
        /// </summary>
        /// <param name="standarizedPositions">Vector3[].</param>
        /// <returns>Vector3[] in a 1x1 Cube.</returns>
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
                if (pos.x > biggestComponent) {
                    biggestComponent = pos.x;
                }

                if (pos.y > biggestComponent) {
                    biggestComponent = pos.y;
                }

                if (pos.z > biggestComponent) {
                    biggestComponent = pos.z;
                }
            }

            for (int i = 0; i < standarizedPositions.Length; i++) {
                standarizedPositions[i] /= biggestComponent;
            }

            return standarizedPositions;
        }

        /// <summary>
        /// Add points to a list of points by interpolating between the points (add 3 additional points in between the points).
        /// </summary>
        /// <param name="pointCloud"><c>List<Vector3></c> with original point list</param>.
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
}
