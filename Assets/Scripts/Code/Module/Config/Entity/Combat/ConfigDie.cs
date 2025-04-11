using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
	[NinoType(false)]
	public partial class ConfigDie
	{
		[NinoMember(1), LabelText("*是否有死亡动画")] 
		[Tooltip("为什么我配置了死亡动画但是有时候没有生效?(也适用于其他Trigger类型的参数)" +
		         "\r\n一般是Die参数配置成了Trigger类型,当前帧因为如下列原因没有触发死亡，然后下一帧Die的Trigger值被清了" +
		         "\r\n1.可能是在动画过渡中死亡，但是FsmConfig对应过渡连线(注意是其他正在过渡的连线不是死亡过渡连线)没有配置过渡中打断处理方法(Setting-Interruption Source)" +
		         "\r\n2.可能是死亡状态过渡优先级比较低,这一帧被其他状态优先过渡掉了,需要对每一个状态调整连线优先级")]
		public bool HasAnimatorDie;
		
		[NinoMember(2)][LabelText("关闭自身所有shader的死亡状态影响")]
		public bool MuteAllShaderDieEff;
		
		[NinoMember(3), LabelText("空中死亡是否下落")] 
		public bool FallWhenAirDie;

		[NinoMember(4), LabelText("*死亡表现持续时间(ms)"), MinValue(-1),Tooltip("单位ms，-1不消失")]
		public int DieEndTime;

		[NinoMember(5), LabelText("死亡后，力延迟消失时间(ms)")] //暂不清楚
		public int DieForceDisappearTime;

		[NinoMember(6), LabelText("死亡特效")] 
		public string DieDisappearEffect;

		[NinoMember(7), LabelText("死亡消失特效延迟播放时间")][MinValue(0)]
		public int DieDisappearEffectDelay;

		[NinoMember(8), LabelText("消融shader")] 
		public ShaderData DieShaderData;

		[NinoMember(9), LabelText("延迟多久开始消融shader(ms)")]
		public int DieShaderEnableDurationTime;

		[NinoMember(10), LabelText("消融shader持续时间(ms)")]
		public int DieShaderDisableDurationTime;

		[NinoMember(11), LabelText("*死亡模型消失时间(ms)"), MinValue(-1),Tooltip("单位ms，-1不消失")]
		public int DieModelFadeDelay;

		[NinoMember(12), LabelText("是否使用RagDoll")]
		public bool UseRagDoll;

		[NinoMember(13), ShowIf(nameof(UseRagDoll)), LabelText("*RagDoll Die延迟时间(ms)"), MinValue(-1), Tooltip("单位ms，-1不消失")]
		public int RagDollDieEndTimeDelay;
	}
}