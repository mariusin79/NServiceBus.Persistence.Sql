DO $$
DECLARE tableName text; sqlStatement text; tablePrefix text;
BEGIN
	tableName = right(concat(@tablePrefix, 'SubscriptionData'), 63);
    sqlStatement = concat('drop table if exists ', tablename);
    execute sqlStatement;
END;
$$;