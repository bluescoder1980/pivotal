-- GET ALL CONSTRUCTION PROJECTS
SELECT EXTERNAL_SOURCE_COMMUNITY_ID, TIC_CONSTRUCTION_PROJECT_ID, * FROM TIC_CONSTRUCTION_PROJECT

-- UPDATE ALL PHASES WITH CONSTRUCTION PROJECT
UPDATE NBHD_PHASE
SET TIC_CONSTRUCTION_PROJECT_ID = 0x000000000000000C
WHERE NEIGHBORHOOD_ID = 0x0000000000000012

-- UPDATE ALL NEIGHBORHOOD_PRODUCTS WITH CONSTRUCTION PROJECT
UPDATE NBHDP_PRODUCT
SET TIC_CONSTRUCTION_PROJECT_ID = 0x000000000000000C
WHERE NEIGHBORHOOD_ID = 0x0000000000000012

-- UPDATE ALL LOTS WITH CONSTRUCTION PROJECTS
UPDATE PRODUCT
SET TIC_CONSTRUCTION_PROJECT_ID = 0x000000000000000C
WHERE NEIGHBORHOOD_ID = 0x0000000000000012

-- Run SSIS Recalculate Formula Processes

-- VALIDATE NBHD_PRODUCT FORMULAS
SELECT np.rn_descriptor, np.rn_create_date, np.release_wildcard, np.wc_neighborhood_id, np.tic_wc_construction_project_id, np.neighborhood_id, np.tic_construction_project_id, np.nbhd_phase_id, pl.nbhdp_product_id, np.* FROM nbhdp_product np 
inner join nbhdp_product pl on np.plan_id = pl.nbhdp_product_id
WHERE np.TYPE = 'Decorator' and np.tic_wc_construction_project_id is null

-- VALIDATE OPPORTUNITY FORMULAS

select count(*) from opportunity
where tic_construction_project_id is not null

select * from product where nbhd_phase_id = (select top 1 nbhd_phase_id from nbhd_phase where phase = '1')