'
' $Workfile: NBHDPhase_Form.bas$
' $Revision: 14$
'   $Author: bchandranna$
'     $Date: Friday, August 17, 2007 1:06:56 AM$
'
' Copyright © Pivotal Corporation
'

Option Explicit

' Module Name: NBHDPhase_Form

' Tables
Const strtNEIGHBORHOOD = "Neighborhood"         ' Parent table name
Const strtRelease = "NBHD_Phase"
Const strtDIVISION = "Division"

' Parent Business Object Name
Const strbNEIGHBORHOOD = "Neighborhood"

'Forms
Const strrHB_NBHD_PHASE = "HB NBHD Phase"
Const strrHB_NBHD_PHASE_SERVICE = "HB NBHD Phase Service"
Const strrHB_LOT_ADMIN = "HB Lot Admin"
Const strrHB_RELEASE_TEAM_MEMBER = "HB Employee / NBHD"
Const strrHB_RELEASE_MASS_PRICE_UPDATE = "HB Release Mass Price Update"
Const strfHB_EMPLOYEE_ADMIN = "HB Employee Admin"
Const strfHB_EMPLOYEE = "HB Employee"


'Form Tabs
Const strsAVAILABLE_PRODUCTS = "Available Products"
Const strsGEO_ADMIN = "Geographical Administration"
'Corrected the Tab name by JWang Apr. 07
Const strsPRODUCT_LIBARY = "Product Library"
Const mstraRELEASE = "Release"

'Form Segments
Const strsRELEASE = "Release"
Const strsADDRESS = "Address"
Const strsSALES_TEAM = "Sales Team"
Const strsNBHD_EMPLOYEES = "NBHD Employees"
Const strsHOME_SITES = "Home Sites"
Const strsNEIGHBORHOOD_PRODUCT = "Neighborhood Product"
Const strsRELEASE_ADJUSTMENTS = "Release Adjustments"
Const strsSERVICE_TEAM = "Service Team"
Const strsADDRESS_INFORMATION = "Address Information"
Const mstrsMILESTONE = "Milestones"
Const strsPLANS = "Plans"
Const strsLOTS = "Lots"
Const strsGLOBAL_OPTIONS = "Global Options"
Const strsWILDCARD_OPTIONS = "Wildcard Options"

'Form Fields
Const strfPHASE_NAME = "Phase_Name"
Const strfNEIGHBORHOOD_ID = "Neighborhood_Id"
Const strfPRICE = "Price"
Const strfNEXT_PRICE = "Next_Price"
Const strfPRICE_CHANGE_DATE = "Price_Change_Date"
Const strfPRODUCT_NAME = "Product_Name"
Const strfCODE_ = "Code_"
Const strfTYPE = "Type"
Const strfCATEGORY = "Category"
Const strfAVAILABLE_DATE = "Available_Date"
Const strfADJUSTMENT_REASON = "Adjustment_Reason"
Const strfADJUSTMENT_TYPE = "Adjustment_Type"
Const strfINACTIVE = "Inactive"
Const strfSTATUS = "Status"
Const strfOPEN_DATE = "Open_Date"
Const strfCLOSE_DATE = "Close_Date"
Const strfFEES = "Fees"
Const strfADDRESS = "Address"
Const strfCITY = "City"
Const strfSTATE_ = "State_"
Const strfZIP = "Zip"
Const strfCOUNTY = "County"
Const strfSALES_MANAGER_ID = "Sales_Manager_Id"
Const strfCONSTRUCTION_MANAGER_ID = "Construction_Manager_Id"
Const strfEMPLOYEE_ID = "Employee_Id"
Const strfLAST_NAME = "Last_Name"
Const strfDISABLE_LOTS = "Disable_Lots"
Const strfDISABLE_PLANS = "Disable_Plans"
Const strfDISABLE_STRUCTURAL = "Disable_Structural"
Const strfDISABLE_DECORATOR = "Disable_Decorator"
Const strfPHASE = "Phase"
Const strfLOT_NUMBER = "Lot_Number"
Const strfBUILDING = "Building"
Const strfUNIT = "Unit"
Const strfNBHD_PHASE_ID = "NBHD_Phase_Id"
Const strfUPDATE_PRICE_TO = "Update_Price_To"
Const strfUPDATE_PRICE_EFF_DATE_TO = "Update_Price_Eff_Date_To"
Const strfDIVISION_ID = "Division_Id"
Const mstrfMILESTONE_NAME = "Milestone_Item_Name"
Const mstrfMILESTONE_OFFSET_DAYS = "OffSet_Days"
Const mstrfMILESTONE_KEY_BASELINE_DATE = "Key_Baseline_Date"
Const mstrfMILESTONE_INACTIVE = "Inactive"
Const strfREGION_ID = "Region_Id"
' Release Adjustment segment
Const mstrfADJUSTMENT_INACTIVE = "Inactive"
Const mstrfADJUSTMENT_DIVISION_ADJ = "Division_Adjustment_Id"
Const strfRELEASE_ADJUSTMENT_ID = "Release_Adjustment_Id"
Const mstrfADJUSTMENT_REASON = "Adjustment_Reason"
Const mstrfADJUSTMENT_TYPE = "Adjustment_Type"
' Fields name on lots segment
Const mstrfLOT_STATUS = "Lot_Status"
Const mstrfLOT_INACTIVE = "Inactive"
' Fields name on Sales Team segment
Const strfSALES_INACTIVE = "Inactive"
Const strfSALES_EMPLOYEE_ID = "Employee_Id"
Const strfSALES_ROLE_ID = "Role_Id"
Const strfSALES_LAST_NAME = "Last_Name"
Const strfSALES_DISABLE_LOTS = "Disable_Lots"
Const strfSALES_DISABLE_PLANS = "Disable_Plans"
Const strfSALES_DISABLE_STRUCTURAL = "Disable_Structural"
Const strfSALES_DISABLE_DECORATOR = "Disable_Decorator"

Const strfENV_ENVISION_ACTIVATED = "Env_Envision_Activated"

' Release Status value list
Const strSTATUS_COMING_SOON = "Coming Soon"
Const strSTATUS_OPEN = "Open"
Const strSTATUS_CLOSED = "Closed"
Const strSTATUS_RESERVED = "Reserved"
Const strSTATUS_INACTIVE = "Inactive"
Const strSTATUS_SOLD = "Sold"
Const strfCONSTRUCTION_STAGE_ORDINAL = "Construction_Stage_Ordinal"
Const strfUPDATE_POST_CUTOFF_TO = "Update_Post_CutOff_To"

' Language Group
Const strgNBHD_PHASE = "NBHD Phase"
Const strgNBHD_PRODUCT = "Neighborhood Product"
Const strlgCOMMON = "Common"

'language strings
Const strlsOF = "Of"
Const strdINACTIVE_ALERT = "Inactive Alert"
Const strdCANNOT_INACTIVATE_RLS_ADJ = "Cannot Inactivate Release Adjustment"
Const strdCANNOT_DELETE_RLS_ADJ = "Cannot Delete Release Adjustment"
Const strdMASS_PRICE_UPDATE = "Mass Price Update"
Const strdCLOSE = "Close"
Const strdCANNOT_ADD_SECONDARY = "Can Not Add Secondary"
Const strldCOPY_OPTION_3 = "Copy Option 3"
Const strldCOPY_OPTION_4 = "Copy Option 4"
Const strldRELEASES = "Releases"
Const strldCLOSE_DATE_ALERT = "Close Date Alert"

