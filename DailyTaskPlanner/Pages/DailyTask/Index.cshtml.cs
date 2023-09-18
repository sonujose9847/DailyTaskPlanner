using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DailyTaskPlanner.Pages.DailyTask
{
    public class IndexModel : PageModel
    {
        #region "Variables"

        public List<TaskInfo> taskInfos = new List<TaskInfo>();
        public TaskInfo tskInfo = new TaskInfo();
        public String crntDate;
        public String newDate;
        public int tskCount = 0;
        public string errmsg = "";
        public string scsmsg = "";
        public String conString = "Data Source=.\\sqlexpress;Initial Catalog=DailyTask;Integrated Security=True";

        #endregion

        #region "Methods"

        public void OnGet()
        {
            try
            {

                using (SqlConnection connection = new SqlConnection(conString))
                {
                     if (newDate is not null) 
                    {
                        crntDate = newDate;
                    }
                    else
                    {
                        crntDate = DateTime.Now.ToString("yyyy-MM-dd");
                    }
                    SqlCommand cmd = new SqlCommand();
                    connection.Open();
                    cmd.Connection = connection;
                    cmd.CommandText = "GetTasks";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 50000;
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TaskInfo taskInfo = new TaskInfo();
                            taskInfo.TaskId = "" + reader.GetInt32(0);
                            taskInfo.Task = "" + reader.GetString(1);
                            taskInfo.TargetDate = "" + reader.GetDateTime(2).ToString("yyyy-MM-dd");
                            taskInfo.TaskOrder = "" + reader.GetInt32(3);
                            taskInfo.Status = reader.GetBoolean(4);

                            taskInfos.Add(taskInfo);
                            tskCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.ToString());
            }
        }

        public void OnPost()
        {
            try
            {

                List<TaskInfo> viewTaskInfos = new List<TaskInfo>();
               
                for (int i = 0; i < 5; i++)
                {

                    if (Request.Form["txtTask" + i].ToString().Trim() != "")
                    {
                        TaskInfo tskInfo1 = new TaskInfo();
                        tskInfo1.TaskId = Request.Form["taskId@(i)" + i];
                        tskInfo1.TaskOrder = Request.Form["tskOrder" + i];
                        tskInfo1.Task = (Request.Form["txtTask" + i]).ToString().Trim();
                        if (Request.Form["chkTask" + i].ToString() == "")
                        {
                            tskInfo1.Status = false;
                        }
                        else
                        {
                            tskInfo1.Status = true;
                        }
                        tskInfo1.TargetDate = Request.Form["tskDate"];
                        viewTaskInfos.Add(tskInfo1);
                    }
                }

                DataTable dt = new DataTable();

                if (viewTaskInfos.Count > 0)
                {
                    string json = JsonConvert.SerializeObject(viewTaskInfos);
                    DataTable pDt = JsonConvert.DeserializeObject<DataTable>(json);
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        using (SqlCommand command = new SqlCommand("spBulkImportTasks", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            var parameter = command.Parameters.AddWithValue("TaskTableType", pDt);
                            parameter.SqlDbType = SqlDbType.Structured;
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();
                            scsmsg = "Saved Successfully";
                        }

                    }
                    }
                
                else
                {
                    errmsg = "Nothing to save";
                }
                newDate = Request.Form["tskDate"];
                crntDate = newDate;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.ToString());
            }
        }


        public void OnGetFindTask(string taskdate)
        {
            try
            {
                newDate = taskdate;
                crntDate = newDate;
                //OnGet();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
        }
        #endregion
    }

    #region "TASK"
    public class TaskInfo
    {
        public string TaskId { get; set; }
        public string Task { get; set; }
        public string TargetDate { get; set; }
        public string TaskOrder { get; set; }
        public bool Status { get; set; }
    }

    #endregion
}
