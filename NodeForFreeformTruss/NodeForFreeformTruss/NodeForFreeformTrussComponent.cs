using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Node_FrameTruss
{
    public class NodeFrameTrussComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public NodeFrameTrussComponent()
          : base("Node_FrameTruss", "Nickname",
              "Description",
              "Extra", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("BeamAxesList", "BeAxLi", "TheLinesOf The Beams", GH_ParamAccess.list);
            pManager.AddPointParameter("NodesPositions", "NPos", "The Positions of the nodes", GH_ParamAccess.list);
            pManager.AddNumberParameter("NodeRadius", "NRad", "The Radius of the nodes", GH_ParamAccess.item);
            pManager.AddNumberParameter("BeamRadius", "BRad", "The Radius of the Crossection", GH_ParamAccess.item);
            pManager.AddNumberParameter("DistanceFromCenter", "DiFrCen", "The dstance between the end of the beam and the node position", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("ResultedNodesList", "ReNoLi", "ResultedNodesList", GH_ParamAccess.list);
            pManager.AddNumberParameter("BeamConnectivity", "BeCon", "The Topoogy of the nodes and Beams", GH_ParamAccess.tree);
            pManager.AddBrepParameter("BeamGeometry", "BeGeo", "The Geometry Of The Connected Beams", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //PlaceHolders
            List<Line> BeamAxisList = new List<Line>();
            List<Point3d> NodePositions = new List<Point3d>();
            double NodeRadius = double.NaN;
            double BeamRadius = double.NaN;
            double DisFromCenter = double.NaN;

            //AsseignValuesToInputs
            if (!DA.GetDataList(0, BeamAxisList)) { return; }
            if (!DA.GetDataList(1, NodePositions)) { return; }
            if (!DA.GetData(2, ref NodeRadius)) { return; }
            if (!DA.GetData(3, ref BeamRadius)) { return; }
            if (!DA.GetData(4, ref DisFromCenter)) { return; }

            //Modelling
            NodeGeometry Node = new NodeGeometry();
            
            List<List<Line>> Conncted = new List<List<Line>>();
            List<List<int>> Topo = new List<List<int>>();
            List<List<Brep>> BeamConn = new List<List<Brep>>();

            //OutPutsValues
            List<Brep> NodeList = new List<Brep>();
            foreach (Point3d i in NodePositions)
            {
                //Define The Elements Connected To each Node
                List<int> ConnectedFaces = new List<int>();
                List<Line> BeamConnectivity = Node.BeamConnectivity(i, BeamAxisList, out ConnectedFaces);
                Conncted.Add(BeamConnectivity);
                Topo.Add(ConnectedFaces);
                List<Brep> CrosssectionsList = new List<Brep>();
                foreach (Line n in BeamConnectivity)
                {
                    Brep Crossection = Node.BeamGeometry(i, n, BeamRadius, DisFromCenter);
                    CrosssectionsList.Add(Crossection);
                }
                Brep NodeGeometry = Node.NodeBrep(i, NodeRadius, CrosssectionsList);
                NodeList.Add(NodeGeometry);
                BeamConn.Add(CrosssectionsList);
            }
            //OutPut The CrossectionGeometry Tree
            Grasshopper.DataTree<Brep> Cyl = new Grasshopper.DataTree<Brep>();
            for (int i = 0; i < BeamConn.Count; i++)
            {
                Cyl.AddRange(BeamConn[i], new Grasshopper.Kernel.Data.GH_Path(i));
            }

            //OutPut The BeamTopology Tree
            Grasshopper.DataTree<int> TopoBeam = new Grasshopper.DataTree<int>();
            for (int i = 0; i < Topo.Count; i++)
            {
                TopoBeam.AddRange(Topo[i], new Grasshopper.Kernel.Data.GH_Path(i));
            }

            DA.SetDataList(0, NodeList);
            DA.SetDataTree(1, TopoBeam);
            DA.SetDataTree(2, Cyl);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{0ae146c6-1cfe-4167-9adc-73feb1422d39}"); }
        }
    }
}
