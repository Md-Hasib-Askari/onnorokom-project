-- ============================================================================
-- Seed step 02: the admin account.
-- ============================================================================
-- Seeds one approved admin (admin@onnorokom.com / Admin@123) only when no
-- admin exists at all, so a deployment that created its own first admin keeps
-- it. The row is inserted with the email's unique partial index in mind, but
-- the guard is the admin-role check, matching the original DbInitializer
-- semantics.
--
-- Fixed IDs: 00000000-0000-0000-0000-000000000001
--
-- Password hashes are static BCrypt hashes (work factor 12) generated with the
-- same BCrypt.Net call the PasswordHasher uses; they are demo credentials with
-- deliberately public passwords, not secrets.
-- ============================================================================

INSERT INTO "AuthUsers"
    ("Id", "FullName", "Email", "PasswordHash", "Role", "Status", "IsActive",
     "MustChangePassword", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
SELECT
    '00000000-0000-0000-0000-000000000001'::uuid,
    'System Administrator',
    'admin@onnorokom.com',
    '$2a$12$z55neNyfKsSEGQk8wv.aiem3CgOmmph6CVHtQmNj.1v4i41BdMYRi',
    'Admin',
    'Approved',
    true,
    false,
    now() - interval '180 days',
    'seed',
    now() - interval '180 days',
    'seed',
    false
WHERE NOT EXISTS (SELECT 1 FROM "AuthUsers" WHERE "Role" = 'Admin' AND NOT "IsDeleted");

INSERT INTO "AdminProfiles"
    ("Id", "AuthUserId", "Position", "PhoneNumber",
     "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
SELECT
    '00000000-0000-0000-0000-000000008001'::uuid,
    '00000000-0000-0000-0000-000000000001'::uuid,
    'System Administrator',
    NULL,
    now() - interval '180 days',
    'seed',
    now() - interval '180 days',
    'seed',
    false
WHERE EXISTS (SELECT 1 FROM "AuthUsers" WHERE "Id" = '00000000-0000-0000-0000-000000000001')
  AND NOT EXISTS (SELECT 1 FROM "AdminProfiles" WHERE "AuthUserId" = '00000000-0000-0000-0000-000000000001');
