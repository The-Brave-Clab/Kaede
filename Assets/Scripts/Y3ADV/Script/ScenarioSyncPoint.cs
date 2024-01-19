using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Y3ADV
{
    public interface IState<T> : IEquatable<T> where T : struct
    {
        T Copy();
    }

    public interface IStateSavable<T> where T : struct, IState<T>
    {
        T GetState();
        IEnumerator RestoreState(T state);
    }

    [Serializable]
    public struct ScenarioSyncPoint : IState<ScenarioSyncPoint>
    {
        public int currentStatementIndex;

        public List<ActorState> actors;
        public List<CommonResourceState> sprites;
        public List<CommonResourceState> backgrounds;
        public List<CommonResourceState> stills;
        public CaptionState caption;
        public MessageBoxState messageBox;
        public FadeState fade;
        public AudioState audio;

        public ScenarioSyncPoint Copy()
        {
            ScenarioSyncPoint copied = new()
            {
                currentStatementIndex = currentStatementIndex,
                actors = new(),
                sprites = new(),
                backgrounds = new(),
                stills = new(),
                caption = caption.Copy(),
                messageBox = messageBox.Copy(),
                fade = fade.Copy(),
                audio = audio.Copy()
            };

            foreach (var a in actors)
            {
                copied.actors.Add(a.Copy());
            }
            foreach (var s in sprites)
            {
                copied.sprites.Add(s.Copy());
            }
            foreach (var b in backgrounds)
            {
                copied.backgrounds.Add(b.Copy());
            }
            foreach (var s in stills)
            {
                copied.stills.Add(s.Copy());
            }

            return copied;
        }
        
        public bool Equals(ScenarioSyncPoint other)
        {
            return currentStatementIndex == other.currentStatementIndex &&
                   actors.SequenceEqual(other.actors) &&
                   sprites.SequenceEqual(other.sprites) &&
                   backgrounds.SequenceEqual(other.backgrounds) &&
                   stills.SequenceEqual(other.stills) &&
                   caption.Equals(other.caption) &&
                   messageBox.Equals(other.messageBox) &&
                   fade.Equals(other.fade) &&
                   audio.Equals(other.audio);
        }

        public override bool Equals(object obj)
        {
            return obj is ScenarioSyncPoint other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(currentStatementIndex);
            foreach (var a in actors)
            {
                hashCode.Add(a);
            }
            foreach (var s in sprites)
            {
                hashCode.Add(s);
            }
            foreach (var b in backgrounds)
            {
                hashCode.Add(b);
            }
            foreach (var s in stills)
            {
                hashCode.Add(s);
            }
            hashCode.Add(caption);
            hashCode.Add(messageBox);
            hashCode.Add(fade);
            hashCode.Add(audio);
            return hashCode.ToHashCode();
        }
    }

    [Serializable]
    public struct EntityTransform : IState<EntityTransform>
    {
        public bool enabled;
        public Vector3 position;
        public float angle;
        public float scale;
        public Vector2 pivot;
        public Color color;

        public EntityTransform Copy()
        {
            return this;
        }

        public bool Equals(EntityTransform other)
        {
            return enabled == other.enabled &&
                   position.Equals(other.position) &&
                   angle.Equals(other.angle) &&
                   scale.Equals(other.scale) &&
                   pivot.Equals(other.pivot) &&
                   color.Equals(other.color);
        }

        public override bool Equals(object obj)
        {
            return obj is EntityTransform other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(enabled);
            hashCode.Add(position);
            hashCode.Add(angle);
            hashCode.Add(scale);
            hashCode.Add(pivot);
            hashCode.Add(color);
            return hashCode.ToHashCode();
        }
    }

    [Serializable]
    public struct ActorState : IState<ActorState>
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

        public ActorState Copy()
        {
            ActorState copied = new()
            {
                name = name,
                currentMotion = currentMotion,
                currentFaceMotion = currentFaceMotion,
                layer = layer,
                hidden = hidden,
                eyeBlink = eyeBlink,
                manualEyeOpen = manualEyeOpen,
                mouthSynced = new(),
                faceAngle = faceAngle,
                bodyAngle = bodyAngle,
                addEye = addEye,
                transform = transform.Copy()
            };

            foreach (var m in mouthSynced)
            {
                copied.mouthSynced.Add(m);
            }

            return copied;
        }

        public bool Equals(ActorState other)
        {
            return name == other.name &&
                   currentMotion == other.currentMotion &&
                   currentFaceMotion == other.currentFaceMotion &&
                   layer == other.layer &&
                   hidden == other.hidden &&
                   eyeBlink == other.eyeBlink &&
                   manualEyeOpen == other.manualEyeOpen &&
                   mouthSynced.SequenceEqual(other.mouthSynced) &&
                   faceAngle.Equals(other.faceAngle) &&
                   bodyAngle.Equals(other.bodyAngle) &&
                   addEye.Equals(other.addEye) &&
                   transform.Equals(other.transform);
        }

        public override bool Equals(object obj)
        {
            return obj is ActorState other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(name);
            hashCode.Add(currentMotion);
            hashCode.Add(currentFaceMotion);
            hashCode.Add(layer);
            hashCode.Add(hidden);
            hashCode.Add(eyeBlink);
            hashCode.Add(manualEyeOpen);
            foreach (var m in mouthSynced)
            {
                hashCode.Add(m);
            }
            hashCode.Add(faceAngle);
            hashCode.Add(bodyAngle);
            hashCode.Add(addEye);
            hashCode.Add(transform);
            return hashCode.ToHashCode();
        }
    }

    [Serializable]
    public struct CommonResourceState : IState<CommonResourceState>
    {
        public string name;
        public string resourceName;

        public EntityTransform transform;

        public CommonResourceState Copy()
        {
            return new()
            {
                name = name,
                resourceName = resourceName,
                transform = transform.Copy()
            };
        }

        public bool Equals(CommonResourceState other)
        {
            return name == other.name &&
                   resourceName == other.resourceName &&
                   transform.Equals(other.transform);
        }

        public override bool Equals(object obj)
        {
            return obj is CommonResourceState other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(name);
            hashCode.Add(resourceName);
            hashCode.Add(transform);
            return hashCode.ToHashCode();
        }
    }

    [Serializable]
    public struct CaptionState : IState<CaptionState>
    {
        public bool enabled;
        public Color boxColor;
        public string text;
        public float textAlpha;

        public CaptionState Copy()
        {
            return this;
        }

        public bool Equals(CaptionState other)
        {
            return enabled == other.enabled &&
                   boxColor.Equals(other.boxColor) &&
                   text == other.text &&
                   textAlpha.Equals(other.textAlpha);
        }

        public override bool Equals(object obj)
        {
            return obj is CaptionState other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(enabled);
            hashCode.Add(boxColor);
            hashCode.Add(text);
            hashCode.Add(textAlpha);
            return hashCode.ToHashCode();
        }
    }

    [Serializable]
    public struct MessageBoxState : IState<MessageBoxState>
    {
        public bool enabled;
        public string speaker;
        public string message;

        public MessageBoxState Copy()
        {
            return this;
        }

        public bool Equals(MessageBoxState other)
        {
            return enabled == other.enabled &&
                   speaker == other.speaker &&
                   message == other.message;
        }

        public override bool Equals(object obj)
        {
            return obj is MessageBoxState other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(enabled);
            hashCode.Add(speaker);
            hashCode.Add(message);
            return hashCode.ToHashCode();
        }
    }

    [Serializable]
    public struct FadeState : IState<FadeState>
    {
        public float progress;

        public FadeState Copy()
        {
            return this;
        }

        public bool Equals(FadeState other)
        {
            return progress.Equals(other.progress);
        }

        public override bool Equals(object obj)
        {
            return obj is FadeState other && Equals(other);
        }

        public override int GetHashCode()
        {
            return progress.GetHashCode();
        }
    }

    [Serializable]
    public struct AudioState : IState<AudioState>
    {
        public bool bgmPlaying;
        public string bgmName;
        public float bgmVolume;

        public AudioState Copy()
        {
            return this;
        }

        public bool Equals(AudioState other)
        {
            return bgmPlaying == other.bgmPlaying &&
                   bgmName == other.bgmName &&
                   bgmVolume.Equals(other.bgmVolume);
        }

        public override bool Equals(object obj)
        {
            return obj is AudioState other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(bgmPlaying);
            hashCode.Add(bgmName);
            hashCode.Add(bgmVolume);
            return hashCode.ToHashCode();
        }
    }
}