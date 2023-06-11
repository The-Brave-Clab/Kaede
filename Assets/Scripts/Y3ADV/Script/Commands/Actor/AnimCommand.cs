using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class AnimCommand : CommandBase
    {
        private Y3Live2DModelController controller = null;
        
        public AnimCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            controller = FindEntity<Y3Live2DModelController>(originalArgs[1]);
        }

        public override bool ShouldWait => false;
        public override IEnumerator Execute()
        {
            bool loop;
            if (3 >= args.Length)
            {
                loop = true;
            }
            else
            {
                loop = (args[3] == "LOOP_ON" || args[3] == "1");
            }

            // if (controller.hidden)
            // {
            //     Y3ScriptModule.SendBugNotification("anim command happens without actor_show! Turning it on assuming it should.");
            //     controller.hidden = false;
            // }
            
            controller.StartMotion(args[2], loop);
            yield return null;
        }
    }
}
