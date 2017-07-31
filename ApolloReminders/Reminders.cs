using NCrontab;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using bcd;

namespace ApolloReminders
{
    public class Reminders
    {
        public string ConStr { get; set; }

        public Reminders()
        {
            getConnectionString();
        }

        internal DataTable GetReminders()
        {
            // get today's reminders
            var dt = new DataTable("ReminderRule");
            using (var con = new SqlConnection(ConStr))
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM [dbo].[ReminderRule] WHERE IsActive = 1 AND IsDeleted = 0";
                    con.Open();
                    var dr = cmd.ExecuteReader();
                    //
                    dt.Columns.Add(new DataColumn("RuleId", typeof(int)));
                    dt.Columns.Add(new DataColumn("ReminderName", typeof(string)));
                    dt.Columns.Add(new DataColumn("BindingProcedure", typeof(string)));
                    dt.Columns.Add(new DataColumn("Threshold", typeof(string)));
                    dt.Columns.Add(new DataColumn("ThresholdNote", typeof(string)));
                    dt.Columns.Add(new DataColumn("RunType", typeof(int)));
                    dt.Columns.Add(new DataColumn("RunCount", typeof(int)));
                    dt.Columns.Add(new DataColumn("ExpiryAction", typeof(int)));
                    dt.Columns.Add(new DataColumn("IntervalType", typeof(int)));
                    dt.Columns.Add(new DataColumn("IntervalValue", typeof(string)));
                    dt.Columns.Add(new DataColumn("ReminderPriority", typeof(int)));
                    dt.Columns.Add(new DataColumn("ReminderRunDate", typeof(DateTime)));
                    //
                    while (dr.Read())
                    {
                        // find out next run date based on interval value (cron tab)
                        //Console.WriteLine($"{dr[1].ToString()} - {dr["IntervalValue"].ToString()}");
                        var cron = dr["IntervalValue"].ToString();
                        var s = CrontabSchedule.Parse(cron);
                        var start = DateTime.Now.Date.AddDays(-1).AddHours(8);
                        var occurence = s.GetNextOccurrence(start);
                        if (occurence.Date == DateTime.Now.Date)
                        {
                            // GOOD TO GO
                            var row = dt.NewRow();
                            row["RuleId"] = int.Parse(dr["ReminderRuleId"].ToString());
                            row["ReminderName"] = dr["ReminderName"].ToString();
                            row["BindingProcedure"] = dr["BindingProcedure"].ToString();
                            row["Threshold"] = dr["Threshold"].ToString();
                            row["ThresholdNote"] = ParseThreshold(dr["Threshold"].ToString());
                            row["RunType"] = int.Parse(dr["RunType"].ToString());
                            row["RunCount"] = int.Parse(dr["RunCount"].ToString());
                            row["ExpiryAction"] = int.Parse(dr["ExpiryAction"].ToString());
                            row["IntervalType"] = int.Parse(dr["IntervalType"].ToString());
                            row["IntervalValue"] = dr["IntervalValue"].ToString();
                            row["ReminderPriority"] = int.Parse(dr["ReminderPriority"].ToString());
                            row["ReminderRunDate"] = occurence;
                            dt.Rows.Add(row);
                        }
                    }
                }
            }
            //
            return dt;
        }

        internal long SendMail(DataRow iRow, int ruleId)
        {
            long dataId = -1;
            // get template
            var tpl = GetTemplate(ruleId);
            // get template values
            Hashtable tplVals = new Hashtable();
            foreach (DataColumn c in iRow.Table.Columns)
            {
                tplVals.Add(c.ToString(), iRow[c].ToString());
            }
            // parse template
            var subject = ParseBlock(tpl.TemplateSubject, tplVals);
            var body = ParseBlock(tpl.TemplateBody, tplVals);
            var toName = string.Empty;
            var toEmail = string.Empty;
            var ccName = string.Empty;
            var ccEmail = string.Empty;

            if (tpl.SentTo.Contains(","))
            {
                var al = tpl.SentTo.Split(',');
                foreach(string ai in al)
                {
                    toName += GetEmail("NAME", (RecipientType)int.Parse(tpl.SentTo), tplVals);
                    toEmail += GetEmail("EMAIL", (RecipientType)int.Parse(tpl.SentTo), tplVals);
                }
            }
            else
            {
                toName = GetEmail("NAME", (RecipientType)int.Parse(tpl.SentTo), tplVals);
                toEmail = GetEmail("EMAIL", (RecipientType)int.Parse(tpl.SentTo), tplVals);
            }

            if (tpl.CopyTo.Contains(","))
            {
                var al = tpl.SentTo.Split(',');
                foreach (string ai in al)
                {
                    ccName += GetEmail("NAME", (RecipientType)int.Parse(tpl.CopyTo), tplVals);
                    ccEmail += GetEmail("EMAIL", (RecipientType)int.Parse(tpl.CopyTo), tplVals);
                }
            }
            else
            {
                ccName = GetEmail("NAME", (RecipientType)int.Parse(tpl.CopyTo), tplVals);
                ccEmail = GetEmail("EMAIL", (RecipientType)int.Parse(tpl.CopyTo), tplVals);
            }
            //
            var sentStatus = new MailSender().SendReminder(toName, toEmail, ccName, ccEmail, subject, body); // from send mail result (2: queue)

            var qry = $"INSERT INTO [dbo].[ReminderData] VALUES (" +
                $" '{DateTime.Parse(iRow["reminder_date"].ToString()).ToString("yyyy-MM-dd")}'," +
                $" '{subject}'," +
                $" '{body}'," +
                $" '{iRow["requester_email"].ToString()}'," +
                $" '{iRow["workflow_admin_email"].ToString()}'," +
                $" 2," +
                $" 4," +
                $" 1," +
                $" {int.Parse(iRow["request_id"].ToString())}," +
                $" '{iRow["awid"].ToString()}'," +
                $" {sentStatus}," +
                $" '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}'," +
                $" 0) SET @ID = SCOPE_IDENTITY();";
            // save data
            try
            {
                using (var con = new SqlConnection(ConStr))
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = qry;
                        SqlParameter param = new SqlParameter("@ID", SqlDbType.Int, 4);
                        param.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(param);
                        con.Open();
                        var x = cmd.ExecuteNonQuery();
                        if (x > 0)
                        {
                            dataId = long.Parse(param.Value.ToString());
                            return dataId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var x = ex.Message;
            }
            //
            return dataId;
        }

        private string GetEmail(string nameEmail, RecipientType recipient, Hashtable tplVals)
        {
            var recipientText = getRecipientText(recipient);
            if (nameEmail == "NAME")
            {
                recipientText = recipientText + "_name";
            }
            else if (nameEmail == "EMAIL")
            {
                recipientText = recipientText + "_email";
            }
            return tplVals[recipientText].ToString();
        }

        private string getRecipientText(RecipientType recipient)
        {
            switch (recipient)
            {
                case RecipientType.Requester:
                    return "requester";
                case RecipientType.PreviousApprover:
                    return "previous_approver";
                case RecipientType.CurrentApprover:
                    return "current_approver";
                case RecipientType.PreviousFollowUpUser:
                    return "previous_followup_user";
                case RecipientType.CurrentFollowUpUser:
                    return "current_followup_user";
                case RecipientType.NotifyUser:
                    return "notify_user";
                case RecipientType.Supplier:
                    return "supplier";
                case RecipientType.Manager:
                    return "manager";
                case RecipientType.WorkflowAdmin:
                    return "workflow_admin";
                case RecipientType.SiteAdmin:
                    return "site_admin";
                case RecipientType.ToEmail:
                    return "to_email";
                case RecipientType.CcEmail:
                    return "cc_email";
                case RecipientType.Delegator:
                    return "delegator";
                case RecipientType.PreviousDelegatee:
                    return "previous_delegatee";
                case RecipientType.CurrentDelegatee:
                    return "current_delegatee";
                default:
                    return "email";
            }
        }

        private ReminderTemplate GetTemplate(int ruleId)
        {
            var tpl = new ReminderTemplate();
            using (var con = new SqlConnection(ConStr))
            {
                using (var cmd = con.CreateCommand())
                {
                    // TODO currently defaults to english language
                    var qry = $"SELECT * FROM [dbo].[ReminderTemplate] WHERE RuleId = {ruleId} AND LangCode = 'en-US'";
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = qry;
                    con.Open();

                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        tpl.TemplateId = int.Parse(reader["TemplateId"].ToString());
                        tpl.TemplateName = reader["TemplateName"].ToString();
                        tpl.TemplateSubject = reader["TemplateSubject"].ToString();
                        tpl.TemplateBody = reader["TemplateBody"].ToString();
                        tpl.TemplateVars = reader["TemplateVars"].ToString();
                        tpl.SentTo = reader["SentTo"].ToString();
                        tpl.CopyTo = reader["CopyTo"].ToString();
                        tpl.AWID = reader["WorkflowId"].ToString();
                        tpl.StepId = int.Parse(reader["StepId"].ToString());
                        tpl.RuleId = int.Parse(reader["RuleId"].ToString());
                        tpl.LangCode = reader["LangCode"].ToString();
                    }

                }
            }
            return tpl;
        }

        internal DataTable GetInstances(DataRow row)
        {
            var dt = new DataTable("AllInstances");
            var dtNew = new DataTable("ReminderInstances");
            //
            using (var con = new SqlConnection(ConStr))
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = row["BindingProcedure"].ToString().Trim();
                    cmd.Parameters.Add("THRESHOLD", SqlDbType.VarChar).Value = row["Threshold"].ToString();
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    //
                    dt.Load(reader);
                    dtNew = dt.Copy();
                    dtNew.Rows.Clear();
                    //
                    foreach (DataRow dr in dt.Rows)
                    {
                        // remove rows that have already been addressed or expired
                        if (int.Parse(row["RunCount"].ToString()) > int.Parse(dr["run_count"].ToString()))
                        {
                            // add this row
                            dtNew.Rows.Add(dr.ItemArray);
                        }
                    }
                }
            }
            //
            return dtNew;
        }

        private string ParseThreshold(string threshold)
        {
            var x = threshold.Split(' ');
            string y = string.Empty;
            switch (x[1])
            {
                case "D":
                    y = "Day(s)";
                    break;
                case "W":
                    y = "Week(s)";
                    break;
                case "M":
                    y = "Month(s)";
                    break;
            }
            //
            if (x[0].Substring(0, 1) == "-")
            {
                // before
                return $"Before {x[0].Substring(1)} {y}";
            }
            else
            {
                // after
                return $"After {x[0]} {y}";
            }
        }

        private void getConnectionString()
        {
            var appSettings = ConfigurationManager.AppSettings;
            if (appSettings.Count > 0)
            {
                var conStrName = appSettings["ConStr"];
                ConStr = ConfigurationManager.ConnectionStrings[conStrName].ToString();
            }
        }

        private string ParseBlock(string block, Hashtable templateVals)
        {
            var mystic = new Mystic(templateVals);
            mystic.TemplateBlock = block;
            var parsedBlock = mystic.Parse();
            return parsedBlock;
        }
    }

    public class ReminderTemplate
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string TemplateSubject { get; set; }
        public string TemplateBody { get; set; }
        public string TemplateVars { get; set; }
        public string SentTo { get; set; }
        public string CopyTo { get; set; }
        public string AWID { get; set; }
        public int StepId { get; set; }
        public int RuleId { get; set; }
        public string LangCode { get; set; }
    }

    public enum RecipientType
    {
        Requester = 1,
        PreviousApprover = 2,
        CurrentApprover = 3,
        PreviousFollowUpUser = 4,
        CurrentFollowUpUser = 5,
        NotifyUser = 6,
        Supplier = 7,
        Manager = 8,
        WorkflowAdmin = 9,
        SiteAdmin = 10,
        ToEmail = 95,
        CcEmail = 96,
        Delegator = 97,
        PreviousDelegatee = 98,
        CurrentDelegatee = 99,
        Email = 0
    }
}
