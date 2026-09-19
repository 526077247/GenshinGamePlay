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
    /// 模拟平面，支持 XZ / XY。
    /// </summary>
    public enum AxisPair
    {
        XZ,
        XY
    }

    /// <summary>
    /// ORCA 半平面约束：可行速度需满足 Det(dir, point - v) &lt;= 0。
    /// </summary>
    public struct ORCALine
    {
        public float2 dir;
        public float2 point;
    }

    /// <summary>
    /// 邻居距离记录。
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
    /// 一组障碍的公共信息。
    /// </summary>
    public struct ObstacleInfos
    {
        public float height;
        public float baseline;
        public float thickness;
        public byte collisionEnabled;
    }

    /// <summary>
    /// 障碍顶点（闭合多边形顶点环）。
    /// prev/next/index 均为打包数组中的绝对索引。
    /// </summary>
    public struct ObstacleVertexData
    {
        public float2 pos;
        public float2 dir;      // 指向下一顶点（next）的单位方向
        public int prev;
        public int next;
        public int index;
        public byte convex;     // 该顶点处内角是否小于 180°
        public int infos;       // 指向 ObstacleInfos 数组索引
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
    /// 空间哈希构建 Job（障碍版）：把每条障碍边写入其 AABB 覆盖的所有格子。
    /// </summary>
    [BurstCompile]
    public struct BuildObstacleHashJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ObstacleVertexData> vertices;
        public int count;
        public float cellSize;
        public NativeParallelMultiHashMap<int, int>.ParallelWriter hashMap;

        public void Execute(int index)
        {
            if (index >= count)
                return;
            ObstacleVertexData v = vertices[index];
            ObstacleVertexData next = vertices[v.next];
            float minX = min(v.pos.x, next.pos.x);
            float minY = min(v.pos.y, next.pos.y);
            float maxX = max(v.pos.x, next.pos.x);
            float maxY = max(v.pos.y, next.pos.y);
            int minCx = (int)floor(minX / cellSize);
            int minCy = (int)floor(minY / cellSize);
            int maxCx = (int)floor(maxX / cellSize);
            int maxCy = (int)floor(maxY / cellSize);
            for (int cx = minCx; cx <= maxCx; cx++)
            {
                for (int cy = minCy; cy <= maxCy; cy++)
                {
                    int key = (cx * 73856093) ^ (cy * 19349663);
                    hashMap.Add(key, index);
                }
            }
        }
    }

    /// <summary>
    /// 核心 ORCA 计算 Job。
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

        public int obstacleCount;
        [ReadOnly] public NativeArray<ObstacleVertexData> obstacleVertices;
        [ReadOnly] public NativeArray<ObstacleInfos> obstacleInfos;
        [ReadOnly] public NativeParallelMultiHashMap<int, int> obstacleHashMap;
        [ReadOnly] public NativeArray<float> radiusObsts;
        [ReadOnly] public NativeArray<float> timeHorizonObsts;

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
            float a_radiusObst = radiusObsts[i];
            float a_timeHorizonObst = timeHorizonObsts[i];

            if (a_maxNeighbors == 0 || !a_enabled)
            {
                newVelocities[i] = a_prefVelocity;
                return;
            }

            NativeList<ORCALine> orcaLines = new NativeList<ORCALine>(32, Allocator.Temp);

            float2 a_newVelocity = a_prefVelocity;
            float rangeSq = lengthsq(a_radius + a_neighborDist);

            // 构造障碍 ORCA 半平面（优先于 agent 半平面）
            int numObstLines = 0;
            BuildObstacleLines(ref a_position, ref a_velocity, a_radius, a_maxSpeed, a_radiusObst, a_timeHorizonObst, a_baseline, a_height, ref numObstLines, ref orcaLines);

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
                LP3(orcaLines, numObstLines, lineFail, a_maxSpeed, ref a_newVelocity);

            newVelocities[i] = a_newVelocity;
            orcaLines.Dispose();
        }

        private void BuildObstacleLines(ref float2 a_position, ref float2 a_velocity,
            float a_radius, float a_maxSpeed, float a_radiusObst, float a_timeHorizonObst,
            float a_baseline, float a_height, ref int numObstLines, ref NativeList<ORCALine> orcaLines)
        {
            if (obstacleCount == 0)
                return;

            float top = a_baseline + a_height;
            float bottom = a_baseline;

            // 邻域查询范围（对应 RVO2：timeHorizonObst * maxSpeed + radius）
            float obsRange = a_timeHorizonObst * a_maxSpeed + a_radius;
            float obsRangeSq = lengthsq(obsRange);

            // 直接遍历全部障碍边（障碍数量级小）并就地排序。
            // 弃用障碍空间哈希 + 临时 HashSet：并行写哈希表/中途扩容会引发
            // NativeArray 越界内存损坏（IndexOutOfRange + ReadWrite 限制），且无此必要。
            NativeList<DVP> obstacleNeighbors = new NativeList<DVP>(32, Allocator.Temp);
            for (int seg = 0; seg < obstacleCount; seg++)
            {
                ObstacleVertexData vertex = obstacleVertices[seg];
                ObstacleVertexData nextVertex = obstacleVertices[vertex.next];
                ObstacleInfos infos = obstacleInfos[vertex.infos];

                if (infos.collisionEnabled == 0)
                    continue;
                if (top < infos.baseline || bottom > infos.baseline + infos.height)
                    continue;

                // 仅处理 agent 位于边右侧（可见）且足够近的边
                float distSq = DistSqPointLineSegment(vertex.pos, nextVertex.pos, a_position);
                if (LeftOf(vertex.pos, nextVertex.pos, a_position) >= 0.0f) continue;
                if (distSq >= obsRangeSq) continue;

                int len = obstacleNeighbors.Length;
                obstacleNeighbors.Add(new DVP(distSq, seg));
                int k = len;
                while (k > 0 && distSq < obstacleNeighbors[k - 1].distSq)
                {
                    (obstacleNeighbors[k], obstacleNeighbors[k - 1]) =
                        (obstacleNeighbors[k - 1], obstacleNeighbors[k]);
                    k--;
                }
            }

            float invTimeHorizonObst = 1.0f / a_timeHorizonObst;

            // 逐条障碍边构造 ORCA 半平面（移植自 RVO2 Agent::computeNewVelocity）
            for (int n = 0; n < obstacleNeighbors.Length; ++n)
            {
                int seg = obstacleNeighbors[n].index;
                ObstacleVertexData vertex = obstacleVertices[seg];
                float thickness = obstacleInfos[vertex.infos].thickness;
                ObstacleVertexData nextVertex = obstacleVertices[vertex.next];

                float2 relPos1 = vertex.pos - a_position;
                float2 relPos2 = nextVertex.pos - a_position;
                float obstacleRadius = a_radiusObst + thickness;

                // 检查该障碍的速度障碍是否已被之前的障碍半平面覆盖
                bool alreadyCovered = false;
                for (int j = 0; j < orcaLines.Length; ++j)
                {
                    if (Det(invTimeHorizonObst * relPos1 - orcaLines[j].point, orcaLines[j].dir) -
                            invTimeHorizonObst * obstacleRadius >= -EPSILON &&
                        Det(invTimeHorizonObst * relPos2 - orcaLines[j].point, orcaLines[j].dir) -
                            invTimeHorizonObst * obstacleRadius >= -EPSILON)
                    {
                        alreadyCovered = true;
                        break;
                    }
                }

                if (alreadyCovered)
                    continue;

                float distSq1 = lengthsq(relPos1);
                float distSq2 = lengthsq(relPos2);
                float radiusSq = lengthsq(obstacleRadius);

                float2 obstacleVector = nextVertex.pos - vertex.pos;
                float s = dot(-relPos1, obstacleVector) / lengthsq(obstacleVector);
                float distSqLine = lengthsq(-relPos1 - s * obstacleVector);

                ORCALine line = new ORCALine();

                if (s < 0.0f && distSq1 <= radiusSq)
                {
                    // 与左顶点碰撞
                    if (vertex.convex != 0)
                    {
                        line.point = float2(0.0f, 0.0f);
                        line.dir = normalize(float2(-relPos1.y, relPos1.x));
                        orcaLines.Add(line);
                    }
                    continue;
                }

                if (s > 1.0f && distSq2 <= radiusSq)
                {
                    // 与右顶点碰撞
                    if (nextVertex.convex != 0 && Det(relPos2, nextVertex.dir) >= 0.0f)
                    {
                        line.point = float2(0.0f, 0.0f);
                        line.dir = normalize(float2(-relPos2.y, relPos2.x));
                        orcaLines.Add(line);
                    }
                    continue;
                }

                if (s >= 0.0f && s <= 1.0f && distSqLine <= radiusSq)
                {
                    // 与障碍边碰撞
                    line.point = float2(0.0f, 0.0f);
                    line.dir = -vertex.dir;
                    orcaLines.Add(line);
                    continue;
                }

                // 无碰撞，计算左右腿
                float2 leftLegDirection;
                float2 rightLegDirection;

                if (s < 0.0f && distSqLine <= radiusSq)
                {
                    // 斜视障碍，左顶点决定速度障碍
                    if (vertex.convex == 0)
                        continue;
                    nextVertex = vertex;

                    float leg1 = sqrt(distSq1 - radiusSq);
                    leftLegDirection = float2(
                        relPos1.x * leg1 - relPos1.y * obstacleRadius,
                        relPos1.x * obstacleRadius + relPos1.y * leg1) / distSq1;
                    rightLegDirection = float2(
                        relPos1.x * leg1 + relPos1.y * obstacleRadius,
                        -relPos1.x * obstacleRadius + relPos1.y * leg1) / distSq1;
                }
                else if (s > 1.0f && distSqLine <= radiusSq)
                {
                    // 斜视障碍，右顶点决定速度障碍
                    if (nextVertex.convex == 0)
                        continue;
                    vertex = nextVertex;

                    float leg2 = sqrt(distSq2 - radiusSq);
                    leftLegDirection = float2(
                        relPos2.x * leg2 - relPos2.y * obstacleRadius,
                        relPos2.x * obstacleRadius + relPos2.y * leg2) / distSq2;
                    rightLegDirection = float2(
                        relPos2.x * leg2 + relPos2.y * obstacleRadius,
                        -relPos2.x * obstacleRadius + relPos2.y * leg2) / distSq2;
                }
                else
                {
                    // 通常情况
                    if (vertex.convex != 0)
                    {
                        float leg1 = sqrt(distSq1 - radiusSq);
                        leftLegDirection = float2(
                            relPos1.x * leg1 - relPos1.y * obstacleRadius,
                            relPos1.x * obstacleRadius + relPos1.y * leg1) / distSq1;
                    }
                    else
                    {
                        // 左顶点非凸，左腿延伸截止线
                        leftLegDirection = -vertex.dir;
                    }

                    if (nextVertex.convex != 0)
                    {
                        float leg2 = sqrt(distSq2 - radiusSq);
                        rightLegDirection = float2(
                            relPos2.x * leg2 + relPos2.y * obstacleRadius,
                            -relPos2.x * obstacleRadius + relPos2.y * leg2) / distSq2;
                    }
                    else
                    {
                        // 右顶点非凸，右腿延伸截止线
                        rightLegDirection = vertex.dir;
                    }
                }

                // 凸顶点上的腿不能指向相邻边，否则改用相邻边截止线
                ObstacleVertexData leftNeighbor = obstacleVertices[vertex.prev];
                bool isLeftLegForeign = false;
                bool isRightLegForeign = false;

                if (vertex.convex != 0 && Det(leftLegDirection, -leftNeighbor.dir) >= 0.0f)
                {
                    leftLegDirection = -leftNeighbor.dir;
                    isLeftLegForeign = true;
                }

                if (nextVertex.convex != 0 && Det(rightLegDirection, nextVertex.dir) <= 0.0f)
                {
                    rightLegDirection = nextVertex.dir;
                    isRightLegForeign = true;
                }

                // 斜视分支可能重赋 vertex/nextVertex，截止圆需基于当前顶点重算
                relPos1 = vertex.pos - a_position;
                relPos2 = nextVertex.pos - a_position;

                float2 leftCutoff = invTimeHorizonObst * relPos1;
                float2 rightCutoff = invTimeHorizonObst * relPos2;
                float2 cutoffVector = rightCutoff - leftCutoff;

                // 将当前速度投影到速度障碍上
                float t = vertex.index == nextVertex.index
                    ? 0.5f
                    : dot(a_velocity - leftCutoff, cutoffVector) / lengthsq(cutoffVector);
                float tLeft = dot(a_velocity - leftCutoff, leftLegDirection);
                float tRight = dot(a_velocity - rightCutoff, rightLegDirection);

                if ((t < 0.0f && tLeft < 0.0f) ||
                    (vertex.index == nextVertex.index && tLeft < 0.0f && tRight < 0.0f))
                {
                    // 投影到左侧截止圆
                    float2 unitW = normalize(a_velocity - leftCutoff);
                    line.dir = float2(unitW.y, -unitW.x);
                    line.point = leftCutoff + obstacleRadius * invTimeHorizonObst * unitW;
                    orcaLines.Add(line);
                    continue;
                }

                if (t > 1.0f && tRight < 0.0f)
                {
                    // 投影到右侧截止圆
                    float2 unitW = normalize(a_velocity - rightCutoff);
                    line.dir = float2(unitW.y, -unitW.x);
                    line.point = rightCutoff + obstacleRadius * invTimeHorizonObst * unitW;
                    orcaLines.Add(line);
                    continue;
                }

                // 分别计算到截止线、左腿、右腿的距离，取最近者
                float distSqCutoff = (t < 0.0f || t > 1.0f || vertex.index == nextVertex.index)
                    ? float.PositiveInfinity
                    : lengthsq(a_velocity - (leftCutoff + t * cutoffVector));
                float distSqLeft = tLeft < 0.0f
                    ? float.PositiveInfinity
                    : lengthsq(a_velocity - (leftCutoff + tLeft * leftLegDirection));
                float distSqRight = tRight < 0.0f
                    ? float.PositiveInfinity
                    : lengthsq(a_velocity - (rightCutoff + tRight * rightLegDirection));

                if (distSqCutoff <= distSqLeft && distSqCutoff <= distSqRight)
                {
                    // 投影到截止线
                    line.dir = -vertex.dir;
                    line.point = leftCutoff + obstacleRadius * invTimeHorizonObst * float2(-line.dir.y, line.dir.x);
                    orcaLines.Add(line);
                    continue;
                }

                if (distSqLeft <= distSqRight)
                {
                    // 投影到左腿
                    if (isLeftLegForeign)
                        continue;

                    line.dir = leftLegDirection;
                    line.point = leftCutoff + obstacleRadius * invTimeHorizonObst * float2(-line.dir.y, line.dir.x);
                    orcaLines.Add(line);
                    continue;
                }

                // 投影到右腿
                if (isRightLegForeign)
                    continue;

                line.dir = -rightLegDirection;
                line.point = rightCutoff + obstacleRadius * invTimeHorizonObst * float2(-line.dir.y, line.dir.x);
                orcaLines.Add(line);
            }

            obstacleNeighbors.Dispose();
            numObstLines = orcaLines.Length;
        }

        /// <summary>
        /// 返回 c 相对 a->b 的方向：c 在左侧时 > 0。
        /// </summary>
        private float LeftOf(float2 a, float2 b, float2 c)
        {
            return Det(b - a, c - a);
        }

        private float DistSqPointLineSegment(float2 a, float2 b, float2 p)
        {
            float2 ab = b - a;
            float2 ap = p - a;
            float abSq = lengthsq(ab);
            float t = dot(ap, ab);
            if (t <= 0.0f)
                return lengthsq(ap);
            if (t >= abSq)
                return lengthsq(p - b);
            return lengthsq(p - (a + (t / abSq) * ab));
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
                    projLines.Dispose();

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
