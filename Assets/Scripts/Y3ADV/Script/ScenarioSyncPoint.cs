using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Y3ADV
{
    public interface IStateSavable<T> where T : struct
    {
        T GetState();
        IEnumerator RestoreState(T state);
    }

    [Serializable]
    public struct ScenarioSyncPoint
    {
        public int currentStatementIndex;

        public List<ActorState> actors;
        public List<CommonResourceState> sprites;
        public List<CommonResourceState> backgrounds;
        public List<CommonResourceState> stills;
        public CaptionState caption;
        public MessageBoxState messageBox;
        public float fadeInProgress;
        public AudioState audio;
    }

    [Serializable]
    public struct EntityTransform
    {
        public bool enabled;
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

    [Serializable]
    public struct CommonResourceState
    {
        public string name;
        public string resourceName;

        public EntityTransform transform;
    }

    [Serializable]
    public struct CaptionState
    {
        public bool enabled;
        public Color boxColor;
        public string text;
        public float textAlpha;
    }

    [Serializable]
    public struct MessageBoxState
    {
        public bool enabled;
        public string speaker;
        public string message;
    }

    [Serializable]
    public struct AudioState
    {
        public bool bgmPlaying;
        public string bgmName;
        public float bgmVolume;
    }
}