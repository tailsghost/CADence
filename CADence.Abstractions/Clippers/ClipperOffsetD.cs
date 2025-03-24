using Clipper2Lib;
using System.Runtime.CompilerServices;

namespace CADence.Abstractions.Clippers
{
    public class ClipperOffsetD
    {
        private class Group
        {
            internal PathsD inPaths;
            internal JoinType joinType;
            internal EndType endType;
            internal bool pathsReversed;
            internal int lowestPathIdx;

            public Group(PathsD paths, JoinType joinType, EndType endType = EndType.Polygon)
            {
                this.joinType = joinType;
                this.endType = endType;
                bool isClosedPath = (endType == EndType.Polygon || endType == EndType.Joined);
                inPaths = new PathsD(paths.Count);
                foreach (PathD path in paths)
                {
                    inPaths.Add(StripDuplicates(path, isClosedPath));
                }

                if (endType == EndType.Polygon)
                {
                    lowestPathIdx = GetLowestPathIdx(inPaths);
                    pathsReversed = lowestPathIdx >= 0 && Clipper.Area(inPaths[lowestPathIdx]) < 0.0;
                }
                else
                {
                    lowestPathIdx = -1;
                    pathsReversed = false;
                }
            }
        }

        public delegate double DeltaCallbackD(PathD path, PathD path_norms, int currPt, int prevPt);

        private static readonly double Tolerance = 1E-9;

        private readonly List<Group> groupList = new List<Group>();
        private PathD pathOut = new PathD();
        private readonly PathD normals = new PathD();
        private PathsD solution = new PathsD();
        private PolyTreeD? solutionTree;
        private double groupDelta;
        private double delta;
        private double mitLimSqr;
        private double stepsPerRad;
        private double stepSin;
        private double stepCos;
        private JoinType joinType;
        private EndType endType;
        public double ArcTolerance { get; set; }
        public bool MergeGroups { get; set; }
        public double MiterLimit { get; set; }
        public bool PreserveCollinear { get; set; }
        public bool ReverseSolution { get; set; }
        public DeltaCallbackD? DeltaCallback { get; set; }

        public ClipperOffsetD(double miterLimit = 2.0, double arcTolerance = 0.25, bool preserveCollinear = false, bool reverseSolution = false)
        {
            MiterLimit = miterLimit;
            ArcTolerance = arcTolerance;
            MergeGroups = true;
            PreserveCollinear = preserveCollinear;
            ReverseSolution = reverseSolution;
        }

        public void Clear()
        {
            groupList.Clear();
        }

        public void AddPath(PathD path, JoinType joinType, EndType endType)
        {
            if (path.Count != 0)
            {
                PathsD paths = new PathsD(1) { path };
                AddPaths(paths, joinType, endType);
            }
        }

        public void AddPaths(PathsD paths, JoinType joinType, EndType endType)
        {
            if (paths.Count != 0)
            {
                groupList.Add(new Group(paths, joinType, endType));
            }
        }

        private int CalcSolutionCapacity()
        {
            int num = 0;
            foreach (Group group in groupList)
            {
                num += (group.endType == EndType.Joined ? (group.inPaths.Count * 2) : group.inPaths.Count);
            }
            return num;
        }

        internal bool CheckPathsReversed()
        {
            bool result = false;
            foreach (Group group in groupList)
            {
                if (group.endType == EndType.Polygon)
                {
                    result = group.pathsReversed;
                    break;
                }
            }
            return result;
        }

