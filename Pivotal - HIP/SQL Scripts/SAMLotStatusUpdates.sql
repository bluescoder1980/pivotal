SELECT TIC_LOT_NUMBER AS LOT_NUMBER, TIC_UNIT_NUMBER, T.TIC_TRACT_NUMBER FROM TIC_LOT L
INNER JOIN TIC_TRACT T ON L.TIC_TRACT_ID = T.TIC_TRACT_ID
WHERE TIC_LOT_NUMBER = '2' AND TIC_UNIT_NUMBER = '37'
ORDER BY TIC_LOT_NUMBER ASC


Select Neighborhood,Tract, Lot_Number, Unit,
CP.TIC_Construction_Project_Name, T.TIC_Tract_Number,L.TIC_Lot_Number, L.TIC_Unit_Number

-- SOURCE QUERY TO GET ALL LOT STATUS HISTORY RECORDS AND CONTRACT FIELDS
O.STATUS, ST.TIC_MAP_STATUS

SELECT LH.TIC_INT_SAM_CONTRACT_ID,
L.TIC_LOT_ID AS SAM_LOT_ID, 
LH.STATUS_CHANGE_NUMBER AS HIP_STATUS_CHANGE_NUMBER, 
LH.CHANGED_BY AS HIP_CHANGED_BY, 
LH.CHANGED_ON AS HIP_CHANGED_ON, 
LH.DATE_OF_BUS_TRANSACTION AS HIP_DATE_OF_BUS_TRANSACTION, 
LH.COMMENTS AS HIP_COMMENTS, 

LH.LOT_STATUS_CHANGED_TO AS Disconnected_1_2_21, -- Disconnected_1_2_21
CONVERT(INT, LH.OPPORTUNITY_ID) AS Disconnected_1_2_19, -- Disconnected_1_2_19
CP.TIC_Neighborhood_Id AS Disconnected_1_2_1,  -- Disconnected_1_2_1
T.TIC_Tract_Id Disconnected_1_2_2, -- Disconnected_1_2_2
O.ECOE_DATE AS Disconnected_1_2_3, -- Disconnected_1_2_3
O.Reservation_Date AS Disconnected_1_2_4, -- Disconnected_1_2_4
O.TIC_Reservation_Can_Date AS Disconnected_1_2_5, -- Disconnected_1_2_5
O.Actual_Revenue_Date AS Disconnected_1_2_6, -- Disconnected_1_2_6
O.Cancel_Date AS Disconnected_1_2_7, -- Disconnected_1_2_7
O.Additional_Price AS Disconnected_1_2_8, -- Disconnected_1_2_8
CR.CANCEL_REASON AS Disconnected_1_2_9, -- Disconnected_1_2_9
O.Elevation_Premium AS Disconnectec_1_2_10,-- Disconnectec_1_2_10
O.Lot_Premium AS Disconnected_1_2_11, -- Disconnected_1_2_11
O.Price AS Disconnected_1_2_12, -- Disconnected_1_2_12
O.TIC_Design_Options_Total AS Disconnected_1_2_13, -- Disconnected_1_2_13
O.TIC_Preplot_Options AS Disconnected_1_2_14,-- Disconnected_1_2_14
CONVERT(INT, CO.OPPORTUNITY_ID) AS Disconnected_1_2_15,-- Disconnected_1_2_15
CONVERT(INT, CG.OPPORTUNITY_ID) AS Disconnected_1_2_16,-- Disconnected_1_2_16
CB1.CONTACT_ID AS BUYER_ID, -- Disconnected_1_2_17
CB2.CONTACT_ID AS CO_BUYER_ID, -- Disconnected_1_2_18

-- BUYER LOOKUP KEYS
CB1.FIRST_NAME AS CO_BUYER_1_FIRST_NAME,
CB1.LAST_NAME AS CO_BUYER_1_LAST_NAME,
CB1.EMAIL AS CO_BUYER_1_EMAIL,
CB1.PHONE AS CO_BUYER_1_PHONE,
CB1.CELL AS CO_BUYER_1_CELL,
CB1.ADDRESS_1 AS CO_BUYER_1_ADDRESS_1,

