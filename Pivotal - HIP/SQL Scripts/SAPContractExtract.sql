SELECT PRICE, * FROM PRODUCT WHERE RN_EDIT_DATE > '2010-09-29 00:00:00'

SELECT P.LOT_NUMBER, P.UNIT, P.TRACT, O. Additional_Price,
O.Concessions, O.Lot_Premium, O.Elevation_Premium, O.TIC_Design_Options_Total,
O.TIC_Structural_Options_Total FROM OPPORTUNITY O
INNER JOIN PRODUCT P ON O.LOT_ID = P.PRODUCT_ID
WHERE O.EXTERNAL_SOURCE_SYNC_STATUS = 'Pending Sync'

Product.Tract
•	Lot – Product.Lot_Number
•	Unit – Product.Unit
•	Base Price - Additional_Price
•	Adjustment Total - Concessions
•	Lot Premium – Lot_Premium
•	Elevation Premium – Elevation_Premium
•	Design Option Total Price - TIC_Design_Options_Total
•	Design Option Total Cost – We are not storing this, if we need to we can but there is not a field to accommodate this at this time.
•	Structural Option Total Price - TIC_Structural_Options_Total