'search
Const strSEARCH_TYPE = "search"
Const strhCOPY_OPTIONS_GLOBAL_ALL_RELEASES = "Copy Options Global All - Releases"
'buttons
Const strdOK = "OK"
Const strdCANCEL = "Cancel"
Const strgCOMMON = "Common"
Const strbNEW = "New"
Const strbSHORTCUTS = "Shortcuts"
Const strbMASSPRICEUPD = "Mass Price Update"
Const strbBNTBUTTON = "bntButton"
Const strbBNTLOADING = "bntLoading"
Const strbDELETE = "Delete"
Const strbNEW_BUTTON = "NEW_BUTTON"
Const strbDELETE_BUTTON = "DELETE_BUTTON"

'LD Groups for Buttons
Const strlgBUTTONS = "Buttons"
Const strlgFORM = "FORM"

'scripts
Const strcsNEIGHBORHOOD_PHASE_MASS_PRICE_UPDATE = "NBHDPhase_MassPriceUpdate"
Const strcsNEIGHBORHOOD_PHASE_CLOSE_PRICING = "NeighborhoodProd_Close_Pricing"
' Lot Status value list
Const mstrLOT_STATUS_SOLD = "Sold"
Const mstrLOT_STATUS_CLOSED = "Closed"
'methods
Const strmCOPY_OPTIONS_GLOBAL_ALL_TO_SELECTED_RELEASES = "CopyOptionsGlobalAllToSelectedReleases"
Const strmRLS_ADJ_IN_USE = "ReleaseAdjInUse"

'events
Const streON_CLICK = "onclick"

'tabs
Const inttRELEASE = 0
Const inttHOMESITES = 1
Const inttHOMESITES_LOADER = 2
Const inttPRODUCT = 3
Const inttPRODUCT_LOADER = 4
Const inttRELEASE_ADJ = 5
Const inttMILESTONES = 6

'AM2010.09.02 - Added constant for IP
Const strsecTIC_ESCROW = "TIC_Escrow" 
Const segDREPrimary = "DRE/FHA/VA Info"


' -----------------------------------------------------------------------------------------------------------------
' Name:    LoadFormData
' Purpose: Open form with an existing record
' ------------------------------------------------------------------------------------------------------
' Inputs:
'       rfrmForm       : The IRform object reference
'       vntRecord_Id   : The Primary Record Id
'       vntParameters  : The Parameters passed to Middle tier
' Returns:
'       LoadFormData   : The loaded form data
' History:
' Revision#     Date        Author  Description
' ----------    ----        ------  -----------
' HB
' 1.0           04/29/2003   AVasa Initial version
' ------------------------------------------------------------------------------------------
Function LoadFormData(rfrmForm, vntRecordId, vntParameters)
  Dim vntRecordsets
  
     
  On Error Resume Next
  vntRecordsets = rfrmForm.DoLoadFormData(vntRecordId, vntParameters)
  If Err.Number <> 0 Then
    UIMaster.ShowErrorMessage Err.Description
    Err.Clear
    Exit Function
  End If
  LoadFormData = vntRecordsets
End Function
' -------------------------------------------------------------------------------------------
' Name:     OnFormLoaded
' Purpose:  On form loaded
' -------------------------------------------------------------------------------------------
' Inputs:
'       vntParameters : The Parameters passed
' Returns:
'       None
' History:
' Revision#     Date         Author     Description
' ----------    ----         ------     -----------
' HB
' 1.0           04/29/2003    AVasa     Initial version
' 3.6           03/11/2005    BHan      Disable/Enable fields based on Inactive flag
' 3.6           03/15/2005    BHan      En/Disable Milestone rows based on Milestone_Inactive flag
'                                       En/Disable Adjustment rows based on Adjustment_Inactive flag
' 3.6           03/17/2005    BHan      Start Date will be disabled when the Status is Open
'                                       Start Date and Close Date will be disabled when the Status is Closed
' 3.6           03/28/2005    BHan      En/Disable Sales Team rows based on SalesTeam_Inactive flag
' 3.6           03/29/2005    JWang     Add "Mass Price Update" button for Admin user
' 3.6           03/30/2005    JWang     Remove "Delete" button for the form "HB Release Mass Price Update"
' 3.6           04/04/2005    JWang     Add "Close" button for the form "HB Release Mass Price Update"
' 3.6           04/07/2005    JWang     Delete unexistant tab refresh
' 3.6           28-Sept-2005  BA        Comment out refresh of tree web tab #issue 20345
' 5.9           15-Feb- 2007 NDcunha To load the Tab based on the Product Configuration level
' 5.9           15-Aug-2007   BC 	Modified to hide the geo lib
' -------------------------------------------------------------------------------------
Sub OnFormLoaded(vntParameters)
    Dim rstPrimary
    Dim blnIsInactive
    Dim rstMilestone
    Dim lngRow
    Dim blnMilestoneInactive
    Dim rstAdjustment
    Dim blnAdjustmentInactive
    
    Dim rstSalesTeam
    Dim blnSalesTeamInactive
    
    Dim vntStartDate
    Dim vntCloseDate
    Dim strStatus
    
    Dim strButtonText
    Dim vntFormField
    On Error Resume Next
    Set rstPrimary = UIMaster.RUICenter.PrimaryRecordset
    
    SharedLoad vntParameters


    If Not IsInEscrowSecurityGroup Then
       Call DisableAllDREFields
    End If  

    
    Select Case UIMaster.RUICenter.Form.FormName
    Case strrHB_NBHD_PHASE
        'Added by Jwang for "Mass Price Update" button
        'Is the Current User an Admin
        If Global.CurrentUserInGroup(Global.gstrsecHOMEBUILDERS_ADMIN) Then
            strButtonText = UIMaster.RSysClient.GetLDGroup(strgNBHD_PRODUCT).GetText(strdMASS_PRICE_UPDATE)
            UIMaster.RUITop.AddButton 0, strButtonText, 9999
            UIMaster.RUITop.AddEventHookScript strButtonText, strcsNEIGHBORHOOD_PHASE_MASS_PRICE_UPDATE, streON_CLICK
        End If
        ' added for Tree Tabs
        Global.UpdateWebTabURL UImaster.RUICenter
        'Feb 15th 2007, To load the Tab based on the Product Configuration level
        If Not IsNull(rstPrimary.Fields(strfNBHD_PHASE_ID).Value) Then
           UpdateTreeWebTabURL UIMaster.RUICenter.PrimaryRecordset.Fields(strfNBHD_PHASE_ID).Value
        End If

    Case strrHB_NBHD_PHASE_SERVICE
        Call UIMaster.RUICenter.HideTab(inttHOMESITES)
        'Call UIMaster.RUICenter.HideTab(inttPRODUCT)

    Case strrHB_RELEASE_MASS_PRICE_UPDATE
        'Mar 30, Added by Jwang for "Mass Price Update" button
        UIMaster.RUIBottom.RemoveButton UIMaster.RSysClient.GetLDGroup(strlgFORM).GetText(strbDELETE_BUTTON)
        
        'add close button    Apr 4, Added by Jwang
        strButtonText = UIMaster.RSysClient.GetLDGroup(strgNBHD_PRODUCT).GetText(strdCLOSE)
        UIMaster.RUIBottom.AddButton 3, strButtonText, 9999
        UIMaster.RUIBottom.AddEventHookScript strButtonText, strcsNEIGHBORHOOD_PHASE_CLOSE_PRICING, streON_CLICK

        'Call Global function DisableSecondaryFields to En/Disable secondary records based on Inactive flag
        '  for two segments,strsPLANS and strsLOTS.
        Call Global.DisableSecondaryFields(Array(strsPLANS, strsLOTS), _
                                    Array(strfUPDATE_PRICE_TO, strfUPDATE_PRICE_EFF_DATE_TO))
        Exit Sub
    End Select
    
    
    
    
    'Call Global function DisableSecondaryFields to En/Disable secondary records based on Inactive flag
    '  for three segments, mstrsMILESTONE, strsRELEASE_ADJUSTMENTS and strsNBHD_EMPLOYEES.
    Call Global.DisableSecondaryFields(Array(mstrsMILESTONE, strsRELEASE_ADJUSTMENTS, strsNBHD_EMPLOYEES), "")

    ' Mar. 11,2005 - BH
    blnIsInactive = rstPrimary.Fields(strfINACTIVE).Value
    ' For existing records:
    ' if Inactive flag is checked, disable all fields but itself
    If blnIsInactive Then
        '2005/10/20 by JWang. Call localized sub DisableAllFieldsExceptServiceTeam instaed of the global.DisableAllFields
        Call DisableAllFieldsExceptServiceTeam
        
        'disable Shortcut, Mass Price Update and remove New buttons
        UIMaster.RUITop.DisableButton strbSHORTCUTS, True
        UIMaster.RUITop.DisableButton strbMASSPRICEUPD, True
        UIMaster.RUIBottom.RemoveButton UIMaster.RSysClient.GetLDGroup(strlgFORM).GetText(strbNEW_BUTTON)
        'clear the error in case buttons are not on the form already
        Err.Clear
        
        UIMaster.RUICenter.DisableFieldEx mstraRELEASE, strsRELEASE, strfINACTIVE, False
    End If

    ' Mar. 17, 2005
    ' Start Date will be disabled when the Status is "Reserved", "Sold", "Closed", or "Inactive"
    strStatus = rstPrimary.Fields(strfSTATUS).Value
    If strStatus = strSTATUS_CLOSED Or strStatus = strSTATUS_RESERVED Or strStatus = strSTATUS_INACTIVE Or _
                strStatus = strSTATUS_SOLD Then
        UIMaster.RUICenter.DisableFieldEx mstraRELEASE, strsRELEASE, strfOPEN_DATE, True
    End If

    If strStatus = strSTATUS_COMING_SOON Then
        If Not IsNull(rstPrimary.Fields(strfZIP).Value) Then
            Call Global.EnDisableAddrForZip(True, strsADDRESS_INFORMATION, False)
        End If
    Else
        For Each vntFormField In UIMaster.RUICenter.Form.Segments(strsADDRESS_INFORMATION).FormFields
            Call UIMaster.RUICenter.DisableField(strsADDRESS_INFORMATION, vntFormField.FieldName, True)
        Next
    End If
    If Err.Number <> 0 Then
        UIMaster.ShowErrorMessage Err.Description
        Err.Clear
        Exit Sub
    End If


