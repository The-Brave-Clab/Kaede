using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class MesCommand : CommandBase
    {
        private Y3Live2DModelController controller = null;
        private string[] resourceSplit = null;

        public MesCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            resourceSplit = originalArgs[1].Split(':');
            controller = resourceSplit.Length > 1 ? FindEntity<Y3Live2DModelController>(originalArgs[1].Split(':')[0]) : null;
        }

        public override bool ShouldWait => true;

        public override IEnumerator Execute()
        {
            string name = "";
            if (resourceSplit.Length > 1)
                name = resourceSplit[1];
            else
                name = resourceSplit[0];
            string soundName = Arg(2, "");
            string message = Arg(3, "");

            //Debug.Log(name != "" ? $"{name}: {message}" : message);

            UIManager.Instance.messageBox.gameObject.SetActive(true);
            UIManager.Instance.message.SetText(message);
            UIManager.Instance.nameTag.text = name;
            
#if WEBGL_BUILD
            WebGLInterops.OnMessageCommand(name, soundName, message);
#endif

            scriptModule.mesCommandOnGoing = true;

            if (soundName != "")
                SoundManager.Instance.PlayVoice(soundName);

            float time = 0.0f;

            while (scriptModule.paused || !scriptModule.autoMode || 
                   SoundManager.Instance.IsVoicePlaying() ||
                   !UIManager.Instance.message.IsCompleteDisplayText || time < 1.0f)
            {
                if (scriptModule.lipSync)
                {
                    if (controller != null)
                        controller.SetLip(SoundManager.Instance.GetVoiceVolume(0.0f) * scriptModule.lipSyncScale);
                }

                if (scriptModule.ShouldSkipMesCommand)
                {
                    if (UIManager.Instance.message.IsCompleteDisplayText)
                        break;
                    UIManager.Instance.message.SkipDisplay();
                    scriptModule.ShouldSkipMesCommand = false;
                }

                yield return null;
                
                if (UIManager.Instance.message.IsCompleteDisplayText)
                    time += Time.deltaTime;
            }

            if (controller != null)
            {
                if (scriptModule.lipSync)
                {
                    controller.SetLip(0);
                }
                controller.RemoveAllMouthSync();
            }
            SoundManager.Instance.StopVoice();
            //yield return new WaitUntil(() => !SoundManager.Instance.IsVoicePlaying());
            scriptModule.mesCommandOnGoing = false;
        }
    }
}