using NCrontab;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

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

        internal bool SendMail(DataRow iRow)
        {
            var status = false;
            // save record to ReminderData
            // TODO get associated template
            // TODO parse template
            // TODO send mail routine

            var subject = "License about to expire on " + iRow["ValidityDate"].ToString().Substring(0, 10);
            var body = "blah blah blah blah";
            var sentStatus = 2; // from send mail result (2: queue)

            var qry = $"INSERT INTO [dbo].[ReminderData] VALUES (" +
                $" '{DateTime.Parse(iRow["ReminderDate"].ToString()).ToString("yyyy-MM-dd")}'," +
                $" '{subject}'," +
                $" '{body}'," +
                $" '{iRow["RequesterEmail"].ToString()}'," +
                $" '{iRow["OwnerEmail"].ToString()}'," +
                $" 2," +
                $" 4," +
                $" 1," +
                $" {int.Parse(iRow["RequestId"].ToString())}," +
                $" '{iRow["AWID"].ToString()}'," +
                $" {sentStatus}," +
                $" '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}'," +
                $" 0)";
            // save data
            using (var con = new SqlConnection(ConStr))
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = qry;
                    con.Open();
                    var x = cmd.ExecuteNonQuery();
                    if (x > 0) status = true;
                }
            }
            //
            return status;
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
                        if (int.Parse(row["RunCount"].ToString()) > int.Parse(dr["RunCount"].ToString()))
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

        private object ParseThreshold(string threshold)
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
    }
}
