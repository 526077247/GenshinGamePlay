using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

namespace TaoTie
{
    public class SoundManager : IManager
    {
        private class SoundItem : IDisposable
        {
            public bool IsHttp;
            public long Id;
            public AudioSource AudioSource;
            public AudioClip Clip;
            public string Path;
            public ETCancellationToken Token;

            private bool isLoading;

            public static SoundItem Create(string path, bool isHttp, AudioSource audioSource,
                ETCancellationToken cancel = null)
            {
                SoundItem res = ObjectPool.Instance.Fetch<SoundItem>();
                res.Id = IdGenerater.Instance.GenerateId();
                res.Path = path;
                res.IsHttp = isHttp;
                res.AudioSource = audioSource;
                if (cancel == null)
                    res.Token = new ETCancellationToken();
                else
                    res.Token = cancel;
                return res;
            }

            public async ETTask LoadClip()
            {
                if (Clip == null)
                {
                    var id = Id;
                    isLoading = true;
                    if (IsHttp)
                    {
                        var clip = await GetOnlineClip(Path);
                        isLoading = false;
                        if (Id != id)
                        {
                            GameObject.Destroy(clip);
                            ObjectPool.Instance.Recycle(this);
                            return;
                        }
                        if(clip == null) return;
                        Clip = clip;
                    }
                    else
                    {
                        var clip = await ResourcesManager.Instance.LoadAsync<AudioClip>(Path);
                        isLoading = false;
                        if (Id != id)
                        {
                            ResourcesManager.Instance.ReleaseAsset(clip);
                            ObjectPool.Instance.Recycle(this);
                            return;
                        }
                        if(clip == null) return;
                        Clip = clip;
                    }
                    AudioSource.clip = Clip;
                }
            }

            #region 在线音频

            private async ETTask<AudioClip> GetOnlineClip(string url, int tryCount = 3,
                ETCancellationToken cancel = null)
            {
                if (string.IsNullOrWhiteSpace(url)) return null;
                CoroutineLock coroutineLock = null;
                try
                {
                    coroutineLock =
                        await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, url.GetHashCode());

                    var clip = await HttpManager.Instance.HttpGetSoundOnline(url, true, cancelToken: cancel);
                    if (clip != null) //本地已经存在
                    {
                        return clip;
                    }
                    else
                    {
                        for (int i = 0; i < tryCount; i++)
                        {
                            clip = await HttpManager.Instance.HttpGetSoundOnline(url, false, cancelToken: cancel);
                            if (clip != null) break;
                        }

                        if (clip != null)
                        {
#if !UNITY_WEBGL || UNITY_EDITOR
                            var bytes = GetRealAudio(clip);
                            int hz = clip.frequency;
                            int channels = clip.channels;
                            int samples = clip.samples;
                            ThreadPool.QueueUserWorkItem(_ =>
                            {
                                var path = HttpManager.Instance.LocalFile(url, "downloadSound", ".wav");
                                using (FileStream fs = CreateEmpty(path))
                                {
                                    fs.Write(bytes, 0, bytes.Length);
                                    WriteHeader(fs, hz, channels, samples);
                                }
                            });
#endif
                            return clip;
                        }
                        else
                        {
                            Log.Error("网络无资源 " + url);
                        }
                    }
                }
                finally
                {
                    coroutineLock?.Dispose();
                }

                return null;
            }

            /// <summary>
            /// 创建wav格式文件头
            /// </summary>
            /// <param name="filepath"></param>
            /// <returns></returns>
            private FileStream CreateEmpty(string filepath)
            {
                FileStream fileStream = new FileStream(filepath, FileMode.Create);
                byte emptyByte = new byte();

                for (int i = 0; i < 44; i++) //为wav文件头留出空间
                {
                    fileStream.WriteByte(emptyByte);
                }

                return fileStream;
            }

            /// <summary>
            /// 写文件头
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="hz"></param>
            /// <param name="channels"></param>
            /// <param name="samples"></param>
            private void WriteHeader(Stream stream, int hz, int channels, int samples)
            {
                stream.Seek(0, SeekOrigin.Begin);

                Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
                stream.Write(riff, 0, 4);

                Byte[] chunkSize = BitConverter.GetBytes(stream.Length - 8);
                stream.Write(chunkSize, 0, 4);

                Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
                stream.Write(wave, 0, 4);

                Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
                stream.Write(fmt, 0, 4);

                Byte[] subChunk1 = BitConverter.GetBytes(16);
                stream.Write(subChunk1, 0, 4);

                UInt16 one = 1;

                Byte[] audioFormat = BitConverter.GetBytes(one);
                stream.Write(audioFormat, 0, 2);

                Byte[] numChannels = BitConverter.GetBytes(channels);
                stream.Write(numChannels, 0, 2);

                Byte[] sampleRate = BitConverter.GetBytes(hz);
                stream.Write(sampleRate, 0, 4);

                Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2);
                stream.Write(byteRate, 0, 4);

