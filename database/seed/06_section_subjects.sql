-- ============================================================================
-- Seed step 06: section-subject links and per-section teacher assignments.
-- ============================================================================
-- A subject is catalogued on the grade; the teacher varies per section and
-- lives in this join row. Every section gets a link to all five subjects of
-- its grade. Slots listed in teacher_assignments get a teacher (the demo
-- teacher owns Mathematics in Grade 1 Section A, which is what the demo
-- assignments in step 07 build on); the rest stay unassigned so the admin
-- "assign a teacher" flow has realistic empty slots.
--
-- Fixed IDs: 00000000-0000-0000-0000-000000004001 .. 000000004120
--   Link n (1..120) is section (1 + (n-1)/5) subject slot ((n-1) % 5 + 1),
--   so links 1-5 belong to section 2001, 6-10 to section 2002, and so on.
-- ============================================================================

WITH teacher_assignments (s, j, t) AS (
    VALUES
        -- s = section index 1..24 (2001..2024), j = subject slot 1..5
        -- (1=Mathematics, 2=English, 3=Science, 4=ICT, 5=Bangla),
        -- t = teacher user id suffix (0002 = demo teacher, 101..109 = bulk).
        -- Grade 1 Section A
        (1, 1, 2),   (1, 2, 101), (1, 3, 102), (1, 4, 103), (1, 5, 104),
        -- Grade 1 Section B
        (2, 1, 105), (2, 2, 102), (2, 5, 101),
        -- Grade 1 Section C
        (3, 1, 102), (3, 2, 104), (3, 3, 103),
        -- Grade 2 Section A
        (4, 1, 105), (4, 2, 106), (4, 3, 107),
        -- Grade 2 Section B
        (5, 1, 107), (5, 2, 108), (5, 5, 105),
        -- Grade 2 Section C
        (6, 3, 106), (6, 4, 107), (6, 5, 109),
        -- Grade 3 Section A
        (7, 1, 108), (7, 2, 109),
        -- Grade 3 Section B
        (8, 3, 108), (8, 4, 109),
        -- Grade 4 Section A
        (10, 1, 108), (10, 2, 101), (10, 3, 103),
        -- Grade 5 Section A
        (13, 1, 105), (13, 5, 109),
        -- Grade 6 Section A
        (16, 3, 107), (16, 4, 102)
)
INSERT INTO "SectionSubjects"
    ("Id", "SectionId", "SubjectId", "TeacherId",
     "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
SELECT
    ('00000000-0000-0000-0000-' || lpad((4000 + n)::text, 12, '0'))::uuid,
    ('00000000-0000-0000-0000-' || lpad((2000 + section_of)::text, 12, '0'))::uuid,
    ('00000000-0000-0000-0000-' || lpad((3000 + (grade_of - 1) * 5 + subject_of)::text, 12, '0'))::uuid,
    CASE WHEN ta.t IS NULL THEN NULL
         ELSE ('00000000-0000-0000-0000-' || lpad(ta.t::text, 12, '0'))::uuid
    END,
    now() - interval '160 days',
    'seed',
    now() - interval '160 days',
    'seed',
    false
FROM generate_series(1, 120) AS n
CROSS JOIN LATERAL (
    SELECT 1 + ((n - 1) / 5) AS section_of,
           ((n - 1) % 5) + 1 AS subject_of
) AS s
CROSS JOIN LATERAL (
    -- Section index -> grade number. Sections 1-18 (grades 1-6) have A/B/C,
    -- sections 19-24 (grades 7-12) only A, matching step 03.
    SELECT CASE
        WHEN s.section_of <= 18 THEN 1 + ((s.section_of - 1) / 3)
        ELSE s.section_of - 12
    END AS grade_of
) AS g
LEFT JOIN teacher_assignments ta
    ON ta.s = s.section_of AND ta.j = s.subject_of
ON CONFLICT ("SectionId", "SubjectId") WHERE "IsDeleted" = false DO NOTHING;
