using System;
using System.Collections.Generic;
#if RoslynAnalyzer
using Unity.Code.NinoGen;
#endif
using UnityEngine;

namespace TaoTie
{
    public class EnvironmentManager: IManager, IUpdate
    {
        public static EnvironmentManager Instance { get; private set; }
        //游戏世界一天时间，换算成GameTime的总时长，下同
        const int mDayTimeCount = 120_000;
        //早上开始时间
        const int mMorningTimeStart = 0;
        //中午开始时间
        const int mNoonTimeStart = 10_000;
        //下午开始时间
        const int mAfterNoonTimeStart = 70_000;
        //晚上开始时间
        const int mNightTimeStart = 80_000;

        private bool isLoad = false;
        
        private PriorityStack<EnvironmentRunner> envInfoStack;
        private Dictionary<long, EnvironmentRunner> envInfoMap;

        private EnvironmentRunner curRunner;
        private EnvironmentInfo preInfo;
        private EnvironmentInfo curInfo;

        public ConfigBlender DefaultBlend;
        
        public int DayTimeCount{ get; private set; }
        public int MorningTimeStart { get; private set; }
        public int NoonTimeStart{ get; private set; }
        public int AfterNoonTimeStart{ get; private set; }
        public int NightTimeStart{ get; private set; }
        public long NowTime{ get; private set; }

        private Dictionary<int, ConfigEnvironment> configs;

        #region SkyBox

        private Material skybox;

        #endregion
        
        #region IManager

        public void Init()
        {
            isLoad = false;
            Instance = this;
            DayTimeCount = mDayTimeCount;
            MorningTimeStart = mMorningTimeStart;
            NoonTimeStart = mNoonTimeStart;
            AfterNoonTimeStart = mAfterNoonTimeStart;
            NightTimeStart = mNightTimeStart;
        }

        private async ETTask<ConfigEnvironments> GetConfig(string path = "EditConfig/Others/ConfigEnvironments")
        {
            if (Define.ConfigType == 0)
            {
                var jStr = await ResourcesManager.Instance.LoadConfigJsonAsync(path);
                return JsonHelper.FromJson<ConfigEnvironments>(jStr);
            }
#if RoslynAnalyzer
            else
            {
                var bytes = await ResourcesManager.Instance.LoadConfigBytesAsync(path);
                Deserializer.Deserialize(bytes,out ConfigEnvironments res);
                return res;
            }
#endif
            Log.Error($"GetConfig 失败，ConfigType = {Define.ConfigType} 未处理");
            return null;
        }
        public async ETTask LoadAsync()
        {
            skybox = await MaterialManager.Instance.LoadMaterialAsync("SkyBox/Skybox_SkyboxBlender.mat");
            RenderSettings.skybox = skybox;
          
            #region Config
            
            var config = await GetConfig();
            DefaultBlend = config.DefaultBlend;
            var defaultEnvironmentId = config.DefaultEnvironment.Id;
            configs = new Dictionary<int, ConfigEnvironment>();
            configs.Add(config.DefaultEnvironment.Id,config.DefaultEnvironment);
            if (config.Environments != null)
            {
                for (int i = 0; i < config.Environments.Length; i++)
                {
                    configs.Add(config.Environments[i].Id,config.Environments[i]);
                }
            }
            #endregion

            envInfoStack = new PriorityStack<EnvironmentRunner>();
            envInfoMap = new Dictionary<long, EnvironmentRunner>();
            
            NowTime = GameTimerManager.Instance.GetTimeNow();
            Create(defaultEnvironmentId, EnvironmentPriorityType.Default);
            isLoad = true;
        }

        public void Destroy()
        {
            foreach (var item in envInfoMap)
            {
                item.Value.Dispose();
            }
            envInfoStack = null;
            envInfoMap = null;
            DefaultBlend = null;
            configs = null;
            Instance = null;
        }
        
        public void Update()
        {
            if(!isLoad) return;
            NowTime = GameTimerManager.Instance.GetTimeNow();
            NowTime %= DayTimeCount;
            foreach (var item in envInfoStack.Data)
            {
                if(item.Value == null) continue;
                for (int i = 0; i < item.Value.Count; i++)
                {
                    item.Value[i]?.Update();
                }
            }
            if (envInfoStack.Count == 0) return;

            Process();
            if (curRunner != null)
            {
                ApplyEnvironmentInfo(curRunner.Data);
            }
            else
            {
                ApplyEnvironmentInfo(null);
            }
        }
        #endregion

