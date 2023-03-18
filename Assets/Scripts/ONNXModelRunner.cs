using UnityEngine;
using UnityEngine.UI;
using Unity.Barracuda;
using TMPro;

public class ONNXModelRunner : MonoBehaviour {
    public NNModel modelAsset;
    public TMP_Text resultText;

    private IWorker worker;

    void Start() {
        var model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
    }

    void Update() {
        // Input data
        var input = new Tensor(1, 28, 28, 1);
        // Set input data here...

        // Run model
        worker.Execute(input);

        // Get output
        var output = worker.PeekOutput();
        var outputData = output.ToReadOnlyArray();

        // Display result
        resultText.text = $"Output: {outputData[0]}";
    }

    void OnDestroy() {
        worker.Dispose();
    }
}
