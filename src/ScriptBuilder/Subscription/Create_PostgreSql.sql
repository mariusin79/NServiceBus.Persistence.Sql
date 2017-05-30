﻿DO $$
DECLARE tableName text; sqlStatement text; tablePrefix text;
BEGIN
	tableName = right(concat(@tablePrefix, 'SubscriptionData'), 63);

	sqlStatement =  concat('
		create table if not exists ', tableName, '(
			Subscriber varchar(200) not null,
			Endpoint varchar(200),
			MessageType varchar(200) not null,
			PersistenceVersion varchar(23) not null,
			primary key (Subscriber, MessageType)
		)
	');
    execute sqlStatement;
END;
$$;