-- ============================================================================
-- Seed step 07: demo worked example, bulk assignments, and submissions.
-- ============================================================================
-- Everything in this step is guarded on the assignments table being completely
-- empty (including soft-deleted rows), matching the original DbInitializer
-- semantics: an assignment somebody deleted while trying the system out stays
-- deleted rather than returning on the next run. The guard is evaluated once
-- in a single DO block, so all parts of this step run or none do.
--
-- Demo worked example (unchanged from the original seed):
--   5001 "Algebra Worksheet 1" (published, past deadline, graded submission)
--   5002 "Geometry Basics"     (published, open, nobody submitted yet)
--
-- Bulk volume (pagination-testable at the default page size of 20):
--   150 assignments total: 139 published, 11 drafts, spread over 30
--   section-subject slots. The demo teacher owns 40 in Grade 1 Section A
--   Mathematics (2 pages for the teacher), and the demo student sees 56
--   published assignments in Section A (3 pages).
--   163 submissions: all 30 Grade 1 Section A students on 5001 (2 pages for
--   the teacher), ~15 students on seven more past Mathematics assignments,
--   plus smaller English/Science pools and the demo student's own rows. Status
--   mix: Submitted / Resubmitted / Returned / Graded, some late.
--
-- Fixed IDs:
--   Assignments 00000000-0000-0000-0000-000000005001 .. 000000005150
--   Submissions 00000000-0000-0000-0000-000000006001 .. 000000006163
--
-- Deadlines and timestamps are relative to now() so the dataset stays fresh
-- whenever the scripts are applied, and CreatedAt/SubmittedAt values are
-- spread one day apart so cursor pagination has a stable, meaningful order.
-- ============================================================================

DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM "Assignments") THEN
        RAISE NOTICE 'Assignments already exist; skipping seed step 07.';
        RETURN;
    END IF;

    -- ------------------------------------------------------------------------
    -- Part A: the demo worked example (demo teacher 0002, Grade 1 Section A
    -- 2001, Mathematics 3001).
    -- ------------------------------------------------------------------------
    INSERT INTO "Assignments"
        ("Id", "Title", "Description", "SectionId", "SubjectId", "TeacherId",
         "Deadline", "MaxMarks", "Status", "AllowLateSubmission", "SubmissionsOpen",
         "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
    VALUES
        ('00000000-0000-0000-0000-000000005001',
         'Algebra Worksheet 1',
         'Solve questions 1 to 10 from chapter 3 and show your working for each step.',
         '00000000-0000-0000-0000-000000002001',
         '00000000-0000-0000-0000-000000003001',
         '00000000-0000-0000-0000-000000000002',
         now() - interval '30 days', 20, 'Published', false, true,
         now() - interval '120 days', 'seed', now() - interval '120 days', 'seed', false),
        ('00000000-0000-0000-0000-000000005002',
         'Geometry Basics',
         'Identify the angle types in the attached figures and justify each answer in one line.',
         '00000000-0000-0000-0000-000000002001',
         '00000000-0000-0000-0000-000000003001',
         '00000000-0000-0000-0000-000000000002',
         now() + interval '7 days', 20, 'Published', false, true,
         now() - interval '119 days', 'seed', now() - interval '119 days', 'seed', false);

    -- ------------------------------------------------------------------------
    -- Part B: bulk assignments, one INSERT over a specs table.
    -- ------------------------------------------------------------------------
    -- spec columns: gid (first generated id), section id, subject id, teacher
    -- id, count (rows), max marks, draft range (local n, 0 = no draft),
    -- deadline base (published deadline = now() - (base - n) days; a negative
    -- base means the newest rows have future deadlines and are open),
    -- created base (created_at = now() - (base - n) days).
    -- =========================================================================
    WITH specs (gid, section_id, subject_id, teacher_id, count, max_marks,
                draft_from, draft_to, deadline_base, created_base) AS (
        VALUES
            -- Grade 1 Section A (2001): the demo teacher's Mathematics slot
            -- gets 40 assignments (2 pages for the teacher, 3 pages for the
            -- student).
            (5003, '00000000-0000-0000-0000-000000002001', '00000000-0000-0000-0000-000000003001', '00000000-0000-0000-0000-000000000002', 38, 20, 37, 38, 97, 117),
            (5041, '00000000-0000-0000-0000-000000002001', '00000000-0000-0000-0000-000000003002', '00000000-0000-0000-0000-000000000101', 6,  10, 6,  6,  40, 79),
            (5047, '00000000-0000-0000-0000-000000002001', '00000000-0000-0000-0000-000000003003', '00000000-0000-0000-0000-000000000102', 6,  10, 6,  6,  42, 73),
            (5053, '00000000-0000-0000-0000-000000002001', '00000000-0000-0000-0000-000000003004', '00000000-0000-0000-0000-000000000103', 4,  20, 0,  0,  40, 67),
            (5057, '00000000-0000-0000-0000-000000002001', '00000000-0000-0000-0000-000000003005', '00000000-0000-0000-0000-000000000104', 4,  10, 0,  0,  38, 63),
            -- Grade 1 Section B (2002)
            (5061, '00000000-0000-0000-0000-000000002002', '00000000-0000-0000-0000-000000003001', '00000000-0000-0000-0000-000000000105', 6,  20, 6,  6,  40, 59),
            (5067, '00000000-0000-0000-0000-000000002002', '00000000-0000-0000-0000-000000003002', '00000000-0000-0000-0000-000000000102', 4,  10, 0,  0,  34, 53),
            (5071, '00000000-0000-0000-0000-000000002002', '00000000-0000-0000-0000-000000003005', '00000000-0000-0000-0000-000000000101', 4,  10, 0,  0,  32, 49),
            -- Grade 1 Section C (2003)
            (5075, '00000000-0000-0000-0000-000000002003', '00000000-0000-0000-0000-000000003001', '00000000-0000-0000-0000-000000000102', 4,  20, 0,  0,  30, 45),
            (5079, '00000000-0000-0000-0000-000000002003', '00000000-0000-0000-0000-000000003003', '00000000-0000-0000-0000-000000000103', 4,  10, 0,  0,  28, 41),
            (5083, '00000000-0000-0000-0000-000000002003', '00000000-0000-0000-0000-000000003002', '00000000-0000-0000-0000-000000000104', 4,  10, 4,  4,  26, 37),
            -- Grade 2 (2004-2006)
            (5087, '00000000-0000-0000-0000-000000002004', '00000000-0000-0000-0000-000000003006', '00000000-0000-0000-0000-000000000105', 4,  20, 0,  0,  40, 33),
            (5091, '00000000-0000-0000-0000-000000002004', '00000000-0000-0000-0000-000000003007', '00000000-0000-0000-0000-000000000106', 4,  10, 0,  0,  36, 29),
            (5095, '00000000-0000-0000-0000-000000002004', '00000000-0000-0000-0000-000000003008', '00000000-0000-0000-0000-000000000107', 2,  10, 0,  0, -1,  25),
            (5097, '00000000-0000-0000-0000-000000002005', '00000000-0000-0000-0000-000000003006', '00000000-0000-0000-0000-000000000107', 4,  20, 0,  0,  33, 23),
            (5101, '00000000-0000-0000-0000-000000002005', '00000000-0000-0000-0000-000000003010', '00000000-0000-0000-0000-000000000105', 4,  10, 4,  4,  30, 19),
            (5105, '00000000-0000-0000-0000-000000002005', '00000000-0000-0000-0000-000000003007', '00000000-0000-0000-0000-000000000108', 2,  10, 0,  0,  28, 15),
            (5107, '00000000-0000-0000-0000-000000002006', '00000000-0000-0000-0000-000000003008', '00000000-0000-0000-0000-000000000106', 4,  10, 0,  0,  13, 13),
            (5111, '00000000-0000-0000-0000-000000002006', '00000000-0000-0000-0000-000000003009', '00000000-0000-0000-0000-000000000107', 4,  20, 0,  0,   9,  9),
            (5115, '00000000-0000-0000-0000-000000002006', '00000000-0000-0000-0000-000000003010', '00000000-0000-0000-0000-000000000109', 2,  10, 0,  0,   7,  5),
            -- Grades 3-6 (2007, 2008, 2010, 2013, 2016)
            (5117, '00000000-0000-0000-0000-000000002007', '00000000-0000-0000-0000-000000003011', '00000000-0000-0000-0000-000000000108', 4,  20, 4,  4,   0, 25),
            (5121, '00000000-0000-0000-0000-000000002007', '00000000-0000-0000-0000-000000003012', '00000000-0000-0000-0000-000000000109', 4,  10, 0,  0,  20, 21),
            (5125, '00000000-0000-0000-0000-000000002008', '00000000-0000-0000-0000-000000003013', '00000000-0000-0000-0000-000000000108', 4,  10, 4,  4,  18, 17),
            (5129, '00000000-0000-0000-0000-000000002008', '00000000-0000-0000-0000-000000003014', '00000000-0000-0000-0000-000000000109', 4,  20, 4,  4,  15, 13),
            (5133, '00000000-0000-0000-0000-000000002010', '00000000-0000-0000-0000-000000003016', '00000000-0000-0000-0000-000000000108', 4,  20, 0,  0,  24,  9),
            (5137, '00000000-0000-0000-0000-000000002010', '00000000-0000-0000-0000-000000003017', '00000000-0000-0000-0000-000000000101', 4,  10, 4,  4,  22,  5),
            (5141, '00000000-0000-0000-0000-000000002013', '00000000-0000-0000-0000-000000003021', '00000000-0000-0000-0000-000000000105', 3,  20, 0,  0,   0, 21),
            (5144, '00000000-0000-0000-0000-000000002013', '00000000-0000-0000-0000-000000003025', '00000000-0000-0000-0000-000000000109', 2,  10, 0,  0,  10, 18),
            (5146, '00000000-0000-0000-0000-000000002016', '00000000-0000-0000-0000-000000003028', '00000000-0000-0000-0000-000000000107', 3,  10, 0,  0,   8, 15),
            (5149, '00000000-0000-0000-0000-000000002016', '00000000-0000-0000-0000-000000003029', '00000000-0000-0000-0000-000000000102', 2,  20, 0,  0,   6, 12)
    )
    INSERT INTO "Assignments"
        ("Id", "Title", "Description", "SectionId", "SubjectId", "TeacherId",
         "Deadline", "MaxMarks", "Status", "AllowLateSubmission", "SubmissionsOpen",
         "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
    SELECT
        ('00000000-0000-0000-0000-' || lpad((specs.gid + n - 1)::text, 12, '0'))::uuid,
        subj."Name" || ' Worksheet ' || lpad(n::text, 2, '0'),
        'Complete all questions in ' || subj."Name" || ' Worksheet ' || lpad(n::text, 2, '0')
            || ' and submit your answers before the deadline.',
        specs.section_id::uuid,
        specs.subject_id::uuid,
        specs.teacher_id::uuid,
        CASE
            WHEN n BETWEEN specs.draft_from AND specs.draft_to THEN now() + interval '14 days'
            ELSE now() - ((specs.deadline_base - n) || ' days')::interval
        END,
        specs.max_marks,
        CASE
            WHEN n BETWEEN specs.draft_from AND specs.draft_to THEN 'Draft'
            ELSE 'Published'
        END,
        (n % 4) = 0,
        true,
        now() - ((specs.created_base - n) || ' days')::interval,
        'seed',
        now() - ((specs.created_base - n) || ' days')::interval,
        'seed',
        false
    FROM specs
    JOIN "Subjects" subj ON subj."Id" = specs.subject_id::uuid
    CROSS JOIN generate_series(1, specs.count) AS n;

    -- ------------------------------------------------------------------------
    -- Part C: submissions.
    -- ------------------------------------------------------------------------
    -- Pool 1: every Grade 1 Section A student (demo 0003 + seed.student.1..29)
    -- submitted to "Algebra Worksheet 1" (5001, deadline 30 days ago). Status
    -- cycle by k: 0-4 Graded, 5-7 Submitted, 8 Resubmitted, 9 Returned; every
    -- ninth student submitted late.
    -- =========================================================================
    INSERT INTO "Submissions"
        ("Id", "AssignmentId", "StudentId", "Content", "AttachmentUrl", "Status",
         "Marks", "Feedback", "SubmittedAt", "GradedAt", "GradedByTeacherId",
         "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
    SELECT
        ('00000000-0000-0000-0000-' || lpad((6001 + k)::text, 12, '0'))::uuid,
        '00000000-0000-0000-0000-000000005001',
        CASE WHEN k = 29 THEN '00000000-0000-0000-0000-000000000003'::uuid
             ELSE ('00000000-0000-0000-0000-' || lpad((201 + k)::text, 12, '0'))::uuid
        END,
        'Answers submitted for all questions with full working shown.',
        NULL,
        CASE WHEN k = 29 THEN 'Graded'
             ELSE (ARRAY['Graded','Graded','Graded','Graded','Graded',
                         'Submitted','Submitted','Submitted','Resubmitted','Returned'])
                  [1 + (k % 10)]
        END,
        CASE
            WHEN k = 29 THEN 17
            WHEN (k % 10) <= 4 THEN 5 + ((k * 3) % 15)
            ELSE NULL
        END,
        CASE
            WHEN k = 29 THEN 'Solid work overall. Question 6 lost marks for skipping the factorisation step.'
            WHEN (k % 10) <= 4 THEN 'Good work. Review question ' || (1 + (k % 10)) || '.'
            ELSE NULL
        END,
        CASE WHEN (k % 9) = 0 THEN now() - interval '28 days'
             ELSE now() - interval '30 days' - ((k * 2) || ' days')::interval
        END,
        CASE WHEN k = 29 OR (k % 10) <= 4
             THEN now() - interval '30 days' - ((k * 2) || ' days')::interval + interval '1 day'
             ELSE NULL
        END,
        CASE WHEN k = 29 OR (k % 10) <= 4
             THEN '00000000-0000-0000-0000-000000000002'::uuid
             ELSE NULL
        END,
        now() - interval '30 days' - ((k * 2) || ' days')::interval,
        'seed',
        now() - interval '30 days' - ((k * 2) || ' days')::interval,
        'seed',
        false
    FROM generate_series(0, 29) AS k;

    -- Demo student's own rows on other assignments: a late Submitted on 5003
    -- and an on-time Submitted on 5041 (submitted 45 days ago, deadline 39 days
    -- ago), so the student detail view shows more than one submission state.
    INSERT INTO "Submissions"
        ("Id", "AssignmentId", "StudentId", "Content", "AttachmentUrl", "Status",
         "Marks", "Feedback", "SubmittedAt", "GradedAt", "GradedByTeacherId",
         "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
    VALUES
        ('00000000-0000-0000-0000-000000006031', '00000000-0000-0000-0000-000000005003',
         '00000000-0000-0000-0000-000000000003',
         'Answers for questions 1 to 10, submitted late.',
         NULL, 'Submitted', NULL, NULL,
         now() - interval '88 days', NULL, NULL,
         now() - interval '88 days', 'seed', now() - interval '88 days', 'seed', false),
        ('00000000-0000-0000-0000-000000006032', '00000000-0000-0000-0000-000000005041',
         '00000000-0000-0000-0000-000000000003',
         'Grammar exercises completed on time.',
         NULL, 'Submitted', NULL, NULL,
         now() - interval '45 days', NULL, NULL,
         now() - interval '45 days', 'seed', now() - interval '45 days', 'seed', false);

    -- Pool 3: ~15 students (seed.student.1..15) on seven past Mathematics
    -- assignments (5003..5009, deadlines 96 to 90 days ago). Status cycle by
    -- k: 0-1 Graded, 2-3 Submitted, 4 Resubmitted, 5 Returned; late when
    -- k % 6 = 3.
    INSERT INTO "Submissions"
        ("Id", "AssignmentId", "StudentId", "Content", "AttachmentUrl", "Status",
         "Marks", "Feedback", "SubmittedAt", "GradedAt", "GradedByTeacherId",
         "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
    SELECT
        ('00000000-0000-0000-0000-' || lpad((6033 + (a - 3) * 15 + k)::text, 12, '0'))::uuid,
        ('00000000-0000-0000-0000-' || lpad((5000 + a)::text, 12, '0'))::uuid,
        ('00000000-0000-0000-0000-' || lpad((201 + k)::text, 12, '0'))::uuid,
        'Answers submitted for all questions with full working shown.',
        NULL,
        (ARRAY['Graded','Graded','Submitted','Submitted','Resubmitted','Returned'])[1 + (k % 6)],
        CASE WHEN (k % 6) <= 1 THEN 4 + (((k * 5) + a) % 15) ELSE NULL END,
        CASE WHEN (k % 6) <= 1 THEN 'Good work. Review question ' || (1 + (k % 10)) || '.' ELSE NULL END,
        CASE WHEN (k % 6) = 3
             THEN now() - ((99 - a) || ' days')::interval + interval '1 day'
             ELSE now() - ((99 - a) || ' days')::interval - ((k % 12) || ' days')::interval
        END,
        CASE WHEN (k % 6) <= 1
             THEN now() - ((99 - a) || ' days')::interval - ((k % 12) || ' days')::interval + interval '1 day'
             ELSE NULL
        END,
        CASE WHEN (k % 6) <= 1 THEN '00000000-0000-0000-0000-000000000002'::uuid ELSE NULL END,
        now() - ((99 - a) || ' days')::interval - ((k % 12) || ' days')::interval,
        'seed',
        now() - ((99 - a) || ' days')::interval - ((k % 12) || ' days')::interval,
        'seed',
        false
    FROM generate_series(3, 9) AS a
    CROSS JOIN generate_series(0, 14) AS k;

    -- Pool 4: English worksheets 5041 and 5042 (deadlines 39 and 38 days ago),
    -- eight students each (seed.student.1..8), graded by teacher 0101.
    INSERT INTO "Submissions"
        ("Id", "AssignmentId", "StudentId", "Content", "AttachmentUrl", "Status",
         "Marks", "Feedback", "SubmittedAt", "GradedAt", "GradedByTeacherId",
         "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
    SELECT
        ('00000000-0000-0000-0000-' || lpad((6138 + (w - 1) * 8 + k)::text, 12, '0'))::uuid,
        ('00000000-0000-0000-0000-' || lpad((5040 + w)::text, 12, '0'))::uuid,
        ('00000000-0000-0000-0000-' || lpad((201 + k)::text, 12, '0'))::uuid,
        'Essay and grammar exercises submitted.',
        NULL,
        (ARRAY['Graded','Submitted','Submitted','Graded','Resubmitted','Submitted'])[1 + (k % 6)],
        CASE WHEN (k % 6) IN (0, 3) THEN 4 + ((k * 3) % 7) ELSE NULL END,
        CASE WHEN (k % 6) IN (0, 3) THEN 'Well structured. Watch spelling on question ' || (1 + (k % 5)) || '.' ELSE NULL END,
        CASE WHEN (k % 4) = 1
             THEN now() - ((40 - w) || ' days')::interval + interval '1 day'
             ELSE now() - ((40 - w) || ' days')::interval - ((k % 5) || ' days')::interval
        END,
        CASE WHEN (k % 6) IN (0, 3)
             THEN now() - ((40 - w) || ' days')::interval - ((k % 5) || ' days')::interval + interval '1 day'
             ELSE NULL
        END,
        CASE WHEN (k % 6) IN (0, 3) THEN '00000000-0000-0000-0000-000000000101'::uuid ELSE NULL END,
        now() - ((40 - w) || ' days')::interval - ((k % 5) || ' days')::interval,
        'seed',
        now() - ((40 - w) || ' days')::interval - ((k % 5) || ' days')::interval,
        'seed',
        false
    FROM generate_series(1, 2) AS w
    CROSS JOIN generate_series(0, 7) AS k;

    -- Pool 5: Science worksheets 5047 and 5048 (deadlines 41 and 40 days ago),
    -- five students each (seed.student.1..5), graded by teacher 0102.
    INSERT INTO "Submissions"
        ("Id", "AssignmentId", "StudentId", "Content", "AttachmentUrl", "Status",
         "Marks", "Feedback", "SubmittedAt", "GradedAt", "GradedByTeacherId",
         "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted")
    SELECT
        ('00000000-0000-0000-0000-' || lpad((6154 + (w - 1) * 5 + k)::text, 12, '0'))::uuid,
        ('00000000-0000-0000-0000-' || lpad((5046 + w)::text, 12, '0'))::uuid,
        ('00000000-0000-0000-0000-' || lpad((201 + k)::text, 12, '0'))::uuid,
        'Lab report submitted.',
        NULL,
        (ARRAY['Graded','Submitted','Resubmitted','Submitted','Graded'])[1 + (k % 5)],
        CASE WHEN (k % 5) IN (0, 4) THEN 5 + ((k * 2) % 5) ELSE NULL END,
        CASE WHEN (k % 5) IN (0, 4) THEN 'Clear conclusions. Check the units on question ' || (1 + (k % 3)) || '.' ELSE NULL END,
        now() - ((42 - w) || ' days')::interval - (((k * 2) % 9) || ' days')::interval,
        CASE WHEN (k % 5) IN (0, 4)
             THEN now() - ((42 - w) || ' days')::interval - (((k * 2) % 9) || ' days')::interval + interval '1 day'
             ELSE NULL
        END,
        CASE WHEN (k % 5) IN (0, 4) THEN '00000000-0000-0000-0000-000000000102'::uuid ELSE NULL END,
        now() - ((42 - w) || ' days')::interval - (((k * 2) % 9) || ' days')::interval,
        'seed',
        now() - ((42 - w) || ' days')::interval - (((k * 2) % 9) || ' days')::interval,
        'seed',
        false
    FROM generate_series(1, 2) AS w
    CROSS JOIN generate_series(0, 4) AS k;

    RAISE NOTICE 'Seeded 150 assignments and 163 submissions.';
END $$;
