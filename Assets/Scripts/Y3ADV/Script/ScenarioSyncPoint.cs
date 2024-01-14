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
        public Vector2 pivot;
        public Color color;
    }

    [Serializable]
    public struct ActorState
    {
        public string name;
        public string currentMotion;
        public string currentFaceMotion;

        public int layer;

        public bool hidden;

        public bool eyeBlink;
        public bool manualEyeOpen;

        public List<string> mouthSynced;

        public Vector2 faceAngle;
        public float bodyAngle;

        public float addEye;

        public EntityTransform transform;
    }

}