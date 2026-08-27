using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.Jobs;
namespace TaoTie
{
    /// <summary>
    /// 模拟平面（对应原 Nebukam.Common.AxisPair），支持 XZ / XY。
    /// </summary>
    public enum AxisPair
    {
        XZ,
        XY
    }

    /// <summary>
    /// ORCA 半平面约束：可行速度需满足 Det(dir, point - v) <= 0。
    /// 对应原 Nebukam.ORCA.ORCALine。
    /// </summary>
    public struct ORCALine
    {
        public float2 dir;
        public float2 point;
    }

    /// <summary>
    /// 邻居距离记录（对应原 Nebukam.ORCA.ORCALinesJob.DVP）。
    /// </summary>
    public struct DVP
    {
        public float distSq;
        public int index;
        public DVP(float dist, int i)
        {
            distSq = dist;
            index = i;
        }
    }

    /// <summary>
    /// 空间哈希构建 Job：将每个智能体按其平面坐标写入哈希表，供邻域查询。
    /// </summary>
    [BurstCompile]
    public struct BuildHashJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<byte> nActive;
        [ReadOnly] public NativeArray<float2> positions;
        public float cellSize;
        public NativeParallelMultiHashMap<int, int>.ParallelWriter hashMap;

        public void Execute(int index)
        {
            if (nActive[index] == 0)
                return;
            float2 p = positions[index];
            int cx = (int)floor(p.x / cellSize);
            int cy = (int)floor(p.y / cellSize);
            int key = (cx * 73856093) ^ (cy * 19349663);
            hashMap.Add(key, index);
        }
    }

    /// <summary>
    /// 核心 ORCA 计算 Job（解析法，等价于原 Nebukam.ORCA.ORCALinesJob 的 agent 部分）。
    /// 逐智能体构造 ORCA 半平面，再用 LP1/LP2/LP3 求最优速度。
    /// </summary>
    [BurstCompile]
    public struct ORCALinesJob : IJobParallelFor
    {
        private const float EPSILON = 0.00001f;

        [ReadOnly] public NativeArray<byte> nActive;
        [ReadOnly] public NativeArray<float2> positions;
        [ReadOnly] public NativeArray<float2> prefVels;
        [ReadOnly] public NativeArray<float2> velocities;
        [ReadOnly] public NativeArray<float> radii;
        [ReadOnly] public NativeArray<float> maxSpeeds;
        [ReadOnly] public NativeArray<int> maxNeighborsArr;
        [ReadOnly] public NativeArray<float> neighborDistArr;
        [ReadOnly] public NativeArray<float> timeHorizonArr;
        [ReadOnly] public NativeArray<byte> enableds;
        [ReadOnly] public NativeArray<float> baselines;
        [ReadOnly] public NativeArray<float> heights;

        [ReadOnly] public NativeParallelMultiHashMap<int, int> hashMap;
        public float cellSize;
        public float timestep;

        public NativeArray<float2> newVelocities;

        public void Execute(int index)
        {
            if (nActive[index] == 0)
                return;
            int i = index;

            float2 a_position = positions[i];
            float2 a_prefVelocity = prefVels[i];
            float2 a_velocity = velocities[i];
            float a_radius = radii[i];
            float a_maxSpeed = maxSpeeds[i];
            int a_maxNeighbors = maxNeighborsArr[i];
            float a_neighborDist = neighborDistArr[i];
            float a_timeHorizon = timeHorizonArr[i];
            bool a_enabled = enableds[i] != 0;
            float a_baseline = baselines[i];
            float a_height = heights[i];

            if (a_maxNeighbors == 0 || !a_enabled)
            {
                newVelocities[i] = a_prefVelocity;
                return;
            }

            NativeList<ORCALine> orcaLines = new NativeList<ORCALine>(16, Allocator.Temp);

            float2 a_newVelocity = a_prefVelocity;
            float rangeSq = lengthsq(a_radius + a_neighborDist);

            // 邻域查询（空间哈希 3x3）
            NativeList<DVP> agentNeighbors = new NativeList<DVP>(a_maxNeighbors, Allocator.Temp);
            int cx = (int)floor(a_position.x / cellSize);
            int cy = (int)floor(a_position.y / cellSize);
            float top = a_baseline + a_height;
            float bottom = a_baseline;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int key = ((cx + dx) * 73856093) ^ ((cy + dy) * 19349663);
                    if (!hashMap.ContainsKey(key))
                        continue;

                    var it = hashMap.GetValuesForKey(key);
                    while (it.MoveNext())
                    {
                        int j = it.Current;
                        if (j == i)
                            continue;

                        float jTop = baselines[j] + heights[j];
                        float jBottom = baselines[j];
                        if (top < jBottom || bottom > jTop)
                            continue;

                        float2 rel = a_position - positions[j];
                        float distSq = lengthsq(rel);
                        if (distSq < rangeSq)
                        {
                            if (agentNeighbors.Length < a_maxNeighbors)
                            {
                                agentNeighbors.Add(new DVP(distSq, j));
                                int k = agentNeighbors.Length - 1;
                                while (k > 0 && distSq < agentNeighbors[k - 1].distSq)
                                {
                                    (agentNeighbors[k], agentNeighbors[k - 1]) =
                                        (agentNeighbors[k - 1], agentNeighbors[k]);
                                    k--;
                                }
                            }
                            else
                            {
                                int last = agentNeighbors.Length - 1;
                                if (distSq < agentNeighbors[last].distSq)
                                {
                                    agentNeighbors[last] = new DVP(distSq, j);
                                    int k = last;
                                    while (k > 0 && distSq < agentNeighbors[k - 1].distSq)
                                    {
                                        (agentNeighbors[k], agentNeighbors[k - 1]) =
                                            (agentNeighbors[k - 1], agentNeighbors[k]);
                                        k--;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 构造 agent-agent ORCA 半平面（等价于原 ORCALinesJob 675-744）
            float invTimeHorizon = 1.0f / a_timeHorizon;

            for (int n = 0; n < agentNeighbors.Length; ++n)
            {
                int otherIndex = agentNeighbors[n].index;
                float2 otherPos = positions[otherIndex];
                float2 otherVel = velocities[otherIndex];
                float otherRadius = radii[otherIndex];

                float2 relPos = otherPos - a_position;
                float2 relVel = a_velocity - otherVel;
                float distSq = lengthsq(relPos);
                float cRad = a_radius + otherRadius;
                float cRadSq = lengthsq(cRad);

                ORCALine line = new ORCALine();
                float2 u;

                if (distSq > cRadSq)
                {
                    float2 w = relVel - invTimeHorizon * relPos;
                    float wLengthSq = lengthsq(w);
                    float dotProduct1 = dot(w, relPos);

                    if (dotProduct1 < 0.0f && lengthsq(dotProduct1) > cRadSq * wLengthSq)
                    {
                        float wLength = sqrt(wLengthSq);
                        float2 unitW = w / wLength;
                        line.dir = float2(unitW.y, -unitW.x);
                        u = (cRad * invTimeHorizon - wLength) * unitW;
                    }
                    else
                    {
                        float leg = sqrt(distSq - cRadSq);
                        if (Det(relPos, w) > 0.0f)
                        {
                            line.dir = float2(relPos.x * leg - relPos.y * cRad, relPos.x * cRad + relPos.y * leg) / distSq;
                        }
                        else
                        {
                            line.dir = -float2(relPos.x * leg + relPos.y * cRad, -relPos.x * cRad + relPos.y * leg) / distSq;
                        }
                        float dotProduct2 = dot(relVel, line.dir);
                        u = dotProduct2 * line.dir - relVel;
                    }
                }
                else
                {
                    float invTimeStep = 1.0f / timestep;
                    float2 w = relVel - invTimeStep * relPos;
                    float wLength = length(w);
                    float2 unitW = w / wLength;
                    line.dir = float2(unitW.y, -unitW.x);
                    u = (cRad * invTimeStep - wLength) * unitW;
                }

                line.point = a_velocity + 0.5f * u;
                orcaLines.Add(line);
            }

            agentNeighbors.Dispose();

            int lineFail = LP2(orcaLines, a_maxSpeed, a_prefVelocity, false, ref a_newVelocity);
            if (lineFail < orcaLines.Length)
                LP3(orcaLines, 0, lineFail, a_maxSpeed, ref a_newVelocity);

            newVelocities[i] = a_newVelocity;
            orcaLines.Dispose();
        }

        #region Linear programs（等价于原 ORCALinesJob 1004-1222）

        private bool LP1(NativeList<ORCALine> lines, int lineNo, float radius, float2 optVel, bool dirOpt, ref float2 result)
        {
            ORCALine line = lines[lineNo];
            float2 dir = line.dir;
            float2 pt = line.point;

            float dotProduct = dot(pt, dir);
            float discriminant = lengthsq(dotProduct) + lengthsq(radius) - lengthsq(pt);

            if (discriminant < 0.0f)
                return false;

            ORCALine lineA;
            float2 dirA, ptA;

            float sqrtDiscriminant = sqrt(discriminant);
            float tLeft = -dotProduct - sqrtDiscriminant;
            float tRight = -dotProduct + sqrtDiscriminant;

            for (int i = 0; i < lineNo; ++i)
            {
                lineA = lines[i]; dirA = lineA.dir; ptA = lineA.point;

                float denominator = Det(dir, dirA);
                float numerator = Det(dirA, pt - ptA);

                if (abs(denominator) <= EPSILON)
                {
                    if (numerator < 0.0f)
                        return false;
                    continue;
                }

                float t = numerator / denominator;

                if (denominator >= 0.0f)
                    tRight = min(tRight, t);
                else
                    tLeft = max(tLeft, t);

                if (tLeft > tRight)
                    return false;
            }

            if (dirOpt)
            {
                if (dot(optVel, dir) > 0.0f)
                    result = pt + tRight * dir;
                else
                    result = pt + tLeft * dir;
            }
            else
            {
                float t = dot(dir, (optVel - pt));
                if (t < tLeft)
                    result = pt + tLeft * dir;
                else if (t > tRight)
                    result = pt + tRight * dir;
                else
                    result = pt + t * dir;
            }

            return true;
        }

        private int LP2(NativeList<ORCALine> lines, float radius, float2 optVel, bool dirOpt, ref float2 result)
        {
            if (dirOpt)
            {
                result = optVel * radius;
            }
            else if (lengthsq(optVel) > (radius * radius))
            {
                result = normalize(optVel) * radius;
            }
            else
            {
                result = optVel;
            }

            for (int i = 0, count = lines.Length; i < count; ++i)
            {
                if (Det(lines[i].dir, lines[i].point - result) > 0.0f)
                {
                    float2 tempResult = result;
                    if (!LP1(lines, i, radius, optVel, dirOpt, ref result))
                    {
                        result = tempResult;
                        return i;
                    }
                }
            }

            return lines.Length;
        }

        private void LP3(NativeList<ORCALine> lines, int numObstLines, int beginLine, float radius, ref float2 result)
        {
            float distance = 0.0f;

            ORCALine lineA, lineB;
            float2 dirA, ptA, dirB, ptB;

            for (int i = beginLine, iCount = lines.Length; i < iCount; ++i)
            {
                lineA = lines[i]; dirA = lineA.dir; ptA = lineA.point;

                if (Det(dirA, ptA - result) > distance)
                {
                    NativeList<ORCALine> projLines = new NativeList<ORCALine>(numObstLines, Allocator.Temp);

                    for (int ii = 0; ii < numObstLines; ++ii)
                        projLines.Add(lines[ii]);

                    for (int j = numObstLines; j < i; ++j)
                    {
                        lineB = lines[j]; dirB = lineB.dir; ptB = lineB.point;

                        ORCALine line = new ORCALine();
                        float determinant = Det(dirA, dirB);

                        if (abs(determinant) <= EPSILON)
                        {
                            if (dot(dirA, dirB) > 0.0f)
                                continue;
                            else
                                line.point = 0.5f * (ptA + ptB);
                        }
                        else
                        {
                            line.point = ptA + (Det(dirB, ptA - ptB) / determinant) * dirA;
                        }

                        line.dir = normalize(dirB - dirA);
                        projLines.Add(line);
                    }

                    float2 tempResult = result;
                    if (LP2(projLines, radius, float2(-dirA.y, dirA.x), true, ref result) < projLines.Length)
                        result = tempResult;

                    distance = Det(dirA, ptA - result);
                }
            }
        }

        #endregion

        private float Det(float2 a, float2 b)
        {
            return a.x * b.y - a.y * b.x;
        }
    }

    /// <summary>
    /// 将本帧求得的新速度写回速度缓冲（ORCA 计算使用上一帧速度，避免并行写冲突）。
    /// </summary>
    [BurstCompile]
    public struct ApplyJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<byte> nActive;
        [ReadOnly] public NativeArray<float2> newVelocities;
        public NativeArray<float2> velocities;

        public void Execute(int index)
        {
            if (nActive[index] == 0)
                return;
            velocities[index] = newVelocities[index];
        }
    }
}
