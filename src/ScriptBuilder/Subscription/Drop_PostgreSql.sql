DO $$
DECLARE tableName text; sqlStatement text; tablePrefix text;
BEGIN
    tableName = concat(@tablePrefix, 'SubscriptionData');
    sqlStatement = concat('drop table if exists ', tablename);
    execute sqlStatement;
END;
$$;