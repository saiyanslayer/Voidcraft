using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Text.RegularExpressions;
using System.Web.Security;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace VoidCraftTest
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void LoginControl_Authenticate(object sender, AuthenticateEventArgs e)
        {
            bool authenticated = this.ValidateCredentials(LoginControl.UserName, LoginControl.Password);

            if (authenticated)
            {
                FormsAuthentication.RedirectFromLoginPage(LoginControl.UserName, LoginControl.RememberMeSet);
            }
        }

        public bool IsAlphaNumeric(string text)
        {
            return Regex.IsMatch(text, "^[a-zA-Z0-9]+$");
        }

        private bool ValidateCredentials(string userName, string password)
        {
            bool returnValue = false;

            if (this.IsAlphaNumeric(userName) && userName.Length < 50 && this.IsAlphaNumeric(password) && password.Length < 50)
            {
                MySqlConnection connection = null;

                try
                {
                    string sql = "select count(*) from users where username = @username and password = @password";

                    connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["MembershipSiteConStr"].ConnectionString);
                    MySqlCommand cmd = new MySqlCommand(sql, connection);

                    MySqlParameter user = new MySqlParameter();
                    user.ParameterName = "@username";
                    user.Value = userName.Trim();
                    cmd.Parameters.Add(user);

                    MySqlParameter pass = new MySqlParameter();
                    pass.ParameterName = "@password";
                    pass.Value = Hasher.HashString(password.Trim());
                    cmd.Parameters.Add(pass);

                    connection.Open();

                    int count = Convert.ToInt16(cmd.ExecuteScalar());

                    if (count > 0)
                    {
                        returnValue = true;
                    }


                }

                catch(Exception ex)
                {
                    
                }

                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
            }
            else
            {

            }

            return returnValue;
        }
    }
}