-- CO BUYER 2 LOOKUP KEYS
CB2.FIRST_NAME AS CO_BUYER_2_FIRST_NAME,
CB2.LAST_NAME AS CO_BUYER_2_LAST_NAME,
CB2.EMAIL AS CO_BUYER_2_EMAIL,
CB2.PHONE AS CO_BUYER_2_PHONE,
CB2.CELL AS CO_BUYER_2_CELL,
CB2.ADDRESS_1 AS CO_BUYER_2_ADDRESS_1


FROM 
IP_ED..TIC_INT_SAM_CONTRACT LH 
INNER JOIN IP_Ed..Product P ON P.PRODUCT_ID = LH.PRODUCT_ID
INNER JOIN IP_Ed..Opportunity O on LH.OPPORTUNITY_ID = O.OPPORTUNITY_ID
JOIN ProdMasterED..TIC_Construction_Project CP 
     on P.Neighborhood = CP.TIC_Construction_Project_Name
JOIN ProdMasterED..TIC_Tract T 
     on CP.TIC_Construction_Project_Id=T.TIC_Construction_Project_Id
     AND P.Tract=T.TIC_Tract_Number
JOIN ProdMasterED..TIC_Lot L 
     ON T.TIC_Tract_Id=L.TIC_Tract_Id
     AND P.Lot_Number=L.TIC_Lot_Number
     AND IsNull(P.Unit,'')=IsNull(L.TIC_Unit_Number,'')

LEFT JOIN IP_ED..CANCEL_REASON CR ON O.CANCEL_REASON_ID = CR.CANCEL_REASON_ID

LEFT JOIN (SELECT CO.OPPORTUNITY_ID FROM IP_ED..COMPANY__OPPORTUNITY CO

GROUP BY CO.OPPORTUNITY_ID) CO ON O.OPPORTUNITY_ID = CO.OPPORTUNITY_ID

LEFT JOIN (SELECT CG.OPPORTUNITY_ID FROM IP_ED..CONTINGENCY CG
GROUP BY CG.OPPORTUNITY_ID) CG ON O.OPPORTUNITY_ID = CG.OPPORTUNITY_ID

LEFT JOIN IP_ED..CONTACT CB1 ON O.CONTACT_ID = CB1.CONTACT_ID
LEFT JOIN IP_ED..CONTACT CB2 ON O.TIC_CO_BUYER_ID = CB2.CONTACT_ID

WHERE ISNULL(LH.TIC_SYNCHED, 0) = 0
AND (LH.LOT_STATUS_CHANGED_TO = 'Not Released' 
	OR LH.LOT_STATUS_CHANGED_TO = 'Released'
	OR LH.LOT_STATUS_CHANGED_TO = 'Available'
	OR LH.LOT_STATUS_CHANGED_TO = 'Reserved'
	OR LH.LOT_STATUS_CHANGED_TO = 'Sold'
	OR LH.LOT_STATUS_CHANGED_TO = 'Closed'
    OR LH.LOT_STATUS_CHANGED_TO = 'Cancelled'
	OR LH.LOT_STATUS_CHANGED_TO = 'Cancelled Reserve')

ORDER BY L.TIC_LOT_ID, LH.STATUS_CHANGE_NUMBER ASC



-- WE ONLY CARE ABOUT THE TRANSFER SALE RECORD SO THAT WE CAN CANCEL THE 
-- TRANSFER FROM LOT (CONTRACT) IN SAM
SELECT LH.TIC_INT_SAM_CONTRACT_ID,
L.TIC_LOT_ID AS SAM_LOT_ID, 
LH.STATUS_CHANGE_NUMBER AS HIP_STATUS_CHANGE_NUMBER, 
LH.CHANGED_BY AS HIP_CHANGED_BY, 
LH.CHANGED_ON AS HIP_CHANGED_ON, 
LH.DATE_OF_BUS_TRANSACTION AS HIP_DATE_OF_BUS_TRANSACTION, 
LH.COMMENTS AS HIP_COMMENTS, 

