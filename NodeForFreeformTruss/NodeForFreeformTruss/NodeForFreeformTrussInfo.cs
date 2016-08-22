using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace NodeForFreeformTruss
{
    public class NodeForFreeformTrussInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "NodeForFreeformTruss";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("be349c9a-98c8-423f-b244-8f54a2bc0d97");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Microsoft";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
