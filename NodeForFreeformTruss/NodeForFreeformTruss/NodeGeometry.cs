using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NodeForFreeformTruss
{
    class NodeGeometry
    {
        //List<int> ConnBeAxeInt;
        //public double BeamRad;
        public List<Line> BeamConnectivity(Point3d NodePosition, List<Line> AxesList, out List<int> ConnBeAxeInt)
        {
            List<Line> ConnectedAxes = new List<Line>();
            ConnBeAxeInt = new List<int>();
            int Counter = 0;
            foreach (Line Li in AxesList)
            {
                Counter++;
                Point3d PoSt = Li.PointAt(0);
                Point3d PoEd = Li.PointAt(1);
                if (PoSt.DistanceTo(NodePosition) <= Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                {
                    ConnectedAxes.Add(Li);
                    ConnBeAxeInt.Add(Counter);
                }
                if (PoEd.DistanceTo(NodePosition) <= Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                {
                    ConnectedAxes.Add(Li);
                    ConnBeAxeInt.Add(Counter);
                }
            }

            return ConnectedAxes;
        }

        public Brep BeamGeometry(Point3d Cen, Line BeamAxis, double BeamRad, double distanceFromCenter)
        {
            Line CorrectedLen = ShrinkBeam(Cen, BeamAxis, distanceFromCenter);

            Point3d St = CorrectedLen.PointAt(0);
            Point3d Ed = CorrectedLen.PointAt(1);
            Vector3d LiVec = Ed - St;
            Plane plaStrt = new Plane(St, LiVec);
            Plane plaEd = new Plane(Ed, LiVec);

            List<Curve> LoftCir = new List<Curve>();
            Circle StCir = new Rhino.Geometry.Circle(plaStrt, BeamRad);
            Curve Stcrve = StCir.ToNurbsCurve();
            LoftCir.Add(Stcrve);
            Circle EdCir = new Rhino.Geometry.Circle(plaEd, BeamRad);
            Curve Edcrve = EdCir.ToNurbsCurve();
            LoftCir.Add(Edcrve);
            Brep[] BeamLi = Rhino.Geometry.Brep.CreateFromLoft(LoftCir, Point3d.Unset, Point3d.Unset, LoftType.Tight, false);
            Brep Beam = BeamLi[0];
            Brep CapBeam = Beam.CapPlanarHoles(0.1);

            return CapBeam;
        }

        public Brep AdvancedBeamGeometry(Point3d Cen, Line BeamAxis, double BeamRad, double MidRad, double inclination, double distanceFromCenter)
        {
            Line CorrectedLen = ShrinkBeam(Cen, BeamAxis, distanceFromCenter);

            Point3d St = CorrectedLen.PointAt(0);
            Point3d Mid = CorrectedLen.PointAt(0.5);
            Point3d Ed = CorrectedLen.PointAt(1);

            Vector3d LiVec = Ed - St;
            Plane plaStrt = new Plane(St, LiVec);
            Plane plaEd = new Plane(Ed, LiVec);
            Plane plaMid = new Plane(Mid, LiVec);

            Circle MidCir = new Rhino.Geometry.Circle(plaMid, MidRad);
            Curve mid = MidCir.ToNurbsCurve();

            Vector3d Up = St - Mid;
            Up.Unitize();
            Vector3d Down = Ed - Mid;
            Down.Unitize();

            double Dist = BeamAxis.Length;
            double HalDis = Dist / 2;
            double SiDis = Dist / 6;
            double TrDis = HalDis - SiDis;

            Vector3d Tr01 = Up * TrDis;
            Vector3d TrIn = SiDis * Up;
            Transform Uptr = Transform.Translation(Tr01);
            Transform Intr = Transform.Translation(TrIn);

            List<Curve> LoftList = new List<Curve>();
            Plane plaEd00 = new Plane(Mid, LiVec);
            Circle PlEdCir00 = new Rhino.Geometry.Circle(plaEd00, MidRad);
            Curve PlEdCir0 = PlEdCir00.ToNurbsCurve();
            LoftList.Add(PlEdCir0);
            Mid.Transform(Uptr);
            Plane plaEd01 = new Plane(Mid, LiVec);
            Circle PlEdCir01 = new Rhino.Geometry.Circle(plaEd01, MidRad);
            Curve PlEdCir01Crv = PlEdCir01.ToNurbsCurve();
            LoftList.Add(PlEdCir01Crv);
            Mid.Transform(Intr);
            Plane plaEd02 = new Plane(Mid, LiVec);
            Circle PlEdCir02 = new Rhino.Geometry.Circle(plaEd02, inclination);
            Curve PlEdCir02Crv = PlEdCir02.ToNurbsCurve();
            LoftList.Add(PlEdCir02Crv);
            Plane plaEdFinal = new Plane(St, LiVec);
            Circle PlEdCirFi = new Rhino.Geometry.Circle(plaEdFinal, inclination);
            Curve PlEdCirFiCrv = PlEdCirFi.ToNurbsCurve();
            LoftList.Add(PlEdCirFiCrv);
            Brep[] BeamLi = Rhino.Geometry.Brep.CreateFromLoft(LoftList, Point3d.Unset, Point3d.Unset, LoftType.Tight, false);
            Brep LoftHalf = BeamLi[0];

            Transform Mirror = Transform.Mirror(plaEd00);

            List<Brep> JoinBrepList = new List<Brep>();
            JoinBrepList.Add(LoftHalf);

            Brep LoftHalf01 = LoftHalf;
            LoftHalf01.Transform(Mirror);
            JoinBrepList.Add(LoftHalf01);

            Brep[] Beam = Brep.JoinBreps(JoinBrepList, 0.1);

            Brep OpenBeam = Beam[0];

            Brep CapBeam = OpenBeam.CapPlanarHoles(0.1);

            return CapBeam;
        }

        public Line ShrinkBeam(Point3d Center, Line BeamAxe, double distanceFromCenter)
        {
            Point3d StPt = BeamAxe.PointAt(0);
            Point3d EdPt = BeamAxe.PointAt(1);
            Vector3d LiVec = EdPt - StPt;
            Vector3d TrVec;
            Point3d EndSide;
            if (StPt.DistanceTo(Center) <= Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
            {
                TrVec = LiVec;
                EndSide = EdPt;
            }
            else
            {
                TrVec = LiVec * -1;
                EndSide = StPt;
            }
            TrVec.Unitize();
            Transform Tr = Transform.Translation(TrVec * distanceFromCenter);
            Point3d TrPt = new Point3d(Center);

            TrPt.Transform(Tr);

            Line Corrected = new Line(TrPt, EndSide);

            return Corrected;
        }

        public Brep NodeBrep(Point3d SphereCenter, double Radius, List<Brep> ListBeams)
        {
            Sphere NodeSphere = new Rhino.Geometry.Sphere(SphereCenter, Radius);
            Brep SpBrep = NodeSphere.ToBrep();

            Brep[] Node01 = Rhino.Geometry.Brep.CreateBooleanDifference(SpBrep, ListBeams[0], 0.1);
            Brep Node01Brep = Node01[0];
            List<Brep> DifferenceBreps = new List<Brep>();
            DifferenceBreps.Add(Node01Brep);

            for (int i = 1; i < ListBeams.Count; i++)
            {
                Brep[] Node02 = Rhino.Geometry.Brep.CreateBooleanDifference(DifferenceBreps[i - 1], ListBeams[i], 0.1);
                DifferenceBreps.Add(Node02[0]);
            }

            int BrepIndex = DifferenceBreps.Count - 1;
            return DifferenceBreps[BrepIndex];
        }
    }
}