LH.LOT_STATUS_CHANGED_TO AS Disconnected_1_2_21, -- Disconnected_1_2_21
CONVERT(INT, LH.OPPORTUNITY_ID) AS Disconnected_1_2_19, -- Disconnected_1_2_19
CP.TIC_Neighborhood_Id AS Disconnected_1_2_1,  -- Disconnected_1_2_1
T.TIC_Tract_Id Disconnected_1_2_2, -- Disconnected_1_2_2
O.ECOE_DATE AS Disconnected_1_2_3, -- Disconnected_1_2_3
O.Reservation_Date AS Disconnected_1_2_4, -- Disconnected_1_2_4
O.TIC_Reservation_Can_Date AS Disconnected_1_2_5, -- Disconnected_1_2_5
O.Actual_Revenue_Date AS Disconnected_1_2_6, -- Disconnected_1_2_6
O.Cancel_Date AS Disconnected_1_2_7, -- Disconnected_1_2_7
O.Additional_Price AS Disconnected_1_2_8, -- Disconnected_1_2_8
CR.CANCEL_REASON AS Disconnected_1_2_9, -- Disconnected_1_2_9
O.Elevation_Premium AS Disconnectec_1_2_10,-- Disconnectec_1_2_10
O.Lot_Premium AS Disconnected_1_2_11, -- Disconnected_1_2_11
O.Price AS Disconnected_1_2_12, -- Disconnected_1_2_12
O.TIC_Design_Options_Total AS Disconnected_1_2_13, -- Disconnected_1_2_13
O.TIC_Preplot_Options AS Disconnected_1_2_14,-- Disconnected_1_2_14
CONVERT(INT, CO.OPPORTUNITY_ID) AS Disconnected_1_2_15,-- Disconnected_1_2_15
CONVERT(INT, CG.OPPORTUNITY_ID) AS Disconnected_1_2_16,-- Disconnected_1_2_16
CB1.CONTACT_ID AS BUYER_ID, -- Disconnected_1_2_17
CB2.CONTACT_ID AS CO_BUYER_ID, -- Disconnected_1_2_18

-- BUYER LOOKUP KEYS
CB1.FIRST_NAME AS CO_BUYER_1_FIRST_NAME,
CB1.LAST_NAME AS CO_BUYER_1_LAST_NAME,
CB1.EMAIL AS CO_BUYER_1_EMAIL,
CB1.PHONE AS CO_BUYER_1_PHONE,
CB1.CELL AS CO_BUYER_1_CELL,
CB1.ADDRESS_1 AS CO_BUYER_1_ADDRESS_1,

-- CO BUYER 2 LOOKUP KEYS
CB2.FIRST_NAME AS CO_BUYER_2_FIRST_NAME,
CB2.LAST_NAME AS CO_BUYER_2_LAST_NAME,
CB2.EMAIL AS CO_BUYER_2_EMAIL,
CB2.PHONE AS CO_BUYER_2_PHONE,
CB2.CELL AS CO_BUYER_2_CELL,
CB2.ADDRESS_1 AS CO_BUYER_2_ADDRESS_1


FROM 
IP_ED..TIC_INT_SAM_CONTRACT LH 
INNER JOIN IP_Ed..Product P ON P.PRODUCT_ID = LH.PRODUCT_ID
INNER JOIN IP_Ed..Opportunity O on LH.OPPORTUNITY_ID = O.OPPORTUNITY_ID
JOIN ProdMasterED..TIC_Construction_Project CP 
     on P.Neighborhood = CP.TIC_Construction_Project_Name
JOIN ProdMasterED..TIC_Tract T 
     on CP.TIC_Construction_Project_Id=T.TIC_Construction_Project_Id
     AND P.Tract=T.TIC_Tract_Number
