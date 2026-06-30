-- 房源种子数据：3 家公司 × 2 栋楼 × 3 层 × 2 户 = 36 条
DECLARE @Sys uniqueidentifier = '00000000-0000-0000-0000-000000000000';
DECLARE @Now datetime2 = GETDATE();

-- 清除旧数据
DELETE FROM HousingUnits;

-- ====== 上海茂源 GS001 ======
INSERT INTO HousingUnits (Id, BuildingName, BuildingCode, BuildingAddress, CompanyId, FloorName, FloorSortOrder, UnitNo, FullCode, Status, RoomTypeId, CreatedBy, CreatedAt)
SELECT NEWID(), N'A栋', N'A', N'上海市浦东新区陆家嘴金融中心A座', 'A1111111-1111-1111-1111-111111111001', N'1层', 1, N'101', N'A栋-1层-101', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'A栋', N'A', N'上海市浦东新区陆家嘴金融中心A座', 'A1111111-1111-1111-1111-111111111001', N'1层', 1, N'102', N'A栋-1层-102', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'A栋', N'A', N'上海市浦东新区陆家嘴金融中心A座', 'A1111111-1111-1111-1111-111111111001', N'2层', 2, N'201', N'A栋-2层-201', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'A栋', N'A', N'上海市浦东新区陆家嘴金融中心A座', 'A1111111-1111-1111-1111-111111111001', N'2层', 2, N'202', N'A栋-2层-202', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'A栋', N'A', N'上海市浦东新区陆家嘴金融中心A座', 'A1111111-1111-1111-1111-111111111001', N'3层', 3, N'301', N'A栋-3层-301', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'A栋', N'A', N'上海市浦东新区陆家嘴金融中心A座', 'A1111111-1111-1111-1111-111111111001', N'3层', 3, N'302', N'A栋-3层-302', 'Vacant', NULL, @Sys, @Now;
INSERT INTO HousingUnits (Id, BuildingName, BuildingCode, BuildingAddress, CompanyId, FloorName, FloorSortOrder, UnitNo, FullCode, Status, RoomTypeId, CreatedBy, CreatedAt)
SELECT NEWID(), N'B栋', N'B', N'上海市浦东新区陆家嘴金融中心B座', 'A1111111-1111-1111-1111-111111111001', N'1层', 1, N'101', N'B栋-1层-101', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'B栋', N'B', N'上海市浦东新区陆家嘴金融中心B座', 'A1111111-1111-1111-1111-111111111001', N'1层', 1, N'102', N'B栋-1层-102', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'B栋', N'B', N'上海市浦东新区陆家嘴金融中心B座', 'A1111111-1111-1111-1111-111111111001', N'2层', 2, N'201', N'B栋-2层-201', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'B栋', N'B', N'上海市浦东新区陆家嘴金融中心B座', 'A1111111-1111-1111-1111-111111111001', N'2层', 2, N'202', N'B栋-2层-202', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'B栋', N'B', N'上海市浦东新区陆家嘴金融中心B座', 'A1111111-1111-1111-1111-111111111001', N'3层', 3, N'301', N'B栋-3层-301', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'B栋', N'B', N'上海市浦东新区陆家嘴金融中心B座', 'A1111111-1111-1111-1111-111111111001', N'3层', 3, N'302', N'B栋-3层-302', 'Vacant', NULL, @Sys, @Now;

