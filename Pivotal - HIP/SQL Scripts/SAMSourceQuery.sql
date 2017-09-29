-- GET MOST RECENT SAM CONTACT FOR UPDATE
Select top 1 CONVERT(INT, RN_UPDATE)
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
ORDER BY CONVERT(INT, RN_UPDATE) DESC


-- SOURCE QUERY FOR SAM CONTACTS
Select CONTACT_ID AS LINKED_ID, 'SAM' AS SOURCE_SYSTEM, FIRST_NAME, LAST_NAME, TITLE, TIC_SUFFIX, 
ADDRESS_1, CITY, STATE_, ZIP, AREA_CODE, TIC_COUNTY, COUNTRY, PHONE, CELL, FAX, EMAIL, M1_UNSUBSCRIBE, 0 AS PROCESSED

from contact C1
where C1.contact_id Not in (

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

AND ISNULL(C1.TIC_INACTIVE, 0) <> 1
AND CONVERT(INT, C1.RN_UPDATE) > 478436


select * from contact where contact_id = 0x0000000000014560
--
--update contact 
--set tic_suffix = 'Jr.'
--where contact_id = 0x0000000000014560

Select CONTACT_ID AS LINKED_ID, 'SAM' AS SOURCE_SYSTEM, FIRST_NAME, LAST_NAME, TITLE, TIC_SUFFIX, ADDRESS_1, CITY, STATE_, ZIP, AREA_CODE, TIC_COUNTY, COUNTRY, PHONE, CELL, FAX, EMAIL, M1_UNSUBSCRIBE, 0 AS PROCESSED from contact C1 INNER JOIN USERS U ON C1.RN_EDIT_USER = U.USERS_ID where C1.contact_id Not in (Select distinct Contact_Id From Contact c Join TIC_sale s on c.contact_Id = s.TIC_Buyer_1_Contact_Id or c.contact_id = TIC_Buyer_2_Contact_Id Join TIC_Project p on p.tic_project_id=s.TIC_project_id where TIC_Default_Lot_Type ='Custom Lot'And Isnull(S.TIC_Deleted,0)=0 Union Select Distinct Contact_id From Contact c join TIC_Custom_lot_Compliance clc on c.contact_id=clc.tic_Owner_1_Contact_Id or c.contact_id=clc.tic_Owner_2_Contact_Id Union Select Distinct Contact_id From Contact c join TIC_lot_Owner_History loh on c.contact_id=loh.tic_Owner_1_Contact_Id or c.contact_id=loh.tic_Owner_2_Contact_Id )  AND ISNULL(C1.TIC_INACTIVE, 0) <> 1 AND (CONVERT(INT, C1.RN_UPDATE) > 478391 AND CONVERT(INT, C1.RN_UPDATE) < 478391 AND U.LOGIN_NAME <> 'amaldonado')


Select CONTACT_ID AS LINKED_ID, 'SAM' AS SOURCE_SYSTEM, FIRST_NAME, LAST_NAME, TITLE, TIC_SUFFIX, ADDRESS_1, CITY, STATE_, ZIP, AREA_CODE, TIC_COUNTY, COUNTRY, PHONE, CELL, FAX, EMAIL, M1_UNSUBSCRIBE, 0 AS PROCESSED from contact C1 INNER JOIN USERS U ON C1.RN_EDIT_USER = U.USERS_ID where C1.contact_id Not in (Select distinct Contact_Id From Contact c Join TIC_sale s on c.contact_Id = s.TIC_Buyer_1_Contact_Id or c.contact_id = TIC_Buyer_2_Contact_Id Join TIC_Project p on p.tic_project_id=s.TIC_project_id where TIC_Default_Lot_Type ='Custom Lot'And Isnull(S.TIC_Deleted,0)=0 Union Select Distinct Contact_id From Contact c join TIC_Custom_lot_Compliance clc on c.contact_id=clc.tic_Owner_1_Contact_Id or c.contact_id=clc.tic_Owner_2_Contact_Id Union Select Distinct Contact_id From Contact c join TIC_lot_Owner_History loh on c.contact_id=loh.tic_Owner_1_Contact_Id or c.contact_id=loh.tic_Owner_2_Contact_Id )  AND ISNULL(C1.TIC_INACTIVE, 0) <> 1 
AND (CONVERT(INT, C1.RN_UPDATE) > 0          AND CONVERT(INT, C1.RN_UPDATE) <  0
      AND U.LOGIN_NAME <> 'amaldonado')