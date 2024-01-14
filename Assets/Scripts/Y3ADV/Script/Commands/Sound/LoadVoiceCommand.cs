using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class LoadVoiceCommand : CommandBase
    {
        public LoadVoiceCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
        }

        public override bool SyncExecution => true;
        public override IEnumerator Execute()
        {
            yield return LoadVoice(GameManager.ScriptName, args[1]);
        }

        public static IEnumerator LoadVoice(string scenarioName, string voiceName, SoundManager.LoadCallback cb = null)
        {
            if (SoundManager.IsInvalidVoice(voiceName))
                yield break;

            yield return SoundManager.LoadAudio(
                    voiceName,
                    new [] {scenarioName}, 
                    $"{voiceName}.wav", 
                    SoundManager.SoundType.Voice,
                    cb);
        }
    }
}