End Sub


' -------------------------------------------------------------------------------------------
' Name      : OnFormReLoaded
' Purpose   : Refresh web tabs when the content of the form is updated
' -------------------------------------------------------------------------------------------
' Revision#        Date         Author   Note
' HB 3.3          2004/07/04    AV      Added RefreshTab
' HB 3.6          2005/04/07    JWang   Delete unexistent tab refresh
'   3.6           2005/04/12    BHan    En/Disable secondary rows based on its Inactive flag
'   3.7           2006/30/29    RY      Only apply logics for Release and Release Service forms.
' 5.9           15-Feb- 2007 NDcunha To load the Tab once a new Release has just been created
'---------------------------------------------------------------------------------------
Sub OnFormReLoaded(vntParameterList)
Dim blnIsInactive, rstPrimary
    On Error Resume Next
    
    SharedLoad vntParameterList
    
    Select Case UIMaster.RUICenter.Form.FormName
    Case strrHB_NBHD_PHASE, strrHB_NBHD_PHASE_SERVICE
        Global.UpdateWebTabURL UImaster.RUICenter
        'UIMaster.RUICenter.RefreshTab (strsPRODUCT_LIBARY)
        'Feb 15th 2007, To load the Tab once a new Release has just been created
        UpdateTreeWebTabURL UIMaster.RUICenter.PrimaryRecordset.Fields(strfNBHD_PHASE_ID).Value

        Set rstPrimary = UIMaster.RUICenter.PrimaryRecordset
        blnIsInactive = rstPrimary.Fields(strfINACTIVE).Value
        ' For existing records:
        ' if Inactive flag is checked, disable all fields but itself
        If blnIsInactive Then
        
            'disable Shortcut, Mass Price Update and remove New buttons
            UIMaster.RUITop.DisableButton strbSHORTCUTS, True
            UIMaster.RUITop.DisableButton strbMASSPRICEUPD, True
        
            UIMaster.RUICenter.DisableFieldEx mstraRELEASE, strsRELEASE, strfINACTIVE, False
        End If
    
        ' Apr. 12, 2005 - BH
        ' En/Disable secondary rows based on its Inactive flag
        Call Global.DisableSecondaryFields(Array(mstrsMILESTONE, strsRELEASE_ADJUSTMENTS, strsNBHD_EMPLOYEES), "")
        If Err.Number <> 0 Then
            UIMaster.ShowErrorMessage Err.Description
            Err.Clear
            Exit Sub
        End If
    End Select

End Sub
' -------------------------------------------------------------------------------------------
' Name      : SharedLoad
' Purpose   : Called by both OnFormLoaded and OnFormReLoaded
' -------------------------------------------------------------------------------------------
' Revision#        Date          Author   Note
' HB 3.6           2005/10/20    JWang    Initial version.
' hb 5.9           2007/06/mar   ML       changes for mass price update
'---------------------------------------------------------------------------------------
Sub SharedLoad(vntParameterList)
    On Error Resume Next
    Dim rstMPU_Options
    Dim intCount

    'Apr 17, 2007. By JWang
    'Disable delete button for neighborhood under division whom integrated with Envision.
    Dim blnDivisionIntegratedWithEnvision
    Dim vntDivisionId
    
    vntDivisionId = UIMaster.RUICenter.PrimaryRecordset.Fields(strfDIVISION_ID).Value
    blnDivisionIntegratedWithEnvision = UIMaster.RSysClient.GetTable(strtDIVISION).Fields(strfENV_ENVISION_ACTIVATED).Index(vntDivisionId)
    UIMaster.RUIBottom.DisableButton UIMaster.RSysClient.GetLDGroup(strlgFORM).GetText(strbDELETE_BUTTON), blnDivisionIntegratedWithEnvision
    Err.Clear
    Select Case UIMaster.RUICenter.Form.FormName
       Case strrHB_RELEASE_MASS_PRICE_UPDATE
            'for wildcarded options
            Set rstMPU_Options = UIMaster.RUICenter.GetRecordset(strsWILDCARD_OPTIONS)
            intCount = 1
            If rstMPU_Options.RecordCount > 0 Then
             rstMPU_Options.Movefirst
             While Not rstMPU_Options.EOF
                If IsNull(rstMPU_Options.Fields(strfCONSTRUCTION_STAGE_ORDINAL).Value) Then
                 UIMaster.RUICenter.DisableSecondaryFieldForRow strsWILDCARD_OPTIONS, strfUPDATE_POST_CUTOFF_TO, intCount, True
                End If
              intCount = intCount + 1
              rstMPU_Options.MoveNext
             Wend
            End If
            
            'for global options
            Set rstMPU_Options = UIMaster.RUICenter.GetRecordset(strsGLOBAL_OPTIONS)
            intCount = 1
            If rstMPU_Options.RecordCount > 0 Then
              rstMPU_Options.Movefirst
              While Not rstMPU_Options.EOF
                If IsNull(rstMPU_Options.Fields(strfCONSTRUCTION_STAGE_ORDINAL).Value) Then
                  UIMaster.RUICenter.DisableSecondaryFieldForRow strsGLOBAL_OPTIONS, strfUPDATE_POST_CUTOFF_TO, intCount, True
                End If
                intCount = intCount + 1
                rstMPU_Options.MoveNext
              Wend
            End If
       
   End Select
