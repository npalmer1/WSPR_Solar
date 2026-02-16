using Google.Protobuf.WellKnownTypes;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.RootFinding;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ApplicationServices;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Tls;
using Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static WSPR_Solar.Solar;
using Application = System.Windows.Forms.Application;



namespace WSPR_Solar
{
    public partial class Solar : Form
    {


        private string[] cells1 = new string[14];
        private string[] cells2 = new string[14];
        private string[] cells3 = new string[14];

        private string server;
        private string user;
        private string pass;
        public struct SolarIndexes
        {
            public string Ap;
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
            public string Xray;
        }

        public struct ProtonFlux
        {
            public string pf00;
            public string pf03;
            public string pf06;
            public string pf09;
            public string pf12;
            public string pf15;
            public string pf18;
            public string pf21;
        }
        ProtonFlux pflux = new ProtonFlux();

        public struct Flares
        {
            public string fl00;
            public string fl03;
            public string fl06;
            public string fl09;
            public string fl12;
            public string fl15;
            public string fl18;
            public string fl21;
        }
        Flares flare = new Flares();

        public struct Bursts
        {
            public string s00;
            public string s03;
            public string s06;
            public string s09;
            public string s12;
            public string s15;
            public string s18;
            public string s21;
        }
        Bursts rb = new Bursts();

        MessageClass Msg = new MessageClass();

        string fluxdata = "";
        string flaredata = "";

        bool satErr = false;

        int Glevel = 0;
        int Slevel = 0;
        int Rlevel = 0;
        int GClevel = 0;
        int SClevel = 0;
        int RClevel = 0;
        string prevG = "";

        string SFI = "";

        public bool stopUrl = false;

        public string results = "";

        bool hamqslopened = false;

        int timercount = 0;

        string serverName = "127.0.0.1";
        string db_user = "admin";
        string db_pass = "wspr";
        string slash = "\\"; //default to Windows
        string root = "/";
        int OpSystem = 0; //default windwows
        string dateformat = "yyyy-MM-dd";

        string activity_level = "";
        string current_burst_level = "";
        string current_burst_band = "";

        public SolarIndexes solar = new SolarIndexes();

        Help helpform = new Help();
        public Solar()
        {
            InitializeComponent();
        }

        public async Task setConfig(string serverName, string db_user, string db_pass)
        {
            server = serverName;
            user = db_user;
            pass = db_pass;
            bool check = await checkNOAA();
            DateTime date = DateTime.Now.ToUniversalTime();
            if (!check)
            {
                Msg.TMessageBox("Warning: unable to connect to NOAA", "Solar data", 1500);
            }
            else
            {
                await savedefaultdata(date); //set blank values in db for today
                await getLatestSolar(serverName, db_user, db_pass);
                await updateGeo(serverName, db_user, db_pass, true); //true - update yesterday as well
                await updateSolar(serverName, db_user, db_pass);
                await updateAllProtonandFlare(serverName, db_user, db_pass, true); //update yesterday
                await updateAllProtonandFlare(serverName, db_user, db_pass, false); //update today

            }

        }

        public async Task savedefaultdata(DateTime date)
        {

            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_sol";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            //date = date.Date;   //set h,m,s to 000
            string datetime = date.ToString("yyyy-MM-dd");
            try
            {

                MySqlCommand command = connection.CreateCommand();

                command.CommandText = "INSERT INTO weather(datetime,Ap,Kp00,Kp03,Kp06,Kp09,Kp12,Kp15,Kp18,Kp21,flux,SSN,Xray,pf00,pf03,pf06,pf09,pf12,pf15,pf18,pf21,";
                command.CommandText += "fl00,fl03,fl06,fl09,fl12,fl15,fl18,fl21,s00,s03,s06,s09,s12,s15,s18,s21) ";
                command.CommandText += "VALUES('" + datetime + "', 0,  0,0,0,0,0,0,0,0,  0,0,'0',  '0','0','0','0','0','0','0','0',";
                command.CommandText += "'0','0','0','0','0','0','0','0', '0','0','0','0','0','0','0','0')";
                //command.CommandText += " ON DUPLICATE KEY UPDATE Ap = '" + solar.Ap + "', Kp00 = '" + solar.K00 + "'";
                //command.CommandText += ", Kp03 = '" + solar.K03 + "', Kp06 = '" + solar.K06 + "', Kp09 = '" + solar.K09 + "'";
                //command.CommandText += ", Kp12 = '" + solar.K12 + "', Kp15 = '" + solar.K15 + "', Kp18 = '" + solar.K18 + "'";
                //command.CommandText += ", Kp21 = '" + solar.K21 + "'";

                connection.Open();


                command.ExecuteNonQuery();


                connection.Close();

            }
            catch
            {
                connection.Close();
            }
        }


