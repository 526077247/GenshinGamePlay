using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using YooAsset;

namespace TaoTie
{
    public class SoundManager: IManager
    {
        public static SoundManager Instance;
        private List<AudioSource> soundsPool = new List<AudioSource>();
        private List<AudioSource> httpAudioPool = new List<AudioSource>();
        //背景音乐默认音量
        private float initMusicVolume = 1f;
        private int initSoundsCount = 3;
        private Dictionary<string, AudioClip> sounds = new Dictionary<string,AudioClip> ();

        public int MusicVolume;
        public int SoundVolume;

        public string CurMusic;
        public ArrayList ValueList = new ArrayList(){-80,-30, -20, -10, -5, 0, 1, 2, 4, 6, 10};
        

        private AudioSource bgm;

        private Transform soundsRoot;

        private GameObject soundsClipClone;
        
        public AudioMixer BGM;
        public AudioMixer Sound;
        
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
            var temp = await ResourcesManager.Instance.LoadAsync<GameObject>("Audio/Common/BGMManager.prefab");
            var go = GameObject.Instantiate(temp);
            bgm = go.GetComponent<AudioSource>();
            bgm.transform.SetParent(soundsRoot);
            initMusicVolume = bgm.volume;
            soundsClipClone = await ResourcesManager.Instance.LoadAsync<GameObject>("Audio/Common/Source.prefab");

            for (int i = 0; i < initSoundsCount; i++)
            {
                var item = await CreateClipSource();
                item.gameObject.SetActive(false);
                soundsPool.Add(item);
            }
            
            BGM = bgm.outputAudioMixerGroup.audioMixer;
            Sound = soundsClipClone.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer;
            
            //给个初始值覆盖
            SoundVolume = -1;
            MusicVolume = -1;
            SetMusicVolume(CacheManager.Instance.GetInt(CacheKeys.MusicVolume, 5));
            SetSoundVolume(CacheManager.Instance.GetInt(CacheKeys.SoundVolume, 5));
            ResourcesManager.Instance.ReleaseAsset(temp);
        }

