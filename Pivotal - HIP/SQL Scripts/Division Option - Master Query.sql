SELECT DP.PRODUCT_NAME, CT.CONFIGURATION_TYPE_NAME AS CATEGORY, SC.NAME AS SUB_CATEGORY, 
DP.CODE_, DP.TYPE, DP.DIVISION_ID, 
R.EXTERNAL_SOURCE_ID AS REGION, DP.STYLE_NUMBER, DP.MANUFACTURER, DP.UNITS_OF_MEASURE, DP.TIC_MODEL,
DP.REMOVAL_DATE, DP.DESCRIPTION, U.LOGIN_NAME, DP.EXTERNAL_SOURCE_ID, DP.RN_CREATE_DATE FROM DIVISION_PRODUCT DP
INNER JOIN USERS U ON DP.RN_CREATE_USER = U.USERS_ID
INNER JOIN CONFIGURATION_TYPE CT ON DP.CATEGORY_ID = CT.CONFIGURATION_TYPE_ID
INNER JOIN REGION R ON DP.REGION_ID = R.REGION_ID
INNER JOIN SUB_CATEGORY SC ON DP.SUB_CATEGORY_ID = SC.SUB_CATEGORY_ID
WHERE DP.RN_CREATE_DATE > '2010-08-17 00:00:00'
ORDER BY DP.RN_CREATE_DATE DESC

SELECT * FROM CONFIGURATION_TYPE
SELECT * FROM SUB_CATEGORY

delete from division_product where rn_create_date > '2010-08-18 16:08:00'
delete from configuration_type where rn_create_date > '2010-08-18 16:08:00'
delete from SUB_CATEGORY where rn_create_date > '2010-08-18 16:08:00'
