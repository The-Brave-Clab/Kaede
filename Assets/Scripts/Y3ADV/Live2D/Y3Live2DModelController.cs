using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using live2d;
using live2d.framework;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Y3ADV
{
    public class Y3Live2DModelController : BaseEntity
    {
        [Serializable]
        public class Motion
        {
            public string Name = "";
            public byte[] Asset = null;
        }

        public string modelName;

        [HideInInspector] public byte[] mocFile;
        [HideInInspector] public string poseFile;

        //public TextAsset physicsFile;
        [HideInInspector] public Texture2D[] textureFiles;
        [HideInInspector] public Motion[] motionFiles;

        private Dictionary<string, Live2DMotion> motions;
        private Live2DMotion idleMotion;
        private Live2DMotion nextMotion;
        private L2DPose pose;
        private MotionQueueManager motionMgr;
        private MotionQueueManager faceMotionMgr;

        private Live2DModelUnity live2DModel;
        private EyeBlinkMotion eyeBlink = new EyeBlinkMotion();

        private int layer = 0;

        public bool hidden = true;

        public bool useEyeBlink = true;
        public bool manualEyeOpen = false;

        [SerializeField, HideInInspector]
        private List<Y3Live2DModelController> mouthSynced = null;
        public float mouthOpenY = 0.0f;

        public float addAngleX = 0.0f;
        public float addAngleY = 0.0f;
        public float addBodyAngleX = 0.0f;

        public float addEyeX = 0.0f;
        public float absoluteEyeX = 0.0f;

        public RenderTexture targetTexture;

        private RawImage rawImage = null;
        private RectTransform rectTransform = null;

        public int Layer
        {
            get => layer;
            set
            {
                layer = value;
                Y3Live2DManager.ReorderModels();
            }
        }

        //private L2DPhysics physics;
        private Matrix4x4 live2DCanvasPos;

        public List<string> MotionNames => motions.Keys.ToList();

        protected override void Awake()
        {
            base.Awake();
            rawImage = GetComponent<RawImage>();
            rawImage.color = Color.clear;
            rectTransform = transform as RectTransform;
            modelName = "";
            idleMotion = null;
            nextMotion = null;
        }

        void Start()
        {
            motionMgr = new MotionQueueManager();
            faceMotionMgr = new MotionQueueManager();
            //motionMgr.setMotionDebugMode(true);
            motions = new(motionFiles.Length);
            foreach (var motionFile in motionFiles)
            {
                var loadedMotion = Live2DMotion.loadMotion(motionFile.Asset);
                motions[motionFile.Name] = loadedMotion;
                if (motionFile.Name == "mtn_idle") idleMotion = loadedMotion;
            }
            if (poseFile != null)
            {
                pose = L2DPose.load(poseFile);
            }

            mouthSynced = new List<Y3Live2DModelController>();

            LoadModel();

            StartMotion("mtn_idle00");
            StartFaceMotion("face_idle01");
        }

        void LoadModel()
        {
            if (mocFile == null) return;
            live2DModel = Live2DModelUnity.loadModel(mocFile);
            live2DModel.setRenderMode(Live2D.L2D_RENDER_DRAW_MESH_NOW);
            for (int i = 0; i < textureFiles.Length; i++)
            {
                if (textureFiles[i] == null) continue;
                live2DModel.setTexture(i, textureFiles[i]);
            }

            const float canvasScale = 2.0f;
            const float scale = 1.05f;
            float modelWidth = live2DModel.getCanvasWidth();
            float percentage = (canvasScale / scale - 1.0f) / 2.0f;
            float lowBound = modelWidth * (-percentage);
            float highBound = modelWidth * (1 + percentage);
            live2DCanvasPos = Matrix4x4.Ortho(lowBound, highBound, highBound, lowBound, -50.0f, 50.0f);

            //if (physicsFile != null) physics = L2DPhysics.load(physicsFile.bytes);

            
            targetTexture = RenderTexture.GetTemporary((int) (modelWidth * canvasScale),
                (int) (modelWidth * canvasScale), 32);
            targetTexture.name = $"{gameObject.name} RT";
            
            RenderTexture.active = targetTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = null;

            rawImage.texture = targetTexture;
            rawImage.color = Color.white;

            rectTransform.sizeDelta = new Vector2(modelWidth, modelWidth) * 2;
        }


        void Update()
        {
            // skip live2d updating in batch mode
            if (StartupSettings.BatchMode) return;

            if (live2DModel == null) LoadModel();
            
            live2DModel.setMatrix(Matrix4x4.identity);
            if (!Application.isPlaying)
            {
                live2DModel.update();
                return;
            }
            
            if (nextMotion != null)
            {
                motionMgr.startMotion(nextMotion);
                nextMotion = null;
            }
            else if (motionMgr.isFinished() && idleMotion != null)
            {
                motionMgr.startMotion(idleMotion);
            }

            double timeSec = UtSystem.getUserTimeMSec() / 1000.0;
            double t = timeSec * 2 * Math.PI;
            live2DModel.setParamFloat("PARAM_BREATH", (float) (0.5f + 0.5f * Math.Sin(t / 3.0)));

            motionMgr.updateParam(live2DModel);
            faceMotionMgr.updateParam(live2DModel);

            if (useEyeBlink)
                eyeBlink.setParam(live2DModel);
            else
            {
                live2DModel.setParamFloat("PARAM_EYE_L_OPEN", manualEyeOpen ? 1.0f : 0.0f);
                live2DModel.setParamFloat("PARAM_EYE_R_OPEN", manualEyeOpen ? 1.0f : 0.0f);
            }
            if (pose != null)
            {
                pose.updateParam(live2DModel);
            }
            live2DModel.saveParam();
            live2DModel.addToParamFloat("PARAM_ANGLE_X", addAngleX);
            live2DModel.addToParamFloat("PARAM_ANGLE_Y", addAngleY);
            live2DModel.addToParamFloat("PARAM_BODY_ANGLE_X", addBodyAngleX);

            if (mouthOpenY != 0f)
            {
                live2DModel.setParamFloat("PARAM_MOUTH_OPEN_Y", mouthOpenY);
            }
            if (addEyeX != 0f)
            {
                live2DModel.addToParamFloat("PARAM_EYE_BALL_X", addEyeX);
            }
            if (absoluteEyeX != 0f)
            {
                live2DModel.setParamFloat("PARAM_EYE_BALL_X", absoluteEyeX);
            }
            
            //if (physics != null) physics.updateParam(live2DModel);

            live2DModel.update();
            live2DModel.loadParam();
        }

        public void Render()
        {
            // skip live2d rendering in batch mode
            if (StartupSettings.BatchMode) return;

            RenderTexture active = RenderTexture.active;
            RenderTexture.active = targetTexture;

            GL.Clear(true, true, Color.clear);

            if (isActiveAndEnabled && !hidden)
            {
                if (live2DModel == null) LoadModel();
                if (live2DModel.getRenderMode() == Live2D.L2D_RENDER_DRAW_MESH_NOW)
                {
                    GL.PushMatrix();
                    GL.LoadIdentity();
                    GL.LoadProjectionMatrix(live2DCanvasPos);

                    //live2DModel.update();
                    
                    // This will generate one and only one error but it is absolutely okay
                    // The cause is that in the draw() call, live2d SDK checks if the stacktrace string
                    // contains "OnPostRender()" while in the extracted stacktrace the function string
                    // is actually "OnPostRender ()" instead.
                    // Other than an error log, the SDK does absolutely nothing so it's benign.
                    // On iOS platform it doesn't generate the error message.
                    live2DModel.draw();

                    GL.PopMatrix();
                }
            }
            
            RenderTexture.active = active;
        }

        private bool FixMotionName(ref string motionName)
        {
            if (motions.ContainsKey(motionName)) return true;

            var substituteMotionName = Utils.FindClosestMatch(motionName, motions.Keys, out var distance);
            var acceptable = distance <= 5;

            if (acceptable)
            {
                Debug.LogWarning(
                    $"Motion '{motionName}' doesn't exist in model '{name}', using '{substituteMotionName}' instead. Distance is {distance}.");
                motionName = substituteMotionName;
            }
            else
            {
                Utils.SendBugNotification($"Motion '{motionName}' doesn't exist in model '{name}' and no acceptable substitution is found.");
            }

            return acceptable;
        }

        public void StartMotion(string motionName, bool loop = false)
        {
            if (!FixMotionName(ref motionName)) return;

            var motion = motions[motionName];
            motion.setLoop(loop);
            nextMotion = motion;
        }

        public void StartFaceMotion(string motionName)
        {
            if (!FixMotionName(ref motionName)) return;

            var motion = motions[motionName];
            faceMotionMgr.startMotion(motion);
        }

        private void OnEnable()
        {
            Y3Live2DManager.AddController(this);
            Y3Live2DManager.ReorderModels();
        }

        protected override void OnDestroy()
        {
            RenderTexture.ReleaseTemporary(targetTexture);
            Y3Live2DManager.RemoveController(this);
            Y3Live2DManager.ReorderModels();
            live2DModel?.releaseModel();
            base.OnDestroy();
        }

        public void SetLip(float volume, List<Y3Live2DModelController> traversalBuffer = null)
        {
            mouthOpenY = volume;

            traversalBuffer ??= new List<Y3Live2DModelController>();
            
            traversalBuffer.Add(this);
            
            foreach (var modelController in mouthSynced
                .Where(modelController => !traversalBuffer.Contains(modelController)))
            {
                modelController.SetLip(volume, traversalBuffer);
            }
        }

        public void AddMouthSync(Y3Live2DModelController model)
        {
            if (model == this) return;
            if (mouthSynced.Contains(model)) return;
            mouthSynced.Add(model);
            model.AddMouthSync(this);
        }

        public void RemoveAllMouthSync(List<Y3Live2DModelController> traversalBuffer = null)
        {
            traversalBuffer ??= new List<Y3Live2DModelController>();
            
            traversalBuffer.Add(this);
            
            foreach (var modelController in mouthSynced
                .Where(modelController => !traversalBuffer.Contains(modelController)))
            {
                modelController.RemoveAllMouthSync(traversalBuffer);
            }

            mouthSynced.RemoveAll(x => true);
        }

        public void SetEye(string motion)
        {
            if (motion == "閉じ")
            {
                this.useEyeBlink = false;
                this.manualEyeOpen = false;
            }
            else if (motion == "開き")
            {
                this.useEyeBlink = false;
                this.manualEyeOpen = true;
            }
            else
            {
                this.useEyeBlink = true;
                this.manualEyeOpen = false;
            }
        }

        public override Color GetColor()
        {
            return rawImage.color;
        }

        public override void SetColor(Color color)
        {
            rawImage.color = color;
        }
        
        public override float ScaleScalar => 1.0f;

        protected override Vector3 TransformVector(Vector3 vec)
        {
            Vector3 result = new Vector3(vec.x * ScreenWidthScalar, vec.y, vec.z);
            return result;
        }

        protected override Vector3 UntransformVector(Vector3 vec)
        {
            Vector3 result = new Vector3(vec.x / ScreenWidthScalar, vec.y, vec.z);
            return result;
        }

        public IEnumerator ActorAngle(float angleX, float angleY, float duration)
        {
            if (duration == 0f)
            {
                addAngleX = angleX;
                addAngleY = angleY;
                yield break;
            }

            Sequence s = GetSequence();
            s.Append(DOVirtual.Float(addAngleX, angleX, duration,
                value => { addAngleX = value; }));
            s.Join(DOVirtual.Float(addAngleY, angleY, duration,
                value => { addAngleY = value; }));

            yield return s.WaitForCompletion();
            RemoveSequence(s);
        }

        public IEnumerator ActorBodyAngle(float angleX, float duration)
        {
            if (duration == 0f)
            {
                addBodyAngleX = angleX;
                yield break;
            }

            Sequence s = GetSequence();
            s.Append(DOVirtual.Float(addBodyAngleX, angleX, duration,
                value => { addBodyAngleX = value; }));

            yield return s.WaitForCompletion();
            RemoveSequence(s);
        }

        public IEnumerator ActorEnter(Vector3 originalPos, Vector3 targetPos, float duration)
        {
            Position = targetPos;
            hidden = false;

            if (duration == 0)
            {
                Position = originalPos;
                yield break;
            }

            Sequence sequence = GetSequence();
            sequence.Append(DOVirtual.Vector3(targetPos, originalPos, duration, value => { Position = value; }));

            yield return sequence.WaitForCompletion();
            RemoveSequence(sequence);
        }

        public IEnumerator ActorExit(Vector3 originalPos, Vector3 targetPos, float duration)
        {
            if (duration == 0)
            {
                Position = targetPos;
                yield break;
            }
            
            Sequence s = GetSequence();
            s.Append(DOVirtual.Vector3(originalPos, targetPos, duration, value => Position = value));

            yield return s.WaitForCompletion();
            RemoveSequence(s);
        }

        public IEnumerator ActorEyeAdd(float addAngle, float duration)
        {
            if (duration == 0)
            {
                addEyeX = addAngle;
                absoluteEyeX = 0;
                yield break;
            }
            
            Sequence s = GetSequence();
            s.Append(DOVirtual.Float(0, addAngle, duration,
                value =>
                {
                    addEyeX = value;
                    absoluteEyeX = 0;
                }));

            yield return s.WaitForCompletion();
            RemoveSequence(s);
        }

        public IEnumerator ActorScale(float scale, float duration)
        {
            RectTransform transform = GetComponent<RectTransform>();

            float originalScale = transform.localScale.x;

            if (duration == 0)
            {
                transform.localScale = Vector3.one * scale;
                yield break;
            }

            Sequence s = GetSequence();
            s.Append(DOVirtual.Float(originalScale, scale, duration,
                value => transform.localScale = Vector3.one * value));

            yield return s.WaitForCompletion();
            RemoveSequence(s);
        }
    }
}