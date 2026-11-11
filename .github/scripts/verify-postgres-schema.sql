-- Runs only against the disposable PostgreSQL service in API CI.
DO $$
DECLARE
    missing_names text;
    invalid_tables text;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "__EFMigrationsHistory"
                   WHERE "MigrationId" = '20261107000138_InitialDemo') THEN
        RAISE EXCEPTION 'InitialDemo migration is missing';
    END IF;

    SELECT string_agg(expected.name, ', ' ORDER BY expected.name)
      INTO missing_names
      FROM (VALUES ('ben_reference_seq'), ('out_reference_seq'),
                   ('rec_reference_seq'), ('ref_reference_seq'),
                   ('req_reference_seq')) AS expected(name)
     WHERE NOT EXISTS (SELECT 1 FROM pg_sequences
                       WHERE schemaname = 'public' AND sequencename = expected.name);
    IF missing_names IS NOT NULL THEN
        RAISE EXCEPTION 'Missing reference sequences: %', missing_names;
    END IF;

    SELECT string_agg(t.table_name, ', ' ORDER BY t.table_name)
      INTO invalid_tables
      FROM information_schema.tables t
     WHERE t.table_schema = 'public' AND t.table_type = 'BASE TABLE'
       AND t.table_name <> '__EFMigrationsHistory'
       AND NOT EXISTS (
           SELECT 1
             FROM pg_constraint c
             JOIN pg_class r ON r.oid = c.conrelid
             JOIN pg_namespace n ON n.oid = r.relnamespace
             JOIN pg_attribute a ON a.attrelid = r.oid AND a.attnum = c.conkey[1]
            WHERE n.nspname = t.table_schema AND r.relname = t.table_name
              AND c.contype = 'p' AND cardinality(c.conkey) = 1
              AND a.atttypid = 'uuid'::regtype);
    IF invalid_tables IS NOT NULL THEN
        RAISE EXCEPTION 'Missing single UUID primary key: %', invalid_tables;
    END IF;

    SELECT string_agg(expected.name, ', ' ORDER BY expected.name)
      INTO missing_names
      FROM (VALUES ('IX_AssistanceRequest_Reference'), ('IX_Beneficiary_Reference'),
                   ('IX_Outcome_Reference'), ('IX_Referral_Reference'),
                   ('IX_ReferralRecipient_Reference')) AS expected(name)
     WHERE NOT EXISTS (SELECT 1 FROM pg_indexes
                       WHERE schemaname = 'public' AND indexname = expected.name
                         AND indexdef LIKE 'CREATE UNIQUE INDEX%');
    IF missing_names IS NOT NULL THEN
        RAISE EXCEPTION 'Missing unique reference indexes: %', missing_names;
    END IF;
END $$;