JOIN ProdMasterED..TIC_Lot L 
     ON T.TIC_Tract_Id=L.TIC_Tract_Id
     AND P.Lot_Number=L.TIC_Lot_Number
     AND IsNull(P.Unit,'')=IsNull(L.TIC_Unit_Number,'')

LEFT JOIN IP_ED..CANCEL_REASON CR ON O.CANCEL_REASON_ID = CR.CANCEL_REASON_ID

LEFT JOIN (SELECT CO.OPPORTUNITY_ID FROM IP_ED..COMPANY__OPPORTUNITY CO

GROUP BY CO.OPPORTUNITY_ID) CO ON O.OPPORTUNITY_ID = CO.OPPORTUNITY_ID

LEFT JOIN (SELECT CG.OPPORTUNITY_ID FROM IP_ED..CONTINGENCY CG
GROUP BY CG.OPPORTUNITY_ID) CG ON O.OPPORTUNITY_ID = CG.OPPORTUNITY_ID

LEFT JOIN IP_ED..CONTACT CB1 ON O.CONTACT_ID = CB1.CONTACT_ID
LEFT JOIN IP_ED..CONTACT CB2 ON O.TIC_CO_BUYER_ID = CB2.CONTACT_ID

WHERE (ISNULL(LH.TIC_SYNCHED, 0) = 0 OR LH.TIC_SYNCHED = 0)
AND (LH.LOT_STATUS_CHANGED_TO = 'Transfer Sale') 
AND (LH.PRODUCT_ID = LH.TRANSFER_FROM_lOT_ID)




-- WE ONLY CARE ABOUT THE ROLLBACK RECORD SO THAT WE CAN ROLLBACK THE CORRECT
-- LOT HISTORY STATUS RECORD IN SAM
SELECT LH.TIC_INT_SAM_CONTRACT_ID,
L.TIC_LOT_ID AS SAM_LOT_ID, 
LH.STATUS_CHANGE_NUMBER AS HIP_STATUS_CHANGE_NUMBER, 
LH.CHANGED_BY AS HIP_CHANGED_BY, 
LH.CHANGED_ON AS HIP_CHANGED_ON, 
LH.DATE_OF_BUS_TRANSACTION AS HIP_DATE_OF_BUS_TRANSACTION, 
LH.COMMENTS AS HIP_COMMENTS, 

LH.LOT_STATUS_CHANGED_TO AS Disconnected_1_2_21, -- Disconnected_1_2_21
CONVERT(INT, LH.OPPORTUNITY_ID) AS Disconnected_1_2_19, -- Disconnected_1_2_19
CP.TIC_Neighborhood_Id AS Disconnected_1_2_1,  -- Disconnected_1_2_1
T.TIC_Tract_Id Disconnected_1_2_2, -- Disconnected_1_2_2
O.ECOE_DATE AS Disconnected_1_2_3, -- Disconnected_1_2_3
O.Reservation_Date AS Disconnected_1_2_4, -- Disconnected_1_2_4
O.TIC_Reservation_Can_Date AS Disconnected_1_2_5, -- Disconnected_1_2_5
O.Actual_Revenue_Date AS Disconnected_1_2_6, -- Disconnected_1_2_6
O.Cancel_Date AS Disconnected_1_2_7, -- Disconnected_1_2_7
O.Additional_Price AS Disconnected_1_2_8, -- Disconnected_1_2_8
CR.CANCEL_REASON AS Disconnected_1_2_9, -- Disconnected_1_2_9
O.Elevation_Premium AS Disconnectec_1_2_10,-- Disconnectec_1_2_10
O.Lot_Premium AS Disconnected_1_2_11, -- Disconnected_1_2_11
O.Price AS Disconnected_1_2_12, -- Disconnected_1_2_12
O.TIC_Design_Options_Total AS Disconnected_1_2_13, -- Disconnected_1_2_13
O.TIC_Preplot_Options AS Disconnected_1_2_14,-- Disconnected_1_2_14
CONVERT(INT, CO.OPPORTUNITY_ID) AS Disconnected_1_2_15,-- Disconnected_1_2_15
CONVERT(INT, CG.OPPORTUNITY_ID) AS Disconnected_1_2_16,-- Disconnected_1_2_16
CB1.CONTACT_ID AS BUYER_ID, -- Disconnected_1_2_17
CB2.CONTACT_ID AS CO_BUYER_ID, -- Disconnected_1_2_18

