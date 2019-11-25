--[beginscript]

create table CK.tUserFacebook
(
	UserId int not null,
	-- The Facebook account identifier is the key to identify a Facebook user.
	FacebookAccountId varchar(36) collate Latin1_General_100_BIN2 not null,
	LastLoginTime datetime2(2) not null,
	constraint PK_CK_UserFacebook primary key (UserId),
	constraint FK_CK_UserFacebook_UserId foreign key (UserId) references CK.tUser(UserId),
	constraint UK_CK_UserFacebook_FacebookAccountId unique( FacebookAccountId )
);

insert into CK.tUserFacebook( UserId, FacebookAccountId, LastLoginTime ) 
	values( 0, '', sysutcdatetime() );

--[endscript]
