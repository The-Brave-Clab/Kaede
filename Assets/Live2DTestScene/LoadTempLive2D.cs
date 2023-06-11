using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Y3ADV;

public class LoadTempLive2D : MonoBehaviour
{
    public string gameObjectName;
    public string modelName;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadLive2DModel());
    }

    IEnumerator LoadLive2DModel()
    {
        Y3Live2DManager.ModelInfo modelInfo = new()
        {
            Name = gameObjectName,
            JsonFile = $"live2d/{modelName}/model.json",
            Path = $"live2d/{modelName}"
        };
        yield return Y3Live2DManager.ActorSetup(modelInfo, controller =>
        {
            StartCoroutine(ResetLive2DModel(controller));
        });
    }

    IEnumerator ResetLive2DModel(Y3Live2DModelController controller)
    {
        yield return null;

        controller.StartMotion("mtn_idle00");
        controller.StartFaceMotion("face_idle01");
        controller.hidden = false;
    }
}
