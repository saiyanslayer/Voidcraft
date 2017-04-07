using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using System.Text.RegularExpressions;
using System.Data;
using System.Web.Security;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace VoidCraftTest.Member
{
    public partial class Member : System.Web.UI.Page
    {
        //global values
        string DatabaseConnection = ConfigurationManager.ConnectionStrings["MembershipSiteConStr"].ConnectionString;
        Player user;
        DataTable workers;
        DataTable events;
        DataTable resources;
        DataTable recipes;

        public struct Player
        {
            private int userID;
            private int workerID;

            public int ID
            {
                get { return userID;  }
                set { userID = value; }
            }

            public int WorkerID
            {
                get { return workerID; }
                set { workerID = value; }
            }
        }

        public struct Resource
        {
            public string Name { get; private set; }
            public int Value { get; private set; }

            public Resource(string resource)
            {
                string[] text = resource.Split(':');

                Name = text[0];
                int i;
                bool result = int.TryParse(text[1], out i);
                Value = i;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //SQL Optimization
            GetSqlData();

            EvaluateEvents(events);

            //must occur after EvaluateEvents to accurately show worker situation
            string sql = String.Format("SELECT worker_name, idworkers, current_job, created_at, worker_type FROM workers w left JOIN events e on w.idworkers = e.worker WHERE w.worker_owner = '{0}'", user.ID);
            workers = GetDataTable(sql);

            EvaluateWorkers(workers);

            resources = GetResources(); //list of resources that user has

            EvaluateResources(resources);

            sql = String.Format("SELECT idrecipe, recipename, requiredresources, requiredtime, typecreated, typedetail FROM build");
            recipes = GetDataTable(sql);

            EvaluateBuild(recipes);
        }

        protected void GetSqlData()
        {
            MySqlConnection connection = null;
            DataTable dt = new DataTable();
            var u = User.Identity.Name;

            try
            {
                connection = new MySqlConnection(DatabaseConnection);

                //get the user id
                string sql = "SELECT idusers FROM users WHERE username = @user";
                MySqlCommand cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@user", u);

                connection.Open();

                var item = cmd.ExecuteScalar();

                int i = 0;
                bool result = int.TryParse(item.ToString(), out i);
                user.ID = i;

                sql = String.Format("SELECT idworkers FROM workers WHERE worker_owner = '{0}' AND worker_type = 'owner' LIMIT 1; ", user.ID);
                cmd = new MySqlCommand(sql, connection);
                item = cmd.ExecuteScalar();

                result = int.TryParse(item.ToString(), out i);
                
                user.WorkerID = i;

                //get list of events
                string getEvents = String.Format("SELECT * FROM events WHERE userid = {0}", user.ID);
                MySqlCommand getEventsCommand = new MySqlCommand(getEvents, connection);

                events = new DataTable();
                events.Load(getEventsCommand.ExecuteReader());
            }

            catch (Exception ex)
            {            }

            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
        }

        protected void EvaluateBuild(DataTable recipes)
        {
            foreach(DataRow recipe in recipes.Rows)
            {
                string id = recipe[0].ToString();
                string recipename = recipe[1].ToString();
                string[] resources = recipe[2].ToString().Split(';');
                int minutes = Convert.ToInt16(recipe[3].ToString());
                string type = recipe[4].ToString();
                string typeDetail = recipe[5].ToString();

                TableRow entry = new TableRow();

                for(int i = 0; i < recipe.ItemArray.Count(); i++)
                {
                    TableCell cell = new TableCell();
                    cell.Text = recipe[i].ToString();
                    entry.Cells.Add(cell);
                }

                //add the build button
                TableCell buttonCell = new TableCell();
                Button button = new Button();
                button.Text = "Build";
                button.Click += new System.EventHandler(SelectRecipeButton_Click);
                button.CommandName = recipename;
                buttonCell.Controls.Add(button);
                entry.Cells.Add(buttonCell);

                RecipeTable.Rows.Add(entry);
            }
        }

        //displays the workers belonging to the user
        protected void EvaluateWorkers(DataTable workers)
        {
            foreach (DataRow row in workers.Rows)
            {
                string workerName = row[0].ToString();
                int workerID = (int)row[1];
                int currentJob;
                bool workerIsFree = int.TryParse(row[2].ToString(), out currentJob);
                string workerTime = row[3].ToString();
                string workerType = row[4].ToString();

                HtmlGenericControl header = new HtmlGenericControl("h3");
                header.InnerHtml = workerName;
                WorkerPanel.Controls.Add(header);

                if (!workerIsFree)
                {
                    AddWorkerButtons(workerType, workerID);
                }
                else
                {
                    Label label = new Label();
                    DateTime time = DateTime.Parse(workerTime);
                    TimeSpan span = time.Subtract(DateTime.Now);
                    label.Text = TicksToString(span);
                    WorkerPanel.Controls.Add(label);
                }
                
            }

            

        }

        protected void AddWorkerButtons(string workerType, int workerID)
        {
            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ul.Attributes["class"] = "ListButtons";

            switch (workerType)
            {
                case "owner":
                    CreateResourceButton(ul, workerID, "CashButton", "Earn Cash", CashButton_Click);
                    CreateResourceButton(ul, workerID, "TrainButton", "Train Workers", TeamButton_Click);
                    break;

                case "security_team":
                    CreateResourceButton(ul, workerID, "EarnCashButton", "Security Work", CashButton_Click);
                    CreateResourceButton(ul, workerID, "ProtectButton", "Protect Assets", ProtectionButton_Click);
                    break;

                case "mining_team":
                    CreateResourceButton(ul, workerID, "EarnCashButton", "Sell Minerals", CashButton_Click);
                    CreateResourceButton(ul, workerID, "MineButton", "Mine Minerals", MiningButton_Click);
                    CreateResourceButton(ul, workerID, "TrainButton", "Train Staff", TrainMiningButton_Click);
                    break;

                default:
                    break;
            }
            WorkerPanel.Controls.Add(ul);
        }

        protected void CreateResourceButton(HtmlGenericControl ul, int workerID, string buttonID, string buttonText, EventHandler click)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            Button worker = new Button();
            worker.ID = workerID.ToString() + "_" + buttonID;
            worker.CommandName = workerID.ToString();
            worker.Text = buttonText;
            worker.Click += new System.EventHandler(click);
            worker.CssClass = "ResourceButtons";
            li.Controls.Add(worker);
            ul.Controls.Add(li);
        }

        protected DataTable GetDataTable(string sql)
        {
            DataTable value = new DataTable();

            MySqlConnection connection = null;

            try
            {
                connection = new MySqlConnection(DatabaseConnection);
                MySqlCommand cmd = new MySqlCommand(sql, connection);

                connection.Open();

                value.Load(cmd.ExecuteReader());

            }

            catch (Exception ex)
            {

            }

            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }

            return value;
        }

        //get all resources the user owns
        public DataTable GetResources()
        {
            DataTable value = new DataTable();
            int user = GetUserID();

            MySqlConnection connection = null;

            try
            {
                string sql = String.Format("SELECT * FROM resources WHERE iduser = '{0}' LIMIT 1;", user);
                connection = new MySqlConnection(DatabaseConnection);
                MySqlCommand cmd = new MySqlCommand(sql, connection);
                connection.Open();

                value.Load(cmd.ExecuteReader());
            }

            catch (Exception ex)
            {
            }

            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }

            return value;
        }

        protected void AddEvent(string event_type, string workerID, TimeSpan delay)
        {
            //checks if the request is valid for each specific type of event
            //if valid, it prepares the sql command
            string sql = "";
            string time = DateTime.Now.Add(delay).ToString("yyyy-MM-dd HH:mm:ss");
            
            switch (event_type)
            {
                case "More Cash":
                    sql = String.Format("INSERT INTO events (userid, event_type, event_info, created_at, worker) " +
                        "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')",
                        user.ID, "more_cash", "testing", time, workerID);
                    break;

                case "More Team":
                    sql = String.Format("INSERT INTO events (userid, event_type, event_info, created_at, worker) " +
                        "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')",
                        user.ID, "more_team", "testing", time, workerID);
                    break;

                default:
                    break;
            }

            //if request is valid
            bool success = EventSqlCommand(sql);

            if (success)
            {
                success = UpdateWorkers();
            }

        }

        protected object GetSqlScalar(string sql)
        {
            MySqlConnection connection = null;

            try
            {
                connection = new MySqlConnection(DatabaseConnection);
                MySqlCommand cmd = new MySqlCommand(sql, connection);

                connection.Open();

                var result = cmd.ExecuteScalar();

                if (result != null)
                {
                    return result;
                }


            }

            catch (Exception ex)
            {

            }

            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }

            return null;
        }

        protected bool UpdateWorkers()
        {
            bool result = false;
            
            string sql = "UPDATE workers w left JOIN events e on w.idworkers = e.worker SET current_job = e.idevents";
            result = EventSqlCommand(sql);

            return result;
        }

        protected void EvaluateEvents(DataTable events)
        {
            string sql = "";

            //get all pending events for the user
            foreach (DataRow row in events.Rows)
            {
                sql += ProcessEvent(row);
            }

            if (sql != "")
            {
                //update workers after all is done
                sql += "UPDATE workers w left JOIN events e on w.idworkers = e.worker SET current_job = e.idevents;";
                EventSqlCommand(sql);
            }

        }

        protected string ProcessEvent(DataRow row)
        {
            string sql = "";
            string actionType = row[2].ToString();

            string mysqlTime = row[4].ToString();
            DateTime time = DateTime.Parse(mysqlTime);

            if (time < DateTime.Now)
            {
                switch (actionType)
                {
                    case "more_cash":
                            sql = String.Format("UPDATE resources SET cash = cash + '{0}' WHERE iduser = '{1}'; ",
                                1000, user.ID);
                        break;

                    case "more_team":
                            sql = String.Format("Update resources SET teams = teams + {0} WHERE iduser = '{1}'; ",
                                1, user.ID);
                        break;

                    case "create":
                            sql = String.Format("INSERT INTO workers (worker_owner, worker_type, worker_name, worker_level) VALUES ('{0}', '{1}', '{2}', '0'); ",
                               user.ID, row[3].ToString(), User.Identity.Name.ToString() + " team");
                        break;

                    default:
                        break;
                }
                sql += String.Format("DELETE FROM events WHERE idevents = '{0}';", row[0]);
                return (sql);
            }

            else
            {
                TableRow entry = new TableRow();

                //event id
                TableCell cell1 = new TableCell();
                cell1.Text = row[0].ToString();
                entry.Cells.Add(cell1);

                //event type
                TableCell cell2 = new TableCell();
                cell2.Text = row[2].ToString();
                entry.Cells.Add(cell2);

                //event notes
                TableCell cell3 = new TableCell();
                cell3.Text = row[3].ToString();
                entry.Cells.Add(cell3);

                //event time
                TableCell cell4 = new TableCell();
                TimeSpan span = time.Subtract(DateTime.Now);
                cell4.Text = TicksToString(span);
                entry.Cells.Add(cell4);

                EventsTable.Rows.Add(entry);
            }
            return "";
        }

        protected void TestThing(string text)
        {
            TableRow test = new TableRow();
            TableCell cell = new TableCell();
            cell.Text = text;
            test.Cells.Add(cell);
            ResourceTable.Rows.Add(test);
        }

        protected void EvaluateResources(DataTable resources)
        {
            int numberOfColumns = resources.Columns.Count; //counts number of resources

            foreach (DataRow row in resources.Rows)
            {
                //row should represent the current user
                for (int i = 1; i < numberOfColumns; i++)
                {
                    if ((int)row[i] > 0)
                    {
                        TableRow entry = new TableRow();

                        //resource amount
                        TableCell cell1 = new TableCell();
                        cell1.Text = row[i].ToString();
                        entry.Cells.Add(cell1);

                        //Resource name
                        TableCell cell2 = new TableCell();
                        cell2.Text = resources.Columns[i].ColumnName.ToString();
                        entry.Cells.Add(cell2);

                        ResourceTable.Rows.Add(entry);
                    }
                }
            }
        }

        protected DataTable GetUserEvents(int user)
        {
            DataTable value = new DataTable();

            MySqlConnection connection = null;
            string sql = String.Format("SELECT * FROM events WHERE userid = {0}", user);

            try
            {
                connection = new MySqlConnection(DatabaseConnection);
                MySqlCommand cmd = new MySqlCommand(sql, connection);

                connection.Open();

                value.Load(cmd.ExecuteReader());

            }

            catch (Exception ex)
            {

            }

            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }

            return value;
        }

        protected bool EventSqlCommand(string sql)
        {
            bool value = false;
            MySqlConnection connection = null;

            try
            {
                connection = new MySqlConnection(DatabaseConnection);
                MySqlCommand cmd = new MySqlCommand(sql, connection);

                connection.Open();

                var result = cmd.ExecuteNonQuery();

                if (result != null)
                {
                    value = true;
                }
            }

            catch (Exception ex)
            {

            }

            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }

            return value;
        }

        protected int GetUserID()
        {
            var user = User.Identity.Name;
            int value = 0;

            MySqlConnection connection = null;

            try
            {
                connection = new MySqlConnection(DatabaseConnection);
                string sql = "SELECT idusers FROM users WHERE username = @user LIMIT 1;";
                MySqlCommand cmd = new MySqlCommand(sql, connection);

                cmd.Parameters.AddWithValue("@user", user);

                connection.Open();

                var result = cmd.ExecuteScalar();

                if (result != null)
                {
                    value = Convert.ToInt16(result);
                }
            }

            catch (Exception ex)
            {

            }

            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }

            return value;
        }

        protected bool IsWorkerFree(int workerID)
        {
            string sql = String.Format("SELECT current_job FROM workers WHERE idworkers = '{0}' ", workerID);
            object work =  GetSqlScalar(sql); //idworkers, current_job
            int currentJob;
            bool workerIsFree = int.TryParse(work.ToString(), out currentJob);

            return !workerIsFree;
        }

        protected bool IsOwnerFree(int user)
        {
            string sql = String.Format("SELECT idworkers, current_job FROM workers WHERE worker_owner = '{0}' AND worker_type = 'owner' LIMIT 1", user);
            DataTable work = GetDataTable(sql); //idworkers, current_job
            int currentJob;
            bool workerIsFree = int.TryParse(work.Rows[0][1].ToString(), out currentJob);

            return !workerIsFree;
        }

        protected string TicksToString(TimeSpan time)
        {
            string text = "";
            
            if (time.TotalDays >= 1)
            {
                text = time.Days.ToString() + " Days ";
            }

            if (time.TotalHours >= 1)
            {
                text += time.Hours.ToString() + " Hrs ";
            }

            if (time.TotalMinutes >= 1)
            {
                text += time.Minutes.ToString() + " Mins ";
            }

            if (time.TotalSeconds >= 1)
            {
                text += time.Seconds.ToString() + " Secs ";
            }
            return text;
        }

        protected bool BuildRecipe(string recipeName)
        {
            bool result = false;

            //exit if the owner is busy
            if (!IsOwnerFree(user.ID))
            {
                return false;
            }

            DataRow recipe = recipes.Select("recipename = '" + recipeName + "'").First();

            //check if recipe requirements met
            string text = recipe[2].ToString();
            string[] requirements = text.Split(';');

            //if resources are enough
            //foreach resource, subtract the resources
            if (RequiredResources(requirements))
            {
                string update = "";
                bool firstValue = true;

                foreach (string s in requirements)
                {
                    //create value to be subtracted
                    Resource r = new Resource(s);

                    if (firstValue)
                    {
                        firstValue = false;
                    }
                    else
                    {
                        update += ", ";
                    }

                    update += String.Format("{0} = {0} - '{1}'",
                             r.Name, r.Value);
                }

                //take resources, create worker, get new worker's id, assign it to building itself, 
                string sql = String.Format("UPDATE resources SET {0} WHERE iduser = '{1}';", 
                    update, user.ID);

                string workerType = recipe[5].ToString();

                sql += String.Format("INSERT INTO workers(worker_owner, worker_type, worker_name) VALUES('{0}', '{1}', '{2}');",
                    user.ID, workerType, User.Identity.Name + "s Team");

                TimeSpan delay = new TimeSpan(0, Convert.ToInt16(recipe[3].ToString()), 0);
                string time = DateTime.Now.Add(delay).ToString("yyyy-MM-dd HH:mm:ss");

                sql += String.Format("INSERT INTO events(userid, event_type, event_info, created_at, worker)" +
                    "VALUES('{0}', 'create', '{1}', '{2}', last_insert_id());",
                    user.ID, workerType, time);

                sql += "UPDATE workers w left JOIN events e on w.idworkers = e.worker SET current_job = e.idevents;";
                
                result = EventSqlCommand(sql);
            }

            return result;
        }

        protected bool RequiredResources(string[] requirements)
        {
            bool result = true;

            //determine if there are enough resources
            for (int i = 0; i < requirements.Count(); i++)
            {
                string[] cost = requirements[i].Split(':');
                if (cost.Count() > 1) // if there's an entry
                {
                    string resourceName = cost[0];
                    int resourceCost = int.Parse(cost[1]);
                    bool resourceGood = false;

                    for (int ii = 0; ii < resources.Columns.Count; ii++)
                    {
                        string columnName = resources.Columns[ii].ColumnName;
                        if (columnName == resourceName)
                        {
                            int resourceAmount = (int)resources.Rows[0][ii];
                            if (resourceAmount >= resourceCost)
                            {
                                resourceGood = true;
                                continue;
                            }
                        }
                    }

                    if (resourceGood)
                    {
                        continue;
                    }

                }

                result = false;
            }

            return result;
        }

        protected void CashButton_Click(object sender, EventArgs e)
        {
            Button worker = sender as Button;
            if (IsWorkerFree(Convert.ToInt16(worker.CommandName)))
            {
                TimeSpan delay = new TimeSpan(0, 20, 0);
                AddEvent("More Cash", worker.CommandName, delay);
                Response.Redirect("~/Members/Member.aspx");
            }
        }

        protected void TeamButton_Click(object sender, EventArgs e)
        {
            Button worker = sender as Button;
            TimeSpan delay = new TimeSpan(1, 0, 0);
            AddEvent("More Team", worker.CommandName, delay);
            Response.Redirect("~/Members/Member.aspx");
        }

        protected void WorkerButton_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;
            string id = b.UniqueID;
        }

        protected void ProtectionButton_Click(object sender, EventArgs e)
        {

        }

        protected void MiningButton_Click(Object sender, EventArgs e)
        {

        }

        protected void TrainMiningButton_Click(Object sender, EventArgs e)
        {

        }

        protected void BuildRecipeButton_Click(Object sender, EventArgs e)
        {
            Button worker = sender as Button;
            string recipeName = worker.CommandName;

            if (recipeName != "")
            {
                BuildRecipe(recipeName);
            }

            Response.Redirect(Request.RawUrl);
        }

        protected void SelectRecipeButton_Click(Object sender, EventArgs e)
        {
            Button button = sender as Button;
            string command = button.CommandName.ToString();
            string arg = button.CommandArgument.ToString();

            BuildPopUpLabel.Text = "You pressed the button to " + command + " for " + arg;

            BuildButton.CommandName = command;
            BuildButton.CommandArgument = arg;

            BuildPopUp.Show();
        }
    }
}