-- ============================================================================
-- Seed step 03: grades and sections for the current academic year.
-- ============================================================================
-- Grade 1 through Grade 12 for the year now() is in. Grades 1-6 get sections
-- A, B and C, grades 7-12 get section A only; 24 sections in total. Students
-- enrol into a section, never directly into a grade.
--
-- Idempotent: the whole step runs only when the current academic year has no
-- grades yet, so an admin's later edits (renames, deletes) are never undone.
--
-- Fixed IDs:
--   Grades   00000000-0000-0000-0000-000000001001 .. 0000000001012
--   Sections 00000000-0000-0000-0000-000000002001 .. 000000002024
-- ============================================================================

INSERT INTO "Grades"
    ("Id", "Name", "Description", "AcademicYear",
     "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
SELECT
    ('00000000-0000-0000-0000-' || lpad((1000 + g)::text, 12, '0'))::uuid,
    'Grade ' || g,
    NULL,
    EXTRACT(YEAR FROM now())::text,
    now() - interval '180 days',
    'seed',
    now() - interval '180 days',
    'seed',
    false
FROM generate_series(1, 12) AS g
WHERE NOT EXISTS (
    SELECT 1 FROM "Grades"
    WHERE "AcademicYear" = EXTRACT(YEAR FROM now())::text
      AND NOT "IsDeleted"
);

-- Sections are seeded only alongside the grades, so a section an admin deletes
-- later does not reappear on the next run of the scripts. The guard looks at
-- the Sections table itself (which only this script populates): checking the
-- Grades table here would always see the grades just inserted above and skip.
INSERT INTO "Sections"
    ("Id", "Name", "GradeId",
     "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
SELECT
    ('00000000-0000-0000-0000-' || lpad((2000 + s)::text, 12, '0'))::uuid,
    CASE
        WHEN s <= 18 THEN 'Section ' || chr(64 + ((s - 1) % 3) + 1)
        ELSE 'Section A'
    END,
    ('00000000-0000-0000-0000-' || lpad((1000 + g.grade_of)::text, 12, '0'))::uuid,
    now() - interval '180 days',
    'seed',
    now() - interval '180 days',
    'seed',
    false
FROM generate_series(1, 24) AS s
CROSS JOIN LATERAL (
    SELECT CASE
        WHEN s <= 18 THEN 1 + ((s - 1) / 3)
        ELSE s - 12
    END AS grade_of
) AS g
WHERE NOT EXISTS (
    SELECT 1 FROM "Sections" WHERE NOT "IsDeleted"
);
