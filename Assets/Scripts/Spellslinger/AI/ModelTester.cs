using System;
using Spellslinger.Misc;
using UnityEngine;

namespace Spellslinger.AI
{
    public class ModelTester: MonoBehaviour
    {
        public ModelRunner modelRunner;

        private void Start()
        {
            modelRunner.OnPredictionReceived += (prediction) =>
            {
                Debug.Log($"Prediction: {prediction}");
            };
        }

        private void Update()
        {
            // if P is pressed, test the model
            if (Input.GetKeyDown(KeyCode.P))
            {
                // create a random point cloud
                Vector3[] pointCloud = new Vector3[20];
                for (int i = 0; i < pointCloud.Length; i++)
                {
                    pointCloud[i] = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
                }

                // identify the rune
               this.modelRunner.IdentifyRune(pointCloud);

            }
        }
    }
}