        private void ExecuteInternal(double delta)
        {
            if (groupList.Count == 0)
                return;

            EnsureCapacity(solution, CalcSolutionCapacity());
            if (Math.Abs(delta) < 0.05)
            {
                foreach (Group group in groupList)
                {
                    foreach (PathD inPath in group.inPaths)
                    {
                        solution.Add(inPath);
                    }
                }
                return;
            }

            this.delta = delta;
            mitLimSqr = (MiterLimit <= 1.0 ? 2.0 : (2.0 / Clipper.Sqr(MiterLimit)));
            foreach (Group group in groupList)
            {
                DoGroupOffset(group);
            }

            if (groupList.Count != 0)
            {
                bool flag = CheckPathsReversed();
                FillRule fillRule = (flag ? FillRule.Negative : FillRule.Positive);
                ClipperD clipper = new ClipperD();
                clipper.PreserveCollinear = PreserveCollinear;
                clipper.ReverseSolution = ReverseSolution != flag;
                clipper.AddSubject(solution);
                if (solutionTree != null)
                {
                    clipper.Execute(ClipType.Union, fillRule, solutionTree);
                }
                else
                {
                    clipper.Execute(ClipType.Union, fillRule, solution);
                }
            }
        }

        public void Execute(double delta, PathsD solution)
        {
            solution.Clear();
            this.solution = solution;
            ExecuteInternal(delta);
        }