End Sub

' ------------------------------------------------------------------------------------------------------
' Name : DisableAllFieldsExceptServiceTeam
' Purpose: Disable all fields except Service Team secondary segment
'-------------------------------------------------------------------------------------------------------
' Inputs:
'       None
' Returns:
'       None
' Implements Agent: None
' History:
' Revision# Date                Author     Description
' --------- --------------      --------   ---------------
' 3.6       2005/10/20          JWang      Initial version.
'                                          localization of Golbal.DisableAllFields.
'                                             Do not Disable Service Team Secondary segment
'                                             Do not disable bntButton and bntLoading buttons.
' ------------------------------------------------------------------------------------------------------
Sub DisableAllFieldsExceptServiceTeam()

    Dim vSegment, vField, vElement
    Dim strTabName
     Dim intCounter
    strTabName = UIMaster.RUICenter.Form.Segments(0).ParentTab.TabName
    
    On Error Resume Next
     
    For intCounter = UIMaster.RUICenter.Form.Tabs.Count - 1 To 0 Step -1
        For Each vSegment In UIMaster.RUICenter.Form.Tabs(intCounter).Segments
'Skip Service Team Secodary
            If vSegment.SegmentName <> strsSERVICE_TEAM Then
                 For Each vField In vSegment.FormFields
                   
                   If (Not vField.Visible) Then
                       ' Nothing
                   ElseIf vField.ReadOnly Then
                       ' Nothing
                   ElseIf vField.IsSeparator Then
                       ' Nothing
                   ElseIf vField.IsStaticText Then
                       ' Nothing
                   ElseIf vField.IsIcon Then
                        'Nothing
                   ElseIf vField.IsButton Then
                       Set vElement = UIMaster.RUICenter.GetButton(vSegment.SegmentName, vField.ButtonName)
                       If Err.Number = 0 Then
'Skip bntButton and bntLoading buttons
                            If vField.ButtonName <> strbBNTBUTTON And vField.ButtonName <> strbBNTLOADING Then vElement.disabled = True
                       End If
                       Err.Clear
                   ElseIf vField.DisconnectedName <> "" Then
                       'Nothing
                   Else
                           
                       If Not IsEmpty(vSegment.ParentTab) Then
                           UIMaster.RUICenter.DisableFieldEx vSegment.ParentTab.TabName, vSegment.SegmentName, vField.FieldName, True
                       Else
                           Set vElement = UIMaster.RUICenter.GetField(vSegment.SegmentName, vField.FieldName)
                           If Err.Number = 0 Then vElement.disabled = True
                           Err.Clear
                       End If
                   End If
                Next
            End If
       Next
    Next
    If strTabName <> "" Then
     UIMaster.RUICenter.SelectTab strTabName
    End If
        If Err.Number <> 0 Then
        UIMaster.ShowErrorMessage Err.Description
        Err.Clear
        Exit Sub
    End If
End Sub

' -----------------------------------------------------------------------------------------------------------------
' Name:     AddFormData
' Purpose:  Add a new form data into database
' ------------------------------------------------------------------------------------------------------
' Inputs:
'       rfrmForm      : The IRform object reference to the client IRForm object
'       vntRecordsets : Hold the reference for the current primary recordset and its
'                       all secondaries in the form
'       vntParameters : The Parameters passed from Client to Middle tier for Business rule
' Returns:
'       AddFormData   : Return information to MT COM
' History:
' Revision#     Date        Author  Description
' ----------    ----        ------  -----------
' HB
' 1.0           10/15/2003  AV      Home Builders Version
' 3.6           04/01/2005  BHan    Enable Inactive flag after creating a new record and Applying the changes
' 3.7           Mar/24/2006 YK      Issue #56574
' ------------------------------------------------------------------------------------------
Function AddFormData(rfrmForm, vntRecordsets, vntParameters)
    On Error Resume Next
    ' YK - March 24, 2006 - Issue #56574
    If CheckCloseDate = True Then
            UIMaster.RUICenter.SaveCanceled = True
            Exit Function
    End If
    
    AddFormData = rfrmForm.DoAddFormData(vntRecordsets, vntParameters)
    If Err.Number <> 0 Then
        UIMaster.ShowErrorMessage Err.Description
        Err.Clear
        Exit Function
    End If
    
    ' BH - Enable Inactive flag after creating a new record and Applying the changes
    UIMaster.RUICenter.DisableFieldEx mstraRELEASE, strsRELEASE, strfINACTIVE, False
