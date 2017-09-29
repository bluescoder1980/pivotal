/* 
=========================================================================================
THE FOLLOWING QUERIES SHOULD BE USED TO SEED THE TIC_RN_LAST_SYNC IN PRODUCTION FOR 
HIP CONTACTS AND CONTACT_NEIGHBORHOOD_PROFILES
========================================================================================= 
*/

SELECT TOP 1 CONVERT(INT, RN_UPDATE), * FROM CONTACT 
WHERE ISNULL(TIC_INACTIVE, 0) <> 1
ORDER BY CONVERT(INT, RN_UPDATE) DESC

-- GET HIP LAST RN_UPDATE FOR LEAD_
SELECT TOP 1 CONVERT(INT, RN_UPDATE), * FROM LEAD_ 
WHERE ISNULL(TIC_INACTIVE, 0) <> 1
ORDER BY CONVERT(INT, RN_UPDATE) DESC

-- GET LAST RN_UPDATE FOR CONTACT_NBHD_PROFILE
SELECT TOP 1 * FROM
(SELECT CONVERT(INT, CPN.RN_UPDATE) AS RN_UPDATE
FROM CONTACT_PROFILE_NEIGHBORHOOD CPN
INNER JOIN CONTACT C ON CPN.CONTACT_ID = C.CONTACT_ID
WHERE ISNULL(C.TIC_INACTIVE, 0) <> 1

UNION

SELECT CONVERT(INT, CPN.RN_UPDATE) AS RN_UPDATE
FROM CONTACT_PROFILE_NEIGHBORHOOD CPN
INNER JOIN LEAD_ L ON CPN.LEAD_ID = L.LEAD__ID
WHERE ISNULL(L.TIC_INACTIVE, 0) <> 1) AS INLINEVIEW
ORDER BY CONVERT(INT, RN_UPDATE) DESC

/* 
=========================================================================================
THE FOLLOWING QUERIES SHOULD BE USED TO SEED THE TIC_RN_LAST_SYNC IN PRODUCTION FOR 
SAM CONTACTS AND CONTACT_VILLAGE_PROJECTS
========================================================================================= 
*/

Select top 1 CONVERT(INT, RN_UPDATE) as LAST_RN_UPDATE_CONTACT_SAM
from contact
where contact_id Not in (

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
AND ISNULL(TIC_INACTIVE, 0) <> 1
AND EMAIL NOT LIKE 'Unique%' 
ORDER BY CONVERT(INT, RN_UPDATE) DESC

-- Contact Village Project
SELECT TOP 1 CONVERT(INT, VP.RN_UPDATE) AS VP_LAST_UPDATE_ID
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
AND C.EMAIL NOT LIKE 'Unique%'
ORDER BY VP.RN_UPDATE DESC


