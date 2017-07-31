select * from ReminderRule
select * from ReminderTemplate
--
insert into ReminderRule
	values(
		'AWJP0038-DL-Before-1M', 1, 'spReminder_AWJP0038_LicenseValidity',
		'-1 M', 1, 1, 0, 1, '0 8 * * 1-5',2, 1, 0, 'SESA432166', GETDATE(), 'SESA432166',GETDATE()
	);

insert into ReminderRule
	values(
		'AWJP0038-DL-Before-1W', 1, 'spReminder_AWJP0038_LicenseValidity',
		'-1 W', 1, 1, 0, 1, '0 8 * * 1-5',2, 1, 0, 'SESA432166', GETDATE(), 'SESA432166',GETDATE()
	);
--
insert into ReminderTemplate 
	values(
		'AWJP0038-DriverLicense',
		'{{workflow_name}} is about to expire on {{validity_date}}',
		'{{request_title}} of {{workflow_name}} is about to expire on {{validity_date}}. Please take necessary action.',
		'request_no, request_title, requester_name, requester_email, wfowner_name, wfowner_email, wfname',
		'1','9', 'AWJP0038',0,1,'en-US',1,0,'SESA432166',GETDATE(),'SESA432166',GETDATE()
	);