End Function
' Name:    SaveFormData
' Purpose: This function updates the form
' ------------------------------------------------------------------------------------------
' Inputs:
'        rfrmForm      : The IRform object reference to the client IRForm object
'        vntRecordsets  : Hold the reference for the current primary recordset and its
'                        all secondaries in the form
'        vntParameters : The Parameters passed from Client to Middle tier for Business rule
' Returns:
'       None
' History:
' Revision#     Date        Author  Description
' ----------    ----        ------  -----------
' HB
' 1.0           04/29/2003    AVasa  Initial version
' 3.6           2005/04/02  rjordan mass price update changes
' 3.7           Mar/24/2006 YK      Issue #56574
' 5.9           feb/15/2007  ML     changes for Mass Price Update
' --------------------------------------------------------------------------------------
Public Sub SaveFormData(rfrmForm, vntRecordsets, vntParameters)
    On Error Resume Next
    Const intNBHDP_PRODUCT = 1
    Const intNBHDP_PRODUCT_NOT_PLAN = 2
    Const intLOT = 3
    Const intNBHDP_PRODUCT_WILDCARD = 4
    'for MassPriceUpdate enum
    Const intMPU_PLANS_FOR_RELEASE = 1
    Const intMPU_GLOBAL_OPTIONS_FOR_RELEASE = 2
    Const intMPU_OTHER_OPTIONS_FOR_RELEASE = 3
    Const intMPU_LOT = 6
    Dim intUpdatePriceTo1
    Dim intUpdatePriceTo2
    Dim intUpdatePriceTo3
    Dim intUpdatePriceTo4
    Dim rstPricing1
    Dim rstPricing2
    Dim rstPricing3
    Dim rstPricing4
    Dim vntNBHDPhase_Id
    
    Select Case rfrmForm.FormName
    Case strrHB_RELEASE_MASS_PRICE_UPDATE
        Set rstPricing1 = UIMaster.RUICenter.GetRecordset(strsPLANS)
        intUpdatePriceTo1 = Global.IsPriceUpdateToValid(rstPricing1)
        If intUpdatePriceTo1 = -1 Then
            'bad data, focus to the UPDATE_PRICE_EFFECTIVE_DATE_TO field
            UIMaster.RUICenter.FocusField strsPLANS, .strfUPDATE_PRICE_TO, .RowPosition
            UIMaster.RUICenter.SaveCanceled = True
            Exit Sub
        End If
        Set rstPricing2 = UIMaster.RUICenter.GetRecordset(strsLOTS)
        intUpdatePriceTo2 = Global.IsPriceUpdateToValid(rstPricing2)
        If intUpdatePriceTo2 = -1 Then
            'bad data, focus to the UPDATE_PRICE_EFFECTIVE_DATE_TO field
            UIMaster.RUICenter.FocusField strsLOTS, .strfUPDATE_PRICE_TO, .RowPosition
            UIMaster.RUICenter.SaveCanceled = True
            Exit Sub
        End If
        Set rstPricing3 = UIMaster.RUICenter.GetRecordset(strsGLOBAL_OPTIONS)
        intUpdatePriceTo3 = Global.IsPriceUpdateToValid(rstPricing3)
        If intUpdatePriceTo3 = -1 Then
            'bad data, focus to the UPDATE_PRICE_EFFECTIVE_DATE_TO field
            UIMaster.RUICenter.FocusField strsGLOBAL_OPTIONS, .strfUPDATE_PRICE_TO, .RowPosition
            UIMaster.RUICenter.SaveCanceled = True
            Exit Sub
        End If
        Set rstPricing4 = UIMaster.RUICenter.GetRecordset(strsWILDCARD_OPTIONS)
        intUpdatePriceTo4 = Global.IsPriceUpdateToValid(rstPricing4)
        If intUpdatePriceTo4 = -1 Then
            'bad data, focus to the UPDATE_PRICE_EFFECTIVE_DATE_TO field
            UIMaster.RUICenter.FocusField strsWILDCARD_OPTIONS, .strfUPDATE_PRICE_TO, .RowPosition
            UIMaster.RUICenter.SaveCanceled = True
            Exit Sub
        End If
    Case strrHB_NBHD_PHASE, strrHB_NBHD_PHASE_SERVICE ' YK - March 24, 2006 - Issue #56574
           If CheckCloseDate = True Then
               UIMaster.RUICenter.SaveCanceled = True
               Exit Sub
           End If
    End Select
    
    Call rfrmForm.DoSaveFormData(vntRecordsets, vntParameters)
    If Err.Number <> 0 Then
        UIMaster.ShowErrorMessage Err.Description
        Err.Clear
        Exit Sub
    End If
    
    'do we need to do any post save update price to processing
    Select Case rfrmForm.FormName
    Case strrHB_RELEASE_MASS_PRICE_UPDATE
        vntNBHDPhase_Id = UIMaster.RUICenter.RecordId
        If intUpdatePriceTo1 = 1 Then Global.ProcessUpdatePrice vntNBHDPhase_Id, intNBHDP_PRODUCT, intMPU_PLANS_FOR_RELEASE
        If intUpdatePriceTo2 = 1 Then Global.ProcessUpdatePrice vntNBHDPhase_Id, intLOT, intMPU_LOT
        If intUpdatePriceTo3 = 1 Then Global.ProcessUpdatePrice vntNBHDPhase_Id, intNBHDP_PRODUCT_NOT_PLAN, intMPU_GLOBAL_OPTIONS_FOR_RELEASE
        If intUpdatePriceTo4 = 1 Then Global.ProcessUpdatePrice vntNBHDPhase_Id, intNBHDP_PRODUCT_NOT_PLAN, intMPU_OTHER_OPTIONS_FOR_RELEASE
    End Select
End Sub
' ------------------------------------------------------------------------------------------------------
' Name:     NewFormData
' Purpose:  This function opens a new form
' ------------------------------------------------------------------------------------------------------
' Inputs:
'       rfrmForm : Hold the reference for the primary IRForm
'       vntParameters : The parameters passed to/from MT
' Returns:
'       NewFormData   : Returned recordset array of new form data
' History:
' Revision#     Date        Author  Description
' ----------    ----        ------  -----------
' HB
' 1.0           04/29/2003    AVasa  Initial version
' 2.0           03/11/2005    BHan   Disable Inactive flag for new record
' ---------------------------------------------------------------------------------------
Function NewFormData(rfrmForm, vntParameters)
    On Error Resume Next
    NewFormData = rfrmForm.DoNewFormData(vntParameters)
    If Err.Number <> 0 Then
        UIMaster.ShowErrorMessage Err.Description
        Err.Clear
        Exit Function
    End If
    
    ' Disable Inactive flag
    UIMaster.RUICenter.DisableFieldEx mstraRELEASE, strsRELEASE, strfINACTIVE, True

End Function
' -------------------------------------------------------------------------------------------
' Name:     NewSecondaryData
' Purpose:  This client script is used to add a new secondary record
' -------------------------------------------------------------------------------------------
' Inputs:
'        rfrmForm      : The IRform object reference to the client IRForm object
'        vntSecondary  : The secondary name
'        vntParameters : The Parameters passed from Client to Middle tier for Business rule
'        rstRecordset  : The secondary recordset
' Returns:
'        None
' History:
' Revision#     Date        Author  Description
' ----------    ----        ------  -----------
' HB
' 1.0           04/29/2003    AVasa  Initial version
' ------------------------------------------------------------------------------------------
Sub NewSecondaryData(rfrmForm, vntSecondary, vntParameters, rstRecordset)
    On Error Resume Next
    rfrmForm.DoNewSecondaryData vntSecondary, vntParameters, rstRecordset
    If Err.Number <> 0 Then
        UIMaster.ShowErrorMessage Err.Description
        Err.Clear
        Exit Sub
    End If
End Sub
' -----------------------------------------------------------------------------------------------------------------
' Name:    DeleteFormData
' Purpose: This function calls the cascade delete agent when the primary
'          record is deleted.
' ------------------------------------------------------------------------------------------------------
' Inputs:
'       rfrmForm      : The IRform object reference to the client IRForm object
'       vntRecordId   : The primary record Id
'       vntParameters : The Parameters passed from Client to Middle tier for Business rule
' Returns:
'       None
' History:
' Revision#     Date        Author  Description
' ----------    ----        ------  -----------
' HB
' 1.0           04/29/2003    AVasa  Initial version
' --------------------------------------------------------------------------------------
Public Sub DeleteFormData(rfrmForm, vntRecordId, vntParameters)
    
    Dim strMsg
    Dim objParam
    
    On Error Resume Next
    Call rfrmForm.DoDeleteFormData(vntRecordId, vntParameters)
    If Err.Number <> 0 Then
        UIMaster.ShowErrorMessage Err.Description
        Err.Clear
        Exit Sub
    Else
        Set objParam = Global.CreateTransitPointParamsObj()
        strMsg = objParam.GetInfoString(vntParameters)
        If Len(strMsg) > 0 Then
            UIMaster.ShowErrorMessage strMsg
            UIMaster.RUICenter.DeleteCanceled = True
        End If
    End If
    
End Sub

