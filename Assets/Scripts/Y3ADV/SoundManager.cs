using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Networking;

namespace Y3ADV
{
    public class SoundManager : SingletonMonoBehaviour<SoundManager>
    {
        public List<AudioSource> SESources = null;
        public AudioSource BGMSource = null;
        public AudioSource VoiceSource = null;

        private Dictionary<string, AudioSource> PlayingSE = new();

        public Dictionary<string, AudioClip> LoadedSE = null;
        public Dictionary<string, AudioClip> LoadedBGM = null;
        public Dictionary<string, AudioClip> LoadedVoice = null;

        private float currentBGMVolume = 1.0f;
        private float currentVoiceVolume = 1.0f;
        private float currentSEVolume = 1.0f;

        private float masterVolume = 1.0f;
        private float bgmVolume = 1.0f;
        private float voiceVolume = 1.0f;
        private float seVolume = 1.0f;

        public void SetMasterVolume(float volume)
        {
            if (StartupSettings.TestMode)
            {
                volume = 0.0f;
            }
            masterVolume = Mathf.Clamp01(volume);
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
        }

        public void SetVoiceVolume(float volume)
        {
            voiceVolume = Mathf.Clamp01(volume);
        }

        public void SetSEVolume(float volume)
        {
            seVolume = Mathf.Clamp01(volume);
        }
    
#if WEBGL_BUILD
        private struct LoopInfo
        {
            public bool loop;
            public float loopStart;
            public float loopStop;
        }

        private Dictionary<AudioClip, LoopInfo> loopInfos;
#endif

        const int FFT_SIZE = 128;

        public enum SoundType
        {
            SE,
            BGM,
            Voice
        }

        protected override void Awake()
        {
            base.Awake();

            PlayingSE = new();
#if WEBGL_BUILD
            loopInfos = new Dictionary<AudioClip, LoopInfo>();
            WebGLInterops.RegisterSoundManagerGameObject(gameObject.name);
#endif

            if (StartupSettings.TestMode)
            {
                masterVolume = 0.0f;
            }
        }

        public delegate void LoadCallback();

