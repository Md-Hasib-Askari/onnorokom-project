-- ============================================================================
-- Seed step 01: application-wide system settings.
-- ============================================================================
-- One row per SystemSettingKey, so every policy value the code reads has a
-- row on a fresh database. Idempotent: the unique partial index on Key makes
-- ON CONFLICT DO NOTHING safe to re-run at any time, and existing rows are
-- never overwritten.
--
-- Fixed IDs: 00000000-0000-0000-0000-000000007001 .. 0000000007004
-- ============================================================================

INSERT INTO "SystemSettings"
    ("Id", "Key", "Value", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
VALUES
    ('00000000-0000-0000-0000-000000007001', 'TeacherSelfRegistrationEnabled', 'true',  now(), 'seed', now(), 'seed', false),
    ('00000000-0000-0000-0000-000000007002', 'StudentSelfRegistrationEnabled', 'false', now(), 'seed', now(), 'seed', false),
    ('00000000-0000-0000-0000-000000007003', 'TeacherProfileSelfEditEnabled',  'true',  now(), 'seed', now(), 'seed', false),
    ('00000000-0000-0000-0000-000000007004', 'StudentProfileSelfEditEnabled',  'true',  now(), 'seed', now(), 'seed', false)
ON CONFLICT ("Key") WHERE "IsDeleted" = false DO NOTHING;
