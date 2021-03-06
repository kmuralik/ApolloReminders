/****** Object:  StoredProcedure [dbo].[spReminder_AWJP0038_LicenseValidity]    Script Date: 31-07-2017 18:45:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[spReminder_AWJP0038_LicenseValidity]
	@THRESHOLD VARCHAR(6)
AS
-- =============================================
-- Author:		Murali Kunapareddy
-- Create date: 2017-07-20
-- Description:	Prepares required data for Driver Registration - License Validity Reminder
--				takes Threshold as parameter to calculate reminder date
-- Usage:		EXEC spReminder_AWJP0038_LicenseValidity '-1 M'
-- =============================================
	BEGIN
		SET NOCOUNT ON;
		-- PREPARE THRESHOLD DATA
		DECLARE @THRESHOLD_UNIT CHAR(1);
		DECLARE @THRESHOLD_VAL INT;
		DECLARE @POS INT;
		--
		SELECT @POS = CHARINDEX(' ', @THRESHOLD);
		SET @THRESHOLD_VAL = CAST(SUBSTRING(@THRESHOLD, 0, @POS) AS INT);
		SET @THRESHOLD_UNIT = SUBSTRING(@THRESHOLD, @POS+1, 1);
		--
		--PRINT 'THRESHOLD : '+@THRESHOLD;
		--PRINT 'POS : '+CAST(@POS AS VARCHAR(MAX));
		--PRINT 'THRESHOLD VAL : '+CAST(@THRESHOLD_VAL AS VARCHAR(MAX));
		--PRINT 'THRESHOLD UNIT : '+@THRESHOLD_UNIT;
		--
		WITH rd (RunCount, ReminderDate, InstanceId)
			AS (SELECT COUNT(ReminderID), MAX(ReminderDate), InstanceID FROM [dbo].[ReminderData] GROUP BY InstanceID)
		SELECT 
				wmd.AWID AS [awid]
				, wmd.[WorkflowName] AS [workflow_name]
				, wif.[WFInstanceId] AS [request_id]
				, wi.[Requestnumber] AS [request_no]
				, wi.[WFInstancename] AS [request_title]
				, wi.[Requestedby] AS [requester_sesa]
				, CONCAT(umr.[FirstName],' ',umr.[LastName]) AS [requester_name]
				, umr.[Email] AS [requester_email]
				, wm.[OwnerSESA] AS [workflow_admin_sesa]
				, CONCAT(umo.[FirstName],' ',umo.[LastName]) AS [workflow_admin_name]
				, umo.[Email] AS [workflow_admin_email]
				, wif.[XMLValue].value('(/Field/Value/node())[1]', 'nvarchar(max)') AS [validity_date]
				, CASE 
					WHEN @THRESHOLD_UNIT = 'M' THEN 
						DATEADD(M, CAST(@THRESHOLD_VAL AS INT), wif.[XMLValue].value('(/Field/Value/node())[1]', 'nvarchar(max)'))
					WHEN @THRESHOLD_UNIT = 'W' THEN 
						DATEADD(W, CAST(@THRESHOLD_VAL AS INT), wif.[XMLValue].value('(/Field/Value/node())[1]', 'nvarchar(max)'))
					WHEN @THRESHOLD_UNIT = 'D' THEN
						DATEADD(D, CAST(@THRESHOLD_VAL AS INT), wif.[XMLValue].value('(/Field/Value/node())[1]', 'nvarchar(max)'))
					END AS [reminder_date]
				, ISNULL(rd.[RunCount], 0) AS [run_count]
				, ISNULL(rd.[ReminderDate], 0) AS [last_reminder_date]
				--, COUNT(rd.[ReminderID]) AS [RunCount]
		FROM [dbo].[WFInstanceField] wif
				LEFT JOIN [dbo].[WFInstance] wi ON (wif.[WFInstanceId] = wi.[WFInstanceId])
				LEFT JOIN [dbo].[UserMaster] umr ON (wi.[Requestedby] = umr.[UserSESA])
				LEFT JOIN [dbo].[WorkflowMasterDetail] wmd ON (wi.[WFMasterId] = wmd.[AWID])
				LEFT JOIN [dbo].[WorkflowMaster] wm ON (wi.[WFMasterId] = wm.[AWID])
				LEFT JOIN [dbo].[UserMaster] umo ON (wm.[OwnerSESA] = umo.[UserSESA])
				LEFT JOIN rd ON (wi.[WFInstanceId] = rd.[InstanceId])
		WHERE [WFFieldId] = (SELECT [WFFieldId] FROM [dbo].[WFField]
						WHERE [WFMasterId] = 'AWJP0038' AND [WFFieldname] = 'LicenseValidityDate')
				AND wi.[WFInstanceStatus] = 'Completed'
				AND wmd.[LangCode] = 'en-US'
				AND CASE 
					WHEN @THRESHOLD_UNIT = 'M' THEN 
						DATEADD(M, @THRESHOLD_VAL, wif.[XMLValue].value('(/Field/Value/node())[1]', 'nvarchar(max)'))
					WHEN @THRESHOLD_UNIT = 'W' THEN 
						DATEADD(W, @THRESHOLD_VAL, wif.[XMLValue].value('(/Field/Value/node())[1]', 'nvarchar(max)'))
					WHEN @THRESHOLD_UNIT = 'D' THEN
						DATEADD(D, @THRESHOLD_VAL, wif.[XMLValue].value('(/Field/Value/node())[1]', 'nvarchar(max)'))
					END <= GETDATE();

END
