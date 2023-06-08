using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
	[NinoSerialize]
	public partial class ConfigDie
	{
		[NinoMember(1), LabelText("是否有死亡动画")] 
		public bool HasAnimatorDie;
		
		[NinoMember(2)]
		public bool MuteAllShaderDieEff;
		
		[NinoMember(3), LabelText("空中死亡是否下落")] 
		public bool FallWhenAirDie;

		[NinoMember(4), LabelText("死亡表现持续时间"), Min(-1),Tooltip("单位ms，-1不消失")]
		public int DieEndTime;

		[NinoMember(5), LabelText("死亡后，力延迟消失时间")] //暂不清楚
		public int DieForceDisappearTime;

		[NinoMember(6), LabelText("死亡特效")] 
		public string DieDisappearEffect;

		[NinoMember(7), LabelText("死亡消失特效延迟播放时间")][Min(0)]
		public int DieDisappearEffectDelay;

		[NinoMember(8), LabelText("消融shader")] 
		public ShaderData DieShaderData;

		[NinoMember(9), LabelText("延迟多久开始消融shader")]
		public int DieShaderEnableDurationTime;

		[NinoMember(10), LabelText("消融shader持续时间")]
		public int DieShaderDisableDurationTime;

		[NinoMember(11), LabelText("死亡模型消失时间"), Min(-1),Tooltip("单位ms，-1不消失")]
		public int DieModelFadeDelay;

		[NinoMember(12), LabelText("是否使用RagDoll")]
		public bool UseRagDoll;

		[NinoMember(13), ShowIf(nameof(UseRagDoll)), LabelText("RagDoll Die延迟时间"), Min(-1), Tooltip("单位ms，-1不消失")]
		public int RagDollDieEndTimeDelay;
	}
}