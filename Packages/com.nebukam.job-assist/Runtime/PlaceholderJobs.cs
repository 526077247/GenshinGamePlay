using Unity.Burst;
using Unity.Jobs;

namespace Nebukam.JobAssist
{

    [BurstCompile]
    public struct DisabledProcessor : IJob { public void Execute() { } }

    [BurstCompile]
    public struct Unemployed : IJob { public void Execute() { } }

    [BurstCompile]
    public struct UnemployedParallel : IJobParallelFor { public void Execute(int index) { } }

    [BurstCompile]
    public struct EmptyCompound : IJob { public void Execute() { } }

}