-- ====== 南京恒达 GS002 ======
INSERT INTO HousingUnits (Id, BuildingName, BuildingCode, BuildingAddress, CompanyId, FloorName, FloorSortOrder, UnitNo, FullCode, Status, RoomTypeId, CreatedBy, CreatedAt)
SELECT NEWID(), N'C栋', N'C', N'南京市鼓楼区新街口广场C座', 'A1111111-1111-1111-1111-111111111002', N'1层', 1, N'101', N'C栋-1层-101', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'C栋', N'C', N'南京市鼓楼区新街口广场C座', 'A1111111-1111-1111-1111-111111111002', N'1层', 1, N'102', N'C栋-1层-102', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'C栋', N'C', N'南京市鼓楼区新街口广场C座', 'A1111111-1111-1111-1111-111111111002', N'2层', 2, N'201', N'C栋-2层-201', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'C栋', N'C', N'南京市鼓楼区新街口广场C座', 'A1111111-1111-1111-1111-111111111002', N'2层', 2, N'202', N'C栋-2层-202', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'C栋', N'C', N'南京市鼓楼区新街口广场C座', 'A1111111-1111-1111-1111-111111111002', N'3层', 3, N'301', N'C栋-3层-301', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'C栋', N'C', N'南京市鼓楼区新街口广场C座', 'A1111111-1111-1111-1111-111111111002', N'3层', 3, N'302', N'C栋-3层-302', 'Vacant', NULL, @Sys, @Now;
INSERT INTO HousingUnits (Id, BuildingName, BuildingCode, BuildingAddress, CompanyId, FloorName, FloorSortOrder, UnitNo, FullCode, Status, RoomTypeId, CreatedBy, CreatedAt)
SELECT NEWID(), N'D栋', N'D', N'南京市鼓楼区新街口广场D座', 'A1111111-1111-1111-1111-111111111002', N'1层', 1, N'101', N'D栋-1层-101', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'D栋', N'D', N'南京市鼓楼区新街口广场D座', 'A1111111-1111-1111-1111-111111111002', N'1层', 1, N'102', N'D栋-1层-102', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'D栋', N'D', N'南京市鼓楼区新街口广场D座', 'A1111111-1111-1111-1111-111111111002', N'2层', 2, N'201', N'D栋-2层-201', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'D栋', N'D', N'南京市鼓楼区新街口广场D座', 'A1111111-1111-1111-1111-111111111002', N'2层', 2, N'202', N'D栋-2层-202', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'D栋', N'D', N'南京市鼓楼区新街口广场D座', 'A1111111-1111-1111-1111-111111111002', N'3层', 3, N'301', N'D栋-3层-301', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'D栋', N'D', N'南京市鼓楼区新街口广场D座', 'A1111111-1111-1111-1111-111111111002', N'3层', 3, N'302', N'D栋-3层-302', 'Vacant', NULL, @Sys, @Now;

-- ====== 深圳万方 GS003 ======
INSERT INTO HousingUnits (Id, BuildingName, BuildingCode, BuildingAddress, CompanyId, FloorName, FloorSortOrder, UnitNo, FullCode, Status, RoomTypeId, CreatedBy, CreatedAt)
SELECT NEWID(), N'E栋', N'E', N'深圳市南山区科技园E栋', 'A1111111-1111-1111-1111-111111111003', N'1层', 1, N'101', N'E栋-1层-101', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'E栋', N'E', N'深圳市南山区科技园E栋', 'A1111111-1111-1111-1111-111111111003', N'1层', 1, N'102', N'E栋-1层-102', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'E栋', N'E', N'深圳市南山区科技园E栋', 'A1111111-1111-1111-1111-111111111003', N'2层', 2, N'201', N'E栋-2层-201', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'E栋', N'E', N'深圳市南山区科技园E栋', 'A1111111-1111-1111-1111-111111111003', N'2层', 2, N'202', N'E栋-2层-202', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'E栋', N'E', N'深圳市南山区科技园E栋', 'A1111111-1111-1111-1111-111111111003', N'3层', 3, N'301', N'E栋-3层-301', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'E栋', N'E', N'深圳市南山区科技园E栋', 'A1111111-1111-1111-1111-111111111003', N'3层', 3, N'302', N'E栋-3层-302', 'Vacant', NULL, @Sys, @Now;
INSERT INTO HousingUnits (Id, BuildingName, BuildingCode, BuildingAddress, CompanyId, FloorName, FloorSortOrder, UnitNo, FullCode, Status, RoomTypeId, CreatedBy, CreatedAt)
SELECT NEWID(), N'F栋', N'F', N'深圳市南山区科技园F栋', 'A1111111-1111-1111-1111-111111111003', N'1层', 1, N'101', N'F栋-1层-101', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'F栋', N'F', N'深圳市南山区科技园F栋', 'A1111111-1111-1111-1111-111111111003', N'1层', 1, N'102', N'F栋-1层-102', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'F栋', N'F', N'深圳市南山区科技园F栋', 'A1111111-1111-1111-1111-111111111003', N'2层', 2, N'201', N'F栋-2层-201', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'F栋', N'F', N'深圳市南山区科技园F栋', 'A1111111-1111-1111-1111-111111111003', N'2层', 2, N'202', N'F栋-2层-202', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'F栋', N'F', N'深圳市南山区科技园F栋', 'A1111111-1111-1111-1111-111111111003', N'3层', 3, N'301', N'F栋-3层-301', 'Vacant', NULL, @Sys, @Now UNION ALL
SELECT NEWID(), N'F栋', N'F', N'深圳市南山区科技园F栋', 'A1111111-1111-1111-1111-111111111003', N'3层', 3, N'302', N'F栋-3层-302', 'Vacant', NULL, @Sys, @Now;

SELECT COUNT(*) AS TotalHousingUnits FROM HousingUnits;
GO