' ------------------------------------------------------------------------------------------
' Name:     OnSecondaryAddClick
' Purpose:
'-------------------------------------------------------------------------------------------
' Inputs:
'   rfrmForm      : The IRform object reference to the client IRForm object
'   vntParameters : The Parameters passed from Client to Middle tier for Business rule
' Returns:
'   AddFormData   : Return information to MT COM
' Implements Agent:
' History:
' Revision#             Date        Author       Description
'-----------------      ----------  -------     -----------------
' HB
' 1.0                   04/23/2003    CL        Initial Version
' 2.0                   03/09/2005    BHan      Don't allow user to add a new secondary data if Inactive flag is True
' 3.6                   04/10/2005    AKostic   Allowed adding of Secondary Service team even if it is Inactive
' 3.6                   06/28/2005    JWang     Comment out. Issue #17921
' 3.6                   09/16/2005    JWang     Reactivate the part of code, do not allow adding secondary records when Release is inactive.
' ------------------------------------------------------------------------------------------

Function OnSecondaryAddClick()
Dim objEvent, strSegmentName, rstPrimary
Dim strMessage

    On Error Resume Next
    Set rstPrimary = UIMaster.RUICenter.PrimaryRecordset
    Set objEvent = UIMaster.RUICenter.FormEventObj
    strSegmentName = objEvent.SegmentName

    ' If Inactive flag is True, popup message and exit
    If rstPrimary.Fields(strfINACTIVE).Value Then
        ' allow editing only of the service team, do not exit
        If strSegmentName <> strsSERVICE_TEAM Then
            strMessage = UIMaster.RSysClient.GetLDGroup(strgNBHD_PHASE).GetText(strdCANNOT_ADD_SECONDARY)
            Global.CMSMsgBox strMessage, vbOKOnly, ""
            OnSecondaryAddClick = True
            Exit Function
        End If
    End If


    If UIMaster.RUICenter.IsModified Then
        UIMaster.RUICenter.Apply Null
        If Err.Number <> 0 Then
            UIMaster.ShowErrorMessage Err.Description
            OnSecondaryAddClick = True
            Err.Clear
            Exit Function
        End If
    End If

    OnSecondaryAddClick = False
    
End Function


' ------------------------------------------------------------------------------------------
' Name:     OpenLot
' Purpose:  Add the default fields when a lot is opened
'-------------------------------------------------------------------------------------------
' Inputs:
' Returns:
' Implements Agent:
' History:
' Revision#             Date        Author       Description
'-----------------      ----------  -------     -----------------
' HB
' 1.0                   04/23/2003    CL        Initial Version
' ------------------------------------------------------------------------------------------

Function OpenLot(rstPrimary)
    Dim objParam, vntParams

    On Error Resume Next

    Set objParam = Global.CreateTransitPointParamsObj()

    With rstPrimary
        objParam.AddDefaultField strfADDRESS, .Fields(strfADDRESS).Value
        objParam.AddDefaultField strfSTATE_, .Fields(strfSTATE_).Value
        objParam.AddDefaultField strfZIP, .Fields(strfZIP).Value
        objParam.AddDefaultField strfCOUNTY, .Fields(strfCOUNTY).Value
        objParam.AddDefaultField strfCITY, .Fields(strfCITY).Value
        objParam.AddDefaultField strfNBHD_PHASE_ID, .Fields(strfNBHD_PHASE_ID).Value
        objParam.AddDefaultField strfSALES_MANAGER_ID, .Fields(strfSALES_MANAGER_ID).Value
        vntParams = objParam.ConstructParams
    End With
    
    UIMaster.ShowForm 1, strrHB_LOT_ADMIN, Null, vntParams
    If Err.Number <> 0 Then
        UIMaster.ShowErrorMessage Err.Description
        OnSecondaryAddClick = True
        Err.Clear
        Exit Function
    End If
End Function

' ------------------------------------------------------------------------------------------
' Name:     CopyActiveGlobalOptions
' Purpose:  Copies All Active Global options to Selected Releases
'-------------------------------------------------------------------------------------------
' Inputs:
'       None
'
' Returns:
'       Nothing
' History:
' Revision#             Date        Author       Description
'-----------------      ----------  -------     -----------------
' HB
' 3.6                  03/29/2005   AKostic       Initial Version
' 3.6                  2005/04/10   rjordan       review
' ------------------------------------------------------------------------------------------
Sub CopyActiveGlobalOptions()
    On Error Resume Next
    Dim objForm
    Dim rstRecordset
    Dim objSearchFactory
    Dim objMultiSelectionResults
    Dim rstSelectionLists
    Dim intButtonCancelIndex
    Dim intButtonOKIndex
    Dim vntDivisionId
    Dim vntCurrPhaseId
    Dim vntArgument
    Dim objParam
    Dim objButtonCancel
    Dim objButtonOK
    Dim objButtons
    Dim strMessage

    Set objForm = UIMaster.RUICenter.Form
    Set rstRecordset = UIMaster.RUICenter.PrimaryRecordset

    vntDivisionId = rstRecordset.Fields(strfDIVISION_ID).Value
    vntCurrPhaseId = rstRecordset.Fields(strfNBHD_PHASE_ID).Value
 
    Set objSearchFactory = UIMaster.CreateCenterReference(strSEARCH_TYPE)
    objSearchFactory.SearchType = 4
    ' *searchTypeMultiSelectionList* this enum value is currently 6
    ' and causes Show method exception
    Set objSearchFactory.Search = UIMaster.RSysClient.GetSearch(strhCOPY_OPTIONS_GLOBAL_ALL_RELEASES)
    objSearchFactory.Parameters(0) = UIMaster.RSysClient.IdToString(vntDivisionId)
    objSearchFactory.Parameters(1) = UIMaster.RSysClient.IdToString(vntCurrPhaseId)
    objSearchFactory.Options.AutoRun = True
    ' Create two buttons - OK and Cancel
    Set objButtons = UIMaster.CreateIndexButtons
    Set objButtonOK = UIMaster.CreateIndexButton
    objButtonOK.Label = UIMaster.RSysClient.GetLDGroup(strgCOMMON).GetText(strdOK)
    objButtonOK.Tooltip = ""
    intButtonOKIndex = objButtons.SetItem(objButtonOK)
    Set objButtonCancel = UIMaster.CreateIndexButton
    objButtonCancel.Label = UIMaster.RSysClient.GetLDGroup(strgCOMMON).GetText(strdCANCEL)
    objButtonCancel.Tooltip = ""
    intButtonCancelIndex = objButtons.SetItem(objButtonCancel)
    objSearchFactory.MultiSelectMode = mulselBoolean
    Set objSearchFactory.SearchButtons = objButtons
    Set objMultiSelectionResults = UIMaster.ShowMultiSelectModal(objSearchFactory, True)
    ' Cancel Button pressed
    If objMultiSelectionResults.SelectedButton = intButtonCancelIndex Then Exit Sub
    ' OK button pressed
    Set rstSelectionLists = objMultiSelectionResults.SelectedRecords
    If rstSelectionLists Is Nothing Then Exit Sub
    If rstSelectionLists.RecordCount = 0 Then Exit Sub
    'Transfer Selected Items to parameters
    Set objParam = Global.CreateTransitPointParamsObj()
    objParam.SetUserDefParam 1, rstSelectionLists
    objParam.SetUserDefParam 2, vntDivisionId
    objParam.SetUserDefParam 3, vntCurrPhaseId
    vntArgument = objParam.ConstructParams()
    'Call copy method in ASR:
    Call objForm.Execute(strmCOPY_OPTIONS_GLOBAL_ALL_TO_SELECTED_RELEASES, vntArgument)
     If Err.Number <> 0 Then
        UIMaster.ShowErrorMessage Err.Description
        Err.Clear
        Exit Sub
    Else
        strMessage = CStr(objParam.GetUserDefParam(1, vntArgument)) & " "
        strMessage = strMessage & UIMaster.RSysClient.GetLDGroup(strgNBHD_PRODUCT).GetText(strldCOPY_OPTION_4)
        strMessage = strMessage & " " & CStr(objParam.GetUserDefParam(2, vntArgument)) & " "
        strMessage = strMessage & UIMaster.RSysClient.GetLDGroup(strgNBHD_PHASE).GetText(strldRELEASES)
        strMessage = strMessage & UIMaster.RSysClient.GetLDGroup(strgNBHD_PRODUCT).GetText(strldCOPY_OPTION_3)
        Global.CMSMsgBox strMessage, vbOk, ""
    End If