-- BUYER LOOKUP KEYS
CB1.FIRST_NAME AS CO_BUYER_1_FIRST_NAME,
CB1.LAST_NAME AS CO_BUYER_1_LAST_NAME,
CB1.EMAIL AS CO_BUYER_1_EMAIL,
CB1.PHONE AS CO_BUYER_1_PHONE,
CB1.CELL AS CO_BUYER_1_CELL,
CB1.ADDRESS_1 AS CO_BUYER_1_ADDRESS_1,

-- CO BUYER 2 LOOKUP KEYS
CB2.FIRST_NAME AS CO_BUYER_2_FIRST_NAME,
CB2.LAST_NAME AS CO_BUYER_2_LAST_NAME,
CB2.EMAIL AS CO_BUYER_2_EMAIL,
CB2.PHONE AS CO_BUYER_2_PHONE,
CB2.CELL AS CO_BUYER_2_CELL,
CB2.ADDRESS_1 AS CO_BUYER_2_ADDRESS_1


FROM 
IP_ED..TIC_INT_SAM_CONTRACT LH 
INNER JOIN IP_Ed..Product P ON P.PRODUCT_ID = LH.PRODUCT_ID
INNER JOIN IP_Ed..Opportunity O on LH.OPPORTUNITY_ID = O.OPPORTUNITY_ID
JOIN ProdMasterED..TIC_Construction_Project CP 
     on P.Neighborhood = CP.TIC_Construction_Project_Name
JOIN ProdMasterED..TIC_Tract T 
     on CP.TIC_Construction_Project_Id=T.TIC_Construction_Project_Id
     AND P.Tract=T.TIC_Tract_Number
JOIN ProdMasterED..TIC_Lot L 
     ON T.TIC_Tract_Id=L.TIC_Tract_Id
     AND P.Lot_Number=L.TIC_Lot_Number
     AND IsNull(P.Unit,'')=IsNull(L.TIC_Unit_Number,'')

LEFT JOIN IP_ED..CANCEL_REASON CR ON O.CANCEL_REASON_ID = CR.CANCEL_REASON_ID

LEFT JOIN (SELECT CO.OPPORTUNITY_ID FROM IP_ED..COMPANY__OPPORTUNITY CO

GROUP BY CO.OPPORTUNITY_ID) CO ON O.OPPORTUNITY_ID = CO.OPPORTUNITY_ID

LEFT JOIN (SELECT CG.OPPORTUNITY_ID FROM IP_ED..CONTINGENCY CG
GROUP BY CG.OPPORTUNITY_ID) CG ON O.OPPORTUNITY_ID = CG.OPPORTUNITY_ID

LEFT JOIN IP_ED..CONTACT CB1 ON O.CONTACT_ID = CB1.CONTACT_ID
LEFT JOIN IP_ED..CONTACT CB2 ON O.TIC_CO_BUYER_ID = CB2.CONTACT_ID

WHERE ISNULL(LH.TIC_SYNCHED, 0) = 0
AND (LH.LOT_STATUS_CHANGED_TO = 'Rollback Sale' OR LH.LOT_STATUS_CHANGED_TO = 'Rollback Reserve') 
ORDER BY L.TIC_LOT_ID, STATUS_CHANGE_NUMBER ASC


SELECT * FROM ProdMasterED..TIC_LOT_STATUS_HISTORY ORDER BY RN_CREATE_DATE DESC

SELECT * FROM TIC_INT_SAM_CONTRACT WHERE ISNULL(TIC_SYNCHED,0) = 0
SELECT * FROM PRODUCT WHERE PRODUCT_ID = 0x0000000000000024
--
--
--update TIC_INT_SAM_CONTRACT
--set tic_synched = 1
--where tic_int_sam_contract_id = 0x000000000000046F
0x0000000000000440
0x000000000000043F
0x000000000000043E
0x000000000000043D


