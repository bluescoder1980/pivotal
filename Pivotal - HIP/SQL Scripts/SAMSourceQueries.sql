-- TIC CONTACT VILLAGE PROJECT FILTER
SELECT CONVERT(INT, VP.RN_UPDATE)
FROM TIC_CONTACT_VILLAGE_PROJECT VP 
INNER JOIN CONTACT C ON VP.TIC_CONTACT_ID = C.CONTACT_ID

where C.contact_id Not in (

--anyone hooked to sale on custom lot
Select distinct Contact_Id
From Contact c 
Join TIC_sale s on c.contact_Id=s.TIC_Buyer_1_Contact_Id or c.contact_id=TIC_Buyer_2_Contact_Id
Join TIC_Project p on p.tic_project_id=s.TIC_project_id
where TIC_Default_Lot_Type ='Custom Lot'
And Isnull(S.TIC_Deleted,0)=0

Union
--anyone hooked to current custom lot compliance
Select Distinct Contact_id
From Contact c
join TIC_Custom_lot_Compliance clc on c.contact_id=clc.tic_Owner_1_Contact_Id or c.contact_id=clc.tic_Owner_2_Contact_Id

Union
--anyone who previous owned the clc lot
Select Distinct Contact_id
From Contact c
join TIC_lot_Owner_History loh on c.contact_id=loh.tic_Owner_1_Contact_Id or c.contact_id=loh.tic_Owner_2_Contact_Id

) 

AND ISNULL(C.TIC_INACTIVE, 0) <> 1
ORDER BY CONVERT(INT, VP.RN_UPDATE) DESC


-- SOURCE QUERY FOR TIC_CONTACT_VILLAGE PROJECTS
SELECT VP.TIC_CONTACT_VILLAGE_PROJECT_ID, VP.TIC_CONTACT_ID AS LINKED_ID, V.TIC_VILLAGE_NAME, 
P.TIC_PROJECT_NAME, VP.TIC_CONTACT_TYPE, VP.TIC_OPTED_OUT,
'SAM' AS SOURCE_SYSTEM,
0 AS PROCESSED
FROM TIC_CONTACT_VILLAGE_PROJECT VP 
INNER JOIN CONTACT C ON VP.TIC_CONTACT_ID = C.CONTACT_ID
INNER JOIN TIC_CONTACT_VILLAGE CV ON VP.TIC_CONTACT_VILLAGE_ID = CV.TIC_CONTACT_VILLAGE_ID
INNER JOIN TIC_VILLAGE V ON CV.TIC_VILLAGE_ID = V.TIC_VILLAGE_ID
INNER JOIN TIC_PROJECT P ON VP.TIC_PROJECT_ID = P.TIC_PROJECT_ID
where C.contact_id Not in (

--anyone hooked to sale on custom lot
Select distinct Contact_Id
From Contact c 
Join TIC_sale s on c.contact_Id=s.TIC_Buyer_1_Contact_Id or c.contact_id=TIC_Buyer_2_Contact_Id
Join TIC_Project p on p.tic_project_id=s.TIC_project_id
where TIC_Default_Lot_Type ='Custom Lot'
And Isnull(S.TIC_Deleted,0)=0

Union
--anyone hooked to current custom lot compliance
Select Distinct Contact_id
From Contact c
join TIC_Custom_lot_Compliance clc on c.contact_id=clc.tic_Owner_1_Contact_Id or c.contact_id=clc.tic_Owner_2_Contact_Id

Union
--anyone who previous owned the clc lot
Select Distinct Contact_id
From Contact c
join TIC_lot_Owner_History loh on c.contact_id=loh.tic_Owner_1_Contact_Id or c.contact_id=loh.tic_Owner_2_Contact_Id

) 

AND ISNULL(C.TIC_INACTIVE, 0) <> 1
AND CONVERT(INT, VP.RN_UPDATE) > 546920


