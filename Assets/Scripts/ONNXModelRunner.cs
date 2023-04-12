using UnityEngine;
using Unity.Barracuda;

public class ONNXModelRunner : MonoBehaviour {
    public NNModel modelAsset;
    private IWorker worker;
    private Tensor input = new Tensor(1, 60);

    private void Start() {
        var model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
    }

    private void OnDestroy() {
        worker.Dispose();
    }

    /// <summary>
    /// Identifies a rune based on a point cloud. 
    /// The Rune is identified by an ANN integrated with Unity Barracuda. The model returns the index of the class with the highest probability.
    /// </summary>
    /// <param name="pointCloud">The point cloud of the rune</param>
    /// <returns>The index/class of the identified rune</returns>
    public int IdentifyRune(Vector3[] pointCloud) {
        // pointCloud has 20 values -> 60 floats. Use each float as an input
        for (int i = 0; i < pointCloud.Length;i++) {
            int startIndex = i * 3;
            input[startIndex] = pointCloud[i].x;
            input[startIndex + 1] = pointCloud[i].y;
            input[startIndex + 2] = pointCloud[i].z;
        }

        // Run model
        worker.Execute(input);

        // get the output tensor
        Tensor output = worker.PeekOutput();
        float[] outputData = output.ToReadOnlyArray();
        float[] probs = output.ToReadOnlyArray();

        // find the index of the class with the highest probability
        int maxIndex = 0;
        float maxValue = float.NegativeInfinity;
        for (int i = 0; i < probs.Length; i++) {
            if (probs[i] > maxValue) {
                maxIndex = i;
                maxValue = probs[i];
            }
        }

        return maxIndex;
    }
}