        public static IEnumerator LoadAudio(string assetName, string[] relativePaths, string filename, SoundType type,
            LoadCallback cb = null)
        {
            Dictionary<string, AudioClip> loaded;
            bool cacheAsLocalFile;
            switch (type)
            {
                case SoundType.SE:
                    loaded = Instance.LoadedSE;
                    cacheAsLocalFile = true;
                    break;
                case SoundType.BGM:
                    loaded = Instance.LoadedBGM;
                    cacheAsLocalFile = true;
                    break;
                case SoundType.Voice:
                    loaded = Instance.LoadedVoice;
                    cacheAsLocalFile = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (loaded.ContainsKey(assetName))
            {
                cb?.Invoke();
                yield break;
            }

            var relativeFilenames = relativePaths.Select(path => $"{path}/{filename}").ToArray();
            yield return LocalResourceManager.LoadAudioClipFromFile(
                relativeFilenames, cacheAsLocalFile, type == SoundType.BGM,
                audioClip =>
                {
                    audioClip.name = assetName;
                    if (loaded.ContainsKey(assetName))
                    {
                        Destroy(loaded[assetName]);
                        loaded[assetName] = audioClip;
                    }
                    else
                    {
                        loaded.Add(assetName, audioClip);
                    }

                    cb?.Invoke();
                });

            yield return null;
        }


        protected override void OnEnable()
        {
            base.OnEnable();

            LoadedSE ??= new();
            LoadedBGM ??= new();
            LoadedVoice ??= new();
        }

        public static bool IsInvalidVoice(string voiceName)
        {
            return voiceName.StartsWith("null") || string.IsNullOrWhiteSpace(voiceName);
        }

        public void PlayVoice(string voiceName)
        {
            StopVoice();

            // Special case
            if (IsInvalidVoice(voiceName))
            {
                VoiceSource.clip = null;
                return;
            }
            
            if (!LoadedVoice.ContainsKey(voiceName))
            {
                Debug.LogWarning($"Voice {voiceName} not found!");
                VoiceSource.clip = null;
                return;
            }

            AudioClip voice = LoadedVoice[voiceName];

            VoiceSource.clip = voice;
            VoiceSource.volume = currentVoiceVolume * voiceVolume * masterVolume;
            VoiceSource.Play();
#if WEBGL_BUILD
            FixChannelInfinite(voice.length);
            StartSampling(voice.name, voice.length, FFT_SIZE);
#endif
        }

        public void StopVoice()
        {
            if (VoiceSource == null) return;
#if WEBGL_BUILD
            if (VoiceSource.clip != null)
                CloseSampling(VoiceSource.clip.name);
#endif

            VoiceSource.Stop();
        }

        public bool IsVoicePlaying()
        {
            // if (VoiceSource.clip == null) return false;
            // return VoiceSource.time < VoiceSource.clip.length;
            return VoiceSource.isPlaying;
        }

        public float GetVoiceVolume(float timeDelay)
        {
            if (VoiceSource.clip == null) return 0.0f;
            if (!VoiceSource.isPlaying) return 0.0f;
            return GetCurrentVolume(VoiceSource, timeDelay);
        }


        private float GetCurrentVolume(AudioSource audio, float delay = 0.0f)
        {
            float[] data = new float[FFT_SIZE];
            float sum = 0;
            // TODO: apply delay
#if WEBGL_BUILD
            //StartSampling(audio.clip.name, audio.clip.length, FFT_SIZE);
            GetSamples(audio.clip.name, data, data.Length);
#else
            audio.GetSpectrumData(data, 0, FFTWindow.Rectangular);
#endif
            foreach (float s in data)
            {
                sum += Mathf.Abs(s);
            }

            return sum / FFT_SIZE;
        }

        public void PlaySE(string seName, float volume, float duration, bool loop)
        {
            void Play(AudioSource source, AudioClip clip, bool loop)
            {
                source.clip = clip;
                source.volume = 0.0f;
                source.loop = loop;
                StartCoroutine(Fade(SoundType.SE, duration, 0.0f, volume, null));
                source.Play();
                
#if WEBGL_BUILD
                FixChannelInfinite(clip.length);
#endif
            }
            
            AudioSource seSource;
            if (SESources.Any(s => !s.isPlaying))
            {
                seSource = SESources.First(s => !s.isPlaying);
            }
            else
            {
                Debug.LogWarning("Can't find an available sound effect slot, choosing the first one and forcing stop.");
                seSource = SESources[0];
                seSource.Stop();
                ClearStoppedSESource();
            }
            
            if (!LoadedSE.ContainsKey(seName))
            {
                StartCoroutine(LoadSECommand.LoadSE(seName, () =>
                {
                    if (!LoadedSE.ContainsKey(seName))
                    {
                        Debug.LogError($"SE {seName} not found!");
                        return;
                    }
                    AudioClip se2 = LoadedSE[seName];

                    Debug.LogWarning($"SE {seName} not found but successfully loaded on the fly.");
                    Play(seSource, se2, loop);
                }));
                return;
            }

            AudioClip se = LoadedSE[seName];

            PlayingSE[seName] = seSource;

            Play(seSource, se, loop);
        }

        private void ClearStoppedSESource()
        {
            List<string> audioToBeRemoved = PlayingSE
                .Where(source => !source.Value.isPlaying)
                .Select(source => source.Key)
                .ToList();

            foreach (var seName in audioToBeRemoved)
            {
                PlayingSE.Remove(seName);
            }
        }

        private void Update()
        {
            if (BGMSource != null)
                BGMSource.volume = currentBGMVolume * bgmVolume * masterVolume;
            if (VoiceSource != null)
                VoiceSource.volume = currentVoiceVolume * voiceVolume * masterVolume;
            float seFinalVolume = currentSEVolume * seVolume * masterVolume;
            SESources.ForEach(s => { if (s != null) s.volume = seFinalVolume; });
            
            ClearStoppedSESource();
        }

        private delegate void Callback();

        private IEnumerator Fade(SoundType type, float time, float fromVolume, float toVolume, Callback callback)
        {
            void SetCurrentVolume(float volume)
            {
                switch (type)
                {
                    case SoundType.SE:
                        currentSEVolume = volume;
                        break;
                    case SoundType.BGM:
                        currentBGMVolume = volume;
                        break;
                    case SoundType.Voice:
                        currentVoiceVolume = volume;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
            
            if (time <= 0)
            {
                SetCurrentVolume(toVolume);
                yield break;
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(DOVirtual.Float(fromVolume, toVolume, time, SetCurrentVolume));
            seq.OnComplete(() => callback?.Invoke());

            yield return seq.WaitForCompletion();
        }

        public void StopSE(string seName, float duration)
        {
            if (!PlayingSE.ContainsKey(seName))
            {
                return;
            }

            var seSource = PlayingSE[seName];
            if (duration <= 0)
            {
                seSource.Stop();
                PlayingSE.Remove(seName);
                return;
            }

            StartCoroutine(Fade(SoundType.SE, duration, currentSEVolume, 0,
                () =>
                {
                    if (seSource != null)
                    {
                        seSource.Stop();
                        PlayingSE.Remove(seName);
                    }
                }));
        }

        public void PlayBGM(string bgmName, float volume)
        {
            void Play(AudioClip clip)
            {
                currentBGMVolume = volume;
                BGMSource.clip = clip;
                BGMSource.volume = currentBGMVolume * bgmVolume * masterVolume;
                BGMSource.loop = true;
                BGMSource.Play();
#if WEBGL_BUILD
                FixChannelInfinite(clip.length);
                if (loopInfos.ContainsKey(clip))
                {
                    LoopInfo info = loopInfos[clip];
                    SetChannelLoop(clip.length, info.loop, (float) info.loopStart / clip.frequency, (float) info.loopStop / clip.frequency);
                }
#endif
            }
            
            StopBGM(0);
            
            if (!LoadedBGM.ContainsKey(bgmName))
            {
                StartCoroutine(LoadBGMCommand.LoadBGM(bgmName, () =>
                {
                    if (!LoadedBGM.ContainsKey(bgmName))
                    {
                        Debug.LogError($"BGM {bgmName} not found!");
                        return;
                    }
                    
                    AudioClip bgm2 = LoadedBGM[bgmName];

                    Debug.LogWarning($"BGM {bgmName} not found but successfully loaded on the fly.");
                    Play(bgm2);
                }));
                return;
            }

            AudioClip bgm = LoadedBGM[bgmName];

            Play(bgm);
        }

        public void StopBGM(float duration)
        {
            if (duration <= 0)
            {
                BGMSource.Stop();
                return;
            }

            StartCoroutine(Fade(SoundType.BGM, duration, currentBGMVolume, 0,
                () =>
                {
                    if (BGMSource != null) BGMSource.Stop();
                }));
        }

        public bool IsBGMPlaying()
        {
            return BGMSource.isPlaying;
        }


#if WEBGL_BUILD
        [DllImport("__Internal")]
        private static extern bool StartSampling(string name, float duration, int bufferSize);

        [DllImport("__Internal")]
        private static extern bool CloseSampling(string name);

        [DllImport("__Internal")]
        private static extern bool GetSamples(string name, float[] freqData, int size);

        [DllImport("__Internal")]
        private static extern bool SetChannelLoop(float duration, bool loop, float loopStart, float loopStop);

        [DllImport("__Internal")]
        private static extern void FixChannelInfinite(float duration);

        public static void AnalyzeLoopInfo(AudioClip audioClip, byte[] binaryData)
        {
            bool CompareSlice(byte[] a1, int offset1, byte[] a2, int offset2, int size)
            {
                bool equal = true;
                for (int i = 0; i < size; ++i)
                {
                    if (a1[offset1 + i] == a2[offset2 + i]) continue;
                    equal = false;
                }

                return equal;
            }

            int index = 0;
            
            // check RIFF header
            byte[] riffHeader = System.Text.Encoding.ASCII.GetBytes("RIFF");
            bool isRiff = CompareSlice(binaryData, 0, riffHeader, 0, 4);
            if (!isRiff) return;


            // find smpl header and data header
            // data header is too big so we need to skip it
            byte[] smplHeader = System.Text.Encoding.ASCII.GetBytes("smpl");
            byte[] dataHeader = System.Text.Encoding.ASCII.GetBytes("data");
            bool smplHeaderFound = false;
            for (index = 0; index < binaryData.Length - 4; ++index)
            {
                bool isSmpl = CompareSlice(binaryData, index, smplHeader, 0, 4);
                if (isSmpl)
                {
                    smplHeaderFound = true;
                    break;
                }
                bool isData = CompareSlice(binaryData, index, dataHeader, 0, 4);
                if (!isData) continue;
                index += 4;
                int dataChunkSize = BitConverter.ToInt32(binaryData, index);
                index += dataChunkSize + 4;
            }
            if (!smplHeaderFound) return;

            index += 4;
            if (index == binaryData.Length) return;

            int smplChunkSize = BitConverter.ToInt32(binaryData, index);
            index += 4;
            
            index += 4; // manufacturer
            index += 4; // product
            index += 4; // sample period
            index += 4; // MIDI unity note
            index += 4; // MIDI pitch fraction
            index += 4; // SMPTE format
            index += 4; // SMPTE offset
            
            int numberOfSampleLoops = BitConverter.ToInt32(binaryData, index);
            index += 4;
            
            index += 4; // sample data
            
            // we don't care about number of sample loops, only take the first one
            //for (int i = 0; i < numberOfSampleLoops; ++i)
            {
                index += 4; // ID
                index += 4; // type

                int loopStart = BitConverter.ToInt32(binaryData, index);
                index += 4;

                int loopEnd = BitConverter.ToInt32(binaryData, index);
                index += 4;

                index += 4; // fraction
                index += 4; // number of times to play the loop
                
                
                LoopInfo info = new LoopInfo
                {
                    loop = loopStart != 0 && loopEnd != 0,
                    loopStart = loopStart,
                    loopStop = loopEnd
                };
                if (Instance.loopInfos.ContainsKey(audioClip))
                    Instance.loopInfos[audioClip] = info;
                else
                    Instance.loopInfos.Add(audioClip, info);
                return;
            }
        }
#endif
    }
}