End Sub

'--------------------------------------------------------------------------------------------
' Name:     OnSecondaryEditClick
' Purpose:  Grab the SecondaryEditClick event so that we can determine which form to move on to
' -------------------------------------------------------------------------------------------
' Inputs:
'       None
' Returns:
'       None
' History:
' Revision#     Date        Author  Description
' ----------    ----        ------  -----------
' 3.6           06/28/2005  JWang   Initial Version
' ------------------------------------------------------------------------------------------
Public Function OnSecondaryEditClick()
    Dim objForm, objFormEvent, strSegment_Name, vntEmployeeId
    Dim strTab_Name, iRow, rstSalesTeam, vntParameters
    
    On Error Resume Next
    OnSecondaryEditClick = False

    Set objForm = UIMaster.RUICenter.Form
    Set objFormEvent = UIMaster.RUICenter.FormEventObj
    strTab_Name = objFormEvent.TabName
    strSegment_Name = objFormEvent.SegmentName

            
    Select Case strSegment_Name
            
        Case strsNBHD_EMPLOYEES, strsSERVICE_TEAM
            Set rstSalesTeam = UIMaster.RUICenter.GetRecordset(strSegment_Name)
            If rstSalesTeam.RecordCount > 0 Then
                iRow = objFormEvent.RowIndex
                If iRow > -1 Then
                    rstSalesTeam.Move iRow - 1, 1
                    vntEmployeeId = rstSalesTeam.Fields(strfEMPLOYEE_ID).Value
                    if Global.CurrentUserInGroup(Global.gstrsecHOMEBUILDERS_ADMIN) Then
                        UIMaster.ShowForm actionNoSave, strfHB_EMPLOYEE_ADMIN, vntEmployeeId, vntParameters
                    Else
                        UIMaster.ShowForm actionNoSave, strfHB_EMPLOYEE, vntEmployeeId, vntParameters
                    End If
                    
                    OnSecondaryEditClick = True
                End If
            End If
        
    End Select
    If Err.Number <> 0 Then
        UIMaster.ShowErrorMessage Err.Description
        Err.Clear
        Exit Function
    End If
End Function

'--------------------------------------------------------------------------------------------
' Name:      OnSecondaryDeleteClick
' Purpose:   Called when deleting a record by selecting the X on a a secondary segment
'--------------------------------------------------------------------------------------------
' Inputs:
'       None
' Returns:
'       None
' History:
' Revision # Date            Author  Description
'---------- ----            ------  -----------
' 3.6        2005/09/15      JWang   Initial Version
'3.7         06/08/2006      AV           Issue#65536-15404\
'--------------------------------------------------------------------------------------------
Function OnSecondaryDeleteClick()
    On Error Resume Next
        
    Dim objEvent, strSegmentName, rstSecondary, rstPrimary
    Dim iRow, vntReleaseAdjustmentId, vntReleaseId
    Dim objParam
    Dim vntParameters
    Dim blnRlsAdjInUse
    Dim strMessage

    Set objEvent = UIMaster.RUICenter.FormEventObj
    strSegmentName = objEvent.SegmentName
    Set rstPrimary = UIMaster.RUICenter.PrimaryRecordset
    
    Select Case strSegmentName
        Case strsRELEASE_ADJUSTMENTS
            OnSecondaryDeleteClick = False
            Set rstSecondary = UIMaster.RUICenter.GetRecordset(strsRELEASE_ADJUSTMENTS)
            If rstSecondary.RecordCount > 0 Then
                iRow = objEvent.RowIndex
                If iRow > -1 Then
                    rstSecondary.Move iRow - 1, 1
                    vntReleaseAdjustmentId = rstSecondary.Fields(strfRELEASE_ADJUSTMENT_ID).Value
                    vntReleaseId = UIMaster.RUICenter.RecordId
                    Set objParam = Global.CreateTransitPointParamsObj()
                    objParam.SetUserDefParam 1, vntReleaseAdjustmentId
                    objParam.SetUserDefParam 2, True 'clear any Opp Adjustment link
                    vntParameters = objParam.ConstructParams
                    UIMaster.RUICenter.Form.Execute strmRLS_ADJ_IN_USE, vntParameters
                    If Err.Number <> 0 Then
                        UIMaster.ShowErrorMessage Err.Description
                        Exit Function
                    End If
                    blnRlsAdjInUse = objParam.GetUserDefParam(1, vntParameters)
                    If blnRlsAdjInUse Then
                        strMessage = UIMaster.RSysClient.GetLDGroup(strgNBHD_PHASE).GetText(strdCANNOT_DELETE_RLS_ADJ)
                        Global.CMSMsgBox strMessage, vbOKOnly, ""
                        'Cancel deleting the release adjustment.
                        OnSecondaryDeleteClick = True
                    End If
                End If
            End If
            
    End Select
    If Err.Number <> 0 Then
        UIMaster.ShowErrorMessage Err.Description
        Err.Clear
        Exit Function
    End If
End Function

' ------------------------------------------------------------------------------------------
' Name    : CheckCloseDate
' Purpose: This Function is called on exit of Close Date and on Add/Save. If it is earlier than
'          the current date or Start Date then prompt an alert message, and clear out
'          the Start Date, and set focus on the Close Date.
' Revision#         Date           Author   Note
' 3.6               03/15/2005     JWang    Initial Version
' 3.7               Mar/24/2006    YK       Issue #56574
' ------------------------------------------------------------------------------------------
Public Function CheckCloseDate()

'variables
Dim strMessage
Dim blnReturnVal

On Error Resume Next

blnReturnVal = False
    strMessage = UIMaster.RSysClient.GetLDGroup(strgNBHD_PHASE).GetText(strldCLOSE_DATE_ALERT)
    
    If UIMaster.RUICenter.PrimaryRecordset.Fields(strfCLOSE_DATE).Value < Date _
       Or UIMaster.RUICenter.PrimaryRecordset.Fields(strfCLOSE_DATE).Value < UIMaster.RUICenter.PrimaryRecordset.Fields(strfOPEN_DATE).Value _
