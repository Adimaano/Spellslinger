using UnityEngine;
using UnityEngine.UI;
using Unity.Barracuda;
using TMPro;

public class ONNXModelRunner : MonoBehaviour {
    public NNModel modelAsset;
    public TMP_Text resultText;

    private IWorker worker;
    
    private Tensor input = new Tensor(1, 60);

    void Start() {
        var model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
    }

    void OnDestroy() {
        worker.Dispose();
    }

    public void IdentifyRune(Vector3[] pointCloud) {
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

        Debug.Log(output);
        Debug.Log(outputData);

        for (int i = 0; i < outputData.Length;i++) {
            Debug.Log(outputData[i]);
        }

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

        // log the most probable class and its probability
        Debug.Log("Most probable class: " + maxIndex + " (probability = " + maxValue + ")");

        string className = "other";
        
        switch (maxIndex) {
            case 0: 
                className = "Time Spell";
                break;
            case 1:
                className = "Wind Spell";
                break;
            case 2:
                className = "EPIC FAIL";
                break;
            default:
                className = "other";
                break;
        }

        resultText.text = "Rune: " + className;
    }
}
