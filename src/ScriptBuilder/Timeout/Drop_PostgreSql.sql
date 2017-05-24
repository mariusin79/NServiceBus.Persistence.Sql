DO $$
DECLARE tableName text; sqlStatement text; tablePrefix text;
BEGIN
    tableName = concat(@tablePrefix, 'TimeoutData');
    sqlStatement = concat('drop table if exists ', tablename);
    execute sqlStatement;
END;
$$;