SELECT 
TIC_Project_Id,
TIC_Tract_Id,
TIC_Lot_Id,
TIC_Buyer_1_Contact_Id,
TIC_Buyer_2_Contact_Id,
TIC_Lot_Sale_Status_Id,
TIC_Estimated_Closing_Date,
TIC_Sale_Status_Last_Change_Dt,
TIC_Broker_Used_In_Sale_Indic,
TIC_Contingency_Sale,
TIC_Cancellation_Reason,
TIC_Date_Reserved,
TIC_Date_Reservation_Cancelled,
TIC_Date_Sold,
TIC_Date_Sale_Cancelled,
TIC_Date_Closed,
TIC_Base_Price,
TIC_Incentive_Price,
TIC_Selling_Elevation_Premium,
TIC_Selling_Location_Premium,
TIC_Premium_Price,
TIC_Selling_Upgrade_Preplot,
TIC_Selling_Homebuyer_Extr_Opt,
TIC_Selling_Models_Upgrade_Rec,
TIC_Pre_Plots_Price,
TIC_Closing_Base_Price,
TIC_Closing_Incentive_Price,
TIC_Closing_Elevation_Premium,
TIC_Closing_Location_Premium,
TIC_Closing_Premium,
TIC_Closing_Upgrade_Preplot,
TIC_Closing_Homebuyer_Extr_Opt,
TIC_Closing_Models_Upgrade_Rec,
TIC_Closing_Pre_Plots_Price
FROM tic_SALE















SELECT 

LH.TIC_INT_SAM_CONTRACT_ID,
L.TIC_LOT_ID AS SAM_LOT_ID, 
LH.STATUS_CHANGE_NUMBER AS HIP_STATUS_CHANGE_NUMBER, 
LH.CHANGED_BY AS HIP_CHANGED_BY, 
LH.CHANGED_ON AS HIP_CHANGED_ON, 
LH.DATE_OF_BUS_TRANSACTION AS HIP_DATE_OF_BUS_TRANSACTION, 
LH.COMMENTS AS HIP_COMMENTS, 

LH.LOT_STATUS_CHANGED_TO AS Disconnected_1_2_21, -- Disconnected_1_2_21
CONVERT(INT, LH.OPPORTUNITY_ID) AS Disconnected_1_2_19, -- Disconnected_1_2_19
CP.TIC_Neighborhood_Id AS Disconnected_1_2_1,  -- Disconnected_1_2_1
T.TIC_Tract_Id Disconnected_1_2_2, -- Disconnected_1_2_2
O.ECOE_DATE AS Disconnected_1_2_3, -- Disconnected_1_2_3
O.Reservation_Date AS Disconnected_1_2_4, -- Disconnected_1_2_4
O.TIC_Reservation_Can_Date AS Disconnected_1_2_5, -- Disconnected_1_2_5
O.Actual_Revenue_Date AS Disconnected_1_2_6, -- Disconnected_1_2_6
O.Cancel_Date AS Disconnected_1_2_7, -- Disconnected_1_2_7
O.Additional_Price AS Disconnected_1_2_8, -- Disconnected_1_2_8
CR.CANCEL_REASON AS Disconnected_1_2_9, -- Disconnected_1_2_9
O.Elevation_Premium AS Disconnectec_1_2_10,-- Disconnectec_1_2_10
O.Lot_Premium AS Disconnected_1_2_11, -- Disconnected_1_2_11
O.Price AS Disconnected_1_2_12, -- Disconnected_1_2_12
O.TIC_Design_Options_Total AS Disconnected_1_2_13, -- Disconnected_1_2_13
O.TIC_Preplot_Options AS Disconnected_1_2_14,-- Disconnected_1_2_14
CONVERT(INT, CO.OPPORTUNITY_ID) AS Disconnected_1_2_15,-- Disconnected_1_2_15
CONVERT(INT, CG.OPPORTUNITY_ID) AS Disconnected_1_2_16,-- Disconnected_1_2_16
CB1.CONTACT_ID AS BUYER_ID, -- Disconnected_1_2_17
CB2.CONTACT_ID AS CO_BUYER_ID, -- Disconnected_1_2_18

