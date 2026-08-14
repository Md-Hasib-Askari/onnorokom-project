-- ============================================================================
-- Seed step 04: the subject catalog, five subjects per grade.
-- ============================================================================
-- Subjects hang off a grade as catalog entries; the teacher is per section and
-- lives in the SectionSubjects join (seed step 06). The unique partial index
-- is (GradeId, Name), which makes ON CONFLICT DO NOTHING idempotent.
--
-- Fixed IDs: 00000000-0000-0000-0000-000000003001 .. 000000003060
--   Grade g, subject slot j (1=Mathematics, 2=English, 3=Science, 4=ICT,
--   5=Bangla) gets id 3000 + ((g - 1) * 5) + j.
-- ============================================================================

INSERT INTO "Subjects"
    ("Id", "Name", "Code", "GradeId",
     "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
SELECT
    ('00000000-0000-0000-0000-' || lpad((3000 + n)::text, 12, '0'))::uuid,
    CASE ((n - 1) % 5) + 1
        WHEN 1 THEN 'Mathematics'
        WHEN 2 THEN 'English'
        WHEN 3 THEN 'Science'
        WHEN 4 THEN 'ICT'
        ELSE 'Bangla'
    END,
    CASE ((n - 1) % 5) + 1
        WHEN 1 THEN 'MAT-' || lpad((((n - 1) / 5) + 1)::text, 2, '0')
        WHEN 2 THEN 'ENG-' || lpad((((n - 1) / 5) + 1)::text, 2, '0')
        WHEN 3 THEN 'SCI-' || lpad((((n - 1) / 5) + 1)::text, 2, '0')
        WHEN 4 THEN 'ICT-' || lpad((((n - 1) / 5) + 1)::text, 2, '0')
        ELSE 'BAN-' || lpad((((n - 1) / 5) + 1)::text, 2, '0')
    END,
    ('00000000-0000-0000-0000-' || lpad((1000 + ((n - 1) / 5) + 1)::text, 12, '0'))::uuid,
    now() - interval '170 days',
    'seed',
    now() - interval '170 days',
    'seed',
    false
FROM generate_series(1, 60) AS n
ON CONFLICT ("GradeId", "Name") WHERE "IsDeleted" = false DO NOTHING;
