SELECT * FROM 
(SELECT 1 Product_Available, 
NBHDP_Product.Type, 
NBHDP_Product.Product_Name, 
Division_Product.Category_Id, 
Division_Product.Sub_Category_Id, 
NBHDP_Product.Code_, 
NBHDP_Product.Current_Price Price, 
NBHDP_Product.Location_Id, 
NBHDP_Product.Manufacturer, 
NULL Opportunity__Product_Id, 
NBHDP_Product.NBHDP_Product_Id, 
NBHDP_Product.Division_Product_Id, 
NBHDP_Product.Construction_Stage_Ordinal,
NBHDP_Product.WC_Level_With_Plan, 
NBHDP_Product.Option_Available_To, 
NULL Quantity, 
Division_Product.Construction_Stage_Id, 
Division_Product.Required_Deposit_Amount, 
0 Selected, 0 Use_PCO_Price, 
NBHDP_Product.Post_CuttOff_Price, 
NBHDP_Product.Inactive 
FROM NBHDP_Product 
LEFT JOIN Division_Product Division_Product ON NBHDP_Product.Division_Product_Id = Division_Product.Division_Product_Id 
WHERE 
	(NBHDP_Product.Inactive = 0 OR NBHDP_Product.Inactive IS NULL) 
	AND (NBHDP_PRODUCT.TYPE <> 'Decorator') 
	AND 
		(  NBHDP_Product.WC_Corporate = 1  
			OR NBHDP_Product.WC_Region_Id = 0x0000000000000001 
			OR NBHDP_Product.WC_Division_Id = 0x0000000000000001 
			--OR NBHDP_Product.WC_Neighborhood_Id = 0x0000000000000012 
			OR NBHDP_Product.TIC_WC_Construction_Project_Id = 0x0000000000000002
			OR NBHDP_Product.NBHD_Phase_Id = 0x000000000000001E 
		)  

	AND (
			NBHDP_Product.Type = 'Global' 
			OR (NBHDP_Product.Plan_Code 
					= (SELECT Plan_Code FROM NBHDP_Product Plan_Table 
						WHERE Plan_Table.NBHDP_Product_Id = 0x0000000000000012) 
			AND NBHDP_Product.Plan_Id IS NULL) 
			OR (NBHDP_Product.Plan_Code IS NULL AND NBHDP_Product.Plan_Id = 0x0000000000000012) 
			OR (NBHDP_Product.Plan_Code IS NULL AND NBHDP_Product.Plan_Id IS NULL) 
		)  
	
	AND (
			NBHDP_Product.Location_Id IS NULL 
			OR (
					NBHDP_Product.Location_Id is not null AND NBHDP_Product.Location_Id in 
					(SELECT Division_Product_Locations.Location_Id FROM Division_Product_Locations 
						WHERE Division_Product_Locations.Division_Product_Id = 0x0000000000000013 AND 
						(  Division_Product_Locations.Division_Id IS NULL 
							OR  Division_Product_Locations.Division_Id = 0x0000000000000001 )  
							AND (Division_Product_Locations.Inactive = 0 
							OR Division_Product_Locations.Inactive IS NULL) 
						) 
				)
		) 

	AND NOT (NBHDP_Product.Type = 'Plan') 
	AND (NBHDP_Product.Available_Date <= GETDATE() OR NBHDP_Product.Available_Date IS NULL)
	AND (NBHDP_Product.Removal_Date >= GETDATE() OR NBHDP_Product.Removal_Date IS NULL) 
	AND NBHDP_Product.NBHDP_Product_Id NOT IN 
		(SELECT Opportunity__Product.NBHDP_Product_Id 
		 FROM Opportunity__Product WHERE Opportunity__Product.Opportunity_Id = 0x00000000000002AB 
			AND NOT Opportunity__Product.NBHDP_Product_Id IS NULL ) 
	AND (
			(
				NBHDP_Product.Type <> 'Elevation' 
				AND ( 
						(-1 <= ISNULL(NBHDP_Product.Construction_Stage_Ordinal, 1000000000)  
						OR  (-1 > ISNULL(NBHDP_Product.Construction_Stage_Ordinal, 1000000000)  
						AND NBHDP_Product.Post_CuttOff_Price IS NOT NULL ) 
					) 
			 ) 
		)

	AND 
		(
			(
				NBHDP_Product.Division_Product_Id NOT IN 
					(SELECT Product_Option_Rule.Child_Product_Id FROM Product_Option_Rule
					 INNER JOIN Opportunity__Product OP ON OP.Division_Product_Id = Product_Option_Rule.Parent_Product_Id 
					 WHERE Opportunity_Id = 0x00000000000002AB AND Product_Option_Rule.Exclude = 1 
						AND (Product_Option_Rule.Inactive = 0 OR Product_Option_Rule.Inactive IS NULL) 
						AND OP.Selected = 1 
						AND OP.Opportunity_Id = 0x00000000000002AB 
						AND NOT OP.NBHDP_Product_Id IS NULL  
						AND (Product_Option_Rule.Plan_Product_Id = 0x0000000000000013 
								OR (  Product_Option_Rule.Plan_Product_Id IS NULL AND Product_Option_Rule.Child_Product_Id NOT  
										IN (SELECT Product_Option_Rule.Child_Product_Id FROM Product_Option_Rule
											 INNER JOIN Opportunity__Product OP ON OP.Division_Product_Id 
												= Product_Option_Rule.Parent_Product_Id WHERE Opportunity_Id = 0x00000000000002AB 
													AND (Product_Option_Rule.Inactive = 0 OR Product_Option_Rule.Inactive IS NULL)
													 AND OP.Selected = 1 AND Product_Option_Rule.Plan_Product_Id = 0x0000000000000013 
											) 
									) 
							)
			) 

			AND ( 
				NBHDP_Product.Type <> 'Elevation' 
				AND  NBHDP_Product.Division_Product_Id NOT IN 
				( SELECT Opportunity__Product.Division_Product_Id FROM Opportunity__Product
				  WHERE Opportunity__Product.Opportunity_Id = 0x00000000000002AB 
					AND Opportunity__Product.selected = 1 
					AND NOT Opportunity__Product.NBHDP_Product_Id IS NULL
				)
			)

		) 

	AND  
		( 
			-1 <= ISNULL(NBHDP_Product.Construction_Stage_Ordinal, 1000000000)   
			OR ( -1 > ISNULL(NBHDP_Product.Construction_Stage_Ordinal, 1000000000)  
			AND NBHDP_Product.Post_CuttOff_Price IS NOT NULL 
		) 
	) ) ) 


UNION 

SELECT 
1 Product_Available, 
OP0.Type, 
OP0.Product_Name, 
Division_Product.Category_Id, 
Division_Product.Sub_Category_Id, 
OP0.Code_, OP0.Price, 
OP0.Location_Id, 
OP0.Manufacturer, 
OP0.Opportunity__Product_Id, 
OP0.NBHDP_Product_Id, 
OP0.Division_Product_Id, 
OP0.Construction_Stage_Ordinal, 
NBHDP_Product.WC_Level_With_Plan, 
NBHDP_Product.Option_Available_To, 
NULL Quantity, 
Division_Product.Construction_Stage_Id, 
Division_Product.Required_Deposit_Amount, 
0 Selected, 
0 Use_PCO_Price, 
NBHDP_Product.Post_CuttOff_Price, 
NBHDP_Product.Inactive 
FROM Opportunity__Product OP0 
LEFT OUTER JOIN Division_Product Division_Product ON OP0.Division_Product_Id = Division_Product.Division_Product_Id 
LEFT OUTER JOIN Construction_Stage ON OP0.Construction_Stage_Id = Construction_Stage.Construction_Stage_Id 
LEFT OUTER JOIN NBHDP_Product NBHDP_Product ON OP0.NBHDP_Product_Id = NBHDP_Product.NBHDP_Product_Id 
WHERE Opportunity_Id = 0x00000000000002AB 
AND OP0.Selected = 0 
AND (NBHDP_Product.Inactive = 0 OR NBHDP_Product.Inactive IS NULL) 
AND ( 
		(OP0.Division_Product_Id IS NOT NULL AND OP0.NBHDP_Product_Id IS NOT NULL)  
		OR (OP0.Division_Product_Id IS NULL AND OP0.NBHDP_Product_Id IS NULL) 
	) 
AND ( 
		( OP0.NBHDP_Product_Id is NULL AND OP0.Division_Product_Id is NULL)  
		OR (  OP0.Type <> 'Elevation'
				AND  ((OP0.Division_Product_Id NOT IN 
						(SELECT Product_Option_Rule.Child_Product_Id FROM Product_Option_Rule 
							INNER JOIN Opportunity__Product OP ON OP.Division_Product_Id = Product_Option_Rule.Parent_Product_Id 
							WHERE Opportunity_Id = 0x00000000000002AB AND Product_Option_Rule.Exclude = 1 
							AND (Product_Option_Rule.Inactive = 0 OR Product_Option_Rule.Inactive IS NULL) 
							AND OP.Selected = 1 
							AND OP.Opportunity_Id = 0x00000000000002AB 
							AND NOT OP.NBHDP_Product_Id IS NULL  
							AND (Product_Option_Rule.Plan_Product_Id = 0x0000000000000013 
							OR (  Product_Option_Rule.Plan_Product_Id IS NULL AND Product_Option_Rule.Child_Product_Id NOT  IN 
								(SELECT Product_Option_Rule.Child_Product_Id FROM Product_Option_Rule INNER JOIN Opportunity__Product OP 
									ON OP.Division_Product_Id = Product_Option_Rule.Parent_Product_Id 
									WHERE Opportunity_Id = 0x00000000000002AB 
									AND (Product_Option_Rule.Inactive = 0 OR Product_Option_Rule.Inactive IS NULL) 
									AND OP.Selected = 1 AND Product_Option_Rule.Plan_Product_Id = 0x0000000000000013 ) 
								) 
							) 
			) 

AND (OP0.Division_Product_Id NOT IN (SELECT OP2.Division_Product_Id FROM Opportunity__Product OP2 WHERE OP2.Opportunity_Id = 0x00000000000002AB AND OP2.selected = 1 AND NOT OP2.NBHDP_Product_Id IS NULL ) ) ) )  AND ( ( -1 <= ISNULL(Construction_Stage.Construction_Stage_Ordinal, 1000000000) OR ( -1 > ISNULL(NBHDP_Product.Construction_Stage_Ordinal, 1000000000)  AND NBHDP_Product.Post_CuttOff_Price IS NOT NULL ) ) )) )) AS t ORDER BY Division_Product_Id, WC_Level_With_Plan ASC

