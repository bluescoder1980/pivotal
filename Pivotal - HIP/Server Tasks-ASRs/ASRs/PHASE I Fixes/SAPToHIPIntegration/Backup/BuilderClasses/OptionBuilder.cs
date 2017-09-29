//
// $Workfile: OptionBuilder.cs$
// $Revision: 46$
// $Author: RYong$
// $Date: Monday, August 27, 2007 5:07:06 PM$
//
// Copyright © Pivotal Corporation
//


using CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// Use the OptionBuilder class to construct DesignOptionType object.  The DesignOptionType object contains
    /// all primary and secondary information regardless if which information is changed.  For example, if 2 new rules
    /// are added, the existing rules are included in the DesignOptionType object as well.
    /// </summary>
    class OptionBuilder : BuilderBase
    {


        /// <summary>
        /// Create an option object.
        /// </summary>
        /// <param name="optionRst">Recordset of the option.</param>
        /// <param name="objLib">Object library for query data.</param>
        /// <param name="mrsysSystem">RSysSystem for manipulating Ids.</param>
        /// <param name="config">Configuration object to loan NHT Number.</param>
        /// <param name="transportType">Tansport type to indicate Ftp or web service transport.</param>
        public OptionBuilder(Recordset optionRst, DataAccess objLib, IRSystem7 mrsysSystem, Configuration config, EnvisionIntegration.TransportType transportType)
        {
            const string OptionBuilderClass = "OptionBuilder class. ";
            DesignOptionType option;
            string LocationNumber, LocationLevel;

            object optionId = optionRst.Fields[DivisionProductData.DivisionProductIdField].Value;

            try
            {
                if (optionRst.RecordCount > 0)
                {

                    option = new Option();

                    option.OptionName = optionRst.Fields[DivisionProductData.ProductNameField].Value;
                    option.OptionNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(optionId));
                    option.CategoryNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(optionRst.Fields[DivisionProductData.SubCategoryIdField].Value));
                    
                    if (!Convert.IsDBNull(optionRst.Fields[DivisionProductData.ConstructionStageIdField].Value))
                        option.CutoffStageNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(optionRst.Fields[DivisionProductData.ConstructionStageIdField].Value));

                    // Only populate Deactivate for Ftp schema.
                    if (transportType == EnvisionIntegration.TransportType.Ftp)
                        option.Deactivate = TypeConvert.ToBoolean(optionRst.Fields[DivisionProductData.InactiveField].Value);
                    option.OptionDescription = Convert.IsDBNull(optionRst.Fields[DivisionProductData.DescriptionField].Value) == true ? "" : (string)optionRst.Fields[DivisionProductData.DescriptionField].Value;
                    option.Price = Convert.ToDecimal(optionRst.Fields[DivisionProductData.RecommendedPriceField].Value);

                    option.PostCutoffPriceSpecified = Convert.IsDBNull(optionRst.Fields[DivisionProductData.PostCuttOffPriceField].Value) ? false : true; 
                    if (option.PostCutoffPriceSpecified)
                        option.PostCutoffPrice = Convert.ToDecimal(optionRst.Fields[DivisionProductData.PostCuttOffPriceField].Value);

                    option.ValidFromSpecified = Convert.IsDBNull(optionRst.Fields[DivisionProductData.AvailableDateField].Value) ? false : true; 
                    if (option.ValidFromSpecified)
                        option.ValidFrom = TypeConvert.ToDateTime(optionRst.Fields[DivisionProductData.AvailableDateField].Value);

                    option.ValidToSpecified = Convert.IsDBNull(optionRst.Fields[DivisionProductData.RemovalDateField].Value) ? false : true; 
                    if (option.ValidToSpecified)
                        option.ValidTo = TypeConvert.ToDateTime(optionRst.Fields[DivisionProductData.RemovalDateField].Value) - new TimeSpan(1,0,0,0);
                    
                    if (optionRst.Fields[DivisionProductData.DivisionIdField].Value != DBNull.Value)
                    {
                        LocationNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(optionRst.Fields[DivisionProductData.DivisionIdField].Value));
                        LocationLevel = EnvisionIntegration.LocationLevel.CodeDivision.ToUpper();
                    }
                    else if (optionRst.Fields[DivisionProductData.RegionIdField].Value != DBNull.Value)
                    {
                        LocationNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(optionRst.Fields[DivisionProductData.RegionIdField].Value));
                        LocationLevel = EnvisionIntegration.LocationLevel.CodeRegion.ToUpper();
                    }
                    else
                    {                        
                        LocationNumber = config.EnvisionNHTNumber;
                        LocationLevel = EnvisionIntegration.LocationLevel.CodeCorporation.ToUpper();
                    }


                    // Setting Option Type.
                    switch (TypeConvert.ToString(optionRst.Fields[NBHDPProductData.TypeField].Value))
                    {
                        case (NBHDPProductData.ElevationType):
                            option.OptionType = DesignOptionTypeOptionType.Normal;
                            break;
                        case (NBHDPProductData.DecoratorType):
                        case (NBHDPProductData.GlobalType):
                            option.OptionType = DesignOptionTypeOptionType.Group;
                            break;
                        case (NBHDPProductData.StructuralType):
                            option.OptionType = DesignOptionTypeOptionType.Group;
                            option.Structural = true;
                            break;
                        case (NBHDPProductData.PackageType):
                            option.OptionType = DesignOptionTypeOptionType.Package;
                            ArrayList packageComponents = GetIncludedOptions(optionId, LocationNumber, LocationLevel, objLib, mrsysSystem, transportType);
                            if (packageComponents.Count > 0)
                                option.Package = (DesignOptionTypeIncludedOption[])packageComponents.ToArray(typeof(DesignOptionTypeIncludedOption));
                            else
                                option.Package = null;
                            break;
                        default:
                            option.OptionType = DesignOptionTypeOptionType.Normal;
                            break;
                    }

                    option.OptionTypeSpecified = true;

                    List<DesignOptionTypeRule> ruleArr = GetOptionRules(ref option.OptionRuleUpdate, optionRst, 
                        LocationNumber, LocationLevel, objLib, mrsysSystem);
                    DesignOptionTypeRule[] rules = ruleArr.ToArray();

                    if (rules != null && rules.Length > 0)
                        option.Rules = rules;
                    
                    if (transportType == EnvisionIntegration.TransportType.WebService)
                        comments = string.Format("Synchronizing option: {0}", optionRst.Fields[DivisionProductData.RnDescriptorField].Value);
                    
                    xsdObject = option;
                }
            }
            catch (Exception ex)
            {
                throw new PivotalApplicationException(OptionBuilderClass, ex);
            }
        }

        //private DesignOptionTypeIncludedOption[] GetIncludedOptions(object optionId, string LocationNumber, string LocationLevel, DataAccess objLib, IRSystem7 mrsysSystem, EnvisionIntegration.TransportType  transportType)
        private ArrayList GetIncludedOptions(object optionId, string LocationNumber, string LocationLevel, DataAccess objLib, IRSystem7 mrsysSystem, EnvisionIntegration.TransportType transportType)
        {
            Recordset20 packageComponentRst = objLib.GetRecordset(ProductPackageComponentData.ComponentsProductsWithParentProductQuery, 1, optionId, ProductPackageComponentData.ComponentProductIdField, ProductPackageComponentData.RnUpdateField);
            ArrayList packageComponents = new ArrayList();

            if (packageComponentRst.RecordCount > 0)
            {
                DesignOptionTypeIncludedOption iPackageComponent;
                packageComponentRst.MoveFirst();
                while (!packageComponentRst.EOF)
                {
                    iPackageComponent = new DesignOptionTypeIncludedOption();
                    iPackageComponent.OptionNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(packageComponentRst.Fields[ProductPackageComponentData.ComponentProductIdField].Value));
                    iPackageComponent.LocationLevel = LocationLevel;
                    iPackageComponent.LocationNumber = LocationNumber;
                    iPackageComponent.PackageComponentId =  packageComponentRst.Fields[ProductPackageComponentData.ProductPackageComponentIdField].Value;
                    iPackageComponent.RnUpdate = packageComponentRst.Fields[ProductPackageComponentData.RnUpdateField].Value;
                    packageComponents.Add(iPackageComponent);
                    packageComponentRst.MoveNext();
                }            
            }
            return packageComponents;
        }

        /// <summary>
        /// Given the option recordset, return an array of DesignOptionTypeRules which are the option rules of the option.
        /// </summary>
        /// <param name="optionRuleUpdate">Returns a reference of option rules' Rn Update value.</param>
        /// <param name="optionRst">Recordset of 1 option, which to find the rules.</param>
        /// <param name="LocationNumber">Location number where the design option is defined.</param>
        /// <param name="LocationLevel">Location level where the design option is defined.</param>
        /// <param name="objLib">Object library to query the ED.</param>
        /// <param name="mrsysSystem">IRSystem to use its methodss.</param>
        /// <returns>Array of DesignOptionTypeRules of the passed in option.</returns>
       // private DesignOptionTypeRule[] GetOptionRules(ref object[,] optionRuleUpdate, Recordset20 optionRst, string LocationNumber, string LocationLevel, DataAccess objLib, IRSystem7 mrsysSystem)
    private List<DesignOptionTypeRule> GetOptionRules(ref object[,] optionRuleUpdate, Recordset20 optionRst, string LocationNumber, string LocationLevel, DataAccess objLib, IRSystem7 mrsysSystem)
        {
            List<DesignOptionTypeRule> arrListRules = new List<DesignOptionTypeRule>();
            Recordset20 optionRuleRst;
            DesignOptionTypeRule singleOptionRule;
            object optionId;

            if (optionRst.RecordCount > 0 && !optionRst.EOF)
            {
                optionId = optionRst.Fields[DivisionProductData.DivisionProductIdField].Value;

                try
                {
                    //Only return soft and exclude rules.  Don't return hard rules or plan-specific rules.
                    //Hard rules are handled at the configuration level.  Plan-specific rules aren't supported
                    //by Envision.
                    optionRuleRst = objLib.GetRecordset(ProductOptionRuleData.SoftAndExcludeRulesForOptionQuery, 1, optionId
                        , ProductOptionRuleData.ChildProductIdField, ProductOptionRuleData.ExcludeField
                        , ProductOptionRuleData.InactiveField
                        , ProductOptionRuleData.ProductOptionRuleIdField, ProductOptionRuleData.RnUpdateField);

                    int i = 0;

                    if (optionRuleRst.RecordCount > 0)
                    {

                        optionRuleUpdate = new object[optionRuleRst.RecordCount, 2];

                        optionRuleRst.MoveFirst();
                        while (!optionRuleRst.EOF)
                        {
                            singleOptionRule = new DesignOptionTypeRule();
                            if (TypeConvert.ToBoolean(optionRuleRst.Fields[ProductOptionRuleData.ExcludeField].Value))
                            {
                                //Cannot be sold with.
                                singleOptionRule.RuleType = DesignOptionTypeRuleRuleType.Excludes;
                            }
                            else 
                            {
                                //Can be sold with.  Soft rule.
                                singleOptionRule.RuleType = DesignOptionTypeRuleRuleType.Enables;
                            }
                            singleOptionRule.RuleTypeSpecified = true;
                            singleOptionRule.OptionNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(optionRuleRst.Fields[ProductOptionRuleData.ChildProductIdField].Value));

                            singleOptionRule.LocationLevel = LocationLevel;
                            singleOptionRule.LocationNumber = LocationNumber;

                            optionRuleUpdate[i, 0] = optionRuleRst.Fields[ProductOptionRuleData.ProductOptionRuleIdField].Value;
                            optionRuleUpdate[i, 1] = optionRuleRst.Fields[ProductOptionRuleData.RnUpdateField].Value;

                            if (!TypeConvert.ToBoolean(optionRuleRst.Fields[ProductOptionRuleData.InactiveField].Value))
                                arrListRules.Add(singleOptionRule);

                            optionRuleRst.MoveNext();
                            i++;
                        }                        
                    }

                    // Include exclusion rules for elevations.  Elevation exclusion rules are applied programmatically and they're not stored
                    // in the Pivotal ED.                      
                    if (TypeConvert.ToString(optionRst.Fields[DivisionProductData.TypeField].Value) == DivisionProductData.TypeFieldChoice.Elevation.ToString())
                    {
                        Recordset20 otherElevationRst = objLib.GetRecordset(DivisionProductData.OtherElevationsQuery, 3, optionRst.Fields[DivisionProductData.DivisionIdField].Value,
                            optionRst.Fields[DivisionProductData.RegionIdField].Value,
                            optionRst.Fields[DivisionProductData.DivisionProductIdField].Value,
                            DivisionProductData.DivisionProductIdField);
                        if (otherElevationRst.RecordCount > 0)
                        {
                            otherElevationRst.MoveFirst();
                            while (!otherElevationRst.EOF)
                            {
                                singleOptionRule = new DesignOptionTypeRule();
                                singleOptionRule.RuleType = DesignOptionTypeRuleRuleType.Excludes;
                                singleOptionRule.RuleTypeSpecified = true;
                                singleOptionRule.OptionNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(otherElevationRst.Fields[DivisionProductData.DivisionProductIdField].Value));
                                singleOptionRule.LocationLevel = LocationLevel;
                                singleOptionRule.LocationNumber = LocationNumber;
                                arrListRules.Add(singleOptionRule);
                                otherElevationRst.MoveNext();
                            }
                        }
                    }                    
                }
                catch (Exception ex)
                {
                    throw new PivotalApplicationException("OptionBuilder class. ", ex);
                }
            }
            return arrListRules;
        }

    }
}