        public void Destroy()
        {
            Instance = null;
            StopMusic();
            StopAllSound();
            foreach (var item in sounds)
            {
                ResourcesManager.Instance.ReleaseAsset(item.Value);
            }

            sounds = null;
            for (int i = soundsPool.Count-1; i >=0 ; i--) 
            {
                GameObject.Destroy(soundsPool[i]);
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
        
        public async ETTask<AudioSource> CreateClipSource()
        {
            if (this.soundsClipClone == null)
            {
                //todo: 不知道谁给回收了
                soundsClipClone = await ResourcesManager.Instance.LoadAsync<GameObject>("Audio/Common/Source.prefab");
            }
            if (this.soundsClipClone == null || this.soundsRoot == null)
            {
                Log.Error("this.soundsRoot == null");
                return null;
            }

            var obj = GameObject.Instantiate(this.soundsClipClone);
            obj.transform.SetParent(this.soundsRoot, false);
            return obj.GetComponent<AudioSource>();
        }


        public void SetSoundVolume(int value)
        {
            if (this.SoundVolume != value)
            {
                this.SoundVolume = value;
                this.Sound.SetFloat("Sound", (int) this.ValueList[value]);
            }
        }
        
        public void SetMusicVolume(int value)
        {
            if (this.MusicVolume != value)
            {
                this.MusicVolume = value;
                this.BGM.SetFloat("BGM", (int) this.ValueList[value]);
            }
        }
        public async ETTask<AudioSource> GetClipSource() 
        {
            AudioSource clipSource = null;
            for (int i = 0; i <  this.soundsPool.Count; i++) {
                if ( this.soundsPool[i].gameObject.activeSelf == false) {
                    clipSource =  this.soundsPool[i];
                    break;
                }
            }
            if (clipSource == null) {
                clipSource = await this.CreateClipSource();
                if (clipSource == null) return null;
                this.soundsPool.Add(clipSource);
            }
            clipSource.gameObject.SetActive(true);
            return clipSource;
        }

        void Add(string key, AudioClip value) 
        {
           if (value == null) return;
            if (this.sounds.TryGetValue(key, out var clip) && clip!=null && clip != value)
            {
                Log.Error("相同路径不同AudioClip资源");
                return;
            }
            this.sounds[key] = value;
        }

        AudioClip Get(string key) 
        {
            if (string.IsNullOrEmpty(key) || !this.sounds.TryGetValue(key,out var res) || res == null) return null;
            return res;
        }

        public void PlayMusic(string name,string package, bool force = false)
        {
            this.CoPlayMusic(name,package,force).Coroutine();
        }
        public void PauseMusic(bool pause)
        {
            if(this.bgm == null) return;
            if(pause)
                this.bgm.Pause();
            else 
                this.bgm.UnPause();
        }
        
        public void SetBGMVolume(float value = 2)
        {
            if (this.bgm == null) return;
            if (value > 1f)
            {
                this.bgm.volume = initMusicVolume;
            }
            else
            {
                this.bgm.volume = value;
            }
        }

        private async ETTask CoPlayMusic(string name,string package,bool force)
        {
            Log.Info("CoPlayMusic");
            if (!force&&this.CurMusic == name) return;
            this.bgm.loop = true;
            AudioClip ac = this.Get(name);
            if (ac == null)
            {
                ac = await ResourcesManager.Instance.LoadAsync<AudioClip>(name,package:package);
                if (ac != null)
                {
                    var old = this.Get(CurMusic);
                    if (old!=null)
                    {
                        ResourcesManager.Instance.ReleaseAsset(old);
                        this.sounds.Remove(CurMusic);
                    }
                    this.Add(name, ac);
                }
                else {
                    Log.Info("ac is null");
                    return;
                }
            }
            if (this.CurMusic != null)
            {
                var clip = Get(CurMusic);
                if (clip != null)
                {
                    sounds.Remove(CurMusic);
                    ResourcesManager.Instance.ReleaseAsset(clip);
                }
            }
            this.bgm.clip = ac;
            this.bgm.Play();
            this.CurMusic = name;
        }

        public void StopMusic(string path = null,bool clear = false)
        {
            if(path==null||path==this.CurMusic)
            {
                if (this.bgm.clip != null)
                {
                    this.bgm.Stop();
                    var clip = Get(CurMusic);
                    if (clip != null)
                    {
                        sounds.Remove(CurMusic);
                        ResourcesManager.Instance.ReleaseAsset(clip);
                    }
                    this.bgm.clip = null;
                    if (clear)
                    {
                        var old = this.Get(CurMusic);
                        if (old != null)
                        {
                            ResourcesManager.Instance.ReleaseAsset(old);
                            this.sounds.Remove(CurMusic);
                            var package = ResourcesManager.Instance.packageFinder.GetPackageName(path);
                            PackageManager.Instance.UnloadUnusedAssets(package).Coroutine();
                        }
                        ClearMemory();
                    }
                    this.CurMusic = null;
                }
                
            }
        } 
        public void StopSound(string path,bool clear = false)
        {
            if(!string.IsNullOrEmpty(path))
            {
                var old = this.Get(path);
                if (old != null)
                {
                    for (int i = 0; i < this.soundsPool.Count; i++)
                    {
                        if (this.soundsPool[i].gameObject.activeSelf &&  this.soundsPool[i].clip == old)
                        {
                            this.soundsPool[i].Stop();
                            this.soundsPool[i].clip = null;
                        }
                    }

                    if (clear)
                    {
                        ResourcesManager.Instance.ReleaseAsset(old);
                        this.sounds.Remove(path);
                        var package = ResourcesManager.Instance.packageFinder.GetPackageName(path);
                        PackageManager.Instance.UnloadUnusedAssets(package).Coroutine();
                        ClearMemory();
                    }
                }
            }
        }
        public void ClearMemory()
        {
            GC.Collect();
            Resources.UnloadUnusedAssets();
        }
        public void PlaySound(string name,string package,ETCancellationToken token = null,bool clear = false) 
        {
            if(string.IsNullOrEmpty(name)) return;
            this.PoolPlay(name,package,token,clear).Coroutine();
        }
        
        public async ETTask PlaySoundAsync(string name,string package,ETCancellationToken token = null,bool clear = false) 
        {
            if(string.IsNullOrEmpty(name)) return;
            await this.PoolPlay(name,package,token,clear);
        }
        
        public ETTask<AudioSource> PlaySoundAsyncAndReturnSource(string name,string package,ETCancellationToken token = null,bool clear = false) 
        {
            return this.PoolPlayAndReturnSource(name,package,token,clear);
        }
        
        public async ETTask PlaySoundAsync(string name,ETCancellationToken token = null,bool clear = true)
        {
            if(string.IsNullOrEmpty(name)) return;
            string package = ResourcesManager.Instance.packageFinder.GetPackageName(name);
            await this.PoolPlay(name,package,token,clear);
        }

        public void StopAllSound() 
        {
            for (int i = 0; i < this.soundsPool.Count; i++)
            {
                if (this.soundsPool[i].gameObject.activeSelf)
                {
                    this.soundsPool[i].Stop();
                    this.soundsPool[i].clip = null;
                    // this.soundsPool[i].gameObject.SetActive(false);//不用设置SetActive,等WaitAsync
                }
            }
        }
        
        private async ETTask RecycleClipSource(AudioClip clip, AudioSource source,bool clear) {
            await TimerManager.Instance.WaitAsync((long)(clip.length*1000)+100);
            if (source.clip != clip) return;
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            if (clear) ResourcesManager.Instance.ReleaseAsset(clip);
        }
        
        private async ETTask<AudioSource> PoolPlayAndReturnSource(string name,string package,ETCancellationToken token = null,bool clear = false) 
        {
            AudioClip clip = this.Get(name);
            bool isCancel = false;
            token?.Add(() =>
            {
                isCancel = true;
            });
            
            if (clip == null)
            {
                clip =await ResourcesManager.Instance.LoadAsync<AudioClip>(name,package:package);
                if (clip == null)
                {
                    // Debug.Log("clip is null");
                    return null;
                }
                if (isCancel)
                {
                    ResourcesManager.Instance.ReleaseAsset(clip);
                    return null;
                }
                if(!clear) this.Add(name, clip);
            }
            AudioSource source = await this.GetClipSource();
            if (source == null) return source; 
            source.clip = clip;
            source.Play();
            RecycleClipSource(clip,source,clear).Coroutine();
            return source;
        }

        private async ETTask PoolPlay(string name,string package,ETCancellationToken token = null,bool clear = false)
        {
            bool isCancel = false;
            
            token?.Add(() =>
            {
                isCancel = true;
            });
            AudioClip clip = this.Get(name);
            if (clip == null)
            {
                clip =await ResourcesManager.Instance.LoadAsync<AudioClip>(name,package:package);
                if (clip == null)
                {
                    // Debug.Log("clip is null");
                    return;
                }
                if (isCancel)
                {
                    ResourcesManager.Instance.ReleaseAsset(clip);
                    return;
                }
                if (!clear)
                {
                    this.Add(name, clip);
                }
            }
            AudioSource source = await this.GetClipSource();
            if (source == null)
            {
                Log.Error("GetClipSource fail");
                return;
            }
            source.clip = clip;
            source.Play();
            bool res = await TimerManager.Instance.WaitAsync((long)(clip.length*1000)+100,token);
            if (source != null)
            {
                source.Stop();
                source.clip = null;
                source.gameObject.SetActive(false);
            }
            if (clear) ResourcesManager.Instance.ReleaseAsset(clip);
        }

        #region 播放在线音频

        private async ETTask<AudioClip> GetOnlineClip(string url,int tryCount = 3,ETCancellationToken cancel = null)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, url.GetHashCode());

                var clip = await HttpManager.Instance.HttpGetSoundOnline(url, true, cancelToken:cancel);
                if (clip != null) //本地已经存在
                {
                    return clip;
                }
                else
                {
                    for (int i = 0; i < tryCount; i++)
                    {
                        clip = await HttpManager.Instance.HttpGetSoundOnline(url, false, cancelToken:cancel);
                        if (clip != null) break;
                    }

                    if (clip != null)
                    {
                        var bytes = GetRealAudio(clip);
                        int hz = clip.frequency;
                        int channels = clip.channels;
                        int samples = clip.samples;
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            var path = HttpManager.Instance.LocalFile(url,"downloadSound",".wav");
                            using(FileStream fs = CreateEmpty(path))
                            {
                                fs.Write(bytes, 0, bytes.Length);
                                WriteHeader(fs, hz, channels, samples);
                            }
                        });
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
        private void WriteHeader(FileStream stream, int hz,int channels,int samples)
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
 
            UInt16 blockAlign = (ushort)(channels * 2);
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
                short temshort = (short)(soundata[i] * rescaleFactor);
                byte[] temdata = BitConverter.GetBytes(temshort);
                outData[i * 2] = temdata[0];
                outData[i * 2 + 1] = temdata[1];
            }
            //Debug.Log("position=" + position + "  outData.leng=" + outData.Length);
            return outData;
        }

        public async ETTask PlayHttpAudio(string url, bool loop = false,ETCancellationToken cancel = null)
        {
            AudioClip clip = await GetOnlineClip(url, cancel:cancel);
            if (clip == null)
            {
                Log.Error("Failed to load audio clip from URL: " + url);
                return;
            }

            AudioSource source = await GetClipSource();
            if (source == null)
            {
                Log.Error("Failed to get audio source.");
                return;
            }

            source.clip = clip;
            source.loop = loop;
            source.Play();
            if (!httpAudioPool.Contains(source))
            {
                httpAudioPool.Add(source);
            }
            if (loop) return;
            // Wait for the clip to finish playing
            await TimerManager.Instance.WaitAsync((long)(clip.length * 1000) + 100, cancel);
            if (source != null)
            {
                source.Stop();
                if (source.clip != null) 
                    GameObject.Destroy(source.clip);
                source.clip = null;
                source.gameObject.SetActive(false);
                httpAudioPool.Remove(source);
            }
        }
        
        public bool IsHttpAudioPlaying()
        {
            return httpAudioPool.Count>0;
        }
        
        public float GetHttpAudioCurrentTime()
        {
            if (!IsHttpAudioPlaying()) return 0f;
            foreach (var source in httpAudioPool)
            {
                if (source.clip != null && source.isPlaying)
                {
                    return source.time;
                }
            }
            return 0f;
        }
        
        public void StopHttpAudio()
        {
            foreach (var source in httpAudioPool)
            {
                if (source == null) continue;
                source.Stop();
                if (source.clip != null) 
                    GameObject.Destroy(source.clip);
                source.clip = null;
                source.gameObject.SetActive(false);
            }
            httpAudioPool.Clear();
        }
        #endregion
    }
}