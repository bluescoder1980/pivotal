//
// $Workfile: DivisionProductData.cs$
// $Revision: 7$
// $Author: RYong$
// $Date: Friday, February 02, 2007 5:18:05 PM$
//
// Copyright © Pivotal Corporation
//

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    internal static partial class DivisionProductData
    {
        internal enum TypeFieldChoice
        {
            Plan = 0,
            Elevation,
            Structural,
            Decorator,
            Global,
            Package
        }

        internal const string OptionsToSynchronizeForRegionQuery = "Env: Options to Synchronize For Region ?";
        internal const string OptionsToSynchronizeForDivisionQuery = "Env: Options to Synchronize For Division ?";
        internal const string OptionsToSynchronizeForCorporateQuery = "Env: Options to Synchronize for Corporate";
        internal const string OtherElevationsQuery = "Env: Other Elevations Div? Reg? ElevId !=?";
        internal const string DivisionProductOfAssignmentQuery = "HB: Division Product of Assignment ?";
    }
}