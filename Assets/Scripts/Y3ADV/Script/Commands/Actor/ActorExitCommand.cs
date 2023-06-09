using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Y3ADV
{
    public class ActorExitCommand : CommandBase
    {
        private float duration;
        private string a;
        private Y3Live2DModelController actorEntity;
        private bool wait;
        
        public ActorExitCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            duration = Arg(2, 1.0f);
            a = Arg(3, "右");
            wait = Arg(4, true);
            actorEntity = FindEntity<Y3Live2DModelController>(originalArgs[1]);
        }

        public override bool ShouldWait => wait;
        public override IEnumerator Execute()
        {
            Vector3 pos = actorEntity.Position;
            Vector3 targetPos = pos;
            float num = 1920.0f / 2.0f + 1920.0f / 3.0f;
            targetPos.x = a == "右" ? num : -num;

            yield return actorEntity.ActorExit(pos, targetPos, duration);
        }
    }
}