        private void Process()
        {
            var top = envInfoStack.Peek();
            if (curRunner != top)//栈顶环境变更，需要变换
            {
                if (curRunner == null)
                {
                    curRunner = top;
                    return;
                }
                if (curRunner is BlenderEnvironmentRunner blender) //正在变换
                {
                    return;
                    // 中途改变这种，天空盒插值不了，得等正在变换的变换完
                    // envInfoStack.Pop();
                    // while (envInfoStack.Peek().IsOver) //移除已经over的
                    // {
                    //     envInfoStack.Pop().Dispose();
                    // }
                    //
                    // var newTop = envInfoStack.Peek();
                    // blender.ChangeTo(newTop as NormalEnvironmentRunner, false);
                    // envInfoStack.Push(blender);
                }
                else//变换到下一个环境
                {
                    while (envInfoStack.Count>0 && envInfoStack.Peek().IsOver)//移除已经over的
                    {
                        envInfoStack.Pop().Dispose();
                    }
                    top = envInfoStack.Peek();
                    if (top is BlenderEnvironmentRunner) //正在变换
                    {
                        return;
                    }

                    if (curRunner != null && top != null && top != curRunner)
                    {
                        blender = CreateRunner(curRunner as NormalEnvironmentRunner,
                            envInfoStack.Peek() as NormalEnvironmentRunner,
                            true);
                        envInfoStack.Push(blender);
                        curRunner = blender;
                    }
                }
            }
            else if (top.IsOver) //播放完毕，需要变换环境
            {
                if (top is BlenderEnvironmentRunner blender) //正在变换
                {
                    envInfoStack.Pop();
                    var newTop = envInfoStack.Peek();
                    if (blender.To.Id == newTop.Id) //是变换完成了
                    {
                        curRunner = envInfoStack.Peek();
                        top.Dispose();
                    }
                    else //变换时，目标环境改变，需要变换到新的环境
                    {
                        blender.ChangeTo(newTop as NormalEnvironmentRunner, false);
                        envInfoStack.Push(blender);
                    }
                }
                else //一般环境被销毁，需要出栈，变换到下一个环境
                {
                    envInfoStack.Pop();
                    while (envInfoStack.Peek().IsOver) //移除已经over的
                    {
                        envInfoStack.Pop().Dispose();
                    }
                    if (envInfoStack.Count <= 0)
                    {
                        top.Dispose();
                        curRunner = null;
                        return;
                    }
                    blender = CreateRunner(top as NormalEnvironmentRunner, envInfoStack.Peek() as
                        NormalEnvironmentRunner, false);
                    envInfoStack.Push(blender);
                    curRunner = blender;
                }
            }
        }
        private void ApplyEnvironmentInfo(EnvironmentInfo info)
        {
            preInfo = curInfo;
            curInfo = info;
            if (preInfo == curInfo && (info == null || !info.Changed)) return;
            
            //todo:
            RenderSettings.skybox = skybox;
            if (skybox != null)
            {
                if (curInfo.IsBlender)
                {
                    skybox.SetTexture("_Tex2",curInfo.SkyCube2);
                    skybox.SetColor("_Tint2",curInfo.TintColor2);
                    skybox.SetFloat("_BlendCubemaps",curInfo.Progress);
                }
                else
                {
                    skybox.SetFloat("_BlendCubemaps",0);
                }
                skybox.SetColor("_Tint",curInfo.TintColor);
                skybox.SetTexture("_Tex",curInfo.SkyCube);
            }
        }

        private NormalEnvironmentRunner CreateRunner(ConfigEnvironment data, EnvironmentPriorityType type)
        {
            NormalEnvironmentRunner runner = NormalEnvironmentRunner.Create(data, type, this);
            envInfoMap.Add(runner.Id,runner);
            if (curRunner == null) curRunner = runner;
            return runner;
        }
        
        private BlenderEnvironmentRunner CreateRunner(NormalEnvironmentRunner from, NormalEnvironmentRunner to,
            bool isEnter)
        {
            BlenderEnvironmentRunner runner = BlenderEnvironmentRunner.Create(from, to, isEnter, this);
            envInfoMap.Add(runner.Id,runner);
            return runner;
        }
        
        private DayEnvironmentRunner CreateRunner(ConfigEnvironment morning,ConfigEnvironment noon,ConfigEnvironment afternoon,
            ConfigEnvironment night,EnvironmentPriorityType priority)
        {
            DayEnvironmentRunner runner = DayEnvironmentRunner.Create(morning, noon,afternoon,night,priority,this);
            envInfoMap.Add(runner.Id,runner);
            if (curRunner == null) curRunner = runner;
            return runner;
        }
        
        /// <summary>
        /// 创建环境
        /// </summary>
        /// <param name="id"></param>
        /// <param name="priority"></param>
        public long Create(int id, EnvironmentPriorityType priority)
        {
            var data = Get(id);
            var res = CreateRunner(data, priority);
            envInfoStack.Push(res);
            return res.Id;
        }

        /// <summary>
        /// 创建日夜循环环境
        /// </summary>
        public long CreateDayNight(int morningId, int noonId, int afternoonId, int nightId,
            EnvironmentPriorityType priority = EnvironmentPriorityType.DayNight)
        {
            var morning = Get(morningId);
            var noon = Get(noonId);
            var afternoon = Get(afternoonId);
            var night = Get(nightId);
            var res = CreateRunner(morning, noon, afternoon, night, priority);
            envInfoStack.Push(res);
            return res.Id;
        }

        /// <summary>
        /// 移除环境
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Remove(long id)
        {
            if (envInfoMap.TryGetValue(id, out var info) && !info.IsOver)
            {
                info.IsOver = true;
                
                //非生效环境
                if (curRunner is BlenderEnvironmentRunner blender)
                {
                    if (blender.To.Id != info.Id)
                    {
                        envInfoStack.Remove(info);
                        info.Dispose();
                    }
                }
                else if(curRunner.Id != info.Id)
                {
                    envInfoStack.Remove(info);
                    info.Dispose();
                }
                else if(envInfoStack.Count == 1)
                {
                    envInfoStack.Remove(info);
                    info.Dispose();
                    curRunner = null;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 从索引中删除，请不要手动调用
        /// </summary>
        /// <param name="id"></param>
        public void RemoveFromMap(long id)
        {
            envInfoMap.Remove(id);
        }

        private ConfigEnvironment Get(int id)
        {
            this.configs.TryGetValue(id, out ConfigEnvironment item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (ConfigEnvironment)}，配置id: {id}");
            }

            return item;
        }
    }
}