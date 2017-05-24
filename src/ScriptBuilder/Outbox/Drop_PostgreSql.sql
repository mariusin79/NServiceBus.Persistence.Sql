DO $$
DECLARE tableName text; sqlStatement text; tablePrefix text;
BEGIN
    tableName = concat(@tablePrefix, 'OutboxData');
    sqlStatement = concat('drop table if exists ', tablename);
    execute sqlStatement;
END;
$$;