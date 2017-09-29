
-- UPDATE SAM CONTACTS WITH LEAD ADDRESS DATA
UPDATE CONTACT 
SET CONTACT.ADDRESS_1 = L.ADDRESS_1 ,
CONTACT.ADDRESS_2 = L.ADDRESS_2 ,
CONTACT.ADDRESS_3 = L.ADDRESS_3 ,
CONTACT.CITY = L.CITY ,
CONTACT.STATE_ = L.STATE_ 
FROM CONTACT, IP_ED..LEAD_ L
WHERE CONTACT.EMAIL = L.EMAIL
AND (L.ADDRESS_1 IS NOT NULL AND CONTACT.ADDRESS_1 IS NULL)
--AND (L.CITY IS NOT NULL AND CONTACT.CITY IS NULL)
--AND (L.STATE_ IS NOT NULL AND CONTACT.STATE_ IS NULL)

UPDATE CONTACT 
SET CONTACT.ADDRESS_1 = L.ADDRESS_1 ,
CONTACT.ADDRESS_2 = L.ADDRESS_2 ,
CONTACT.ADDRESS_3 = L.ADDRESS_3 ,
CONTACT.CITY = L.CITY ,
CONTACT.STATE_ = L.STATE_ 
FROM CONTACT, IP_ED..CONTACT L
WHERE CONTACT.EMAIL = L.EMAIL
--AND (L.ADDRESS_1 IS NOT NULL AND CONTACT.ADDRESS_1 IS NULL)
AND (L.CITY IS NOT NULL AND CONTACT.CITY IS NULL)
--AND (L.STATE_ IS NOT NULL AND CONTACT.STATE_ IS NULL)