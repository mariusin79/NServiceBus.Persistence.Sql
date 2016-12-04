﻿declare @tableName nvarchar(max) = '[' + @schema + '].[' + @endpointName + 'SubscriptionData]';
if not exists
(
    select *
    from sys.objects
    where
        object_id = object_id(@tableName) and
        type in (N'U')
)
begin
declare @createTable nvarchar(max);
set @createTable = N'
    create table ' + @tableName + '(
        [Subscriber] [varchar](450) not null,
        [Endpoint] [varchar](450) null,
        [MessageType] [varchar](450) not null,
        [PersistenceVersion] [nvarchar](23) not null,
        primary key clustered
        (
            [Subscriber] asc,
            [MessageType] asc
        )
    )
';
exec(@createTable);
end