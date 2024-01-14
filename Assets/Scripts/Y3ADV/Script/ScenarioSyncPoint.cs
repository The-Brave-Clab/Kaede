using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Y3ADV
{
    public interface IStateSavable<T> where T : struct
    {
        T GetState();
        void SetState(T state);
    }

    [Serializable]
    public struct ScenarioSyncPoint
    {
        public int currentStatementIndex;

        // Actors
        public List<ActorState> actors;
        // Sprites
        // Backgrounds
        // Still
        // Caption
        // Message Box
        // Fade
        // BGM
    }

    [Serializable]
    public struct EntityTransform
    {
        public Vector3 position;
        public float angle;
        public float scale;
        public int layer;
    }

    [Serializable]
    public struct ActorState
    {
        public string name;
        public string currentMotion;
        public string currentFaceMotion;

        public bool hidden;

        public bool eyeBlink;
        public bool manualEyeOpen;

        public List<string> mouthSynced;

        [FormerlySerializedAs("actorFaceAngle")] [FormerlySerializedAs("actorAngle")] public Vector2 faceAngle;
        [FormerlySerializedAs("actorBodyAngle")] public float bodyAngle;

        public float addEye;

        public EntityTransform transform;
    }

}