-- TRANSLATION QUERY FOR VILLAGE PROJECTS TO CONTACT NBHD PROFILES
SELECT NBHD.CONTACT_PROFILE_NBHD_ID, HIP_C.CONTACT_ID AS CONTACT_ID,
D.DIVISION_ID, N.NEIGHBORHOOD_ID, NBHD.TYPE, NBHD.INACTIVE
FROM TIC_OBJECTS_SAM.DBO.PRE_CONTACT_NBHD_PROFILE NBHD
INNER JOIN SAM_ED.DBO.CONTACT SAM_C ON NBHD.CONTACT_ID = SAM_C.CONTACT_ID
INNER JOIN IP_ED.DBO.CONTACT HIP_C ON 
(SAM_C.FIRST_NAME = HIP_C.FIRST_NAME AND SAM_C.LAST_NAME = HIP_C.LAST_NAME)
AND (SAM_C.EMAIL = HIP_C.EMAIL
	OR SAM_C.PHONE = HIP_C.PHONE
	OR SAM_C.CELL = HIP_C.CELL
	OR SAM_C.ADDRESS_1 = HIP_C.ADDRESS_1)
INNER JOIN IP_ED.DBO.DIVISION D ON NBHD.DIVISION_LOOKUP = D.NAME
INNER JOIN IP_ED.DBO.NEIGHBORHOOD N ON NBHD.NEIGHBORHOOD_LOOKUP = N.NAME
WHERE ISNULL(NBHD.PROCESSED, 0) = 0
AND ISNULL(SAM_C.TIC_INACTIVE, 0) <> 1
AND NBHD.SOURCE_SYSTEM = 'SAM'
AND SAM_C.contact_id Not in (
--anyone hooked to sale on custom lot
Select distinct Contact_Id
From Contact c 
Join TIC_sale s on c.contact_Id=s.TIC_Buyer_1_Contact_Id or c.contact_id=TIC_Buyer_2_Contact_Id
Join TIC_Project p on p.tic_project_id=s.TIC_project_id
where TIC_Default_Lot_Type ='Custom Lot'
And Isnull(S.TIC_Deleted,0)=0

Union
--anyone hooked to current custom lot compliance
Select Distinct Contact_id
From Contact c
join TIC_Custom_lot_Compliance clc on c.contact_id=clc.tic_Owner_1_Contact_Id or c.contact_id=clc.tic_Owner_2_Contact_Id

Union
--anyone who previous owned the clc lot
Select Distinct Contact_id
From Contact c
join TIC_lot_Owner_History loh on c.contact_id=loh.tic_Owner_1_Contact_Id or c.contact_id=loh.tic_Owner_2_Contact_Id

) 




-- TRANSLATION QUERY FOR VILLAGE PROJECTS FOR LEAD
SELECT NBHD.CONTACT_PROFILE_NBHD_ID, HIP_L.LEAD__ID AS LEAD_ID,
D.DIVISION_ID, N.NEIGHBORHOOD_ID, NBHD.TYPE, NBHD.INACTIVE
FROM TIC_OBJECTS_SAM.DBO.PRE_CONTACT_NBHD_PROFILE NBHD
INNER JOIN SAM_ED.DBO.CONTACT SAM_C ON NBHD.CONTACT_ID = SAM_C.CONTACT_ID
INNER JOIN IP_ED.DBO.LEAD_ HIP_L ON 
(SAM_C.FIRST_NAME = HIP_L.FIRST_NAME AND SAM_C.LAST_NAME = HIP_L.LAST_NAME)
AND (SAM_C.EMAIL = HIP_L.EMAIL
	OR SAM_C.PHONE = HIP_L.PHONE
	OR SAM_C.CELL = HIP_L.CELL
	OR SAM_C.ADDRESS_1 = HIP_L.ADDRESS_1)
