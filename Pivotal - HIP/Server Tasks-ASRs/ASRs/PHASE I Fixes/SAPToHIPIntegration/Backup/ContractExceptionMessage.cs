using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;

using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// Maintains a number of static methods for generating Contract processing exception messages.
    /// </summary>
    internal static class ContractExceptionMessage
    {
        private const string NoInfoAvailable = "N/A";

        /// <summary>
        /// Messages for when new option selections are being added to a contract.
        /// </summary>
        internal enum ContractOptionSelectionProcessing
        {
            UpdateInventorySelections,
            ConfirmInventorySelectionsReciept
        }


        /// <summary>
        /// Messages for when a contract is being used to generate and send Home and Buyer Envision entities.
        /// </summary>
        internal enum ContractSendProcessing
        {
            CreateEnvisionHome,
            CreateEnvisionBuyer,
            UpdateEnvisionHome,
            UpdateEnvisionBuyer,
            DeactivateEnvisionHome,
            DeactivateEnvisionBuyer
        }


        /// <summary>
        /// Returns the appropriate language string for the desired Contract exception message
        /// </summary>
        /// <param name="pivotalSystem">The pivotal system instance.</param>
        /// <param name="pivotalDataAccess">The pivotal data access instance.</param>
        /// <param name="contractId">Id of the Contract (Opportunity) impacted.</param>
        /// <param name="transactionId">The assosciated Envision transaction id of the call.</param>
        /// <param name="type">The exception message type.</param>
        /// <returns>The appropriatly populated exception message.</returns>
        internal static string GetContractOptionsUpdateExceptionMsg(IRSystem7 pivotalSystem, DataAccess pivotalDataAccess, byte[] contractId, int transactionId, ContractOptionSelectionProcessing type)
        {
            string contractIdString = string.Empty;
            string contractDescriptor = string.Empty;

            ILangDict langDictionary = pivotalSystem.GetLDGroup("Envision Integration");

            GetContractIdAndDescription(pivotalSystem, pivotalDataAccess, contractId, out contractIdString, out contractDescriptor);

            switch (type)
            {
                case ContractOptionSelectionProcessing.ConfirmInventorySelectionsReciept:
                    return (string)langDictionary.GetTextSub("ExceptionConfirmInventorySelections", new string[] { contractIdString, contractDescriptor, transactionId.ToString() });
                case ContractOptionSelectionProcessing.UpdateInventorySelections:
                    return (string)langDictionary.GetTextSub("ExceptionUpdateInventorySelections", new string[] { contractIdString, contractDescriptor, transactionId.ToString() });
            }

            return NoInfoAvailable;
        }


        /// <summary>
        /// Returns the appropriate Contract exception message.
        /// </summary>
        /// <param name="pivotalSystem">The pivotal system instance.</param>
        /// <param name="pivotalDataAccess">A pivotal data access instance.</param>
        /// <param name="contractId">The id of the Contract to create the message for.</param>
        /// <param name="type">The type of message to create.</param>
        /// <returns>An appropriately populated message.</returns>
        internal static string GetContractSendExceptionMsg(IRSystem7 pivotalSystem, DataAccess pivotalDataAccess, byte[] contractId, ContractSendProcessing type)
        {
            string contractIdString = string.Empty;
            string contractDescriptor = string.Empty;

            ILangDict langDictionary = pivotalSystem.GetLDGroup("Envision Integration");

            GetContractIdAndDescription(pivotalSystem, pivotalDataAccess, contractId, out contractIdString, out contractDescriptor);

            switch (type)
            {
                case ContractSendProcessing.CreateEnvisionBuyer:
                    return (string)langDictionary.GetTextSub("ExceptionCreateBuyer", new string[] { contractIdString, contractDescriptor });
                case ContractSendProcessing.CreateEnvisionHome:
                    return (string)langDictionary.GetTextSub("ExceptionCreateHome", new string[] { contractIdString, contractDescriptor });
                case ContractSendProcessing.DeactivateEnvisionBuyer:
                    return (string)langDictionary.GetTextSub("ExceptionDeactivateBuyer", new string[] { contractIdString, contractDescriptor });
                case ContractSendProcessing.DeactivateEnvisionHome:
                    return (string)langDictionary.GetTextSub("ExceptionDeactivateHome", new string[] { contractIdString, contractDescriptor });
                case ContractSendProcessing.UpdateEnvisionBuyer:
                    return (string)langDictionary.GetTextSub("ExceptionUpdateBuyer", new string[] { contractIdString, contractDescriptor });
                case ContractSendProcessing.UpdateEnvisionHome:
                    return (string)langDictionary.GetTextSub("ExceptionUpdateHome", new string[] { contractIdString, contractDescriptor });
            }
            return NoInfoAvailable;
        }


        /// <summary>
        /// Queries the database for the Contract description as well as returns the Contract Id in string form.
        /// </summary>
        /// <param name="pivotalSystem">The Pivotal system instance</param>
        /// <param name="pivotalDataAccess">A pivotal data access instance</param>
        /// <param name="contractId">The Id of the contract to from which to get the description.</param>
        /// <param name="contractIdString">Returns the Contract Id is string format.</param>
        /// <param name="contractDescription">Return the Contract description.</param>
        private static void GetContractIdAndDescription(IRSystem7 pivotalSystem, DataAccess pivotalDataAccess, byte[] contractId, out string contractIdString, out string contractDescription)
        {
            contractIdString = "0";
            contractDescription = NoInfoAvailable;

            if ((contractId != null) && (contractId.Length > 0))
            {
                Recordset oppRecords = pivotalDataAccess.GetRecordset(contractId, OpportunityData.TableName, new string[] { OpportunityData.RnDescriptorField });
                try
                {
                    if (oppRecords.RecordCount == 1)
                    {
                        contractDescription = (string)oppRecords.Fields[OpportunityData.RnDescriptorField].Value;
                        contractIdString = pivotalSystem.IdToString(contractId);
                    }
                }
                finally
                {
                    oppRecords.Close();
                }
            }
        }
    }
}
