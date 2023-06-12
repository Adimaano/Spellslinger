using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

namespace Spellslinger.AI
{
    public class ModelRunner : MonoBehaviour
    {
        public System.Action<int> OnPredictionReceived { get; internal set; }

        private WebSocket ws;

        private JsonSerializerSettings settings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private IEnumerator CheckAlive()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                if (ws.ReadyState == WebSocketState.Closed)
                {
                    ws.Connect();
                }
            }
        }

        private Action mainThreadCallback;
        private Coroutine aliveCheck;
        private void Start()
        {
            ws = new WebSocket("ws://138.201.95.51:5555");
            ws.Connect();
            ws.OnMessage += (sender, e) =>
            {
                // json array of floats
                var result = JsonConvert.DeserializeObject<float[]>(e.Data, settings);
                int maxIndex = 0;
                float maxValue = float.NegativeInfinity;
                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i] > maxValue)
                    {
                        maxIndex = i;
                        maxValue = result[i];
                    }
                }

                mainThreadCallback = () => OnPredictionReceived?.Invoke(maxIndex);
            };
            aliveCheck = StartCoroutine(CheckAlive());
        }

        private void OnDestroy()
        {
            StopCoroutine(aliveCheck);
            ws.Close();
        }

        private void Update()
        {
            if (mainThreadCallback != null)
            {
                mainThreadCallback();
                mainThreadCallback = null;
            }
        }

        public void IdentifyRune(Vector3[] pointCloud)
        {
            // convert point cloud to json
            var toSend = new float[pointCloud.Length][];
            for (int i = 0; i < pointCloud.Length; i++)
            {
                toSend[i] = new float[3];
                toSend[i][0] = pointCloud[i].x;
                toSend[i][1] = pointCloud[i].y;
                toSend[i][2] = pointCloud[i].z;
            }

            var json = JsonConvert.SerializeObject(toSend, settings);
            ws.Send(json);
        }
    }
}