        private void Solar_Load(object sender, EventArgs e)
        {
            System.Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string ver = "0.1.14";
            this.Text = "WSPR Solar                       V." + ver + "    GNU GPLv3 License";

            //solarstartuptimer.Enabled = true;
            //solarstartuptimer.Start();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                OpSystem = 0; //Windows
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                OpSystem = 1; //Linux
                slash = "/"; //Linux uses forward slash
                root = "/"; //Linux root
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                OpSystem = 2; //MacOS
                slash = "/"; //MacOS uses forward slash
                root = "/"; //MacOS root
            }
            else if (OperatingSystem.IsAndroid())
            {
                OpSystem = 3; //Android
                slash = "/"; //Android uses forward slash
                root = "/"; //Android root
            }
            solartimer.Enabled = true;
            solartimer.Start();

            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            for (int i = 0; i < dataGridView2.Columns.Count; i++)
            {
                dataGridView2.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            dataGridView1.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 8);
            dataGridView2.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 8);
            dataGridView3.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 8);
            BurstdataGridView.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 8);
            dataGridView3.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            // Automatically adjust row height to fit content
            dataGridView3.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            BurstdataGridView.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 8);
            BurstdataGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            // Automatically adjust row height to fit content
            BurstdataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            setConfig(serverName, db_user, db_pass);
        }

        private bool getUserandPassword()
        {
            string key = "wsproundtheworld";
            Encryption enc = new Encryption();
            string encryptedpassword;
            string content = "";
            bool ok = false;

            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string filepath = homeDirectory;
            //string content = "db_user: " + user + " db_pass: " + passwordhash;

            if (Path.Exists(filepath))
            {

                if (filepath.EndsWith(slash))
                {
                    slash = "";
                }
                filepath = filepath + slash + "DBcredential";
                if (File.Exists(filepath))
                {
                    try
                    {
                        using (StreamReader reader = new StreamReader(filepath))
                        {
                            content = reader.ReadLine();
                            reader.Close();
                        }
                        if (content != null || content != "")
                        {
                            if (content.Contains("db_pass:"))
                            {
                                encryptedpassword = content.Substring(content.IndexOf("db_pass: ") + "db_pass: ".Length);
                                string password = enc.Decrypt(encryptedpassword, key);
                                if (password.Length > 0 && password != null)
                                {
                                    db_pass = password;
                                    //PasstextBox.Text = password;

                                    ok = true;
                                }
                            }
                        }

                        if (!ok)
                        {
                            Msg.TMessageBox("Unable to read database credentials", "", 2000);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Msg.TMessageBox("Unable to read database credentials", "", 2000);
                        return false;
                    }
                }
            }
            //PasstextBox2.Visible = false;
            //Passlabel2.Visible = false;
            return ok;
        }

        private bool checkSolarDB()
        {
            string myConnectionString = "server=" + serverName + ";user id=" + db_user + ";password=" + db_pass + ";database=wspr_solar";

            MySqlConnection connection = new MySqlConnection(myConnectionString);


            try
            {
                connection.Open();
                connection.Close();
                return true;
            }
            catch
            {

                connection.Close();
                return false;
            }
        }
        public async Task<bool> checkNOAA()
        {
            if (stopUrl)
            {
                return false;
            }
            string url = "https://services.swpc.noaa.gov/";
            if (!await Msg.IsUrlReachable(url))
            {
                Msg.TMessageBox("Unable to connect to NOAA url", "Solar data", 1000);
                return false;
            }
            else
            {
                return true;
            }
        }
        public async Task updateGeo(string server, string user, string pass, bool updateyesterday)
        {
            DateTime today = DateTime.Now.ToUniversalTime();
            DateTime ydt;
            ydt = today.AddDays(-1);
            string datetime = today.ToString("yyyy MMM dd");
            string yesterday = ydt.ToString("yyyy MMM dd");
            await fetchGeodata();
            if (updateyesterday)
            {
                await findGeo(yesterday, "Planetary");
                await saveGeoData(ydt);
                prevG = findKplevel(solar.K21);
                if (prevG == "")
                {
                    prevG = "--";
                }
                if (today.Hour < 3)
                {
                    GClabel.Text = prevG;
                }
            }

            await findGeo(datetime, "Planetary");
            await saveGeoData(today);
            await find_data(false, "", "");
        }
        public async Task updateSolar(string server, string user, string pass)
        {

            await fetchSolardata();
            DateTime date = DateTime.Now.ToUniversalTime();
            await findSolar();
            date = date.AddDays(-1); //boulder info is from previous day           
            await SaveSolardata(date); //today
            await find_data(false, "", "");
        }

        public async Task updateBursts(string server, string user, string pass)
        {

            await fetchBurstdata();
            DateTime date = DateTime.Now.ToUniversalTime();
            await findBurst();

            await SaveBurstdata(date); //today
            await find_burst_data(false, "", "");

        }



        private async void getLatestbutton_Click(object sender, EventArgs e)
        {
            if (stopUrl)
            {
                Msg.TMessageBox("You have disabled the Internet connection", "Solar data", 1000);
                return;
            }
            string url = "https://services.swpc.noaa.gov/";
            if (!await Msg.IsUrlReachable(url))
            {
                Msg.TMessageBox("Unable to connect to NOAA url", "Solar data", 1000);
                return;
            }
            getLatestSolar(server, user, pass);
        }

        public async Task getLatestSolar(string server, string user, string pass)
        {
            DateTime today = DateTime.Now.ToUniversalTime();
            string datetime = today.ToString("yyyy MMM dd");
            await fetchGeodata();
            await fetchSolardata();
            dataGridView1.Rows.Clear();
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();
            await findSolar();

            await findGeo(datetime, "Planetary");

            await populateGrid(0);

            await findGeo(datetime, "Boulder");
            await populateGrid(1);
        }

        public async Task findGeo(string datetime, string source)
        {

            bool found = false;

            if (datetime[9] == '0')
            {
                datetime = datetime.Remove(9, 1);
            }
            int index = textBox1.Text.IndexOf(datetime);

            if (index > 1)
            {
                string S = textBox1.Text.Substring(index);
                string line = "";
                using var reader = new StringReader(S);
                {


                    try
                    {
                        while (line != null && !found)
                        {
                            line = reader.ReadLine();
                            if (line.Contains(source))
                            {
                                found = true;
                                datelabel.Text = datetime;
                                break;
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                reader.Close();
                int off = 0;
                if (source == "Boulder")
                {
                    off = 2;
                }
                if (found)
                {
                    string[] aP = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (aP.Count() > 2)
                    {
                        solar.Ap = aP[2 + off];
                        solar.K00 = aP[3 + off];
                        solar.K03 = aP[4 + off];
                        solar.K06 = aP[5 + off];
                        solar.K09 = aP[6 + off];
                        solar.K12 = aP[7 + off];
                        solar.K15 = aP[8 + off];
                        solar.K18 = aP[9 + off];
                        solar.K21 = aP[10 + off];
                    }
                    prevG = solar.K21;
                }
            }
        }

        public async Task findSolar()
        {
            bool found = false;
            bool foundX = false;
            string D = "";
            string S = "";

            string line = "";
            string Xray = "";

            int index = -1;
            index = textBox2.Text.IndexOf(":Issued:");
            if (index > -1)
            {
                D = textBox2.Text.Substring(index, 25);
                string[] d = D.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                D = d[1] + " " + d[2] + " " + d[3];
                DateTime dt = Convert.ToDateTime(D);
                //dt = dt.ToUniversalTime();
                DateTime now = DateTime.Now.ToUniversalTime();
                if (dt.Day != now.Day)
                {
                    return;
                }

                S = textBox2.Text.Substring(index);

            }
            index = textBox2.Text.IndexOf("Daily Indices:");

            if (index > 1)
            {

                using var reader = new StringReader(S);
                {

                    try
                    {
                        reader.ReadLine();
                        while (line != null && !found)
                        {
                            line = reader.ReadLine();

                            if (line.Contains("10 cm"))
                            {
                                found = true;

                            }
                            index = line.IndexOf("X-ray Background");
                            if (index > -1)
                            {
                                Xray = line.Substring(index + "X-ray Background".Length);
                                Xray = Xray.Trim();
                                foundX = true;
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                reader.Close();
                if (found)
                {
                    string[] FS = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (FS.Count() > 2)
                    {
                        solar.flux = FS[2];
                        solar.SSN = FS[4];
                    }
                }
                if (foundX && Xray != "" && Xray != null)
                {
                    solar.Xray = Xray;
                }
                SFI = solar.flux;
            }
        }

        private string findConditions(string flux)
        {
            int t;
            string P = "";
            string F = "SFI: " + flux + " - ";
            int.TryParse(flux, out t);

            if (t < 70)
            {
                P = "poor";
            }
            if (t > 70)
            {
                P = "poor-fair";
            }
            if (t > 99)
            {
                P = "fair";
            }
            if (t > 129)
            {
                P = "good";
            }
            if (t > 149)
            {
                P = "very good";
            }
            if (t > 199)
            {
                P = "outstanding";
            }
            if (flux == "" || flux == null)
            {
                P = "unknown";
            }
            if (activity_level.Contains("storm"))
            {
                P = P + " ...but " + activity_level;
            }
            else
            {
                P = P + " ...Sun: " + activity_level;
            }
            if (GClabel.Text.Contains("G") || RClabel.Text.Contains("R") || SClabel.Text.Contains("S"))
            {
                string S = findStormLevel(activity_level, GClabel.Text, RClabel.Text, SClabel.Text, false);
                stormconditionlabel.Text = S;
            }
            else if (activity_level.Contains("storm"))
            {
                stormconditionlabel.Text = "Possibly unstable/degraded (" + activity_level + ")";
            }
            else if (Glabel.Text.Contains("G") || Rlabel.Text.Contains("R") || Slabel.Text.Contains("S"))
            {
                string S = findStormLevel(activity_level, Glabel.Text, Rlabel.Text, Slabel.Text, true);
                stormconditionlabel.Text = S;
            }
            else
            {
                stormconditionlabel.Text = "Likely normal - (no storm or blackout)";
            }
            conditionlabel.Text = F + "Higher HF propagation: " + P;
            return P;
        }

        private string findStormLevel(string activity, string G, string R, string S, bool previous)
        {
            string s = "";
            string r = "";
            int level = 0;
            int Alevel = 0;
            if (G.Contains("1") || (R.Contains("1")) || S.Contains("1"))
            {
                level = 1;  //minor
            }
            else if (G.Contains("2") || (R.Contains("2")) || S.Contains("2"))
            {
                level = 2;  //moderate
            }
            else if (G.Contains("3") || (R.Contains("3")) || S.Contains("3"))
            {
                level = 3;  //strong
            }
            else if (G.Contains("4") || (R.Contains("4")) || S.Contains("4"))
            {
                level = 4;  //severe
            }
            else if (G.Contains("5") || (R.Contains("5")) || S.Contains("5"))
            {
                level = 5;  //extreme
            }
            if (activity.Contains("minor"))
            {
                Alevel = 1;
            }
            else if (activity.Contains("major"))
            {
                Alevel = 2;
            }
            else if (activity.Contains("severe"))
            {
                Alevel = 3;
            }

            if (level == 1 || Alevel == 1)
            {
                s = "minor";
                r = "Possibly slightly unstable/degraded (" + s + " storm or blackout)";
                if (previous)
                {
                    r = "Likely normal, but " + s + " storm or blackout reported earlier";
                }
            }
            else if (level == 2 && Alevel < 2)
            {
                s = "moderate";
                r = "Possibly moderately unstable/degraded (" + s + " storm or blackout)";
                if (previous)
                {
                    r = "Likely normal, but " + s + " storm or blackout reported earlier";
                }
            }
            else if (level == 3 || Alevel == 2)
            {
                s = "strong";
                r = "Possibly quite unstable/degraded (" + s + " storm or blackout)";
                if (previous)
                {
                    r = "Likely returning to normal (" + s + " storm or blackout reported earlier)";
                }
            }
            else if (level == 4 || Alevel == 3)
            {
                s = "severe";
                r = "Likely severely unstable/degraded (" + s + " storm or blackout)";
                if (previous)
                {
                    r = "Likely returning to normal (" + s + " storm or blackout reported earlier)";
                }
            }
            else if (level == 5)
            {
                s = "extreme";
                r = "Likely extremely unstable/degraded (" + s + " storm or blackout)";
                if (previous)
                {
                    r = "Likely normal/disturbed, " + s + " storm or blackout reported earlier";
                }
            }
            return r;
        }

        private async Task populateGrid(int row)
        {
            if (row > dataGridView1.RowCount)
            {
                return;
            }
            if (row == 0 || (row == 1 && solar.Ap != "-1")) //planetary)
            {
                cells1[0] = solar.Ap;
                double A = Convert.ToDouble(solar.Ap);
                string L = find_activity_level(A);
                activity_level = L.ToLower();

                cells1[1] = L;
            }
            else //if Boulder
            {
                cells1[0] = "n/a";
                cells1[1] = "n/a";
            }

            string[] k = new string[8];
            k[0] = findKplevel(solar.K00);
            k[1] = findKplevel(solar.K03);
            k[2] = findKplevel(solar.K06);
            k[3] = findKplevel(solar.K09);
            k[4] = findKplevel(solar.K12);
            k[5] = findKplevel(solar.K15);
            k[6] = findKplevel(solar.K18);
            k[7] = findKplevel(solar.K21);

            cells1[2] = solar.K00 + k[0];
            cells1[3] = solar.K03 + k[1];
            cells1[4] = solar.K06 + k[2];
            cells1[5] = solar.K09 + k[3];
            cells1[6] = solar.K12 + k[4];
            cells1[7] = solar.K15 + k[5];
            cells1[8] = solar.K18 + k[6];
            cells1[9] = solar.K21 + k[7];
            if (row == 0)
            {
                find_current_G(k);
            }

            if (row == 0) //planetary
            {
                cells1[10] = solar.flux;
                cells1[11] = solar.SSN;
                cells1[12] = solar.Xray;
            }
            else //boulder
            {
                cells1[10] = "-";
                cells1[11] = "-";
                cells1[12] = "-";
            }

            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Rows[row].Cells[i].Value = cells1[i];
            }

        }
        private void find_current_G(string[] k)
        {
            DateTime now = DateTime.Now.ToUniversalTime();
            try
            {
                string l = "--";
                int hour = now.Hour;
                int h = ((hour - 3) / 3) * 3;
                if (hour < 3)
                {
                    h = 1;
                }

                switch (h)
                {
                    case 0: { l = k[0]; break; }
                    case 3: { l = k[1]; break; }
                    case 6: { l = k[2]; break; }
                    case 9: { l = k[3]; break; }
                    case 12: { l = k[4]; break; }
                    case 15: { l = k[5]; break; }
                    case 18: { l = k[6]; break; }
                    case 21: { l = k[7]; break; }
                }
                l = l.Replace("(", "");
                l = l.Replace(")", "");
                if (l == "")
                {
                    l = "--";
                }
                GClabel.Text = l;
            }
            catch
            {
            }
        }

        private string findKplevel(string kp)
        {
            int g = 0;
            double K = Convert.ToDouble(kp);
            string s = "";
            if (K >= 5)
            {
                g = 1;
                s = " (G1)";
            }
            if (K >= 6)
            {
                g = 2;
                s = " (G2)";
            }
            if (K >= 7)
            {
                g = 3;
                s = " (G3)";
            }
            if (K >= 8 && K < 9)
            {
                g = 4;
                s = " (G4)";
            }
            if (K >= 9)
            {
                g = 5;
                s = " (G5)";
            }

            return s;
        }
        private string find_activity_level(double A)
        {
            string s = "";
            if (A >= 0 && A <= 7)
            {
                s = "Quiet";
            }
            else if (A >= 8 && A <= 15)
            {
                s = "Unsettled";
            }
            else if (A >= 16 && A <= 29)
            {
                s = "Active";
            }
            else if (A >= 30 && A <= 49)
            {
                s = "Minor storm";
            }
            else if (A >= 50 && A <= 99)
            {
                s = "Mod-strong storm";
            }
            else if (A >= 100 && A <= 400)
            {
                s = "Severe storm poss.";
            }
            else
            {
                s = "Unknown";
            }
            DateTime now = DateTime.Now.ToUniversalTime();
            if (now.Hour < 3)
            {
                s = "Unknown";
            }
            return s;
        }


        public async Task fetchGeodata()
        {
            textBox1.Text = "";
            string results = "";
            string ftpUrl = "ftp://ftp.swpc.noaa.gov/pub/lists/geomag/AK.txt";
            if (stopUrl)
            {
                return;
            }
            if (await Msg.IsFtpReachable(ftpUrl))
            {
                using (WebClient client = new WebClient())
                {
                    //client.Credentials = new NetworkCredential(username, password);

                    try
                    {
                        results = client.DownloadString(ftpUrl);

                    }
                    catch (WebException ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }

                using var reader = new StringReader(results);
                {
                    string line = "";


                    try
                    {
                        while (line != null)
                        {
                            line = reader.ReadLine();
                            textBox1.Text = textBox1.Text + line + Environment.NewLine;

                        }
                    }
                    catch
                    {

                    }
                }
                reader.Close();
            }
            else
            {
                Msg.TMessageBox("Unable to reach NOAA geomagnetic data", "Solar data", 1500);
            }
        }


        public async Task fetchSolardata()
        {
            textBox2.Text = "";
            string results = "";
            string Url = "https://services.swpc.noaa.gov/text/sgas.txt";
            if (stopUrl)
            {
                return;
            }
            if (await Msg.IsUrlReachable(Url))
            {
                using (HttpClient client = new HttpClient())
                {

                    try
                    {
                        results = client.GetAsync(Url).Result.Content.ReadAsStringAsync().Result;

                    }
                    catch (WebException ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }

                using var reader = new StringReader(results);
                {
                    string line = "";


                    try
                    {
                        while (line != null)
                        {
                            line = reader.ReadLine();
                            textBox2.Text = textBox2.Text + line + Environment.NewLine;

                        }
                    }
                    catch
                    {

                    }
                }
                reader.Close();
            }
            else
            {
                Msg.TMessageBox("Unable to reach NOAA solar data", "Solar data", 1000);
            }
        }



        public async Task fetchBurstdata()
        {

            results = "";
            textBox3.Text = "";
            string line = "";
            string Url = "https://services.swpc.noaa.gov/text/solar-geophysical-event-reports.txt";
            if (stopUrl)
            {
                return;
            }
            if (await Msg.IsUrlReachable(Url))
            {
                using (WebClient client = new WebClient())
                {
                    //client.Credentials = new NetworkCredential(username, password);

                    try
                    {
                        results = client.DownloadString(Url);
                        using var reader = new StringReader(results);
                        {
                            while ((line = reader.ReadLine()) != null)
                            {
                                textBox3.Text = textBox3.Text + line + "\r\n";
                            }
                        }

                    }
                    catch (WebException ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }

            }
            else
            {
                Msg.TMessageBox("Unable to reach NOAA radio burst data", "Solar data", 1000);
            }
        }

        List<string> st = new List<string>();
        List<string> st2 = new List<string>();
        public async Task findBurst()
        {
            string[] S;

            string line = "";
            string Xray = "";

            string time = "";
            string timend = "";
            string rburst = "";
            string part = "";
            string f = "";
            bool found = false;
            st.Clear();
            st2.Clear();

            if (results.Contains("RBR") || results.Contains("RSP") || results.Contains("RNS"))
            {
                int index = 0;
                using var reader = new StringReader(results);
                {

                    try
                    {

                        DateTime dt = new DateTime();
                        DateTime now = DateTime.Now.ToUniversalTime();
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Contains("Created:"))
                            {
                                S = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                string date = S[1] + " " + S[2] + " " + S[3];
                                DateTime.TryParse(date, out dt);
                                if (dt.Day != now.Day)
                                {
                                    return;
                                }

                            }


                            if (line.Contains("RBR") || line.Contains("RSP") || line.Contains("RNS"))
                            {
                                S = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                if (S.Count() > 8)
                                {
                                    int i = 0;
                                    if (S[1] == "+")
                                    {
                                        i = 1;
                                    }
                                    time = S[1 + i];
                                    timend = "";
                                    if (S[1 + i].Contains("///"))
                                    {
                                        time = "?";
                                    }

                                    else if (S[3 + i] != "" && !S[3 + i].Contains("///"))
                                    {
                                        timend = timend = S[3 + i];
                                    }
                                    else
                                    {
                                        timend = "?";
                                    }
                                    rburst = S[6 + i];
                                    part = S[8 + i];
                                    f = S[7 + i];
                                    //st.Add(time + "/" + rburst+"/"+part);
                                    if (timend == "")
                                    {
                                        st.Add(rburst + "/" + time + "/" + part);
                                    }
                                    else
                                    {
                                        st.Add(rburst + "/" + time + "-" + timend + "/" + part);

                                    }
                                    st2.Add(timend);
                                }

                            }
                        }

                    }
                    catch
                    {

                    }
                }
                reader.Close();
                rb.s00 = "";
                rb.s03 = "";
                rb.s06 = "";
                rb.s09 = "";
                rb.s12 = "";
                rb.s15 = "";
                rb.s18 = "";
                rb.s21 = "";


                for (int i = 0; i < st.Count; i++)
                {
                    findBurstTime(i);

                }

            }
        }

        private void findBurstTime(int i)
        {
            string nl;
            bool rsp = false;
            string S = "";
            string[] s;
            try
            {
                DateTime te = new DateTime();
                string[] T = st[i].Split('/');
                if (T[1].Contains("-"))
                {
                    s = T[1].Split('-');
                    S = s[0];
                }
                else
                {
                    S = T[1];
                }

                string time = S.Insert(2, ":");
                DateTime t;
                DateTime.TryParse(time, out t);
                te = t;
                if (st2[i] != "") //not used
                {
                    string TE = st2[i];
                    TE = TE.Insert(2, ":");
                    DateTime.TryParse(TE, out te);
                    rsp = true;
                }
                else
                {
                    rsp = false;
                }

                findBurstTimeSlot(st[i], t);
                //if (rsp) { findBurstTimeSlot(T[0] +"-"+st2[i]+"rsp", te); }

            }
            catch
            {

            }
        }
        private void findBurstTimeSlot(string S, DateTime t)
        {
            if (S == null)
            {
                S = "";
            }
            string nl = "";
            try
            {
                if (t.Hour >= 0 && t.Hour < 3)
                {
                    if (rb.s00 != "")
                    {
                        nl = Environment.NewLine;
                    }
                    else
                    {
                        nl = "";
                    }

                    rb.s00 = rb.s00 + nl + S;
                }
                if (t.Hour >= 3 && t.Hour < 6)
                {
                    if (rb.s03 != "")
                    {
                        nl = Environment.NewLine;
                    }
                    else
                    {
                        nl = "";
                    }
                    rb.s03 = rb.s03 + nl + S;
                }
                if (t.Hour >= 6 && t.Hour < 9)
                {
                    if (rb.s06 != "")
                    {
                        nl = Environment.NewLine;
                    }
                    else
                    {
                        nl = "";
                    }
                    rb.s06 = rb.s06 + nl + S;
                }
                if (t.Hour >= 9 && t.Hour < 12)
                {
                    if (rb.s09 != "")
                    {
                        nl = Environment.NewLine;
                    }
                    else
                    {
                        nl = "";
                    }
                    rb.s09 = rb.s09 + nl + S;
                }
                if (t.Hour >= 12 && t.Hour < 15)
                {
                    if (rb.s12 != "")
                    {
                        nl = Environment.NewLine;
                    }
                    else
                    {
                        nl = "";
                    }
                    rb.s12 = rb.s12 + nl + S;
                }
                if (t.Hour >= 15 && t.Hour < 18)
                {
                    if (rb.s15 != "")
                    {
                        nl = Environment.NewLine;
                    }
                    else
                    {
                        nl = "";
                    }
                    rb.s15 = rb.s15 + nl + S;
                }
                if (t.Hour >= 18 && t.Hour < 21)
                {
                    if (rb.s18 != "")
                    {
                        nl = Environment.NewLine;
                    }
                    else
                    {
                        nl = "";
                    }
                    rb.s18 = rb.s18 + nl + S;
                }
                if (t.Hour >= 21 && t.Hour <= 23)
                {
                    if (rb.s21 != "")
                    {
                        nl = Environment.NewLine;
                    }
                    else
                    {
                        nl = "";
                    }
                    rb.s21 = rb.s21 + nl + S;
                }
            }
            catch
            {

            }
        }

        private async Task find_burst_data(bool filter, string datetime1, string datetime2) //find a slot row for display in grid from the database corresponding to the date/time from the slot
        {
            DataTable Slots = new DataTable();
            //dataGridView3.Rows.Clear();

            int i = 0;
            bool found = false;
            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_sol";
            rb.s00 = "";
            rb.s03 = "";
            rb.s06 = "";
            rb.s09 = "";
            rb.s12 = "";
            rb.s15 = "";
            rb.s18 = "";
            rb.s21 = "";
            cells2[0] = "Radio bursts:";
            cells2[1] = rb.s00;
            cells2[2] = rb.s03;
            cells2[3] = rb.s06;
            cells2[4] = rb.s09;
            cells2[5] = rb.s12;
            cells2[6] = rb.s15;
            cells2[7] = rb.s18;
            cells2[8] = rb.s21;

            MySqlConnection connection = new MySqlConnection(myConnectionString);


            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();

                string C = "";
                string and = "";
                string D = "";
                string order = " ORDER BY datetime DESC LIMIT " + 500;
                if (!filter && !datecheckBox.Checked)
                {

                    command.CommandText = "SELECT * FROM weather ORDER BY datetime DESC LIMIT " + 500;
                }
                else
                {
                    if (flarelistBox.SelectedIndex > -1) //find flare by class
                    {
                        C = flarelistBox.SelectedItem.ToString();
                        C = "%" + C + "%";
                        C = "'" + C + "'";
                        string S = "fl00 LIKE " + C + " OR fl03 LIKE " + C + " OR fl06 LIKE " + C + " OR fl09 LIKE " + C + " OR fl12 LIKE " + C;
                        S += " OR fl15 LIKE " + C + " OR fl18 LIKE " + C + " OR fl21 LIKE " + C;
                        C = S;

                    }


                    if (datecheckBox.Checked)
                    {
                        D = " datetime >= '" + datetime1 + "' AND datetime <= '" + datetime2 + "'";
                        if (C != "")
                        { and = " AND "; }
                    }

                    command.CommandText = "SELECT * FROM weather WHERE " + D + and + C + order;
                }
                MySqlDataReader Reader;
                Reader = command.ExecuteReader();

                int rows = table_count();
                int rcount = 0;
                int rowinsert = 2;
                string B = "";
                while (Reader.Read())
                {
                    rb.s00 = "";
                    rb.s03 = "";
                    rb.s06 = "";
                    rb.s09 = "";
                    rb.s12 = "";
                    rb.s15 = "";
                    rb.s18 = "";
                    rb.s21 = "";
                    found = true;
                    DateTime dt = (DateTime)Reader["datetime"];
                    string date = dt.ToString("yyyy-MM-dd");

                    try
                    {
                        B = (string)Reader["s00"];
                        if (B != null) { rb.s00 = B; }
                        B = (string)Reader["s03"];
                        if (B != null) { rb.s03 = B; }
                        B = (string)Reader["s06"];
                        if (B != null) { rb.s06 = B; }
                        B = (string)Reader["s09"];
                        if (B != null) { rb.s09 = B; }
                        B = (string)Reader["s12"];
                        if (B != null) { rb.s12 = B; }
                        B = (string)Reader["s15"];
                        if (B != null) { rb.s15 = B; }
                        B = (string)Reader["s18"];
                        if (B != null) { rb.s18 = B; }
                        B = (string)Reader["s21"];
                        if (B != null) { rb.s21 = B; }


                    }
                    catch { }
                    cells2[0] = "Radio bursts:";
                    cells2[1] = rb.s00;
                    cells2[2] = rb.s03;
                    cells2[3] = rb.s06;
                    cells2[4] = rb.s09;
                    cells2[5] = rb.s12;
                    cells2[6] = rb.s15;
                    cells2[7] = rb.s18;
                    cells2[8] = rb.s21;

                    if (rcount < rows)
                    {
                        update_grid3_storms(rowinsert); //add this row to the datagridview
                        rcount++;
                        rowinsert = rowinsert + 3;
                    }


                }
                Reader.Close();
                connection.Close();
                findBurstStorms();

            }
            catch
            {
                Msg.TMessageBox("Error reading burst data from database", "Solar data", 2000);
                found = false;
                connection.Close();
            }
        }

        private void update_grid3_storms(int rinsert) //add rows to the datagridview
        {


            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dataGridView3); // Initializes cells based on the grid's columns

            int columns = dataGridView3.ColumnCount;


            for (int i = 0; i < columns; i++)
            {
                if (cells1[i] != null)
                {
                    row.Cells[i].Value = cells2[i];

                }
                else
                {
                    row.Cells[i].Value = " ";
                }
            }


            if (dataGridView3.Rows.Count > rinsert)
            {
                if (dataGridView3.Rows[rinsert].Cells[0].Value != null)
                {
                    string b = dataGridView3.Rows[rinsert].Cells[0].Value.ToString();
                    if (b.Contains("Radio burst"))
                    {
                        dataGridView3.Rows.RemoveAt(rinsert);
                    }

                    dataGridView3.Rows.Insert(rinsert, row);
                    //insert on line after flares
                }
            }

        }


        private bool findBurstStorms()
        {
            BurstdataGridView.Rows.Clear();
            int columns = dataGridView3.ColumnCount;
            int rows = dataGridView3.RowCount;
            if (rows > 1)
            {
                try
                {
                    if (dataGridView3.Rows[2].Cells[0].Value != null)
                    {
                        string cell = "";
                        for (int i = 1; i < columns; i++)
                        {
                            if (dataGridView3.Rows[2].Cells[i].Value != null)
                            {
                                cell = dataGridView3.Rows[2].Cells[i].Value.ToString();

                                ParseBurstStorms(cell, i - 1);
                            }
                        }
                    }
                    //BurstgroupBox.Visible = true;
                   
                    BurstgroupBox.BringToFront();
                    string B = "";
                    for (int b = 0; b < columns-1; b++)
                    {
                        B = "";
                        string s = "min/";
                        if (bd.RSPspan[b] == "")
                        {
                            s = "";
                            B = B + bd.RSPlevel[b] + Environment.NewLine + Environment.NewLine;
                        }
                        else
                        {
                            B = bd.RSPlevel[b] + Environment.NewLine;
                            B = B + bd.RSPspan[b] + s + bd.RSPband[b] + Environment.NewLine;
                        }

                        string min = "";
                        if (bd.rbrSpan[b] != "")
                        {
                            min = "min ";
                        }
                        B = B + bd.rbrSpan[b] + min + " " + bd.rbrLevel[b] + Environment.NewLine;
                        min = "";
                        if (bd.rnsSpan[b] != "")
                        {
                            min = "min ";
                        }
                        B = B + bd.rnsSpan[b] + min+ " " + bd.rnsLevel[b];
                        BurstdataGridView.Rows[0].Cells[b].Value = B;
                    }
                  
                    findcurrentBurst();
                }
                catch {
                    Msg.TMessageBox("Test", "", 2000);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void groupBox1_Paint(object sender, PaintEventArgs e)
        {
            System.Windows.Forms.GroupBox box = (System.Windows.Forms.GroupBox)sender;
            e.Graphics.Clear(this.BackColor);

            Size textSize = TextRenderer.MeasureText(box.Text, box.Font);
            Rectangle borderRect = new Rectangle(
                0,
                textSize.Height / 2,
                box.Width - 1,
                box.Height - textSize.Height / 2 - 1
            );

            using (Pen pen = new Pen(Color.Red, 2))
            {
                e.Graphics.DrawRectangle(pen, borderRect);
            }
        }

        struct BurstData()
        {
            public string[] RSPlevel = new string[8]; //levels for each time period
            public string[] RSPband = new string[8]; //bands for each time period
            public string[] RSPspan = new string[8]; //duration for each time period
            public string[] rbrSpan = new string[8]; //rbs time period
            public string[] rnsSpan = new string[8]; //rnr time period
            public string[] rbrLevel = new string[8]; //rbs intensity
            public string[] rnsLevel = new string[8]; //rnr intensity
        }
        BurstData bd = new BurstData();

        private void ParseBurstStorms(string cell, int period)
        {
            string line;
            //string level = "";

            int Level = 0;
            string band = "";
            string l1band = "";
            string LStr = "";
            int rspDur = 0;
            int rbrDur = 0;
            int rnsDur = 0;
            int rbrLevel = 0;
            int rnsLevel = 0;

            int SFU = 0;

            using (StringReader reader = new StringReader(cell))
            {
                int dur = 0;


                while ((line = reader.ReadLine()) != null)
                {
                    string[] S = line.Split("/");
                    string[] t = S[1].Split("-");
                    dur = 0;
                    if (line.StartsWith("RSP") || line.StartsWith("RNS") || line.StartsWith("RBR"))
                    {
                        try
                        {

                            if (t.Count() > 1 && t[0] != "?" && t[1] != "?" && t[0] != "" && t[1] != "")
                            {
                                t[0] = t[0].Insert(2, ":");
                                t[1] = t[1].Insert(2, ":");
                                DateTime from = Convert.ToDateTime(t[0]);
                                DateTime to = Convert.ToDateTime(t[1]);
                                dur = (int)Math.Abs((from - to).TotalMinutes);

                                if (dur == 0)
                                {
                                    dur = 1;
                                }
                            }
                            else
                            {
                                dur = 1;
                            }
                        }
                        catch { dur = 0; }
                    }
                    else
                    {
                        dur = 0; return;
                    }

                    if (line.StartsWith("RSP")) //sweep frequency radio burst - duration and frequency range indicate severity
                    {
                        if (S.Count() > 0)
                        {



                            if (line.EndsWith("/1"))
                            {
                                rspDur = rspDur + dur;
                                if (dur > 3  && Level < 1)
                                {
                                    LStr = "Weak";
                                    Level = 1;
                                }
                            }
                            else if (line.EndsWith("/2"))
                            {
                                rspDur = rspDur + dur;
                                if ((dur > 3) && Level < 2)
                                {
                                    LStr = "Moderate";
                                    Level = 2;
                                }
                            }
                            else if (line.EndsWith("/3"))
                            {
                                rspDur = rspDur + dur;
                                if ((dur > 3) && Level < 3)
                                {
                                    LStr = "Strong";
                                    Level = 3;
                                }
                            }
                            else if (line.EndsWith("/4"))
                            {
                                rspDur = rspDur + dur;
                                if ((dur > 3) && Level < 4)
                                {
                                    LStr = "Severe";
                                    Level = 4;
                                }
                            }
                            else if (line.EndsWith("/5"))
                            {
                                rspDur = rspDur + dur;
                                if ((dur > 3) && Level < 5)
                                {
                                    LStr = "Extreme";
                                    Level = 5;
                                }
                            }
                            if (line.Contains("III") || (line.Contains("VI")))
                            {
                                if (!l1band.Contains("W")) { l1band = "W"; }
                                if ((dur > 3 || rspDur > 10) && !band.Contains("W"))
                                {
                                    band = band + "W"; //wideband VLF - 1GHz
                                }
                            }
                            else if (line.Contains("II") || line.Contains("V"))
                            {
                                if (!l1band.Contains("T")) { l1band = "T"; }
                                if ((dur > 3 || rspDur > 10) && !band.Contains("T"))
                                {
                                    band = band + "T"; //25-200MHz
                                }
                            }
                            else if (line.Contains("I"))
                            {
                                if (!l1band.Contains("V")) { l1band = "V"; }
                                if ((dur > 3 || rspDur > 10) && !band.Contains("V"))
                                {
                                    band = band + "V"; //VHF
                                }
                            }
                            else if (line.Contains("IV"))
                            {
                                if (!l1band.Contains("G")) { l1band = "G"; }
                                if ((dur > 3 || rspDur > 10) && !band.Contains("G"))
                                {
                                    band = band + "G"; //25MHz-2GHz
                                }
                            }
                            else if (line.Contains("CTM"))
                            {
                                if (!l1band.Contains("W")) { l1band = "W"; }
                                if ((dur > 3 || rspDur > 10) && !band.Contains("W"))
                                {
                                    band = band + "W"; //25MHz-2GHz
                                }
                            }
                            if (S.Count() > 7)  //override if a frequency range specified
                            {
                                if (S[7].Contains("-")) //if a specific frequency range is given, instead of band
                                {
                                    band = S[7];
                                }
                            }

                        }
                    }
                    else if (line.StartsWith("RBR") || line.StartsWith("RNS"))
                    //RBR = radio burst - fixed frequency short duration indicate severity
                    //RNS = radio burst - radio noise storm - longer duration
                    {
                        int sfu = 0;
                        if (S.Count() > 2)
                        {
                            int.TryParse(S[2], out sfu);
                            SFU = sfu;
                        }
                        else
                        {
                            SFU = 0;
                        }
                        if (line.StartsWith("RBR"))
                        {
                            rbrDur = rbrDur + dur;
                            if (rbrLevel < sfu)
                            {
                                rbrLevel = sfu;
                            }
                        }
                        else
                        {
                            rnsDur = rnsDur + dur;
                            if (rnsLevel < sfu)
                            {
                                rnsLevel = sfu;
                            }
                        }


                    }

                }
                if (LStr == "" && rspDur > 0)
                {
                    LStr = "Insignificant";
                }
                else if (LStr == "" && rspDur < 1)
                {
                    if (rbrDur > 0 && rnsDur > 0)
                    {
                        LStr = "Insignificant";
                    }
                    else
                    {
                        LStr = "None/insignificant";
                    }
                }
                if (Level == 1)
                {
                    LStr = "Weak";
                    if (l1band.Contains("W"))
                    {
                        bd.RSPband[period] = "Wideband";
                    }
                    else if (l1band.Contains("G"))
                    { bd.RSPband[period] = "25MHz-2GHz"; }
                    else if (l1band.Contains("T"))
                    { bd.RSPband[period] = "25-200MHz"; }
                    else if (l1band.Contains("V"))
                    { bd.RSPband[period] = "VHF"; }
                    else
                    {
                        bd.RSPband[period] = "";
                    }
                }
                if (band.Contains("W"))
                {
                    bd.RSPband[period] = "Wideband";
                }
                else if (band.Contains("G"))
                { bd.RSPband[period] = "25MHz-2GHz"; }
                else if (band.Contains("T"))
                { bd.RSPband[period] = "25-200MHz"; }
                else if (band.Contains("V"))
                { bd.RSPband[period] = "VHF"; }
                else
                {
                    bd.RSPband[period] = "";
                }
                bd.RSPlevel[period] = LStr;
                if (rspDur > 5)
                {
                    bd.RSPspan[period] = rspDur.ToString(); // + "mins";
                }
                else
                {
                    bd.RSPspan[period] = "";
                }
                if (rbrDur > 0)
                {
                    bd.rbrSpan[period] = rbrDur.ToString(); // + "mins";
                    if (rbrLevel > 0)
                    {
                        bd.rbrLevel[period] = findIntensity(rbrLevel);
                    }
                }
                else
                {
                    bd.rbrSpan[period] = "";
                    bd.rbrLevel[period] = "";

                }
                if (rnsDur > 0)
                {
                    bd.rnsSpan[period] = rnsDur.ToString(); // + "mins";
                    if (rnsLevel > 0)
                    {
                        bd.rnsLevel[period] = findIntensity(rnsLevel);
                    }
                }
                else
                {
                    bd.rnsSpan[period] = "";
                    bd.rnsLevel[period] = "";
                }


                int p = findHour();
                if (p < period)
                {
                    bd.RSPlevel[period] = "";
                    bd.RSPband[period] = "";
                    bd.RSPspan[period] = "";
                    bd.rbrSpan[period] = "";
                    bd.rnsSpan[period] = "";
                    bd.rbrLevel[period] = ""; 
                    bd.rnsLevel[period] = "";
                }
            }

        }

        private string findIntensity(int SFU)
        {
            if (SFU < 50)
            {
                return "Quiet";
            }
            else if (SFU >= 50 && SFU < 200)
            {
                return "Low";
            }
            else if (SFU >= 200 && SFU < 500)
            {
                return "Moderate";
            }
            else if (SFU >= 500 && SFU < 1000)
            {
                return "High";
            }
            else if (SFU >= 1000 && SFU < 10000)
            {
                return "Very high";
            }
            else if (SFU >= 10000)
            {
                return "Extreme";
            }
            else
            {
                return "";
            }
        }
        

        private int findHour()
        {
            DateTime now = DateTime.Now.ToUniversalTime();
            int hour = now.Hour;
            int p = 0;
            if (hour >= 0 && hour < 3)
            {
                p = 0;
            }
            else if (hour >= 3 && hour < 6)
            {
                p = 1;
            }
            else if (hour >= 6 && hour < 9)
            {
                p = 2;
            }
            else if (hour >= 9 && hour < 12)
            {
                p = 3;
            }
            else if (hour >= 12 && hour < 15)
            {
                p = 4;
            }
            else if (hour >= 15 && hour < 18)
            {
                p = 5;
            }
            else if (hour >= 18 && hour < 21)
            {
                p = 6;
            }
            else if (hour >= 21)
            {
                p = 7;

            }
            else
            {
                p = -1;
            }
            return p;
        }

        private void findcurrentBurst()
        {
            DateTime now = DateTime.Now.ToUniversalTime();
            int hour = now.Hour;
            int p = findHour();

            if (p < 0)
            {
                current_burst_band = "";
                current_burst_level = "";
                return;
            }
            string B = "";
            string O = "";

            int RSP = 0;
            int RNS = 0;
            int RBR = 0;
            int totalLevel = 0;
            string RLevel = "";

            string r = bd.RSPlevel[p].ToLower();
            try
            {
                if ((r.Contains("insignificant") || r.Contains("none")) && (bd.rbrLevel[p] == "" && bd.rnsLevel[p] == ""))
                {
                    Burstwarninglabel.Text = "No significant radio bursts";
                    RLevel = "none";
                    RSP = 0;
                }
                else
                {
                    if (r.Contains("weak"))
                    {
                        RSP = 1;
                    }
                    else if (r.Contains("moderate"))
                    {
                        RSP = 2;
                    }
                    else if (r.Contains("strong"))
                    {
                        RSP = 3;
                    }
                    else if (r.ToLower().Contains("very strong"))
                    {
                        RSP = 4;
                    }
                    else if (r.ToLower().Contains("extreme"))
                    {
                        RSP = 5;
                    }

                    if (bd.RSPband[p].ToLower().Contains("wideband"))
                    {
                        B = "wideband";

                    }
                    else
                    {
                        B = bd.RSPband[p];
                    }

                }
                if (bd.rbrLevel[p].ToLower().Contains("very low"))
                {
                    RBR = 0;
                }
                else if (bd.rbrLevel[p].ToLower().Contains("low"))
                {
                    RBR = 1;
                }
                else if (bd.rbrLevel[p].ToLower().Contains("moderate"))
                {
                    RBR = 2;
                }
                else if (bd.rbrLevel[p].ToLower().Contains("high"))
                {
                    RBR = 3;
                }
                else if (bd.rbrLevel[p].ToLower().Contains("very high"))
                {
                    RBR = 4;
                }
                else if (bd.rbrLevel[p].ToLower().Contains("extreme"))
                {
                    RBR = 5;
                }

                if (bd.rnsLevel[p].ToLower().Contains("very low"))
                {
                    RNS = 0;
                }
                else if (bd.rnsLevel[p].ToLower().Contains("low"))
                {
                    RNS = 1;
                }
                else if (bd.rnsLevel[p].ToLower().Contains("moderate"))
                {
                    RNS = 2;
                }
               
                else if (bd.rnsLevel[p].ToLower().Contains("very high"))
                {
                    RNS = 4;
                }
                else if (bd.rnsLevel[p].ToLower().Contains("high"))
                {
                    RNS = 3;
                }
                else if (bd.rnsLevel[p].ToLower().Contains("extreme"))
                {
                    RNS = 5;
                }

                if (RSP >= RBR)
                {
                    totalLevel = RSP;
                }
                else
                {
                    totalLevel = RBR;
                }
                if (totalLevel < RNS)
                {
                    totalLevel = RNS;
                }

                if (totalLevel == 0)
                {
                    RLevel = "none";
                }
                else if (totalLevel == 1)
                {
                    RLevel = "weak";
                }
                else if (totalLevel == 2)
                {
                    RLevel = "moderate";
                }
                else if (totalLevel == 3)
                {
                    RLevel = "strong";
                }
                else if (totalLevel ==4)
                {
                    RLevel = "very strong";
                }
                else if (totalLevel >= 5)
                {
                    RLevel = "extreme";

                }
                string R = "";
                if (RSP > 0)
                {
                    R = RLevel + " " + B + " radio sweep";
                }

                if (RBR > 0 && RBR > RNS)
                {
                    O = "short " + RLevel + " radio burst(s)";
                }
                if (RNS > 0 && RNS >= RBR)
                {
                    if (R.ToLower().Contains("radio sweep"))
                    {
                        R = " and " + R;
                    }
                    O = "longer " + RLevel + " radio noise storm(s)" + R;
                }
                O = "Reception may be affected by " + O;
                if (RSP == 0 && RBR == 0 && RNS == 0)
                {
                    O = "No significant radio bursts at present";
                }
                string P = findPreviousBurst(p);
                if (P != "" && O.Contains("No significant"))
                {
                    O = O + ", but " + P + " bursts reported earlier";
                }
                if (RLevel == "none")
                {
                    currentBurstlabel.Text = "none or no significant bursts";
                }
                else
                {
                    currentBurstlabel.Text = "some " +totalLevel + " bursts";
                }
            }
            catch { }
            Burstwarninglabel.Text = O;
        }


        private int findMaxBurst(int p)
        {


            if (p < 0)
            {
                current_burst_band = "";
                current_burst_level = "";
                return 0;
            }
  
            int RSP = 0;
            int RNS = 0;
            int RBR = 0;
            int totalLevel = 0;           

            string r = bd.RSPlevel[p].ToLower();
            try
            {
                if ((r.Contains("insignificant") || r.Contains("none")) && (bd.rbrLevel[p] == "" && bd.rnsLevel[p] == ""))
                {
                  
                    RSP = 0;
                }
                else
                {
                    if (r.Contains("weak"))
                    {
                        RSP = 1;
                    }
                    else if (r.Contains("moderate"))
                    {
                        RSP = 2;
                    }
                    else if (r.Contains("strong"))
                    {
                        RSP = 3;
                    }
                    else if (r.ToLower().Contains("very strong"))
                    {
                        RSP = 4;
                    }
                    else if (r.ToLower().Contains("extreme"))
                    {
                        RSP = 5;
                    }

                }
                if (bd.rbrLevel[p].ToLower().Contains("very low"))
                {
                    RBR = 0;
                }
                else if (bd.rbrLevel[p].ToLower().Contains("low"))
                {
                    RBR = 1;
                }
                else if (bd.rbrLevel[p].ToLower().Contains("moderate"))
                {
                    RBR = 2;
                }
                else if (bd.rbrLevel[p].ToLower().Contains("high"))
                {
                    RBR = 3;
                }
                else if (bd.rbrLevel[p].ToLower().Contains("very high"))
                {
                    RBR = 4;
                }
                else if (bd.rbrLevel[p].ToLower().Contains("extreme"))
                {
                    RBR = 5;
                }

                if (bd.rnsLevel[p].ToLower().Contains("very low"))
                {
                    RNS = 0;
                }
                else if (bd.rnsLevel[p].ToLower().Contains("low"))
                {
                    RNS = 1;
                }
                else if (bd.rnsLevel[p].ToLower().Contains("moderate"))
                {
                    RNS = 2;
                }

                else if (bd.rnsLevel[p].ToLower().Contains("very high"))
                {
                    RNS = 4;
                }
                else if (bd.rnsLevel[p].ToLower().Contains("high"))
                {
                    RNS = 3;
                }
                else if (bd.rnsLevel[p].ToLower().Contains("extreme"))
                {
                    RNS = 5;
                }

                if (RSP >= RBR)
                {
                    totalLevel = RSP;
                }
                else
                {
                    totalLevel = RBR;
                }
                if (totalLevel < RNS)
                {
                    totalLevel = RNS;
                }
                return totalLevel;
            
            }
            catch { return 0; }
        }

        private string findPreviousBurst(int p)
        {
            string S = "";
            int maxlevel = 0;
            int level = 0;
            try
            {
                if (p <1)
                { return ""; }

                for (int i = 0; i < p; i++)
                {
                    level = findMaxBurst(i);
                    if (level > maxlevel)
                    {
                        maxlevel = level;
                    }

                }
                
                    if (maxlevel == 0)
                    {
                        S = "";
                    }
                    else if (maxlevel == 1)
                    {
                        S = "weak";
                    }
                    else if (maxlevel == 2)
                    {
                        S = "moderate";
                    }
                    else if (maxlevel == 3)
                    {
                        S = "strong";
                    }
                    else if (maxlevel >= 4)
                    {
                        S = "very strong";
                    }
                else if (maxlevel >= 5)
                {
                    S = "extreme";
                }

            }
            catch { }
            return S;
        }

        /*private string findPreviousBurst()
        {
            string S = "";
            int level = 0;
            try
            {
                if (BurstdataGridView.Rows.Count > 0)
                {
                    int cols = BurstdataGridView.Columns.Count;
                    for (int i = 0; i < cols; i++)
                    {
                        if (BurstdataGridView.Rows[0].Cells[i].Value != null)
                        {
                            S = BurstdataGridView.Rows[0].Cells[i].Value.ToString();
                            if (S.ToLower().Contains("weak") || S.ToLower().Contains("low"))
                            {
                                if (level < 1)
                                {
                                    level = 1;
                                }
                            }
                            else if (S.ToLower().Contains("moderate"))
                            {
                                if (level < 2)
                                {
                                    level = 2;
                                }
                            }
                            else if (S.ToLower().Contains("extreme") || S.ToLower().Contains("very high"))
                            {
                                if (level < 4)
                                {
                                    level = 4;
                                }
                            }
                            else if (S.ToLower().Contains("strong") || S.ToLower().Contains("high"))
                            {
                                if (level < 3)
                                {
                                    level = 3;
                                }
                            }

                        }
                    }
                    if (level == 0)
                    {
                        S = "";
                    }
                    else if (level == 1)
                    {
                        S = "weak";
                    }
                    else if (level == 2)
                    {
                        S = "moderate";
                    }
                    else if (level == 3)
                    {
                        S = "strong";
                    }
                    else if (level >= 4)
                    {
                        S = "extreme";
                    }
                }
            }
            catch { }
            return S;
        }*/

        private void findcurrentBurst_OLD()
        {

            DateTime now = DateTime.Now.ToUniversalTime();
            int hour = now.Hour;
            int p = findHour();

            if (p < 0)
            {
                current_burst_band = "";
                current_burst_level = "";
                return;
            }
            string B = "";
            string O = "";

            string RSP = bd.RSPlevel[p].ToUpper();
            string RNS = bd.rnsSpan[p];
            string RBR = bd.rbrSpan[p];

            if ((RSP.ToLower().Contains("insignificant") || RSP.ToLower().Contains("none")) )
            {
                Burstwarninglabel.Text = "No significant radio bursts";

            }
            else
            {
                if (!RSP.ToLower().Contains("weak") && RSP != "")
                {
                    if (bd.RSPband[p].ToLower().Contains("wideband"))
                    {
                        B = "wideband";
                        O = "";
                    }
                    else
                    {
                        O = "affecting " + bd.RSPband[p];
                        B = "";
                    }

                    Burstwarninglabel.Text = "Reception may be affected by " + bd.RSPlevel[p].ToLower() + " " + B + " radio bursts " + O;
                }
            }

            if (bd.RSPlevel[p].ToLower().Contains("none"))
            {
                currentBurstlabel.Text = bd.RSPlevel[p].ToLower() + " bursts";
            }
            else
            {
                currentBurstlabel.Text = "some " + bd.RSPlevel[p] + " " + bd.RSPband[p] + " bursts";
            }



        }






        private async void forceUpdatebutton_Click_1(object sender, EventArgs e)
        {
            if (stopUrl)
            {
                Msg.TMessageBox("You have disabled the Internet connection", "Solar data", 1000);
                return;
            }
            string url = "https://services.swpc.noaa.gov/";
            if (!await Msg.IsUrlReachable(url))
            {
                Msg.TMessageBox("Unable to connect to NOAA url", "Solar data", 1000);
                return;
            }
            await updateGeo(server, user, pass, true);
            await updateSolar(server, user, pass);

            //await find_data(false, "", "");

            await updateAllProtonandFlare(server, user, pass, false);

        }



        private async void Switchbutton_Click(object sender, EventArgs e)
        {
            if (Switchbutton.Text == "Database")
            {
                Switchbutton.Text = "Current data";
                groupBox1.Visible = false;
                groupBox2.Visible = true;
                await find_data(false, "", ""); //do not filter
            }
            else
            {
                Switchbutton.Text = "Database";
                groupBox1.Visible = true;
                groupBox2.Visible = false;
            }
        }
        public async Task SaveSolardata(DateTime date)
        {

            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_sol";
            MySqlConnection connection = new MySqlConnection(myConnectionString);

            string datetime = date.ToString("yyyy-MM-dd");
            try
            {

                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO weather(datetime,flux,SSN,Xray) ";
                command.CommandText += "VALUES('" + datetime + "', '" + solar.flux + "', '" + solar.SSN + "', '" + solar.Xray + "')";

                command.CommandText += " ON DUPLICATE KEY UPDATE flux = '" + solar.flux + "', SSN = '" + solar.SSN + "', Xray = '" + solar.Xray + "'";
                connection.Open();
                command.ExecuteNonQuery();

                connection.Close();

            }
            catch
            {
                connection.Close();
            }


        }

        public async Task saveGeoData(DateTime date)
        {

            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_sol";
            MySqlConnection connection = new MySqlConnection(myConnectionString);
            //date = date.Date;   //set h,m,s to 000
            string datetime = date.ToString("yyyy-MM-dd");
            try
            {

                MySqlCommand command = connection.CreateCommand();

                command.CommandText = "INSERT INTO weather(datetime,Ap,Kp00,Kp03,Kp06,Kp09,Kp12,Kp15,Kp18,Kp21) ";
                command.CommandText += "VALUES('" + datetime + "', '" + solar.Ap + "', '" + solar.K00 + "', '" + solar.K03 + "', ";
                command.CommandText += "'" + solar.K06 + "', '" + solar.K09 + "', '" + solar.K12 + "', '" + solar.K15 + "'";
                command.CommandText += ", '" + solar.K18 + "', '" + solar.K21 + "')";
                command.CommandText += " ON DUPLICATE KEY UPDATE Ap = '" + solar.Ap + "', Kp00 = '" + solar.K00 + "'";
                command.CommandText += ", Kp03 = '" + solar.K03 + "', Kp06 = '" + solar.K06 + "', Kp09 = '" + solar.K09 + "'";
                command.CommandText += ", Kp12 = '" + solar.K12 + "', Kp15 = '" + solar.K15 + "', Kp18 = '" + solar.K18 + "'";
                command.CommandText += ", Kp21 = '" + solar.K21 + "'";

                connection.Open();


                command.ExecuteNonQuery();


                connection.Close();

            }
            catch
            {
                connection.Close();
            }


        }

        private string Truncate(string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (str.Length <= maxLength) { return str; }
            else
            {
                return str.Substring(0, maxLength);
            }
        }

        public async Task SaveBurstdata(DateTime date)
        {

            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_sol";
            MySqlConnection connection = new MySqlConnection(myConnectionString);

            int t = 200;
            rb.s00 = Truncate(rb.s00, t);
            rb.s03 = Truncate(rb.s03, t);
            rb.s06 = Truncate(rb.s06, t);
            rb.s09 = Truncate(rb.s09, t);
            rb.s12 = Truncate(rb.s12, t);
            rb.s15 = Truncate(rb.s15, t);
            rb.s18 = Truncate(rb.s18, t);
            rb.s21 = Truncate(rb.s21, t);

            string datetime = date.ToString("yyyy-MM-dd");
            try
            {

                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO weather(datetime,s00,s03,s06,s09,s12,s15,s18,s21) ";
                command.CommandText += "VALUES('" + datetime + "', '" + rb.s00 + "', '" + rb.s03 + "', '" + rb.s06 + "', '" + rb.s09 + "', '" + rb.s12 + "', '" + rb.s15 + "', '" + rb.s18 + "', '" + rb.s21 + "')";

                command.CommandText += " ON DUPLICATE KEY UPDATE s00 = '" + rb.s00 + "', s03 = '" + rb.s03 + "', s06 = '" + rb.s06 + "', s09 = '" + rb.s09 + "' ";
                command.CommandText += ", s12 = '" + rb.s12 + "', s15 = '" + rb.s15 + "', s18 = '" + rb.s18 + "', s21 = '" + rb.s21 + "'";
                connection.Open();
                command.ExecuteNonQuery();

                connection.Close();

            }
            catch
            {
                connection.Close();
            }


        }

        private async Task find_data(bool filter, string datetime1, string datetime2) //find a slot row for display in grid from the database corresponding to the date/time from the slot
        {
            DataTable Slots = new DataTable();
            dataGridView2.Rows.Clear();
            //DateTime d = new DateTime();
            int i = 0;
            bool found = false;
            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_sol";
            MySqlConnection connection = new MySqlConnection(myConnectionString);



            try
            {
                connection.Open();

                MySqlCommand command = connection.CreateCommand();

                string Ap = "";
                string SSN = "";
                string and = "";
                string D = "";
                if (!filter && !datecheckBox.Checked)
                {

                    command.CommandText = "SELECT * FROM weather ORDER BY datetime DESC LIMIT " + 500;
                }
                else
                {
                    if (AptextBox1.Text != "")
                    {
                        Ap = " Ap >= '" + AptextBox1.Text + "' ";
                        and = "AND";
                    }
                    if (AptextBox2.Text != "")
                    {
                        Ap = Ap + and + " Ap <= '" + AptextBox2.Text + "' ";
                        and = "AND";
                    }
                    if (SSNtextBox1.Text != "")
                    {
                        SSN = and + " SSN >= '" + SSNtextBox1.Text + "' ";
                        and = "AND";
                    }
                    if (SSNtextBox2.Text != "")
                    {
                        SSN = SSN + and + " SSN <= '" + SSNtextBox2.Text + "' ";
                    }
                    if (datecheckBox.Checked)
                    {
                        D = " datetime >= '" + datetime1 + "' AND datetime <= '" + datetime2 + "'";
                    }
                    //command.CommandText = "SELECT * FROM received WHERE datetime >= '" + datetime1 + "' AND datetime <= '" + datetime2 + "' AND " + bandstr + callstr + fromstr + tostr + " ORDER BY datetime DESC LIMIT " + maxrows;
                    command.CommandText = "SELECT * FROM weather WHERE " + D + Ap + SSN + " ORDER BY datetime DESC LIMIT " + 500;
                }
                MySqlDataReader Reader;
                Reader = command.ExecuteReader();

                DateTime Today = DateTime.Now.ToUniversalTime();
                string today = Today.ToString("yyyy-MM-dd");

                while (Reader.Read())
                {
                    found = true;
                    try
                    {
                        DateTime dt = (DateTime)Reader["datetime"];
                        string date = dt.ToString("yyyy-MM-dd");
                        solar.Ap = Convert.ToString((int)Reader["Ap"]);

                        solar.K00 = ((float)Reader["Kp00"]).ToString("F2");

                        solar.K03 = ((float)Reader["Kp03"]).ToString("F2");
                        solar.K06 = ((float)Reader["Kp06"]).ToString("F2");
                        solar.K09 = ((float)Reader["Kp09"]).ToString("F2");
                        solar.K12 = ((float)Reader["Kp12"]).ToString("F2");
                        solar.K15 = ((float)Reader["Kp15"]).ToString("F2");
                        solar.K18 = ((float)Reader["Kp18"]).ToString("F2");
                        solar.K21 = ((float)Reader["Kp21"]).ToString("F2");
                        try
                        {
                            solar.flux = ((float)Reader["flux"]).ToString("F0"); ;
                            solar.SSN = Convert.ToString((int)Reader["SSN"]);
                            solar.Xray = (string)Reader["Xray"];
                        }
                        catch
                        {

                        }

                        cells1[0] = date;
                        cells1[1] = zeros(solar.Ap);
                        cells1[2] = zeros(solar.K00) + findKplevel(solar.K00);
                        cells1[3] = zeros(solar.K03) + findKplevel(solar.K03);
                        cells1[4] = zeros(solar.K06) + findKplevel(solar.K06);
                        cells1[5] = zeros(solar.K09) + findKplevel(solar.K09);
                        cells1[6] = zeros(solar.K12) + findKplevel(solar.K12);
                        cells1[7] = zeros(solar.K15) + findKplevel(solar.K15);
                        cells1[8] = zeros(solar.K18) + findKplevel(solar.K18);
                        cells1[9] = zeros(solar.K21) + findKplevel(solar.K21);
                        if (date == today)
                        {
                            cells1[10] = "-";
                            cells1[11] = "-";
                            cells1[12] = "-";
                        }
                        else
                        {
                            cells1[10] = zeros(solar.flux);
                            cells1[11] = solar.SSN;
                            cells1[12] = solar.Xray;
                        }

                        update_grid2(); //add this row to the datagridview
                    }
                    catch { }

                }
                Reader.Close();
                connection.Close();

            }
            catch
            {
                Msg.TMessageBox("Error reading data from database", "Solar data", 2000);
                found = false;

                connection.Close();
            }
        }

        private string zeros(string Cstr)
        {
            string S = "";
            string[] c = Cstr.Split('.');
            if (c.Count() > 1)
            {
                if (c[1] == "0" || c[1] == "00")
                {
                    S = c[0];
                }
                else
                {
                    S = Cstr;
                }
            }
            else
            {
                S = Cstr;
            }
            return S;
        }

        private void update_grid2() //add rows to the datagridview
        {

            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dataGridView2);
            for (int i = 0; i < dataGridView2.Columns.Count; i++)
            {

                row.Cells[i].Value = cells1[i];
            }

            dataGridView2.Rows.Add(row);
            if (dataGridView2.Rows.Count > 0)
            {
                dataGridView2.AllowUserToAddRows = false;
            }
        }

        private void filterbutton_Click(object sender, EventArgs e)
        {
            if (filterbutton.Text == "Apply")
            {
                if (dataGridView3.Visible)
                {
                    filter_extra_results();
                }
                else
                {
                    filter_results();
                }
                filterbutton.Text = "Clear";
                Burstbutton.Visible = false;
            }
            else
            {
                if (dataGridView3.Visible)
                {
                    show_extra_results();
                }
                else
                {
                    show_results();
                }
                filterbutton.Text = "Apply";
                Burstbutton.Visible = true;
                Burstbutton.Text = "Show bursts";
            }
        }

        private int table_count()
        {
            int count;
            string connectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_sol";

            try
            {
                //string connectionString = "Server=server;Port=3306;Database=wspr;User ID=user;Password=pass;";

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM weather", connection))
                    {
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                    connection.Close();
                }
                return count;

            }
            catch
            {
                return 0;
            }
        }
        public async Task show_results() // read back from the reported table to populate the datagridview
        {
            dataGridView2.Rows.Clear();
            dataGridView2.Sort(dataGridView2.Columns[0], ListSortDirection.Descending);  //order by date

            int rows = table_count();
            if (rows > 0)
            {
                find_data(false, "", "");

            }
            dataGridView2.Sort(dataGridView2.Columns[0], ListSortDirection.Descending);  //order by date        

        }

        private void filter_results()
        {
            dataGridView2.Rows.Clear();
            dataGridView2.Sort(dataGridView2.Columns[0], ListSortDirection.Descending);  //order by date
            DateTime dt1 = dateTimePicker1.Value;
            DateTime dt2 = dateTimePicker2.Value;
            //dt = dt.AddHours(-2);
            string from = dt1.ToString("yyyy-MM-dd 00:00:00");
            string to = dt2.ToString("yyyy-MM-dd 00:00:00");
            int rows = table_count();

            if (rows > 0)
            {
                find_data(true, from, to);

            }

            dataGridView2.Sort(dataGridView2.Columns[0], ListSortDirection.Descending);  //order by date
        }

        private void AptextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

            string maxstr = "400";

            int maxA = Convert.ToInt32(maxstr);
            int t;
            if (e.KeyChar == 45) //no minus allowed
            {
                e.Handled = true;
                return;
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) //will allow offset from -10 to +210 for adjustment
            {
                e.Handled = true;
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && !char.IsSymbol("-", e.KeyChar)) //will also accept - offset up to -10 and +ve up to 210
            {
                e.Handled = true;
            }
            else
            {
                int.TryParse(AptextBox1.Text + e.KeyChar, out t);
                if (t < 0 || t > maxA)
                {
                    MessageBox.Show("Error: range -1 " + maxA, "");
                    e.Handled = true;
                }
            }
        }

        private void AptextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void AptextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {

            string maxstr = "400";

            int maxA = Convert.ToInt32(maxstr);
            int t;
            if (e.KeyChar == 45) //no minus allowed
            {
                e.Handled = true;
                return;
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) //will allow offset from -10 to +210 for adjustment
            {
                e.Handled = true;
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && !char.IsSymbol("-", e.KeyChar)) //will also accept - offset up to -10 and +ve up to 210
            {
                e.Handled = true;
            }
            else
            {
                int.TryParse(AptextBox2.Text + e.KeyChar, out t);
                if (t < 0 || t > maxA)
                {
                    MessageBox.Show("Error: range 0 " + maxA, "");
                    e.Handled = true;
                }
            }
        }

        private void SSNtextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

            string maxstr = "400";

            int maxS = Convert.ToInt32(maxstr);
            int t;
            if (e.KeyChar == 45) //no minus allowed
            {
                e.Handled = true;
                return;
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) //will allow offset from -10 to +210 for adjustment
            {
                e.Handled = true;
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && !char.IsSymbol("-", e.KeyChar)) //will also accept - offset up to -10 and +ve up to 210
            {
                e.Handled = true;
            }
            else
            {
                int.TryParse(SSNtextBox1.Text + e.KeyChar, out t);
                if (t < 0 || t > maxS)
                {
                    MessageBox.Show("Error: range 0 " + maxS, "");
                    e.Handled = true;
                }
            }
        }

        private void SSNtextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            string maxstr = "400";

            int maxS = Convert.ToInt32(maxstr);
            int t;
            if (e.KeyChar == 45) //no minus allowed
            {
                e.Handled = true;
                return;
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) //will allow offset from -10 to +210 for adjustment
            {
                e.Handled = true;
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && !char.IsSymbol("-", e.KeyChar)) //will also accept - offset up to -10 and +ve up to 210
            {
                e.Handled = true;
            }
            else
            {
                int.TryParse(SSNtextBox2.Text + e.KeyChar, out t);
                if (t < 0 || t > maxS)
                {
                    MessageBox.Show("Error: range 0 " + maxS, "");
                    e.Handled = true;
                }
            }
        }
        private async Task setpflux(int h, string fluxdata)
        {
            switch (h)
            {
                case 0: { pflux.pf00 = fluxdata; break; }
                case 3: { pflux.pf03 = fluxdata; break; }
                case 6: { pflux.pf06 = fluxdata; break; }
                case 9: { pflux.pf09 = fluxdata; break; }
                case 12: { pflux.pf12 = fluxdata; break; }
                case 15: { pflux.pf15 = fluxdata; break; }
                case 18: { pflux.pf18 = fluxdata; break; }
                case 21: { pflux.pf21 = fluxdata; break; }
            }

        }

        private async Task setflare(int h, string flaredata)
        {
            switch (h)
            {
                case 0: { flare.fl00 = flaredata; break; }
                case 3: { flare.fl03 = flaredata; break; }
                case 6: { flare.fl06 = flaredata; break; }
                case 9: { flare.fl09 = flaredata; break; }
                case 12: { flare.fl12 = flaredata; break; }
                case 15: { flare.fl15 = flaredata; break; }
                case 18: { flare.fl18 = flaredata; break; }
                case 21: { flare.fl21 = flaredata; break; }
            }

        }

        private string findS2(string pf)
        {
            string S = "";
            int s = 0;
            if (pf.Trim() == "" || pf == null)
            {
                return "";
            }

            string[] P = pf.Split('/');
            if (P.Count() > 0)
            {
                double t = 0;
                double.TryParse(P[1], out t);
                if (t >= 10)
                {
                    s = 1;
                    S = "S1";
                }
                if (t >= 100)
                {
                    s = 2;
                    S = "S2";
                }
                if (t >= 1000)
                {
                    s = 3;
                    S = "S3";
                }
                if (t >= 10000)
                {
                    s = 4;
                    S = "S4";
                }
                if (t > 100000)
                {
                    s = 5;
                    S = "S5";
                }
                if (S != "")
                {
                    S = "-" + S;
                }
            }
            return S;
        }


        private string findR2(string C)
        {
            string R = "";
            string S = "";
            int r = 0;
            int x = 0;
            if (C.Trim() == "" || C == null)
            {
                return "";
            }
            try
            {

                string[] F = C.Split('/');
                R = "";
                if (F.Count() > 0)
                {
                    for (int i = 0; i < F.Count(); i++)
                    {

                        char c = F[i][0];
                        S = F[i].Substring(1);
                        double t = 0;

                        double.TryParse(S, out t);
                        if (t > 0)
                        {
                            if (c == 'M')
                            {
                                if (t < 5 && x < 1)
                                {
                                    r = 1;
                                    R = "R1";
                                }
                                else if (t >= 5 && x < 2)
                                {
                                    r = 2;
                                    R = "R2";
                                }
                            }
                            else if (c == 'X' && x < 3)
                            {
                                if (t < 10)
                                {
                                    r = 3;
                                    R = "R3";
                                }
                                else if (t >= 10 && t < 20 && x < 4)
                                {
                                    r = 4;
                                    R = "R4";
                                }
                                else if (t >= 20 && x < 5)
                                {
                                    r = 5;
                                    R = "R5";
                                }
                                x = r;
                            }

                        }

                    }
                }
                else
                {
                    R = "";
                }
                if (R != "")
                {
                    R = " (" + R + ")";
                }

            }
            catch { }

            return R;
        }

        private string findR(string C, bool addbracket)
        {
            char c = C[0];
            string R = C.Substring(1);
            double t = 0;

            double.TryParse(R, out t);
            if (t > 0)
            {
                if (c == 'M')
                {
                    if (t < 5)
                    {
                        R = "R1";
                    }
                    else if (t >= 5)
                    {
                        R = "R2";
                    }
                }
                else if (c == 'X')
                {
                    if (t < 10)
                    {
                        R = "R3";
                    }
                    else if (t >= 10 && t < 20)
                    {
                        R = "R4";
                    }
                    else if (t >= 20)
                    {
                        R = "R5";
                    }
                }
                else
                {
                    R = "";
                }
            }
            else
            {
                R = "";
            }
            if (R != "" && addbracket)
            {
                R = "/ " + R;
            }


            return R;
        }

        public async Task updateAllProtonandFlare(string server, string user, string pass, bool yesterday)
        {
            int h = 0;
            while (h < 24)
            {

                satErr = false;
                await getProtonFlux(h, yesterday, true); //yesterday = false, primary = true

                if (satErr)
                {
                    await getProtonFlux(h, yesterday, false); //yesterday = false, primary = false (secondary)
                }
                await setpflux(h, fluxdata);
                h = h + 3;
            }
            h = 0;
            while (h < 24)
            {
                satErr = false;
                await getSolarFlares(h, yesterday, true); //yesterday = false, primary = true

                if (satErr)
                {
                    await getSolarFlares(h, yesterday, false);
                }

                await setflare(h, flaredata);
                h = h + 3;
            }

            DateTime date = DateTime.Now.ToUniversalTime();
            if (yesterday)
            {
                date = date.AddDays(-1);
            }
            date = date.Date;
            await SavePFdata(date);
            await SaveFlaredata(date);
            Glevel = 0;
            Rlevel = 0;
            Slevel = 0;
            await find_extra_data(false, "", "");

            findstormlevels();
            if (!yesterday)
            {
                stormlabels();
            }
            if (!yesterday)
            {
                updateBursts(server, user, pass);
            }

        }



        private void findstormlevels()
        {
            int r = 0;
            int s = 0;
            int g = 0;
            try
            {
                if (dataGridView3.Rows.Count > 2)
                {
                    for (int i = 1; i < dataGridView3.Columns.Count; i++)
                    {
                        if (dataGridView3.Rows[0].Cells[i].Value.ToString().Contains("S"))
                        {
                            string S = dataGridView3.Rows[0].Cells[i].Value.ToString();
                            int index = S.IndexOf("S");
                            if (index > -1)
                            {
                                S = S.Substring(index + 1, 1);
                                int t = 0;
                                int.TryParse(S, out t);
                                if (t > s)
                                { s = t; }
                            }
                        }
                        Slevel = s;
                        if (dataGridView3.Rows[1].Cells[i].Value.ToString().Contains("R"))
                        {
                            string R = dataGridView3.Rows[1].Cells[i].Value.ToString();
                            int index = R.IndexOf("R");
                            if (index > -1)
                            {
                                R = R.Substring(index + 1, 1);
                                int t = 0;
                                int.TryParse(R, out t);
                                if (t > r)
                                { r = t; }
                            }
                        }
                        Rlevel = r;
                    }
                }
                if (dataGridView1.Rows.Count > 1)
                {
                    for (int i = 2; i < dataGridView1.Columns.Count - 3; i++)
                    {
                        for (int row = 0; row < 2; row++)
                        {
                            string G = dataGridView1.Rows[row].Cells[i].Value.ToString();
                            int index = G.IndexOf("G");
                            if (index > -1)
                            {
                                G = G.Substring(index + 1, 1);
                                int t = 0;
                                int.TryParse(G, out t);
                                if (t > g)
                                { g = t; }
                            }
                        }
                    }
                    Glevel = g;
                }
            }
            catch
            {

            }
        }
        private void stormlabels()
        {
            if (Glevel > 0)
            {
                Glabel.Text = "G" + Glevel;
            }
            else
            {
                Glabel.Text = "--";
            }
            if (Slevel > 0)
            {
                Slabel.Text = "S" + Slevel;
            }
            else
            {
                Slabel.Text = "--";
            }
            if (Rlevel > 0)
            {
                Rlabel.Text = "R" + Rlevel;
            }
            else
            {
                Rlabel.Text = "--";
            }
            if (activity_level.Contains("storm"))
            {
                stormlabel.Text = activity_level;
            }
            else
            {
                stormlabel.Text = "--";
            }
            findConditions(SFI);
        }

        /*public async Task updateProtonandFlare(string server, string user, string pass, bool yesterday, int h)
        {

            satErr = false;
            await getProtonFlux(h, yesterday, true); //yesterday = false, primary = true

            if (satErr)
            {
                await getProtonFlux(h, yesterday, false); //yesterday = false, primary = false (secondary)
            }
            await setpflux(h, fluxdata);


            satErr = false;
            await getSolarFlares(h, yesterday, true); //yesterday = false, primary = true

            if (satErr)
            {
                await getSolarFlares(h, yesterday, false);
            }

            await setflare(h, flaredata);

            DateTime date = DateTime.Now.ToUniversalTime();
            if (yesterday)
            {
                date = date.AddDays(-1);
            }
            date = date.Date;
            flaredata = "";
            fluxdata = "";
            await SavePFdata(date);
            await SaveFlaredata(date);
            Glevel = 0;
            Rlevel = 0;
            Slevel = 0;
            await find_extra_data(false, "", "");
            if (!yesterday)
            {
                stormlabels();
            }
        }*/

        public async Task SavePFdata(DateTime date)
        {

            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_sol";
            MySqlConnection connection = new MySqlConnection(myConnectionString);

            string datetime = date.ToString("yyyy-MM-dd");
            try
            {

                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO weather(datetime,pf00,pf03,pf06,pf09,pf12,pf15,pf18,pf21) ";
                command.CommandText += "VALUES('" + datetime + "', '" + pflux.pf00 + "', '" + pflux.pf03 + "', '" + pflux.pf06 + "', ";
                command.CommandText += "'" + pflux.pf09 + "', '" + pflux.pf12 + "', '" + pflux.pf15 + "', '" + pflux.pf18 + "', '" + pflux.pf21 + "') ";
                command.CommandText += " ON DUPLICATE KEY UPDATE pf00 = '" + pflux.pf00 + "', pf03 = '" + pflux.pf06 + "', pf06 = '" + pflux.pf06 + "' ";
                command.CommandText += ", pf09 = '" + pflux.pf09 + "', pf12 = '" + pflux.pf12 + "', pf15 = '" + pflux.pf15 + "', pf18 = '" + pflux.pf18 + "'";
                command.CommandText += ", pf21 = '" + pflux.pf21 + "'";
                connection.Open();
                command.ExecuteNonQuery();

                connection.Close();

            }
            catch
            {         //if row already exists then try updating it in database
                connection.Close();

            }


        }

        public async Task SaveFlaredata(DateTime date)
        {

            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_sol";
            MySqlConnection connection = new MySqlConnection(myConnectionString);

            string datetime = date.ToString("yyyy-MM-dd");
            try
            {

                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO weather(datetime,fl00,fl03,fl06,fl09,fl12,fl15,fl18,fl21) ";
                command.CommandText += "VALUES('" + datetime + "', '" + flare.fl00 + "', '" + flare.fl03 + "', '" + flare.fl06 + "', ";
                command.CommandText += "'" + flare.fl09 + "', '" + flare.fl12 + "', '" + flare.fl15 + "', '" + flare.fl18 + "', '" + flare.fl21 + "') ";
                command.CommandText += " ON DUPLICATE KEY UPDATE fl00 = '" + flare.fl00 + "', fl03 = '" + flare.fl03 + "', fl06 = '" + flare.fl06 + "' ";
                command.CommandText += ", fl09 = '" + flare.fl09 + "', fl12 = '" + flare.fl12 + "', fl15 = '" + flare.fl15 + "', fl18 = '" + flare.fl18 + "'";
                command.CommandText += ", fl21 = '" + flare.fl21 + "'";

                connection.Open();
                command.ExecuteNonQuery();

                connection.Close();

            }
            catch
            {         //if row already exists then try updating it in database
                connection.Close();

            }


        }

        public async Task show_extra_results() // read back from the reported table to populate the datagridview
        {

            find_extra_data(false, "", "");

        }
        private void filter_extra_results()
        {
            dataGridView3.Rows.Clear();
            dataGridView3.Sort(dataGridView3.Columns[0], ListSortDirection.Descending);  //order by date
            DateTime dt1 = dateTimePicker1.Value;
            DateTime dt2 = dateTimePicker2.Value;
            //dt = dt.AddHours(-2);
            string from = dt1.ToString("yyyy-MM-dd 00:00:00");
            string to = dt2.ToString("yyyy-MM-dd 00:00:00");
            int rows = table_count();

            if (rows > 0)
            {
                find_extra_data(true, from, to);

            }

            //dataGridView3.Sort(dataGridView3.Columns[0], ListSortDirection.Descending);  //order by date
        }

        private async Task find_extra_data(bool filter, string datetime1, string datetime2) //find a slot row for display in grid from the database corresponding to the date/time from the slot
        {

            cells2[1] = "";
            cells2[2] = "";
            cells2[3] = "";
            cells2[4] = "";
            cells2[5] = "";
            cells2[6] = "";
            cells2[7] = "";
            cells2[8] = "";
            DataTable Slots = new DataTable();
            dataGridView3.Rows.Clear();

            int i = 0;
            bool found = false;
            string myConnectionString = "server=" + server + ";user id=" + user + ";password=" + pass + ";database=wspr_sol";

            MySqlConnection connection = new MySqlConnection(myConnectionString);


            try
            {

                connection.Open();
                MySqlCommand command = connection.CreateCommand();

                string C = "";
                string and = "";
                string D = "";
                string order = " ORDER BY datetime DESC LIMIT " + 500;
                if (!filter && !datecheckBox.Checked)
                {

                    command.CommandText = "SELECT * FROM weather ORDER BY datetime DESC LIMIT " + 500;
                }
                else
                {
                    if (flarelistBox.SelectedIndex > -1) //find flare by class
                    {
                        C = flarelistBox.SelectedItem.ToString();
                        C = "%" + C + "%";
                        C = "'" + C + "'";
                        string S = "fl00 LIKE " + C + " OR fl03 LIKE " + C + " OR fl06 LIKE " + C + " OR fl09 LIKE " + C + " OR fl12 LIKE " + C;
                        S += " OR fl15 LIKE " + C + " OR fl18 LIKE " + C + " OR fl21 LIKE " + C;
                        C = S;

                    }


                    if (datecheckBox.Checked)
                    {
                        D = " datetime >= '" + datetime1 + "' AND datetime <= '" + datetime2 + "'";
                        if (C != "")
                        { and = " AND "; }
                    }

                    command.CommandText = "SELECT * FROM weather WHERE " + D + and + C + order;
                }
                MySqlDataReader Reader;
                Reader = command.ExecuteReader();

                int rows = table_count();
                int rcount = 0;

                while (Reader.Read())
                {
                    found = true;
                    DateTime dt = (DateTime)Reader["datetime"];
                    string date = dt.ToString("yyyy-MM-dd");
                    pflux.pf00 = (string)Reader["pf00"];
                    pflux.pf03 = (string)Reader["pf03"];
                    pflux.pf06 = (string)Reader["pf06"];
                    pflux.pf09 = (string)Reader["pf09"];
                    pflux.pf12 = (string)Reader["pf12"];
                    pflux.pf15 = (string)Reader["pf15"];
                    pflux.pf18 = (string)Reader["pf18"];
                    pflux.pf21 = (string)Reader["pf21"];

                    flare.fl00 = (string)Reader["fl00"];
                    flare.fl03 = (string)Reader["fl03"];
                    flare.fl06 = (string)Reader["fl06"];
                    flare.fl09 = (string)Reader["fl09"];
                    flare.fl12 = (string)Reader["fl12"];
                    flare.fl15 = (string)Reader["fl15"];
                    flare.fl18 = (string)Reader["fl18"];
                    flare.fl21 = (string)Reader["fl21"];
                    string[] s = new string[8];
                    s[0] = findS2(pflux.pf00);
                    s[1] = findS2(pflux.pf03);
                    s[2] = findS2(pflux.pf06);
                    s[3] = findS2(pflux.pf09);
                    s[4] = findS2(pflux.pf12);
                    s[5] = findS2(pflux.pf15);
                    s[6] = findS2(pflux.pf18);
                    s[7] = findS2(pflux.pf21);

                    cells2[0] = date;
                    cells2[1] = pflux.pf00 + s[0];
                    cells2[2] = pflux.pf03 + s[1];
                    cells2[3] = pflux.pf06 + s[2];
                    cells2[4] = pflux.pf09 + s[3];
                    cells2[5] = pflux.pf12 + s[4];
                    cells2[6] = pflux.pf15 + s[5];
                    cells2[7] = pflux.pf18 + s[6];
                    cells2[8] = pflux.pf21 + s[7];
                    DateTime now = DateTime.Now.ToUniversalTime();
                    if (dt.Date == now.Date)
                    {
                        find_current_S(s);
                    }

                    string[] f = new string[8];
                    f[0] = findR2(flare.fl00);
                    f[1] = findR2(flare.fl03);
                    f[2] = findR2(flare.fl06);
                    f[3] = findR2(flare.fl09);
                    f[4] = findR2(flare.fl12);
                    f[5] = findR2(flare.fl15);
                    f[6] = findR2(flare.fl18);
                    f[7] = findR2(flare.fl21);
                    cells3[0] = "Solar flares:";
                    cells3[1] = flare.fl00 + f[0];
                    cells3[2] = flare.fl03 + f[1];
                    cells3[3] = flare.fl06 + f[2];
                    cells3[4] = flare.fl09 + f[3];
                    cells3[5] = flare.fl12 + f[4];
                    cells3[6] = flare.fl15 + f[5];
                    cells3[7] = flare.fl18 + f[6];
                    cells3[8] = flare.fl21 + f[7];
                    if (dt.Date == now.Date)
                    {
                        find_current_R(f);
                    }
                    if (rcount < rows)
                    {
                        update_grid3(); //add this row to the datagridview
                        rcount++;
                    }


                }
                Reader.Close();
                connection.Close();

            }
            catch
            {
                found = false;
                connection.Close();
            }
        }

        private void find_current_S(string[] s)
        {
            DateTime now = DateTime.Now.ToUniversalTime();
            try
            {
                string pfl = "--";
                int hour = now.Hour;
                int h = ((hour - 3) / 3) * 3;
                if (hour < 3)
                {
                    h = 0;
                }
                switch (h)
                {
                    case 0: { pfl = s[0]; break; }
                    case 3: { pfl = s[1]; break; }
                    case 6: { pfl = s[2]; break; }
                    case 9: { pfl = s[3]; break; }
                    case 12: { pfl = s[4]; break; }
                    case 15: { pfl = s[5]; break; }
                    case 18: { pfl = s[6]; break; }
                    case 21: { pfl = s[7]; break; }
                }

                pfl = pfl.Replace("-", "");

                if (pfl == "")
                {
                    pfl = "--";
                }
                SClabel.Text = pfl;
            }
            catch
            {
            }
        }

        private void find_current_R(string[] r)
        {
            DateTime now = DateTime.Now.ToUniversalTime();
            try
            {
                string fl = "--";
                int hour = now.Hour;
                int h = ((hour - 3) / 3) * 3;
                if (hour < 3)
                {
                    h = 0;
                }
                switch (h)
                {
                    case 0: { fl = r[0]; break; }
                    case 3: { fl = r[1]; break; }
                    case 6: { fl = r[2]; break; }
                    case 9: { fl = r[3]; break; }
                    case 12: { fl = r[4]; break; }
                    case 15: { fl = r[5]; break; }
                    case 18: { fl = r[6]; break; }
                    case 21: { fl = r[7]; break; }
                }
                RClabel.Text = fl;
                fl = fl.Replace("(", "");
                fl = fl.Replace(")", "");
                if (fl == "")
                {
                    fl = "--";
                }
                RClabel.Text = fl;
            }
            catch
            {
            }
        }
        private void update_grid3() //add rows to the datagridview
        {

            int columns = dataGridView3.ColumnCount;
            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dataGridView3);
            for (int i = 0; i < columns; i++)
            {

                row.Cells[i].Value = cells2[i];
            }

            dataGridView3.Rows.Add(row);

            DataGridViewRow row2 = new DataGridViewRow();
            row2.CreateCells(dataGridView3);
            for (int i = 0; i < columns; i++)
            {

                row2.Cells[i].Value = cells3[i];
            }

            dataGridView3.Rows.Add(row2);
            if (dataGridView3.Rows.Count > 0)
            {
                dataGridView3.AllowUserToAddRows = false;
            }

        }
        /*private class ProtonFluxData
        {
            public string time_tag { get; set; }
            public string satellite { get; set; }
            public string energy { get; set; }
            public double flux { get; set; }
            public string units { get; set; }
        }*/

        private async Task getProtonFlux(int h1, bool yesterday, bool primary)
        {
            fluxdata = "";
            string sat = "primary";
            if (!primary)
            {
                sat = "secondary";
            }


            string url = "https://services.swpc.noaa.gov/json/goes/" + sat + "/integral-protons-3-day.json";
            if (stopUrl)
            {
                return;
            }

            bool ok = false;
            ok = await Msg.IsUrlReachable(url);
            if (!ok)
            {
                Msg.TMessageBox("Unable to reach NOAA", "GOES data ", 1500); return;
            }

            using HttpClient client = new HttpClient();
            string satno = "";
            try
            {

                int h2 = h1 + 2; //do this for 2h55mins
                int m1 = 0;
                int m2 = 55;    //measure proton flux from 5 min data from GOES in 3 hour periods
                string json = await client.GetStringAsync(url);
                JsonArray array = JsonNode.Parse(json).AsArray();
                if (array == null)
                {
                    Msg.TMessageBox("Not responding", "", 1500);
                    return;
                }
                string time = "";
                string energy = "";
                string flux = "";

                double fluxmax = 0;
                double fluxtotal = 0;
                double fluxav = 0;
                double fluxfig = 0;

                int count = 0;
                foreach (var item in array)
                {
                    energy = item["energy"]?.ToString();
                    time = item["time_tag"]?.ToString();
                    satno = item["satellite"]?.ToString();
                    flux = item["flux"]?.ToString();
                    string[] s = time.Split('T');
                    time = s[0] + " " + s[1];
                    DateTime t = Convert.ToDateTime(time).ToUniversalTime();

                    DateTime dt1 = DateTime.Now.ToUniversalTime();
                    DateTime dt2 = DateTime.Now.ToUniversalTime();
                    if (yesterday)
                    {
                        dt1 = dt1.AddDays(-1);
                        dt2 = dt2.AddDays(-1);
                    }
                    dt1 = new DateTime(dt1.Year, dt1.Month, dt1.Day, h1, m1, 0);

                    dt2 = new DateTime(dt2.Year, dt2.Month, dt2.Day, h2, m2, 0);


                    if (energy == ">=10 MeV" && t >= dt1 && t <= dt2) // Filter for S-level relevant band                    
                    {

                        //fluxdata = time + " " + sat + " " + energy + " " + flux + " " ;
                        fluxfig = Convert.ToDouble(flux);
                        if (fluxfig > fluxmax)
                        {
                            count++;
                        }
                        if (fluxfig > fluxmax && count > 2)   //take max flux over 10 minutes
                        {
                            fluxmax = fluxfig;  //max proton flux (sampled in 2 x 5 minute intervals to avoid glitch in data)
                            count = 0;
                        }

                        fluxtotal = fluxtotal + fluxfig;
                        satErr = false;
                    }
                }
                fluxav = fluxtotal / 35; //average of 35 five min samples

                if (fluxav > 0 && fluxtotal > 0)
                {
                    fluxdata = Math.Round(fluxav, 4) + "/" + Math.Round(fluxmax, 4);
                    return;
                }


            }
            catch
            {
                satErr = true;
                //Msg.TMessageBox("Error reading satellite data", "GOES " + satno, 1000);
            }
        }

        private async Task getSolarFlares(int h1, bool yesterday, bool primary)
        {

            flaredata = "";
            string url = "https://services.swpc.noaa.gov/json/goes/primary/xray-flares-7-day.json";
            if (stopUrl)
            {
                return;
            }
            if (await Msg.IsUrlReachable(url))
            {

                using HttpClient client = new HttpClient();
                string satno = "";
                try
                {
                    string nl = "";
                    int h2 = h1 + 2; //do this for 2h55mins

                    int m1 = 0;
                    int m2 = 55;    //measure proton flux from 5 min data from GOES in 3 hour periods
                    string json = await client.GetStringAsync(url);
                    JsonArray array = JsonNode.Parse(json).AsArray();
                    if (array == null)
                    {
                        Msg.TMessageBox("Not responding", "", 1500);
                        return;
                    }
                    string begin_time = "";
                    string begin_class = "";
                    string max_time = "";
                    string max_class = "";
                    string max_xrlong = ""; // this is the important metric - total energy delivered by the flare
                    string max_ratio = "";  //useful to calculate max temperature of flare
                    string max_ratio_time = "";
                    string current_int_xrlong = "";
                    string end_time = "";
                    string end_class = "";
                    string maxHM = "";


                    int count = 0;
                    foreach (var item in array)
                    {
                        begin_time = item["begin_time"]?.ToString();
                        begin_class = item["begin_class"]?.ToString();
                        max_time = item["max_time"]?.ToString();
                        max_class = item["max_class"]?.ToString();
                        end_time = item["end_time"]?.ToString();
                        end_class = item["end_class"]?.ToString();
                        current_int_xrlong = item["current_int_xrlong"]?.ToString();
                        double intXR = Convert.ToDouble(current_int_xrlong);
                        intXR = Math.Round(intXR, 4);

                        DateTime maxt = Convert.ToDateTime(max_time).ToUniversalTime();
                        maxHM = maxt.Hour.ToString().PadLeft(2, '0') + ":" + maxt.Minute.ToString().PadLeft(2, '0');

                        DateTime dt1 = DateTime.Now.ToUniversalTime();
                        DateTime dt2 = DateTime.Now.ToUniversalTime();
                        if (yesterday)
                        {
                            dt1 = dt1.AddDays(-1);
                            dt2 = dt2.AddDays(-1);
                        }
                        dt1 = new DateTime(dt1.Year, dt1.Month, dt1.Day, h1, m1, 0);

                        dt2 = new DateTime(dt2.Year, dt2.Month, dt2.Day, h2, m2, 0);

                        if (maxt >= dt1 && maxt <= dt2) // Filter for S-level relevant band                    
                        {
                            if (flaredata != "")
                            {
                                nl = Environment.NewLine;
                                //nl = "-";
                            }
                            else
                            {
                                nl = "";
                            }

                            flaredata = flaredata + nl + maxHM + "/" + max_class + "/" + intXR;

                        }
                    }

                }
                catch (Exception ex)
                {

                    //Msg.TMessageBox("Error reading satellite data", "GOES " + satno, 1000);
                }
            }
            else
            {
                Msg.TMessageBox("Unable to reach GOES data", "GOES data", 1000);
            }
        }


        static double ClassToFlux(string flareClass)
        {
            char category = flareClass[0];
            double multiplier = double.Parse(flareClass.Substring(1), CultureInfo.InvariantCulture);

            return category switch
            {
                'A' => multiplier * 1e-8,
                'B' => multiplier * 1e-7,
                'C' => multiplier * 1e-6,
                'M' => multiplier * 1e-5,
                'X' => multiplier * 1e-4,
                _ => throw new ArgumentException("Invalid flare class")
            };
        }

        static string FluxToClass(double flux)
        {
            if (flux < 1e-7) return $"A{flux / 1e-8:F1}";
            if (flux < 1e-6) return $"B{flux / 1e-7:F1}";
            if (flux < 1e-5) return $"C{flux / 1e-6:F1}";
            if (flux < 1e-4) return $"M{flux / 1e-5:F1}";
            return $"X{flux / 1e-4:F1}";
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await updateAllProtonandFlare(server, user, pass, false);


        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void changebutton_Click(object sender, EventArgs e)
        {
            if (changebutton.Text.StartsWith("PF"))
            {
                changebutton.Text = "Ap/Kp/SSN indices";
                groupBox3.Visible = false;
                groupBox4.Visible = true;
                dataGridView2.Visible = false;
                dataGridView3.Visible = true;
                toplabel.Text = "Proton flux (protons/cm²·s·sr)>=10MeV (avg/max):";
                flarelabel.Text = "Flares: max time/ max class/int xrlong (J/m\u00B2)";
                flarelabel.Visible = true;
                Eventsbutton.Visible = true;
                Burstbutton.Visible = true;
                if (Burstbutton.Text.Contains("Hide")) { RBlabel.Visible = true; }
            }
            else
            {
                changebutton.Text = "PF/flare/burst data";
                groupBox3.Visible = true;
                groupBox4.Visible = false;
                dataGridView2.Visible = true;
                dataGridView3.Visible = false;
                toplabel.Text = "Kp indices:";
                flarelabel.Visible = false;
                Eventsbutton.Visible = false;
                Burstbutton.Visible = false;
                RBlabel.Visible = false;
            }
        }

        private void flarelistBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Clabel.Text = flarelistBox.SelectedItem.ToString();
        }

        private async void infobutton_Click(object sender, EventArgs e)
        {
            if (stopUrl)
            {
                Msg.TMessageBox("No Internet connection", "Info", 2000);
                return;
            }
            string url = "https://www.swpc.noaa.gov/noaa-scales-explanation";
            //if (await Msg.IsUrlReachable(url))
            try
            {
                OpenBrowser(url);
            }
            catch
            {

                Msg.TMessageBox("No Internet connection", "Info", 2000);
            }
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
            }
        }

        private void Burstbutton_Click_1(object sender, EventArgs e)
        {
            showBursts();
        }

        private void showBursts()
        {
            if (Burstbutton.Text == "Show bursts")
            {
                updateBursts(server, user, pass);
                Burstbutton.Text = "Hide bursts";
                RBlabel.Visible = true;
            }
            else
            {
                find_extra_data(false, "", "");
                Burstbutton.Text = "Show bursts";
                RBlabel.Visible = false;
            }
        }

        private void Eventsbutton_Click(object sender, EventArgs e)
        {
            if (Eventsbutton.Text == "Current Summary")
            {
                EventsgroupBox.Visible = true;
                Eventsbutton.Text = "Hide";
                fetchBurstdata();
            }
            else
            {
                Eventsbutton.Text = "Current Summary";
                EventsgroupBox.Visible = false;
            }

        }


        private async void Solar_Activated(object sender, EventArgs e)
        {
            //await getLatestSolar(server, user, pass);
        }

        private void Solar_Enter(object sender, EventArgs e)
        {

        }

        private async void hamqslbutton_Click(object sender, EventArgs e)
        {
            if (stopUrl)
            {
                Msg.TMessageBox("No Internet connection", "Info", 2000);
                return;
            }
            string url = "https://www.hamqsl.com/solar2.html";
            if (await Msg.IsUrlReachable(url))
            {
                //OpenBrowser(url);

            }
            else
            {
                Msg.TMessageBox("No Internet connection", "Info", 2000);
                return;
            }
            if (hamqslbutton.Text == "Forecast")
            {
                hamqslbutton.Text = "Hide";
                showHamqsl();
            }
            else
            {
                hamqslbutton.Text = "Forecast";

                hamqslgroupBox.Visible = false;

            }
        }

        private void showHamqsl()
        {
            hamqslopened = false;
            hamqslgroupBox.Text = "Solar-Terrestrial Data - courtesy of N0NBH at hamqsl.com";
            WebBrowser browser = new WebBrowser();
            browser.Size = new Size(hamqslgroupBox.Width - 20, hamqslgroupBox.Height - 30);
            browser.Location = new Point(10, 20);
            browser.ScrollBarsEnabled = false;
            browser.ScriptErrorsSuppressed = true;


            hamqslgroupBox.Controls.Clear();

            hamqslgroupBox.Controls.Add(browser);
            string html = @"<center><a href='https://www.hamqsl.com/solar.html' ";
            html = html + @"title='Click to add Solar-Terrestrial Data to your website!'>";
            html = html + @"<img src='https://www.hamqsl.com/solar101vhf.php'></a></center>";
            browser.DocumentText = html;
            hamqslgroupBox.Visible = true;
            hamqslgroupBox.BringToFront();
            hamqslgroupBox.Focus();
            browser.Navigating += browser_Navigating;

        }
        private async void browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            // Cancel navigation inside the WebBrowser control
            e.Cancel = true;

            if (stopUrl)
            {
                Msg.TMessageBox("No Internet connection", "Info", 2000);
                return;
            }
            string url = "https://www.hamqsl.com/solar2.html";
            if (await Msg.IsUrlReachable(url))
            {
                if (hamqslopened)
                {
                    return;
                }
                OpenBrowser(url);
                hamqslopened = true;
            }
            else
            {
                Msg.TMessageBox("Unable to reach hamqsl.com", "", 2000);
            }

        }

        private async void solartimer_Tick(object sender, EventArgs e)
        {
            await solartimer_action();
        }


        private async Task solartimer_action()
        {
            timercount++;
            DateTime dt = DateTime.Now.ToUniversalTime();
            if (timercount == 9) //45 mins
            {
                await getLatestSolar(server, user, pass); //update 

            }

            bool check = await checkNOAA();
            if (!check)
            {
                Msg.TMessageBox("Unable to reach NOAA", "NOAA data", 1500);
            }


            if (timercount == 8)    //40 mins
            {
                await updateGeo(server, user, pass, true); //true - update yesterday as well


            }
            if (timercount == 6)  //30 mins
            {
                await updateAllProtonandFlare(server, user, pass, false);

            }
            if (timercount == 5 && dt.Hour == 3)  //25 mins
            {
                await updateAllProtonandFlare(server, user, pass, true); //get results for 2100-2400 yesterday

            }

            if (timercount == 7)  //35 mins
            {

                await updateSolar(server, user, pass);
            }
            if (timercount == 11) //55 mins
            {
                timercount = 0; //reset timer

            }
        }

        private void solarstartuptimer_Tick(object sender, EventArgs e)
        {
            getLatestSolar(server, user, pass);
            solarstartuptimer.Enabled = false;
            solarstartuptimer.Stop();

        }

        private void conditionlabel_Click(object sender, EventArgs e)
        {

        }

        private void linksbutton1_Click(object sender, EventArgs e)
        {

            if (stopUrl)
            {
                Msg.TMessageBox("No Internet connection", "Info", 2000);
                return;
            }
            string url = "https://www.qsl.net/4/4x4xm//Books-and-Articles/Understanding-Solar-Indices-Ian-Poole-G3YWX-QST-September-2002.pdf";
            //if (await Msg.IsUrlReachable(url))
            try
            {
                OpenBrowser(url);
            }
            catch
            {

                Msg.TMessageBox("No Internet connection", "Info", 2000);
            }
        }

        private void linksbutton2_Click(object sender, EventArgs e)
        {
            if (stopUrl)
            {
                Msg.TMessageBox("No Internet connection", "Info", 2000);
                return;
            }
            string url = "https://rsgb.org/main/technical/propagation/";
            //if (await Msg.IsUrlReachable(url))
            try
            {
                OpenBrowser(url);
            }
            catch
            {

                Msg.TMessageBox("No Internet connection", "Info", 2000);
            }
        }

        private void Solar_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            string exeFolder = Application.StartupPath;
            helpform.rtf = "C:\\WSPR_Sked\\About_WS_Solar_v3.rtf";
            if (!File.Exists(helpform.rtf))
            {
                helpform.rtf = exeFolder + "\\About_WS_Solar_v3.rtf";
            }
            if (File.Exists(helpform.rtf))
            {
                helpform.Show();
            }
            else
            {
                Msg.TMessageBox(helpform.rtf + " not found", "Help file", 3000);
            }
        }

        private void AusSWSbutton_Click(object sender, EventArgs e)
        {
            if (stopUrl)
            {
                Msg.TMessageBox("No Internet connection", "Info", 2000);
                return;
            }
            string url = "https://www.sws.bom.gov.au/";

            try
            {
                OpenBrowser(url);
            }
            catch
            {

                Msg.TMessageBox("No Internet connection", "Info", 2000);
            }
        }

        private void Burstgridbutton_Click(object sender, EventArgs e)
        {
            if (Burstgridbutton.Text == "Radio bursts")
            {
                if (findBurstStorms())
                {
                    BurstgroupBox.Visible = true;
                    BurstgroupBox.BringToFront();
                    Burstgridbutton.Text = "Hide bursts";
                }
                else
                {
                    Msg.TMessageBox("Try again in a moment ...", "Radio bursts", 2000);
                }
            }
            else
            {
                BurstgroupBox.Visible = false;
                Burstgridbutton.Text = "Radio bursts";

            }
        }

       
    }

}

