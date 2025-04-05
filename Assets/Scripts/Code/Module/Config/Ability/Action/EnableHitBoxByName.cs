using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class EnableHitBoxByName: ConfigAbilityAction
    {
        [NinoMember(10)]
        public string[] HitBoxNames;
        [NinoMember(11)]
        public bool SetEnable;
        [NinoMember(12)]
        public bool IncludeChild;
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (HitBoxNames != null)
            {
                TryEnableHitBox(target);
            }
        }
        
        private void TryEnableHitBox(Entity target)
        {
            if(target == null || target.IsDispose) return;
            UnitModelComponent holderComponent = target.GetComponent<UnitModelComponent>();
            for (int i = 0; i < HitBoxNames.Length; i++)
            {
                holderComponent?.EnableHitBox(HitBoxNames[i], SetEnable).Coroutine();
            }
            if (IncludeChild)
            {
                var ac = target.GetComponent<AttachComponent>();
                if (ac != null)
                {
                    for (int i = 0; i < ac.Childs.Count; i++)
                    {
                        var child = target.Parent.Get<Entity>(ac.Childs[i]);
                        TryEnableHitBox(child);
                    }
                }
            }
        }
    }
}