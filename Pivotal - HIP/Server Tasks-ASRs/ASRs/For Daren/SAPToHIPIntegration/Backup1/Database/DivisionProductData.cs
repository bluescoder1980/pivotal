//
// $Workfile: DivisionProductData.cs$
// $Revision: 2$
// $Author: tlyne$
// $Date: Wednesday, December 19, 2007 11:24:08 AM$
//
// Copyright © Pivotal Corporation
//

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    public static partial class DivisionProductData
    {
        public enum TypeFieldChoice
        {
            Plan = 0,
            Elevation,
            Structural,
            Decorator,
            Global,
            Package
        }

        public const string OptionsToSynchronizeForRegionQuery = "Env: Options to Synchronize For Region ?";
        public const string OptionsToSynchronizeForDivisionQuery = "Env: Options to Synchronize For Division ?";
        public const string OptionsToSynchronizeForCorporateQuery = "Env: Options to Synchronize for Corporate";
        public const string OtherElevationsQuery = "Env: Other Elevations Div? Reg? ElevId !=?";
        public const string DivisionProductOfAssignmentQuery = "HB: Division Product of Assignment ?";
    }
}