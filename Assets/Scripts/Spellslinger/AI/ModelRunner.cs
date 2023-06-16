using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;
using Application = UnityEngine.Device.Application;

namespace Spellslinger.AI
{
    public class ModelRunner : MonoBehaviour
    {
        public Action<int> OnPredictionReceived { get; internal set; }

        private InferenceSession session;
        private Tensor<float> input = new DenseTensor<float>(new[] { 1, 20, 3});

        private void Start()
        {
            session = new InferenceSession(Path.Join(Application.streamingAssetsPath, "model_3d_with_other.onnx"));
        }

        private void OnDestroy()
        {
            session.Dispose();
        }
        
        public void IdentifyRune(Vector3[] pointCloud)
        {
            for (var i = 0; i < pointCloud.Length; i++)
            {
                input[0, i, 0] = pointCloud[i].x;
                input[0, i, 1] = pointCloud[i].y;
                input[0, i, 2] = pointCloud[i].z;
            }
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_4", input)
            };
            using var results = session.Run(inputs);
            var output = results.First().AsEnumerable<float>().ToList();
            var maxIndex = 0;
            var maxValue = float.NegativeInfinity;
            for (var i = 0; i < output.Count(); i++)
            {
                if (!(output[i] > maxValue)) continue;
                maxIndex = i;
                maxValue = output[i];
            }
            results.Dispose();
            Debug.Log(maxValue);
            OnPredictionReceived?.Invoke(maxIndex);
        }
    }
}