using MathNet.Numerics;
using Microsoft.VisualBasic.ApplicationServices;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WSPR_Solar
{

    public partial class SolarDB : Form
    {
        public struct SolarIndexes
        {
            public string A;
            public string K00;
            public string K03;
            public string K06;
            public string K09;
            public string K12;
            public string K15;
            public string K18;
            public string K21;
            public string flux;
            public string SSN;
        }

        string[] cells = new string[12];


        string server;
        string user;
        string pass;

        public SolarIndexes Sol = new SolarIndexes();
        public SolarDB()
        {
            InitializeComponent();
        }

        private void SolarDB_Load(object sender, EventArgs e)
        {

        }

        public void setConfig(string serverName, string db_user, string db_pass)
        {
            server = serverName;
            user = db_user;
            pass = db_pass;
        }

        public async Task Save_SolarDB(string serverName, string db_user, string db_pass, DateTime date)
        {

            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_sol";
            MySqlConnection connection = new MySqlConnection(myConnectionString);

            date = date.Date;   //set h,m,s to 000
            try
            {

                MySqlCommand command = connection.CreateCommand();


                command.CommandText = "INSERT IGNORE INTO weather(datetime,flux,SSN) ";
                command.CommandText += "VALUES(@datetime,@flux,@SSN)";
                //command.CommandText += " ON DUPLICATE KEY UPDATE flux = '" + Sol.flux + "', SSN = '" + Sol.SSN + "'";               

                connection.Open();

                command.Parameters.AddWithValue("@datetime", date);
                command.Parameters.AddWithValue("@flux", Sol.flux);
                command.Parameters.AddWithValue("@SSN", Sol.SSN);


                command.ExecuteNonQuery();


                connection.Close();

            }
            catch
            {         //if row already exists then try updating it in database

            }


        }

        public async Task Save_GeoDB(string serverName, string db_user, string db_pass, DateTime date)
        {

            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_sol";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            date = date.Date;   //set h,m,s to 000

            try
            {

                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT IGNORE INTO weather(datetime,planA,Kp00,Kp03,Kp06,Kp09,Kp12,Kp15,Kp18,Kp21)";

                command.CommandText += " VALUES(@datetime,@planA,@Kp00,@Kp03,@Kp06,@Kp09,@Kp12,@Kp15,@Kp18,@Kp21)";
                connection.Open();


                command.Parameters.AddWithValue("@datetime", date);
                command.Parameters.AddWithValue("@planA", Sol.A);
                command.Parameters.AddWithValue("@Kp00", Sol.K00);
                command.Parameters.AddWithValue("@Kp03", Sol.K03);
                command.Parameters.AddWithValue("@Kp06", Sol.K06);
                command.Parameters.AddWithValue("@Kp09", Sol.K09);
                command.Parameters.AddWithValue("@Kp12", Sol.K12);
                command.Parameters.AddWithValue("@Kp15", Sol.K15);
                command.Parameters.AddWithValue("@Kp18", Sol.K18);
                command.Parameters.AddWithValue("@Kp21", Sol.K21);

                command.ExecuteNonQuery();


                connection.Close();

            }
            catch
            {         //if row already exists then try updating it in database

            }


        }

        private bool find_data(string server, string user, string pass) //find a slot row for display in grid from the database corresponding to the date/time from the slot
        {
            DataTable Slots = new DataTable();
            //DateTime d = new DateTime();
            int i = 0;
            bool found = false;
            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_sol";


            try
            {
                MySqlConnection connection = new MySqlConnection(myConnectionString);

                connection.Open();

                MySqlCommand command = connection.CreateCommand();

                //SELECT* FROM your_table ORDER BY your_date_column DESC LIMIT 500;
                command.CommandText = "SELECT * FROM weather ORDER BY datetime DESC LIMIT " + 100;
                MySqlDataReader Reader;
                Reader = command.ExecuteReader();

                while (Reader.Read())
                {
                    found = true;
                    DateTime dt = (DateTime)Reader["datetime"];
                    string date = dt.ToString("yyyy-MM-dd");
                    Sol.A = Convert.ToString((int)Reader["planA"]);
                    Sol.K00 = Convert.ToString((int)Reader["Kp00"]);
                    Sol.K03 = Convert.ToString((int)Reader["Kp03"]);
                    Sol.K06 = Convert.ToString((int)Reader["Kp06"]);
                    Sol.K09 = Convert.ToString((int)Reader["Kp09"]);
                    Sol.K12 = Convert.ToString((int)Reader["Kp12"]);
                    Sol.K15 = Convert.ToString((int)Reader["Kp15"]);
                    Sol.K18 = Convert.ToString((int)Reader["Kp18"]);
                    Sol.K21 = Convert.ToString((int)Reader["Kp21"]);
                    Sol.flux = Convert.ToString((int)Reader["flux"]);
                    Sol.SSN = Convert.ToString((int)Reader["SSN"]);

                    cells[0] = date;
                    cells[1] = Sol.A;
                    cells[2] = Sol.K00;
                    cells[3] = Sol.K03;
                    cells[4] = Sol.K06;
                    cells[5] = Sol.K09;
                    cells[6] = Sol.K12;
                    cells[7] = Sol.K15;
                    cells[8] = Sol.K18;
                    cells[9] = Sol.K21;
                    cells[10] = Sol.flux;
                    cells[11] = Sol.SSN;
                    //TO DO:
                    update_grid(); //add this row to the datagridview


                }
                Reader.Close();
                connection.Close();

            }
            catch
            {
                //databaseError = true; //stop wasting time trying to connect if database error - ignore for present
                found = false;
            }
            return found;
        }

        private void update_grid() //add rows to the datagridview
        {

            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dataGridView1);
            for (int i = 0; i < 12; i++)
            {

                row.Cells[i].Value = cells[i];
            }

            dataGridView1.Rows.Add(row);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            find_data(server, user, pass);
        }
    }
}