INNER JOIN IP_ED.DBO.DIVISION D ON NBHD.DIVISION_LOOKUP = D.NAME
INNER JOIN IP_ED.DBO.NEIGHBORHOOD N ON NBHD.NEIGHBORHOOD_LOOKUP = N.NAME
WHERE ISNULL(NBHD.PROCESSED, 0) = 0
AND ISNULL(SAM_C.TIC_INACTIVE, 0) <> 1
AND NBHD.SOURCE_SYSTEM = 'SAM'
AND SAM_C.contact_id Not in (
--anyone hooked to sale on custom lot
Select distinct Contact_Id
From Contact c 
Join TIC_sale s on c.contact_Id=s.TIC_Buyer_1_Contact_Id or c.contact_id=TIC_Buyer_2_Contact_Id
Join TIC_Project p on p.tic_project_id=s.TIC_project_id
where TIC_Default_Lot_Type ='Custom Lot'
And Isnull(S.TIC_Deleted,0)=0

Union
--anyone hooked to current custom lot compliance
Select Distinct Contact_id
From Contact c
join TIC_Custom_lot_Compliance clc on c.contact_id=clc.tic_Owner_1_Contact_Id or c.contact_id=clc.tic_Owner_2_Contact_Id

Union
--anyone who previous owned the clc lot
Select Distinct Contact_id
From Contact c
join TIC_lot_Owner_History loh on c.contact_id=loh.tic_Owner_1_Contact_Id or c.contact_id=loh.tic_Owner_2_Contact_Id

) 





SELECT VP.TIC_CONTACT_VILLAGE_PROJECT_ID, VP.TIC_CONTACT_ID AS LINKED_ID, CONVERT(VARCHAR(40), V.TIC_VILLAGE_NAME) AS TIC_VILLAGE_NAME, 
CONVERT(VARCHAR(40), P.TIC_PROJECT_NAME) AS TIC_PROJECT_NAME, VP.TIC_CONTACT_TYPE, VP.TIC_OPTED_OUT,
'SAM' AS SOURCE_SYSTEM,
0 AS PROCESSED
FROM TIC_CONTACT_VILLAGE_PROJECT VP 
INNER JOIN CONTACT C ON VP.TIC_CONTACT_ID = C.CONTACT_ID
INNER JOIN TIC_CONTACT_VILLAGE CV ON VP.TIC_CONTACT_VILLAGE_ID = CV.TIC_CONTACT_VILLAGE_ID
INNER JOIN TIC_VILLAGE V ON CV.TIC_VILLAGE_ID = V.TIC_VILLAGE_ID
INNER JOIN TIC_PROJECT P ON VP.TIC_PROJECT_ID = P.TIC_PROJECT_ID
INNER JOIN USERS U ON VP.RN_EDIT_USER = U.USERS_ID
where C.contact_id Not in (

Select distinct Contact_Id
From Contact c 
Join TIC_sale s on c.contact_Id=s.TIC_Buyer_1_Contact_Id or c.contact_id=TIC_Buyer_2_Contact_Id
Join TIC_Project p on p.tic_project_id=s.TIC_project_id
where TIC_Default_Lot_Type ='Custom Lot'
And Isnull(S.TIC_Deleted,0)=0

Union

Select Distinct Contact_id
From Contact c
join TIC_Custom_lot_Compliance clc on c.contact_id=clc.tic_Owner_1_Contact_Id or c.contact_id=clc.tic_Owner_2_Contact_Id

Union

Select Distinct Contact_id
From Contact c
join TIC_lot_Owner_History loh on c.contact_id=loh.tic_Owner_1_Contact_Id or c.contact_id=loh.tic_Owner_2_Contact_Id

) 

AND ISNULL(C.TIC_INACTIVE, 0) <> 1
AND CONVERT(INT, VP.RN_UPDATE) > ? AND CONVERT(INT, VP.RN_UPDATE) < ?
AND U.LOGIN_NAME <> 'amaldonado'







