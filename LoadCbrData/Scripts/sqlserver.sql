create table [dbo].[record_id]
(
	[id]			bigint			not null,
	constraint [pk_record_id] primary key ([id])
)
go

create table [dbo].[record]
(
	[id]			bigint			not null,
	[code]			varchar(32)		null,
	[inn]			varchar(32)		null,
	[ogrn]			varchar(32)		null,
	[name]			nvarchar(max)	null,
	[short_name]	nvarchar(max)	null,
	[data]			nvarchar(max)	null,
	[modified_date] datetime2(7)	null,
	constraint [pk_record] primary key ([id])
)
go
