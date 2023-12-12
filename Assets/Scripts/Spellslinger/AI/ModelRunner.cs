using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Sentis;
using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace Spellslinger.AI
{
    public class ModelRunner : MonoBehaviour
    {
        public Action<int> OnPredictionReceived { get; internal set; }

        public ModelAsset modelAsset;
        private Model _model;
        private IWorker _worker;

        private void Start()
        {
            _model = ModelLoader.Load(modelAsset);
            _worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, _model);
        }

        private void OnDestroy()
        {
            _worker?.Dispose();
        }

        public void IdentifyRune(Vector3[] pointCloud)
        {
            var tensor = TensorFloat.Zeros(new TensorShape(1, 20, 3));

            for (int i = 0; i < pointCloud.Length; i++)
            {
                tensor[0, i, 0] = pointCloud[i].x;
                tensor[0, i, 1] = pointCloud[i].y;
                tensor[0, i, 2] = pointCloud[i].z;
            }

            _worker.Execute(tensor);

            var output = _worker.PeekOutput() as TensorFloat;
            output.MakeReadable();

            var pred = output.ToReadOnlyArray();
            // get max
            var max = pred.Max();
            // get index of max
            var index = pred.ToList().IndexOf(max);
            Debug.Log($"Prediction: {index}"); // this should be a unit test
            OnPredictionReceived?.Invoke(index);
            output.Dispose();
            tensor.Dispose();
        }
    }
}