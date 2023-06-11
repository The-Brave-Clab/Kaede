using System.Collections;
using DG.Tweening;

namespace Y3ADV
{
    public class CameraResetCommand : CommandBase
    {
        private bool wait;

        public CameraResetCommand(Y3ScriptModule module, string[] arguments) : base(module, arguments)
        {
            wait = Arg(2, true);
        }

        public override bool ShouldWait => wait;

        public override IEnumerator Execute()
        {
            float duration = Arg(1, 0.0f);

            var originalPosition = UIManager.CameraPos;
            var targetPosition = UIManager.CameraPosDefault;
            var originalScale = UIManager.CameraScale;
            var targetScale = UIManager.CameraScaleDefault;

            if (duration == 0)
            {
                UIManager.CameraPos = targetPosition;
                UIManager.CameraScale = targetScale;
                yield break;
            }

            Sequence s = DOTween.Sequence();
            s.Append(DOVirtual.Vector3(originalPosition, targetPosition, duration,
                value => UIManager.CameraPos = value));
            s.Join(DOVirtual.Vector3(originalScale, targetScale, duration,
                value => UIManager.CameraScale = value));

            yield return s.WaitForCompletion();
        }
    }
}