Or (Not IsNull(UIMaster.RUICenter.PrimaryRecordset.Fields(strfCLOSE_DATE).Value) And _
 IsNull(UIMaster.RUICenter.PrimaryRecordset.Fields(strfOPEN_DATE).Value)) Then ' YK - March 24, 2006 - Issue #56574
        Global.CMSMsgBox strMessage, vbOKOnly, ""
        'Clear the Close Date Field
        UIMaster.RUICenter.PrimaryRecordset.Fields(strfCLOSE_DATE).Value = Null
        'Focus to the Close Date field
        UIMaster.RUICenter.FocusField strsRELEASE, strfCLOSE_DATE
        blnReturnVal = True
    End If

    If Err.Number <> 0 Then
        UIMaster.ShowErrorMessage Err.Description
        Err.Clear
        Exit Function
    End If
CheckCloseDate = blnReturnVal
End Function

'----------------------------------------------------------------------
' Name: UpdateTreeWebTabURL
' Purpose: Refresh the WebTab depending on the Product Creation Level
'----------------------------------------------------------------------
' Inputs: none
' Outputs: none
' Returns: n/a
'----------------------------------------------------------------------
' Revision     Date          Author     Description
' --------  --------------      --------    ----------------
' 5.9          2/02/2007    NDcunha     Initial Version
' 5.9          3/02/2007    BC          Constants Initialization
'----------------------------------------------------------------------
Sub UpdateTreeWebTabURL(vntId)
    Dim strWebTabURL
    Dim vntDivisionId
    Dim vntRegionId
    Dim vntNeighborhoodId
    Dim vntReleaseId
    Dim strGeoURL
    Dim vntNewId

    Const strTab_RELEASE_ADMIN = "Release Administration"
    Const strSeg_GEO_LIBRARY = "Geographical Library"
    Const strSeg_PRODUCT_LIBRARY = "Product Library"

    'Initialize Constants
    Global.glblRegion = Null
    Global.glblDivision = Null
    Global.glblNeighborhood = Null
    Global.glblRelease = Null
    Global.glblTree = Null
    
    Global.glblTree = "Release Tree"
    Global.glblRelease = vntId
    Global.glblNeighborhood = UIMaster.RSysClient.GetTable(strtRelease).Fields(strfNEIGHBORHOOD_ID).index(Global.glblRelease)
    Global.glblDivision = UIMaster.RSysClient.GetTable(strtNEIGHBORHOOD).Fields(strfDIVISION_ID).index(UIMaster.RSysClient.IdToString(Global.glblNeighborhood))
    Global.glblRegion = UIMaster.RSysClient.GetTable(strtDIVISION).Fields(strfREGION_ID).index(UIMaster.RSysClient.IdToString(Global.glblDivision))
    
        strGeoURL = UIMaster.RUICenter.WebSegmentUrl(strTab_RELEASE_ADMIN, strSeg_GEO_LIBRARY)
    If Mid(strGeoURL, Len(strGeoURL)) = "=" And Not IsNull(UIMaster.RUICenter.PrimaryRecordset.Fields(strfNBHD_PHASE_ID).Value) Then
        vntNewId = Global.GetDecimalValue(UIMaster.RsysClient.IDtoString(UIMaster.RUICenter.PrimaryRecordset.Fields(strfNBHD_PHASE_ID).value))
        strGeoURL = strGeoURL & vntNewId
        UIMaster.RUICenter.WebSegmentUrl(strTab_RELEASE_ADMIN, strSeg_GEO_LIBRARY) = strGeoURL
        UIMaster.RUICenter.RefreshWebSegment strTab_RELEASE_ADMIN, strSeg_GEO_LIBRARY
    End If
        
    strWebTabURL = Global.gstrTreeWebTabURL

    vntReleaseId = Global.GetDecimalValue(UIMaster.RSysClient.IdToString(Global.glblRelease))
    vntNeighborhoodId = Global.GetDecimalValue(UIMaster.RSysClient.IdToString(Global.glblNeighborhood))
    vntDivisionId = Global.GetDecimalValue(UIMaster.RSysClient.IdToString(Global.glblDivision))
    vntRegionId = Global.GetDecimalValue(UIMaster.RSysClient.IdToString(Global.glblRegion))
        
        If Global.gvntProductCreationLevel = 0 Then
        strWebTabURL = strWebTabURL + "?csname=Tree_CrpPrdDefnReleaseOptions&useCustomJS=True&parameters="
        ElseIf Global.gvntProductCreationLevel = 1 Then
        strWebTabURL = strWebTabURL + "?csname=Tree_RgnPrdDefnReleaseOptions&useCustomJS=True&parameters="
        ElseIf Global.gvntProductCreationLevel = 2 Then
        strWebTabURL = strWebTabURL + "?csname=Tree_DvnPrdDefnReleaseOptions&useCustomJS=True&parameters="
        End If
        strWebTabURL = strWebTabURL & vntReleaseId & ";" & vntNeighborhoodId & ";" & vntDivisionId & ";" & vntRegionId
        UIMaster.RUICenter.WebSegmentUrl(strTab_RELEASE_ADMIN, strSeg_PRODUCT_LIBRARY) = strWebTabURL
        UIMaster.RUICenter.RefreshWebSegment strTab_RELEASE_ADMIN, strSeg_PRODUCT_LIBRARY
        
End Sub





'----------------------------------------------------------------------
' Name: IsInEscrowSecurityGroup
' Purpose: ADded for IP So that the fields in the
'DRE tab are read only if user is in this security group
'----------------------------------------------------------------------
' Inputs: none
' Outputs: none
' Returns: n/a
'----------------------------------------------------------------------
' Revision     Date          Author        Description
' --------  --------------   --------      ----------------
' 5.9          09/03/2010    A.Maldonado   Initial Version
'----------------------------------------------------------------------
Function IsInEscrowSecurityGroup

    Dim group

    'Loop through security list
    For Each group In Global.gvntSecurityGroups
      
      'If in Escrow group disable all DRE fields  
      If group  = strsecTIC_ESCROW Then       
         IsInEscrowSecurityGroup = true       
         exit function

      End If 

    Next 
  
  
  IsInEscrowSecurityGroup = False

End Function

'----------------------------------------------------------------------
' Name: DisableAllDREFields
' Purpose: Added for IP, disable fields in DRE Tab
'----------------------------------------------------------------------
' Inputs: none
' Outputs: none
' Returns: n/a
'----------------------------------------------------------------------
' Revision     Date          Author        Description
' --------  --------------   --------      ----------------
' 5.9          09/03/2010    A.Maldonado   Initial Version
'----------------------------------------------------------------------
Sub DisableAllDREFields
  
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_DRE_Kick_Off_Meeting", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_DRE_File_Number", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_Budget_Expires", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_Pink_Issued", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_Pink_Expires", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_Yellow_Issued", True

UIMaster.RUICenter.DisableField segDREPrimary, "TIC_Yellow_Expires", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_White_Issued", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_White_Expires", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_Init_Sub_App_Budg_To_DRE", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_Receive_Notice_From_DRE", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_Receive_Deficency_From_DRE", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_Resp_Defic_Resubmit_To_DRE", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_Rec_Final_Notice_Or_Def", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_Receive_Budget_Approval", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_Receive_Conditions_Of_App", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_Record_Maps_Plans", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_Rec_Route_Obtain_Sub_To_DR", True

UIMaster.RUICenter.DisableField segDREPrimary, "TIC_Receive_Report", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_DRE_Phase", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_FHA_Approval_Received", True
UIMaster.RUICenter.DisableField segDREPrimary, "TIC_VA_Approval_Received", True
UIMaster.RUICenter.DisableField "Notes", "TIC_Escrow_Notes", True

End Sub