                UInt16 blockAlign = (ushort) (channels * 2);
                stream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

                UInt16 bps = 16;
                Byte[] bitsPerSample = BitConverter.GetBytes(bps);
                stream.Write(bitsPerSample, 0, 2);

                Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
                stream.Write(datastring, 0, 4);

                Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
                stream.Write(subChunk2, 0, 4);
            }

            /// <summary>
            /// 获取真正大小的录音
            /// </summary>
            /// <param name="recordedClip"></param>
            /// <returns></returns>
            private byte[] GetRealAudio(AudioClip recordedClip)
            {
                var position = recordedClip.samples;
                float[] soundata = new float[position * recordedClip.channels];
                recordedClip.GetData(soundata, 0);
                int rescaleFactor = 32767;
                byte[] outData = new byte[soundata.Length * 2];
                for (int i = 0; i < soundata.Length; i++)
                {
                    short temshort = (short) (soundata[i] * rescaleFactor);
                    byte[] temdata = BitConverter.GetBytes(temshort);
                    outData[i * 2] = temdata[0];
                    outData[i * 2 + 1] = temdata[1];
                }

                //Debug.Log("position=" + position + "  outData.leng=" + outData.Length);
                return outData;
            }

            #endregion

            public void Dispose()
            {
                if (Token != null)
                {
                    var token = Token;
                    Token = null;
                    token?.Cancel();

                    Id = 0;
                    if (Clip != null)
                    {
                        if (!IsHttp)
                        {
                            ResourcesManager.Instance.ReleaseAsset(Clip);
                        }
                        else
                        {
                            GameObject.Destroy(Clip);
                        }

                        Clip = null;
                    }

                    if (AudioSource != null)
                    {
                        AudioSource.Stop();
                        AudioSource.clip = null;
                        Instance?.soundsPool.AddLast(AudioSource);
                    }
                    AudioSource = null;
                    if(!isLoading) ObjectPool.Instance?.Recycle(this);
                }
            }
        }
        
        public const int DEFAULTVALUE = 10;
        private const int INITSOUNDCOUNT = 3;

        public static SoundManager Instance { get; private set; }

        private LinkedList<AudioSource> soundsPool = new LinkedList<AudioSource>();

        private Dictionary<long, SoundItem> sounds = new Dictionary<long, SoundItem>();

        public int MusicVolume { get; private set; }
        public int SoundVolume { get; private set; }

        private SoundItem curMusic;
        
        private Transform soundsRoot;

        private GameObject soundsClipClone;


        #region IManager

        public void Init()
        {
            Instance = this;
            InitAsync().Coroutine();
        }

        private async ETTask InitAsync()
        {
            soundsRoot = new GameObject("SoundsRoot").transform;
            GameObject.DontDestroyOnLoad(soundsRoot);
            soundsClipClone =
                await ResourcesManager.Instance.LoadAsync<GameObject>("Audio/Common/Source.prefab", isPersistent: true);

            for (int i = 0; i < INITSOUNDCOUNT; i++)
            {
                var item = CreateClipSource();
                soundsPool.AddLast(item);
            }
            
            //给个初始值覆盖
            SoundVolume = -1;
            MusicVolume = -1;
            SetMusicVolume(CacheManager.Instance.GetInt(CacheKeys.MusicVolume, DEFAULTVALUE));
            SetSoundVolume(CacheManager.Instance.GetInt(CacheKeys.SoundVolume, DEFAULTVALUE));
        }

        public void Destroy()
        {
            Instance = null;
            StopMusic();
            StopAllSound();
            foreach (var item in sounds)
            {
                item.Value.Dispose();
            }

            sounds = null;
            foreach (var item in soundsPool)
            {
                GameObject.Destroy(item);
            }

            soundsPool = null;
            ResourcesManager.Instance.ReleaseAsset(soundsClipClone);
            soundsClipClone = null;
            if (soundsRoot != null)
            {
                GameObject.Destroy(soundsRoot.gameObject);
                soundsRoot = null;
            }
        }

        #endregion

        #region Setting

        public void SetSoundVolume(int value)
        {
            if (SoundVolume != value)
            {
                SoundVolume = value;
                foreach (var item in sounds)
                {
                    if (item.Value?.AudioSource != null && item.Value!=curMusic)
                    {
                        item.Value.AudioSource.volume = SoundVolume / 10f;
                    }
                }
            }
        }

        public void SetMusicVolume(int value)
        {
            if (MusicVolume != value)
            {
                MusicVolume = value;
                if (curMusic?.AudioSource != null)
                {
                    curMusic.AudioSource.volume = MusicVolume / 10f;
                }
            }
        }

        #endregion

        #region Music

        public long PlayMusic(string path, ETCancellationToken token = null)
        {
            if (string.IsNullOrEmpty(path)) return 0;
            AudioSource source = GetClipSource();
            if (source == null)
            {
                Log.Error("GetClipSource fail");
                return 0;
            }
            source.loop = true;
            source.volume = MusicVolume / 10f;
            SoundItem res = SoundItem.Create(path, false, source, token);
            PoolPlay(res, res.Token).Coroutine();
            curMusic?.Dispose();
            curMusic = res;
            return res.Id;
        }

        public void PauseMusic(bool pause)
        {
            if (curMusic == null) return;
            if (pause)
                curMusic.AudioSource.Pause();
            else
                curMusic.AudioSource.UnPause();
        }

        public void StopMusic(long id = 0)
        {
            if (curMusic == null) return;
            if (id == 0 || id == curMusic.Id)
            {
                if (curMusic.Clip != null)
                {
                    curMusic.Dispose();
                    curMusic = null;
                }
            }
        }

        #endregion

        #region Sound

        public long PlaySound(string path, ETCancellationToken token = null, bool isLoop = false)
        {
            if (string.IsNullOrEmpty(path)) return 0;
            AudioSource source = GetClipSource();
            if (source == null)
            {
                Log.Error("GetClipSource fail");
                return 0;
            }
            source.loop = isLoop;
            source.volume = SoundVolume / 10f;
            SoundItem res = SoundItem.Create(path, false, source, token);
            PoolPlay(res, res.Token).Coroutine();
            return res.Id;
        }

        public async ETTask PlaySoundAsync(string path, ETCancellationToken token = null)
        {
            if (string.IsNullOrEmpty(path)) return;
            AudioSource source = GetClipSource();
            if (source == null)
            {
                Log.Error("GetClipSource fail");
                return;
            }

            source.loop = false;
            source.volume = SoundVolume / 10f;
            SoundItem res = SoundItem.Create(path, false, source, token);
            await PoolPlay(res, res.Token);
        }
        public long PlayHttpAudio(string url, bool loop = false, ETCancellationToken cancel = null)
        {
            if (string.IsNullOrEmpty(url)) return 0;
            AudioSource source = GetClipSource();
            if (source == null)
            {
                Log.Error("GetClipSource fail");
                return 0;
            }
            source.loop = loop;
            source.volume = SoundVolume / 10f;
            SoundItem res = SoundItem.Create(url, true, source, cancel);
            PoolPlay(res, res.Token).Coroutine();
            return res.Id;
        }
        public async ETTask PlayHttpAudioAsync(string url, bool loop = false, ETCancellationToken cancel = null)
        {
            if (string.IsNullOrEmpty(url)) return;
            AudioSource source = GetClipSource();
            if (source == null)
            {
                Log.Error("GetClipSource fail");
                return;
            }
            source.loop = loop;
            source.volume = SoundVolume / 10f;
            SoundItem res = SoundItem.Create(url, true, source, cancel);
            await PoolPlay(res, res.Token);
        }

        public void StopSound(long id)
        {
            var old = Get(id);
            if (old != null)
            {
                old.Dispose();
                sounds.Remove(id);
            }
        }

        public void StopAllSound()
        {
            foreach (var item in sounds)
            {
                if (item.Value != curMusic)
                {
                    item.Value.Dispose();
                }
            }
            sounds.Clear();
            if (this.curMusic != null) 
            {
                this.sounds.Add(this.curMusic.Id, this.curMusic);
            }
        }

        private async ETTask PoolPlay(SoundItem soundItem, ETCancellationToken token)
        {
            var id = soundItem.Id;
            Add(soundItem.Id, soundItem);
            await soundItem.LoadClip();
            if (soundItem.Clip == null)
            {
                return;
            }
            if (token.IsCancel())
            {
                soundItem.Dispose();
                return;
            }
            soundItem.AudioSource.Play();
            if (soundItem.AudioSource.loop) return;
            await TimerManager.Instance.WaitAsync((long) (soundItem.Clip.length * 1000) + 100, token);
            if (soundItem.Id == id)
            {
                //回来可能被其他提前终止了
                soundItem.Dispose();
            }
        }

        #endregion

        #region Clip

        private AudioSource CreateClipSource()
        {
            if (soundsClipClone == null || soundsRoot == null)
            {
                Log.Error("soundsRoot == null");
                return null;
            }

            var obj = GameObject.Instantiate(soundsClipClone);
            obj.transform.SetParent(soundsRoot, false);
            return obj.GetComponent<AudioSource>();
        }

        private AudioSource GetClipSource()
        {
            AudioSource clipSource = null;
            if (soundsPool.Count > 0)
            {
                clipSource = soundsPool.First.Value;
                soundsPool.RemoveFirst();
            }

            if (clipSource == null)
            {
                clipSource = CreateClipSource();
                if (clipSource == null) return null;
            }
            
            return clipSource;
        }

        void Add(long id, SoundItem value)
        {
            if (value == null) return;
            sounds[id] = value;
        }

        SoundItem Get(long id)
        {
            if (!sounds.TryGetValue(id, out var res) || res == null) return null;
            return res;
        }

        #endregion
    }
}