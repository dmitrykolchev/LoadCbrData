create table record_id
(
	id			bigint			not null,
	constraint pk_record_id primary key (id)
);


create table record
(
	id				bigint			not null,
	code			varchar(32)		null,
	inn				varchar(32)		null,
	ogrn			varchar(32)		null,
	name			varchar(512)	null,
	short_name		varchar(512)	null,
	data			text			null,
	modified_date	timestamp		null,
	constraint pk_record primary key (id)
);