-- BUYER LOOKUP KEYS
CB1.FIRST_NAME AS CO_BUYER_1_FIRST_NAME,
CB1.LAST_NAME AS CO_BUYER_1_LAST_NAME,
CB1.EMAIL AS CO_BUYER_1_EMAIL,
CB1.PHONE AS CO_BUYER_1_PHONE,
CB1.CELL AS CO_BUYER_1_CELL,
CB1.ADDRESS_1 AS CO_BUYER_1_ADDRESS_1,

-- CO BUYER 2 LOOKUP KEYS
CB2.FIRST_NAME AS CO_BUYER_2_FIRST_NAME,
CB2.LAST_NAME AS CO_BUYER_2_LAST_NAME,
CB2.EMAIL AS CO_BUYER_2_EMAIL,
CB2.PHONE AS CO_BUYER_2_PHONE,
CB2.CELL AS CO_BUYER_2_CELL,
CB2.ADDRESS_1 AS CO_BUYER_2_ADDRESS_1



FROM 
IP_ED..TIC_INT_SAM_CONTRACT LH 
INNER JOIN IP_Ed..Product P ON P.PRODUCT_ID = LH.PRODUCT_ID
LEFT JOIN IP_Ed..Opportunity O on LH.OPPORTUNITY_ID = O.OPPORTUNITY_ID
JOIN ProdMasterED..TIC_Construction_Project CP 
     on P.Neighborhood = CP.TIC_Construction_Project_Name
JOIN ProdMasterED..TIC_Tract T 
     on CP.TIC_Construction_Project_Id=T.TIC_Construction_Project_Id
     AND P.Tract=T.TIC_Tract_Number
JOIN ProdMasterED..TIC_Lot L 
     ON T.TIC_Tract_Id=L.TIC_Tract_Id
     AND P.Lot_Number=L.TIC_Lot_Number
     AND IsNull(P.Unit,'')=IsNull(L.TIC_Unit_Number,'')

LEFT JOIN IP_ED..CANCEL_REASON CR ON O.CANCEL_REASON_ID = CR.CANCEL_REASON_ID

LEFT JOIN (SELECT CO.OPPORTUNITY_ID FROM IP_ED..COMPANY__OPPORTUNITY CO

GROUP BY CO.OPPORTUNITY_ID) CO ON O.OPPORTUNITY_ID = CO.OPPORTUNITY_ID

LEFT JOIN (SELECT CG.OPPORTUNITY_ID FROM IP_ED..CONTINGENCY CG
GROUP BY CG.OPPORTUNITY_ID) CG ON O.OPPORTUNITY_ID = CG.OPPORTUNITY_ID

LEFT JOIN IP_ED..CONTACT CB1 ON O.CONTACT_ID = CB1.CONTACT_ID
LEFT JOIN IP_ED..CONTACT CB2 ON O.TIC_CO_BUYER_ID = CB2.CONTACT_ID

WHERE ISNULL(LH.TIC_SYNCHED, 0) = 0
AND (LH.LOT_STATUS_CHANGED_TO = 'Not Released' 
	OR LH.LOT_STATUS_CHANGED_TO = 'Released'
	OR LH.LOT_STATUS_CHANGED_TO = 'Available'
	OR LH.LOT_STATUS_CHANGED_TO = 'Reserved'
	OR LH.LOT_STATUS_CHANGED_TO = 'Sold'
	OR LH.LOT_STATUS_CHANGED_TO = 'Closed'
    OR LH.LOT_STATUS_CHANGED_TO = 'Cancelled'
	OR LH.LOT_STATUS_CHANGED_TO = 'Cancelled Reserve')

ORDER BY L.TIC_LOT_ID, LH.STATUS_CHANGE_NUMBER ASC

