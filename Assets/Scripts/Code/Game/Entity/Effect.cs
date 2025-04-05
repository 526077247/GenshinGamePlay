using UnityEngine;

namespace TaoTie
{
    public class Effect : SceneEntity, IEntity<string>,IEntity<string,long>
    {
        public override EntityType Type => EntityType.Effect;

        public string EffectName;
        
        public void Init(string name)
        {
            EffectName = name;
            AddComponent<EffectModelComponent, string>($"Effect/{name}/Prefabs/{name}.prefab");
        }
        public void Init(string name, long delay)
        {
            EffectName = name;
            string path = $"Effect/{name}/Prefabs/{name}.prefab";
            if (delay <= 0)
            {
                AddComponent<EffectModelComponent, string>(path);
            }
            else
            {
                GameObjectPoolManager.GetInstance().PreLoadGameObjectAsync(path,1).Coroutine();
                InitViewAsync(path, delay).Coroutine();
            }
        }

        private async ETTask InitViewAsync(string path, long delay)
        {
            await GameTimerManager.Instance.WaitAsync(delay);
            if(IsDispose) return;
            AddComponent<EffectModelComponent, string>(path);
        }
        
        public void Destroy()
        {
            EffectName = null;
        }
    }
}