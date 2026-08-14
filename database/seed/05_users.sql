-- ============================================================================
-- Seed step 05: demo users, bulk teachers and students, and all profiles.
-- ============================================================================
-- Every account is seeded independently and only when its email is free, so
-- deleting one demo account never blocks the others and never collides with an
-- account an admin created by hand. Bulk accounts use namespaced emails
-- (seed.teacher.N@ / seed.student.N@) which double as the idempotency guard.
--
-- Volume (chosen so the admin user list pages several times at the default
-- page size of 20):
--   1 admin (step 02), 1 demo teacher, 1 demo student,
--   9 bulk teachers (seed.teacher.1..9), 129 bulk students (seed.student.1..129),
--   1 pending teacher and 1 pending student (approval-flow demo, no profile).
--   -> 143 accounts, ~7 pages for the admin.
--
-- Student distribution (Section A of Grade 1 holds 30 students including the
-- demo student, which is what later submission volumes build on):
--   seed.student.1..29      -> Grade 1 Section A  (2001)
--   seed.student.30..39     -> Grade 1 Section B  (2002)
--   seed.student.40..49     -> Grade 1 Section C  (2003)
--   seed.student.50..59     -> Grade 2 Section A  (2004)
--   seed.student.60..69     -> Grade 2 Section B  (2005)
--   seed.student.70..79     -> Grade 2 Section C  (2006)
--   seed.student.80..89     -> Grade 3 Section A  (2007)
--   seed.student.90..99     -> Grade 3 Section B  (2008)
--   seed.student.100..109   -> Grade 4 Section A  (2010)
--   seed.student.110..119   -> Grade 5 Section A  (2013)
--   seed.student.120..129   -> Grade 6 Section A  (2016)
--
-- Fixed IDs:
--   Demo teacher 00000000-0000-0000-0000-000000000002
--   Demo student 00000000-0000-0000-0000-000000000003
--   Teachers     00000000-0000-0000-0000-000000000101 .. 000000000109
--   Students     00000000-0000-0000-0000-000000000201 .. 000000000329
--   Pending      00000000-0000-0000-0000-000000000401 (teacher)
--                 00000000-0000-0000-0000-000000000402 (student)
--   Profiles     00000000-0000-0000-0000-000000008101 .. (teacher)
--                 00000000-0000-0000-0000-000000008201 .. (student)
--
-- Password hashes are static BCrypt hashes (work factor 12, generated with the
-- same BCrypt.Net call the PasswordHasher uses) for the three demo passwords.
-- The bulk accounts reuse the teacher/student hashes. These are development
-- fixtures with deliberately public passwords, not secrets.
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Demo teacher and student accounts.
-- ----------------------------------------------------------------------------
INSERT INTO "AuthUsers"
    ("Id", "FullName", "Email", "PasswordHash", "Role", "Status", "IsActive",
     "MustChangePassword", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
VALUES
    ('00000000-0000-0000-0000-000000000002', 'Demo Teacher',
     'teacher@onnorokom.com',
     '$2a$12$NZhcXhDmhZY/ek.ZNbNOHO0XXxCTmiZpO7W/dlY/fR2RL2Ka4qm.m',
     'Teacher', 'Approved', true, false,
     now() - interval '150 days', 'seed', now() - interval '150 days', 'seed', false),
    ('00000000-0000-0000-0000-000000000003', 'Demo Student',
     'student@onnorokom.com',
     '$2a$12$2/oivQhMwFcpoiXR5lFWm.bf7nl5KMmIv3mO15G4sIRgQCOSvQy2u',
     'Student', 'Approved', true, false,
     now() - interval '100 days', 'seed', now() - interval '100 days', 'seed', false)
ON CONFLICT ("Email") WHERE "IsDeleted" = false DO NOTHING;

-- ----------------------------------------------------------------------------
-- Bulk teachers: seed.teacher.1..9.
-- ----------------------------------------------------------------------------
INSERT INTO "AuthUsers"
    ("Id", "FullName", "Email", "PasswordHash", "Role", "Status", "IsActive",
     "MustChangePassword", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
VALUES
    ('00000000-0000-0000-0000-000000000101', 'Ayesha Rahman', 'seed.teacher.1@onnorokom.com',
     '$2a$12$NZhcXhDmhZY/ek.ZNbNOHO0XXxCTmiZpO7W/dlY/fR2RL2Ka4qm.m',
     'Teacher', 'Approved', true, false,
     now() - interval '149 days', 'seed', now() - interval '149 days', 'seed', false),
    ('00000000-0000-0000-0000-000000000102', 'Tanvir Ahmed', 'seed.teacher.2@onnorokom.com',
     '$2a$12$NZhcXhDmhZY/ek.ZNbNOHO0XXxCTmiZpO7W/dlY/fR2RL2Ka4qm.m',
     'Teacher', 'Approved', true, false,
     now() - interval '148 days', 'seed', now() - interval '148 days', 'seed', false),
    ('00000000-0000-0000-0000-000000000103', 'Farhana Islam', 'seed.teacher.3@onnorokom.com',
     '$2a$12$NZhcXhDmhZY/ek.ZNbNOHO0XXxCTmiZpO7W/dlY/fR2RL2Ka4qm.m',
     'Teacher', 'Approved', true, false,
     now() - interval '147 days', 'seed', now() - interval '147 days', 'seed', false),
    ('00000000-0000-0000-0000-000000000104', 'Rafiqul Islam', 'seed.teacher.4@onnorokom.com',
     '$2a$12$NZhcXhDmhZY/ek.ZNbNOHO0XXxCTmiZpO7W/dlY/fR2RL2Ka4qm.m',
     'Teacher', 'Approved', true, false,
     now() - interval '146 days', 'seed', now() - interval '146 days', 'seed', false),
    ('00000000-0000-0000-0000-000000000105', 'Nusrat Jahan', 'seed.teacher.5@onnorokom.com',
     '$2a$12$NZhcXhDmhZY/ek.ZNbNOHO0XXxCTmiZpO7W/dlY/fR2RL2Ka4qm.m',
     'Teacher', 'Approved', true, false,
     now() - interval '145 days', 'seed', now() - interval '145 days', 'seed', false),
    ('00000000-0000-0000-0000-000000000106', 'Imran Hossain', 'seed.teacher.6@onnorokom.com',
     '$2a$12$NZhcXhDmhZY/ek.ZNbNOHO0XXxCTmiZpO7W/dlY/fR2RL2Ka4qm.m',
     'Teacher', 'Approved', true, false,
     now() - interval '144 days', 'seed', now() - interval '144 days', 'seed', false),
    ('00000000-0000-0000-0000-000000000107', 'Sharmin Akter', 'seed.teacher.7@onnorokom.com',
     '$2a$12$NZhcXhDmhZY/ek.ZNbNOHO0XXxCTmiZpO7W/dlY/fR2RL2Ka4qm.m',
     'Teacher', 'Approved', true, false,
     now() - interval '143 days', 'seed', now() - interval '143 days', 'seed', false),
    ('00000000-0000-0000-0000-000000000108', 'Kamal Uddin', 'seed.teacher.8@onnorokom.com',
     '$2a$12$NZhcXhDmhZY/ek.ZNbNOHO0XXxCTmiZpO7W/dlY/fR2RL2Ka4qm.m',
     'Teacher', 'Approved', true, false,
     now() - interval '142 days', 'seed', now() - interval '142 days', 'seed', false),
    ('00000000-0000-0000-0000-000000000109', 'Rina Begum', 'seed.teacher.9@onnorokom.com',
     '$2a$12$NZhcXhDmhZY/ek.ZNbNOHO0XXxCTmiZpO7W/dlY/fR2RL2Ka4qm.m',
     'Teacher', 'Approved', true, false,
     now() - interval '141 days', 'seed', now() - interval '141 days', 'seed', false)
ON CONFLICT ("Email") WHERE "IsDeleted" = false DO NOTHING;

-- ----------------------------------------------------------------------------
-- Bulk students: seed.student.1..129. CreatedAt is spread one day apart so the
-- admin user list has a stable, meaningful cursor order.
-- ----------------------------------------------------------------------------
INSERT INTO "AuthUsers"
    ("Id", "FullName", "Email", "PasswordHash", "Role", "Status", "IsActive",
     "MustChangePassword", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
SELECT
    ('00000000-0000-0000-0000-' || lpad((200 + n)::text, 12, '0'))::uuid,
    (ARRAY['Rahim','Karim','Fatema','Ayesha','Rakib','Sumaiya','Tanvir','Nusrat',
           'Arif','Sadia','Mithila','Shakil','Jannatul','Nabil','Tasnim','Mim',
           'Fahim','Sana','Farhan','Mou'])[1 + (n % 20)]
        || ' '
        || (ARRAY['Hossain','Rahman','Islam','Khan','Ahmed','Chowdhury','Sarker',
                  'Das','Karim','Uddin'])[1 + (n % 10)],
    'seed.student.' || n || '@onnorokom.com',
    '$2a$12$2/oivQhMwFcpoiXR5lFWm.bf7nl5KMmIv3mO15G4sIRgQCOSvQy2u',
    'Student', 'Approved', true, false,
    now() - (129 - n || ' days')::interval,
    'seed',
    now() - (129 - n || ' days')::interval,
    'seed',
    false
FROM generate_series(1, 129) AS n
ON CONFLICT ("Email") WHERE "IsDeleted" = false DO NOTHING;

-- ----------------------------------------------------------------------------
-- Pending accounts: created by the seed to demonstrate the admin approval
-- flow. They have no profile: a self-registered student profile is provisioned
-- only when an admin approves them into a section.
-- ----------------------------------------------------------------------------
INSERT INTO "AuthUsers"
    ("Id", "FullName", "Email", "PasswordHash", "Role", "Status", "IsActive",
     "MustChangePassword", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
VALUES
    ('00000000-0000-0000-0000-000000000401', 'Pending Teacher', 'seed.teacher.pending@onnorokom.com',
     '$2a$12$NZhcXhDmhZY/ek.ZNbNOHO0XXxCTmiZpO7W/dlY/fR2RL2Ka4qm.m',
     'Teacher', 'Pending', true, false,
     now() - interval '3 days', 'seed', now() - interval '3 days', 'seed', false),
    ('00000000-0000-0000-0000-000000000402', 'Pending Student', 'seed.student.pending@onnorokom.com',
     '$2a$12$2/oivQhMwFcpoiXR5lFWm.bf7nl5KMmIv3mO15G4sIRgQCOSvQy2u',
     'Student', 'Pending', true, false,
     now() - interval '1 day', 'seed', now() - interval '1 day', 'seed', false)
ON CONFLICT ("Email") WHERE "IsDeleted" = false DO NOTHING;

-- ----------------------------------------------------------------------------
-- Teacher profiles. A profile is inserted only when the seeded user actually
-- exists under the seeded id, so a rerun after the user insert was skipped
-- never points at a foreign key that does not exist.
-- ----------------------------------------------------------------------------
INSERT INTO "TeacherProfiles"
    ("Id", "AuthUserId", "TeacherCode", "Department", "Designation", "Qualification",
     "PhoneNumber", "Address", "DateOfJoining",
     "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
SELECT
    ('00000000-0000-0000-0000-' || lpad((8100 + n)::text, 12, '0'))::uuid,
    ('00000000-0000-0000-0000-' || lpad((100 + n)::text, 12, '0'))::uuid,
    'T-' || lpad((1000 + n)::text, 4, '0'),
    CASE n
        WHEN 1 THEN 'English' WHEN 2 THEN 'Science' WHEN 3 THEN 'ICT'
        WHEN 4 THEN 'Bangla' WHEN 5 THEN 'Mathematics' WHEN 6 THEN 'English'
        WHEN 7 THEN 'Science' WHEN 8 THEN 'Mathematics' ELSE 'Bangla'
    END,
    'Senior Teacher',
    'BSc / MA in the subject',
    '017' || lpad((1000000 + n * 13)::text, 8, '0'),
    'Dhaka, Bangladesh',
    now() - interval '120 days',
    now() - interval '141 days', 'seed', now() - interval '141 days', 'seed', false
FROM generate_series(1, 9) AS n
WHERE EXISTS (SELECT 1 FROM "AuthUsers" WHERE "Id" = ('00000000-0000-0000-0000-' || lpad((100 + n)::text, 12, '0'))::uuid)
  AND NOT EXISTS (SELECT 1 FROM "TeacherProfiles" WHERE "AuthUserId" = ('00000000-0000-0000-0000-' || lpad((100 + n)::text, 12, '0'))::uuid);

-- Demo teacher profile.
INSERT INTO "TeacherProfiles"
    ("Id", "AuthUserId", "TeacherCode", "Department", "Designation", "Qualification",
     "PhoneNumber", "Address", "DateOfJoining",
     "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
SELECT
    '00000000-0000-0000-0000-000000008100'::uuid,
    '00000000-0000-0000-0000-000000000002'::uuid,
    'T-1000',
    'Mathematics',
    'Senior Teacher',
    'MSc in Mathematics',
    '01700000000',
    'Dhaka, Bangladesh',
    now() - interval '120 days',
    now() - interval '150 days', 'seed', now() - interval '150 days', 'seed', false
WHERE EXISTS (SELECT 1 FROM "AuthUsers" WHERE "Id" = '00000000-0000-0000-0000-000000000002')
  AND NOT EXISTS (SELECT 1 FROM "TeacherProfiles" WHERE "AuthUserId" = '00000000-0000-0000-0000-000000000002');

-- ----------------------------------------------------------------------------
-- Student profiles, including the demo student (section 2001, roll G1A-001).
-- The section for bulk students follows the distribution table at the top.
-- ----------------------------------------------------------------------------
INSERT INTO "StudentProfiles"
    ("Id", "AuthUserId", "SectionId", "RollNumber", "DateOfBirth", "Gender",
     "GuardianName", "GuardianPhone", "Address", "AdmissionDate",
     "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
SELECT
    '00000000-0000-0000-0000-000000008200'::uuid,
    '00000000-0000-0000-0000-000000000003'::uuid,
    '00000000-0000-0000-0000-000000002001'::uuid,
    'G1A-001',
    now() - interval '10 years',
    'Male',
    'Guardian of Demo Student',
    '01800000000',
    'Dhaka, Bangladesh',
    now() - interval '100 days',
    now() - interval '100 days', 'seed', now() - interval '100 days', 'seed', false
WHERE EXISTS (SELECT 1 FROM "AuthUsers" WHERE "Id" = '00000000-0000-0000-0000-000000000003')
  AND NOT EXISTS (SELECT 1 FROM "StudentProfiles" WHERE "AuthUserId" = '00000000-0000-0000-0000-000000000003');

INSERT INTO "StudentProfiles"
    ("Id", "AuthUserId", "SectionId", "RollNumber", "DateOfBirth", "Gender",
     "GuardianName", "GuardianPhone", "Address", "AdmissionDate",
     "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
SELECT
    ('00000000-0000-0000-0000-' || lpad((8200 + n)::text, 12, '0'))::uuid,
    ('00000000-0000-0000-0000-' || lpad((200 + n)::text, 12, '0'))::uuid,
    ('00000000-0000-0000-0000-' || lpad(section_of::text, 12, '0'))::uuid,
    'S-' || lpad(n::text, 3, '0'),
    now() - ((7 + (n % 8)) || ' years')::interval,
    CASE n % 3 WHEN 0 THEN 'Other' WHEN 1 THEN 'Male' ELSE 'Female' END,
    'Guardian of ' || (ARRAY['Rahim','Karim','Fatema','Ayesha','Rakib','Sumaiya',
                             'Tanvir','Nusrat','Arif','Sadia','Mithila','Shakil',
                             'Jannatul','Nabil','Tasnim','Mim','Fahim','Sana',
                             'Farhan','Mou'])[1 + (n % 20)],
    '019' || lpad((2000000 + n * 7)::text, 8, '0'),
    'Dhaka, Bangladesh',
    now() - interval '170 days',
    now() - (129 - n || ' days')::interval,
    'seed',
    now() - (129 - n || ' days')::interval,
    'seed',
    false
FROM generate_series(1, 129) AS n
CROSS JOIN LATERAL (
    SELECT CASE
        WHEN n BETWEEN 1  AND 29  THEN 2001
        WHEN n BETWEEN 30 AND 39  THEN 2002
        WHEN n BETWEEN 40 AND 49  THEN 2003
        WHEN n BETWEEN 50 AND 59  THEN 2004
        WHEN n BETWEEN 60 AND 69  THEN 2005
        WHEN n BETWEEN 70 AND 79  THEN 2006
        WHEN n BETWEEN 80 AND 89  THEN 2007
        WHEN n BETWEEN 90 AND 99  THEN 2008
        WHEN n BETWEEN 100 AND 109 THEN 2010
        WHEN n BETWEEN 110 AND 119 THEN 2013
        ELSE 2016
    END AS section_of
) AS s
WHERE EXISTS (SELECT 1 FROM "AuthUsers" WHERE "Id" = ('00000000-0000-0000-0000-' || lpad((200 + n)::text, 12, '0'))::uuid)
  AND NOT EXISTS (SELECT 1 FROM "StudentProfiles" WHERE "AuthUserId" = ('00000000-0000-0000-0000-' || lpad((200 + n)::text, 12, '0'))::uuid);
