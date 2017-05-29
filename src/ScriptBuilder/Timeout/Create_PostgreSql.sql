DO $$
DECLARE tableName text; sqlStatement text; tablePrefix text;
BEGIN
	tableName = right(concat(@tablePrefix, 'TimeoutData'), 63);

	sqlStatement =  concat('
		create table if not exists ', tableName, '(
			Id uuid not null,
			Destination varchar(200),
			SagaId uuid,
			State bytea,
			Time timestamp(3),
			Headers jsonb not null,
			PersistenceVersion varchar(23) not null,
			primary key (Id)
		)
	');
    execute sqlStatement;

	sqlStatement = concat('create index if not exists Index_SagaId on ', tableName, '(SagaId)');
	execute sqlStatement;

	sqlStatement = concat('create index if not exists Index_Time on ', tableName, '(Time)');
	execute sqlStatement;

END;
$$;