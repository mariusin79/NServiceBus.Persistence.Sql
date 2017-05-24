DO $$
DECLARE tableName text; sqlStatement text;
BEGIN
    tableName = concat(@tablePrefix, 'OutboxData');

	sqlStatement =  concat('
		create table if not exists ', tableName, '(
			MessageId varchar(200) not null,
			Dispatched BOOLEAN not null default false,
			DispatchedAt timestamp(3),
			PersistenceVersion varchar(23) not null,
			Operations jsonb not null,
			primary key (MessageId)
		)
	');
    execute sqlStatement;

	sqlStatement = concat('create index if not exists Index_DispatchedAt on ', tableName, '(DispatchedAt)');
	execute sqlStatement;

	sqlStatement = concat('create index if not exists Index_Dispatched on ', tableName, '(Dispatched)');
	execute sqlStatement;

END;
$$;