        public void Execute(double delta, PolyTreeD solutionTree)
        {
            solutionTree.Clear();
            this.solutionTree = solutionTree;
            solution.Clear();
            ExecuteInternal(delta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PointD GetUnitNormal(PointD pt1, PointD pt2)
        {
            double dx = pt2.x - pt1.x;
            double dy = pt2.y - pt1.y;
            if (dx == 0.0 && dy == 0.0)
                return default(PointD);

            double invLen = 1.0 / Math.Sqrt(dx * dx + dy * dy);
            dx *= invLen;
            dy *= invLen;
            return new PointD(dy, -dx);
        }

        public void Execute(DeltaCallbackD deltaCallback, PathsD solution)
        {
            DeltaCallback = deltaCallback;
            Execute(1.0, solution);
        }

        internal static int GetLowestPathIdx(PathsD paths)
        {
            int result = -1;
            PointD point = new PointD(double.MaxValue, double.MinValue);
            for (int i = 0; i < paths.Count; i++)
            {
                foreach (PointD pt in paths[i])
                {
                    if (pt.y >= point.y && (pt.y != point.y || pt.x < point.x))
                    {
                        result = i;
                        point.x = pt.x;
                        point.y = pt.y;
                    }
                }
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PointD TranslatePoint(PointD pt, double dx, double dy)
        {
            return new PointD(pt.x + dx, pt.y + dy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PointD ReflectPoint(PointD pt, PointD pivot)
        {
            return new PointD(pivot.x + (pivot.x - pt.x), pivot.y + (pivot.y - pt.y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AlmostZero(double value, double epsilon = 0.001)
        {
            return Math.Abs(value) < epsilon;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Hypotenuse(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PointD NormalizeVector(PointD vec)
        {
            double len = Hypotenuse(vec.x, vec.y);
            if (AlmostZero(len))
                return new PointD(0.0, 0.0);
            double inv = 1.0 / len;
            return new PointD(vec.x * inv, vec.y * inv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PointD GetAvgUnitVector(PointD vec1, PointD vec2)
        {
            return NormalizeVector(new PointD(vec1.x + vec2.x, vec1.y + vec2.y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PointD IntersectPoint(PointD pt1a, PointD pt1b, PointD pt2a, PointD pt2b)
        {
            if (IsAlmostZero(pt1a.x - pt1b.x))
            {
                if (IsAlmostZero(pt2a.x - pt2b.x))
                    return new PointD(0.0, 0.0);
                double m = (pt2b.y - pt2a.y) / (pt2b.x - pt2a.x);
                double b = pt2a.y - m * pt2a.x;
                return new PointD(pt1a.x, m * pt1a.x + b);
            }

            if (IsAlmostZero(pt2a.x - pt2b.x))
            {
                double m = (pt1b.y - pt1a.y) / (pt1b.x - pt1a.x);
                double b = pt1a.y - m * pt1a.x;
                return new PointD(pt2a.x, m * pt2a.x + b);
            }

            double m1 = (pt1b.y - pt1a.y) / (pt1b.x - pt1a.x);
            double b1 = pt1a.y - m1 * pt1a.x;
            double m2 = (pt2b.y - pt2a.y) / (pt2b.x - pt2a.x);
            double b2 = pt2a.y - m2 * pt2a.x;
            if (IsAlmostZero(m1 - m2))
                return new PointD(0.0, 0.0);
            double x = (b2 - b1) / (m1 - m2);
            return new PointD(x, m1 * x + b1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PointD GetPerpendic(PointD pt, PointD norm)
        {
            return new PointD(pt.x + norm.x * groupDelta, pt.y + norm.y * groupDelta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PointD GetPerpendicD(PointD pt, PointD norm)
        {
            return new PointD(pt.x + norm.x * groupDelta, pt.y + norm.y * groupDelta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoBevel(PathD path, int j, int k)
        {
            PointD p1, p2;
            if (j == k)
            {
                double absDelta = Math.Abs(groupDelta);
                p1 = new PointD(path[j].x - absDelta * normals[j].x, path[j].y - absDelta * normals[j].y);
                p2 = new PointD(path[j].x + absDelta * normals[j].x, path[j].y + absDelta * normals[j].y);
            }
            else
            {
                p1 = new PointD(path[j].x + groupDelta * normals[k].x, path[j].y + groupDelta * normals[k].y);
                p2 = new PointD(path[j].x + groupDelta * normals[j].x, path[j].y + groupDelta * normals[j].y);
            }
            pathOut.Add(p1);
            pathOut.Add(p2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoSquare(PathD path, int j, int k)
        {
            PointD avg;
            if (j != k)
                avg = GetAvgUnitVector(new PointD(-normals[k].y, normals[k].x), new PointD(normals[j].y, -normals[j].x));
            else
                avg = new PointD(normals[j].y, -normals[j].x);

            double absDelta = Math.Abs(groupDelta);
            PointD pt = path[j];
            pt = TranslatePoint(pt, absDelta * avg.x, absDelta * avg.y);
            PointD pt1 = TranslatePoint(pt, groupDelta * avg.y, groupDelta * (-avg.x));
            PointD pt2 = TranslatePoint(pt, groupDelta * (-avg.y), groupDelta * avg.x);
            PointD perpendicD = GetPerpendicD(path[k], normals[k]);
            if (j == k)
            {
                PointD ptInter = IntersectPoint(
                    TranslatePoint(perpendicD, avg.x * groupDelta, avg.y * groupDelta),
                    pt1,
                    pt2,
                    perpendicD);
                pathOut.Add(ReflectPoint(ptInter, pt));
                pathOut.Add(ptInter);
            }
            else
            {
                PointD perpendicD2 = GetPerpendicD(path[j], normals[k]);
                PointD ptInter = IntersectPoint(pt1, pt2, perpendicD, perpendicD2);
                pathOut.Add(ptInter);
                pathOut.Add(ReflectPoint(ptInter, pt));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoMiter(PathD path, int j, int k, double cosA)
        {
            double factor = groupDelta / (cosA + 1.0);
            pathOut.Add(new PointD(path[j].x + (normals[k].x + normals[j].x) * factor,
                                   path[j].y + (normals[k].y + normals[j].y) * factor));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoRound(PathD path, int j, int k, double angle)
        {
            if (DeltaCallback != null)
            {
                double absDelta = Math.Abs(groupDelta);
                double arcTol = ArcTolerance;
                double steps = Math.PI / Math.Acos(1.0 - arcTol / absDelta);
                stepSin = Math.Sin(Math.PI * 2.0 / steps);
                stepCos = Math.Cos(Math.PI * 2.0 / steps);
                if (groupDelta < 0.0)
                    stepSin = -stepSin;
                stepsPerRad = steps / (Math.PI * 2.0);
            }

            PointD pt = path[j];
            PointD vec = new PointD(normals[k].x * groupDelta, normals[k].y * groupDelta);
            if (j == k)
                vec = new PointD(-vec.x, -vec.y);
            pathOut.Add(new PointD(pt.x + vec.x, pt.y + vec.y));
            int stepsCount = (int)Math.Ceiling(stepsPerRad * Math.Abs(angle));
            for (int i = 1; i < stepsCount; i++)
            {
                vec = new PointD(vec.x * stepCos - stepSin * vec.y, vec.x * stepSin + vec.y * stepCos);
                pathOut.Add(new PointD(pt.x + vec.x, pt.y + vec.y));
            }
            pathOut.Add(GetPerpendic(pt, normals[j]));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BuildNormals(PathD path)
        {
            int count = path.Count;
            normals.Clear();
            if (count != 0)
            {
                EnsureCapacity(normals, count);
                for (int i = 0; i < count - 1; i++)
                {
                    normals.Add(GetUnitNormal(path[i], path[i + 1]));
                }
                normals.Add(GetUnitNormal(path[count - 1], path[0]));
            }
        }

        private void OffsetPoint(Group group, PathD path, int j, ref int k)
        {
            if (path[j].Equals(path[k]))
            {
                k = j;
                return;
            }

            double cross = CrossProduct(normals[j], normals[k]);
            double dot = DotProduct(normals[j], normals[k]);
            cross = (cross > 1.0 ? 1.0 : (cross < -1.0 ? -1.0 : cross));

            if (DeltaCallback != null)
            {
                groupDelta = DeltaCallback(path, normals, j, k);
                if (group.pathsReversed)
                    groupDelta = -groupDelta;
            }

            if (Math.Abs(groupDelta) < Tolerance)
            {
                pathOut.Add(path[j]);
                return;
            }

            if (dot > -0.999 && cross * groupDelta < 0.0)
            {
                pathOut.Add(GetPerpendic(path[j], normals[k]));
                if (dot < 0.99)
                    pathOut.Add(path[j]);
                pathOut.Add(GetPerpendic(path[j], normals[j]));
            }
            else if (dot > 0.999 && joinType != JoinType.Round)
            {
                DoMiter(path, j, k, dot);
            }
            else if (joinType == JoinType.Miter)
            {
                if (dot > mitLimSqr - 1.0)
                    DoMiter(path, j, k, dot);
                else
                    DoSquare(path, j, k);
            }
            else if (joinType == JoinType.Round)
            {
                DoRound(path, j, k, Math.Atan2(cross, dot));
            }
            else if (joinType == JoinType.Bevel)
            {
                DoBevel(path, j, k);
            }
            else
            {
                DoSquare(path, j, k);
            }
            k = j;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OffsetPolygon(Group group, PathD path)
        {
            pathOut = new PathD();
            int count = path.Count;
            int k = count - 1;
            for (int i = 0; i < count; i++)
            {
                OffsetPoint(group, path, i, ref k);
            }
            solution.Add(pathOut);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OffsetOpenJoined(Group group, PathD path)
        {
            OffsetPolygon(group, path);
            path = Clipper.ReversePath(path);
            BuildNormals(path);
            OffsetPolygon(group, path);
        }

        private void OffsetOpenPath(Group group, PathD path)
        {
            pathOut = new PathD();
            int last = path.Count - 1;
            if (DeltaCallback != null)
            {
                groupDelta = DeltaCallback(path, normals, 0, 0);
            }

            if (Math.Abs(groupDelta) < Tolerance)
            {
                pathOut.Add(path[0]);
            }
            else
            {
                switch (endType)
                {
                    case EndType.Butt:
                        DoBevel(path, 0, 0);
                        break;
                    case EndType.Round:
                        DoRound(path, 0, 0, Math.PI);
                        break;
                    default:
                        DoSquare(path, 0, 0);
                        break;
                }
            }

            int i = 1;
            int k = 0;
            for (; i < last; i++)
            {
                OffsetPoint(group, path, i, ref k);
            }

            for (int iRev = last; iRev > 0; iRev--)
            {
                normals[iRev] = new PointD(-normals[iRev - 1].x, -normals[iRev - 1].y);
            }
            normals[0] = normals[last];
            if (DeltaCallback != null)
            {
                groupDelta = DeltaCallback(path, normals, last, last);
            }
            if (Math.Abs(groupDelta) < Tolerance)
            {
                pathOut.Add(path[last]);
            }
            else
            {
                switch (endType)
                {
                    case EndType.Butt:
                        DoBevel(path, last, last);
                        break;
                    case EndType.Round:
                        DoRound(path, last, last, Math.PI);
                        break;
                    default:
                        DoSquare(path, last, last);
                        break;
                }
            }

            int j = last - 1;
            int k2 = last;
            while (j > 0)
            {
                OffsetPoint(group, path, j, ref k2);
                j--;
            }
            solution.Add(pathOut);
        }

        private void DoGroupOffset(Group group)
        {
            if (group.endType == EndType.Polygon)
            {
                if (group.lowestPathIdx < 0)
                    delta = Math.Abs(delta);
                groupDelta = (group.pathsReversed ? -delta : delta);
            }
            else
            {
                groupDelta = Math.Abs(delta);
            }

            double absDelta = Math.Abs(groupDelta);
            joinType = group.joinType;
            endType = group.endType;
            if (group.joinType == JoinType.Round || group.endType == EndType.Round)
            {
                double tol = ArcTolerance; 
                double steps = Math.PI / Math.Acos(1.0 - tol / absDelta);
                stepSin = Math.Sin(Math.PI * 2.0 / steps);
                stepCos = Math.Cos(Math.PI * 2.0 / steps);
                if (groupDelta < 0.0)
                    stepSin = -stepSin;
                stepsPerRad = steps / (Math.PI * 2.0);
            }

            foreach (PathD inPath in group.inPaths)
            {
                pathOut = new PathD();
                switch (inPath.Count)
                {
                    case 1:
                        {
                            PointD center = inPath[0];
                            if (DeltaCallback != null)
                            {
                                groupDelta = DeltaCallback(inPath, normals, 0, 0);
                                if (group.pathsReversed)
                                    groupDelta = -groupDelta;
                                absDelta = Math.Abs(groupDelta);
                            }
                            if (group.endType == EndType.Round)
                            {
                                int steps = (int)Math.Ceiling(stepsPerRad * 2.0 * Math.PI);
                                pathOut = Clipper.Ellipse(center, absDelta, absDelta, steps);
                            }
                            else
                            {
                                int offset = (int)Math.Ceiling(absDelta);
                                pathOut = new RectD(center.x - offset, center.y - offset, center.x + offset, center.y + offset).AsPath();
                            }
                            solution.Add(pathOut);
                            continue;
                        }
                    case 2:
                        if (group.endType == EndType.Joined)
                        {
                            endType = (group.joinType == JoinType.Round ? EndType.Round : EndType.Square);
                        }
                        break;
                }
                BuildNormals(inPath);
                if (endType == EndType.Polygon)
                    OffsetPolygon(group, inPath);
                else if (endType == EndType.Joined)
                    OffsetOpenJoined(group, inPath);
                else
                    OffsetOpenPath(group, inPath);
            }
        }

        private static double CrossProduct(PointD pt1, PointD pt2)
        {
            return pt1.x * pt2.y - pt1.y * pt2.x;
        }

        internal static double DotProduct(PointD pt1, PointD pt2)
        {
            return pt1.x * pt2.x + pt1.y * pt2.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EnsureCapacity<T>(List<T> list, int minCapacity)
        {
            if (list.Capacity < minCapacity)
            {
                list.Capacity = minCapacity;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsAlmostZero(double value)
        {
            return Math.Abs(value) <= 1E-9;
        }

        private static PathD StripDuplicates(PathD path, bool isClosedPath)
        {
            int count = path.Count;
            PathD path2 = new PathD(count);
            if (count == 0)
            {
                return path2;
            }

            PointD point = path[0];
            path2.Add(point);
            for (int i = 1; i < count; i++)
            {
                if (!point.Equals(path[i]))
                {
                    point = path[i];
                    path2.Add(point);
                }
            }

            if (isClosedPath && point.Equals(path2[0]))
            {
                path2.RemoveAt(path2.Count - 1);
            }

            return path2;
        }
    }
}