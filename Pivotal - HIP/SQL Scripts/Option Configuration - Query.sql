-- OPTION CONFIGURATION 
SELECT U.LOGIN_NAME AS CREATED_BY, EXTERNAL_SOURCE_ID, CODE_, * FROM NBHDP_PRODUCT NP(NOLOCK)
INNER JOIN USERS U ON NP.RN_CREATE_USER = U.USERS_ID
ORDER BY NP.RN_CREATE_DATE DESC

--Lookup Information
-- Neighborhood
SELECT EXTERNAL_SOURCE_COMMUNITY_ID, * FROM NEIGHBORHOOD
-- Phases
SELECT * FROM NBHD_PHASE P
INNER JOIN NEIGHBORHOOD N ON P.NEIGHBORHOOD_ID = P.NEIGHBORHOOD_ID
WHERE N.EXTERNAL_SOURCE_COMMUNITY_ID = '10'
-- Plans
SELECT NP.CODE_, * FROM NBHDP_PRODUCT NP
INNER JOIN NEIGHBORHOOD N ON NP.NEIGHBORHOOD_ID = N.NEIGHBORHOOD_ID
WHERE N.EXTERNAL_SOURCE_COMMUNITY_ID = '10'
AND NP.TYPE = 'Plan'
-- Option Config
SELECT NP.RN_CREATE_DATE, NP.NBHDP_PRODUCT_ID, NP.CODE_, NP.TYPE, NP.EXTERNAL_SOURCE_ID,
NP.REMOVAL_DATE, NP.COST_PRICE, NP.TIC_COST, NP.MARGIN, * FROM NBHDP_PRODUCT NP
INNER JOIN NEIGHBORHOOD N ON NP.NEIGHBORHOOD_ID = N.NEIGHBORHOOD_ID
WHERE N.EXTERNAL_SOURCE_COMMUNITY_ID = '10'
--AND NP.TYPE = 'Decorator'
ORDER BY NP.RN_CREATE_DATE DESC



