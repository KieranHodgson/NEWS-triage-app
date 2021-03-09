using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Content.Res;
using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Linq;


namespace NEWS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Button buttonSubmit, buttonRefresh, buttonIDnext, buttonCancel, buttonHome; //initiate submit button
        RadioButton RadioSpo2_1, RadioSpo2_2, RadioAir_Air, RadioAir_Oxygen, RadioAlert_Alert, RadioAlert_CVPU;
        TextView patientID, scoreAdviceShort, scoreAdviceLong, scoreScore;
        spo2scale Spo2Scale = spo2scale.spo2_1;
        breathing Breathing = breathing.air;
        conscious Conscious = conscious.alert;
        LinearLayout selectMain;
        LinearLayout scoreMain;
        GridLayout inputMain;

        string PatientID = "";
        enum spo2scale //spo2 scale option 
        {
            spo2_1,
            spo2_2
        }
        enum breathing //breathing status: breathing air or supplementary oxygen
        {
            air,
            oxygen
        }
        enum conscious //consciousness level
        {
            alert,
            CVPU
        }
        enum newsscore
        {
            low,
            lowmedium,
            medium,
            high
        }

        protected override void OnCreate(Bundle savedInstanceState) //run on startup.
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            selectMain = FindViewById<LinearLayout>(Resource.Id.selectMain);
            scoreMain = FindViewById<LinearLayout>(Resource.Id.scoreMain);
            inputMain = FindViewById<GridLayout>(Resource.Id.inputMain);

            RadioSpo2_1 = FindViewById<RadioButton>(Resource.Id.RadioSpo2_1);
            RadioSpo2_2 = FindViewById<RadioButton>(Resource.Id.RadioSpo2_2);

            RadioAir_Air = FindViewById<RadioButton>(Resource.Id.RadioAir_Air);
            RadioAir_Oxygen = FindViewById<RadioButton>(Resource.Id.RadioAir_Oxygen);

            RadioAlert_Alert = FindViewById<RadioButton>(Resource.Id.RadioAlert_Alert);
            RadioAlert_CVPU = FindViewById<RadioButton>(Resource.Id.RadioAlert_CVPU);

            buttonSubmit = FindViewById<Button>(Resource.Id.buttonSubmit);
            buttonRefresh = FindViewById<Button>(Resource.Id.buttonRefresh);
            buttonIDnext = FindViewById<Button>(Resource.Id.buttonIDnext);
            buttonCancel = FindViewById<Button>(Resource.Id.buttonCancel);
            buttonHome = FindViewById<Button>(Resource.Id.buttonHome);

            patientID = FindViewById<TextView>(Resource.Id.textViewID);
            scoreAdviceShort = FindViewById<TextView>(Resource.Id.scoreAdviceShort);
            scoreAdviceLong = FindViewById<TextView>(Resource.Id.scoreAdviceLong);
            scoreScore = FindViewById<TextView>(Resource.Id.scoreScore);

            RadioSpo2_1.Click += delegate
             {
                 Spo2Scale = spo2scale.spo2_1;
             };
            RadioSpo2_2.Click += delegate
            {
                Spo2Scale = spo2scale.spo2_2;
            };
            RadioAir_Air.Click += delegate
            {
                Breathing = breathing.air;
            };
            RadioAir_Oxygen.Click += delegate
            {
                Breathing = breathing.oxygen;
            };
            RadioAlert_Alert.Click += delegate
            {
                Conscious = conscious.alert;
            };
            RadioAlert_CVPU.Click += delegate
            {
                Conscious = conscious.CVPU;
            };
            buttonIDnext.Click += delegate
            {
                PatientID = FindViewById<EditText>(Resource.Id.editTextID).Text;
                bool patientExists = sqlCheck();
                if (patientExists == true)
                {
                    selectMain.Visibility = ViewStates.Gone;
                    inputMain.Visibility = ViewStates.Visible;
                    scoreMain.Visibility = ViewStates.Gone;
                    EimoExtract();
                }
                else
                {
                    ToastNotification.TostMessage("Patient does not exist");
                }
            };
            buttonCancel.Click += delegate
            {
                PatientID = "";
                FindViewById<TextView>(Resource.Id.TextEditSpo2).Text = "";
                FindViewById<TextView>(Resource.Id.TextEditPulse).Text = "";
                FindViewById<TextView>(Resource.Id.TextEditBP).Text = "";
                FindViewById<TextView>(Resource.Id.TextEditTempereture).Text = "";
                FindViewById<TextView>(Resource.Id.TextEditRespiration).Text = "";
                selectMain.Visibility = ViewStates.Visible;
                inputMain.Visibility = ViewStates.Gone;
                scoreMain.Visibility = ViewStates.Gone;
            };
            buttonHome.Click += delegate
            {
                PatientID = "";
                FindViewById<TextView>(Resource.Id.TextEditSpo2).Text = "";
                FindViewById<TextView>(Resource.Id.TextEditPulse).Text = "";
                FindViewById<TextView>(Resource.Id.TextEditBP).Text = "";
                FindViewById<TextView>(Resource.Id.TextEditTempereture).Text = "";
                FindViewById<TextView>(Resource.Id.TextEditRespiration).Text = "";
                selectMain.Visibility = ViewStates.Visible;
                inputMain.Visibility = ViewStates.Gone;
                scoreMain.Visibility = ViewStates.Gone;
            };
            buttonRefresh.Click += RefreshClicked;
            buttonSubmit.Click += SubmitClicked;
        }

        public static class ToastNotification
        {
            public static void TostMessage(string message)
            {
                var context = Android.App.Application.Context;
                var tostMessage = message;
                var durtion = ToastLength.Long;

                Toast.MakeText(context, tostMessage, durtion).Show();
            }
        } //toast notification generator 

        private void RefreshClicked(object sender, EventArgs e)
        {
            EimoExtract();
        } // click event handler for refresh button

        private void SubmitClicked(object sender, EventArgs e)
        {
            string spo2 = FindViewById<TextView>(Resource.Id.TextEditSpo2).Text;
            string pulse = FindViewById<TextView>(Resource.Id.TextEditPulse).Text;
            string bp = FindViewById<TextView>(Resource.Id.TextEditBP).Text;
            string tempereture = FindViewById<TextView>(Resource.Id.TextEditTempereture).Text;
            string respiration = FindViewById<TextView>(Resource.Id.TextEditRespiration).Text;

            try
            {
                newsscore score = NewsCalc(int.Parse(spo2), int.Parse(pulse), int.Parse(bp), float.Parse(tempereture), int.Parse(respiration), Spo2Scale, Breathing, Conscious);
                sqlupload(int.Parse(spo2), int.Parse(pulse), int.Parse(bp), float.Parse(tempereture), int.Parse(respiration), Spo2Scale, Breathing, Conscious);
                switch (score)
                {
                    case newsscore.low:
                        scoreMain.SetBackgroundColor(Android.Graphics.Color.Gray);
                        scoreAdviceShort.Text = "Low risk";
                        scoreAdviceLong.Text = "A low NEW score (1–4) should prompt assessment by a competent registered nurse or equivalent, who should decide whether a change to frequency of clinical monitoring or an escalation of clinical care is required.";
                        selectMain.Visibility = ViewStates.Gone;
                        inputMain.Visibility = ViewStates.Gone;
                        scoreMain.Visibility = ViewStates.Visible;
                        break;

                    case newsscore.lowmedium:
                        scoreMain.SetBackgroundColor(Android.Graphics.Color.Yellow);
                        scoreAdviceShort.Text = "Low-medium risk";
                        scoreAdviceLong.Text = "A single red score (3 in a single parameter) is unusual, but should prompt an urgent review by a clinician with competencies in the assessment of acute illness (usually a ward-based doctor) to determine the cause, and decide on the frequency of subsequent monitoring and whether an escalation of care is required.";
                        selectMain.Visibility = ViewStates.Gone;
                        inputMain.Visibility = ViewStates.Gone;
                        scoreMain.Visibility = ViewStates.Visible;
                        break;

                    case newsscore.medium:
                        scoreMain.SetBackgroundColor(Android.Graphics.Color.Orange);
                        scoreAdviceShort.Text = "Medium risk";
                        scoreAdviceLong.Text = "A medium NEW score (5–6) is a key trigger threshold and should prompt an urgent review by a clinician with competencies in the assessment of acute illness – usually a ward-based doctor or acute team nurse, who should urgently decide whether escalation of care to a team with critical care skills is required (ie critical care outreach team).";
                        selectMain.Visibility = ViewStates.Gone;
                        inputMain.Visibility = ViewStates.Gone;
                        scoreMain.Visibility = ViewStates.Visible;
                        break;

                    case newsscore.high:
                        scoreMain.SetBackgroundColor(Android.Graphics.Color.Red);
                        scoreAdviceShort.Text = "High risk";
                        scoreAdviceLong.Text = "A high NEW score (7 or more) is a key trigger threshold and should prompt emergency assessment by a clinical team / critical care outreach team with critical care competencies and usually transfer of the patient to a higher-dependency care area.";
                        selectMain.Visibility = ViewStates.Gone;
                        inputMain.Visibility = ViewStates.Gone;
                        scoreMain.Visibility = ViewStates.Visible;
                        break;

                }
            }
            catch
            {
                ToastNotification.TostMessage("error: ");
            }
        } //click event handler for submit button

        private void EimoExtract()
        {
            char[] charsToTrim = { '\"', '{' };
            string rawContent;
            AssetManager assets = this.Assets;
            using (StreamReader sr = new StreamReader(assets.Open("example.csv")))
            {
                rawContent = sr.ReadToEnd();
            }
            string[] contentSplitData = rawContent.Split("\"Data\":");
            string data = contentSplitData[1];
            contentSplitData[0].Replace("\"", string.Empty);
            string contentString = string.Join("", contentSplitData[0].Split(charsToTrim));
            string[] content = contentString.Split("\n");
            string
                spo2_ = content[17],
                pulse_ = content[10],
                bp_ = content[19],
                tempereture_ = content[8];

            spo2_ = spo2_.Substring(spo2_.IndexOf(":") + 1);
            spo2_ = spo2_.Trim('\r', ' ');
            spo2_ = spo2_.Trim(',', ' ');


            pulse_ = pulse_.Substring(pulse_.IndexOf(":") + 1);
            pulse_ = pulse_.Trim('\r', ' ');
            pulse_ = pulse_.Trim(',', ' ');

            bp_ = bp_.Substring(bp_.IndexOf(":") + 1);
            bp_ = bp_.Trim('\r', ' ');
            bp_ = bp_.Trim(',', ' ');

            tempereture_ = tempereture_.Substring(tempereture_.IndexOf(":") + 1);
            tempereture_ = tempereture_.Trim('\r', ' ');
            tempereture_ = tempereture_.Trim(',', ' ');

            FindViewById<EditText>(Resource.Id.TextEditSpo2).Text = spo2_;
            FindViewById<EditText>(Resource.Id.TextEditPulse).Text = pulse_;
            FindViewById<EditText>(Resource.Id.TextEditBP).Text = bp_;
            FindViewById<EditText>(Resource.Id.TextEditTempereture).Text = tempereture_;
        } //extract data from raw EIMO output

        private newsscore NewsCalc(int spo2, int pulse, int bp, float tempereture, int respiration, spo2scale spo2Scale, breathing breathing, conscious conscious)
        {
            uint respirationScore = 0;
            uint spo2Score = 0;
            uint oxygenScore = 0;
            uint bpScore = 0;
            uint pulseScore = 0;
            uint consciousnessScore = 0;
            uint temperetureScore = 0;

            //calculate score for respiration
            if (respiration <= 8)
            {
                respirationScore = 3;
            }                       //score: 3
            else
            if (respiration >= 9 && respiration <= 11)
            {
                respirationScore = 1;
            }  //score: 1
            else
            if (respiration >= 12 && respiration <= 20)
            {
                respirationScore = 0;
            } //score: 0
            else
            if (respiration >= 21 && respiration <= 24)
            {
                respirationScore = 2;
            } //score: 2
            else
            if (respiration >= 25)
            {
                respirationScore = 3;
            }                      //score: 3


            //calculate score for SpO2
            if (Spo2Scale == spo2scale.spo2_1) //spo2 score calculation
            {
                //if spo2 scale 1 is selected
                if (spo2 >= 96)
                {
                    spo2Score = 0;
                }               //score: 0
                else
                if (spo2 == 94 || spo2 == 95)
                {
                    spo2Score = 1;
                } //score: 1
                else
                if (spo2 == 92 || spo2 == 93)
                {
                    spo2Score = 2;
                } //score: 2
                else
                if (spo2 <= 91)
                {
                    spo2Score = 3;
                }               //score: 3
            }
            else
            if (Spo2Scale == spo2scale.spo2_2)
            {
                //if spo2 scale 2 is selected
                if (Breathing == breathing.oxygen)
                {
                    if (spo2 >= 97)
                    {
                        spo2Score = 3;
                    }               //score: 3
                    else
                    if (spo2 == 95 || spo2 == 96)
                    {
                        spo2Score = 2;
                    } //score: 2
                    else
                    if (spo2 == 93 || spo2 == 94)
                    {
                        spo2Score = 1;
                    } //score: 1
                    else
                    if (spo2 <= 92 && spo2 >= 88)
                    {
                        spo2Score = 0;
                    } //score: 0
                    else
                    if (spo2 == 86 || spo2 == 87)
                    {
                        spo2Score = 1;
                    } //score: 1
                    else
                    if (spo2 == 84 || spo2 == 85)
                    {
                        spo2Score = 2;
                    } //score: 2
                    else
                    if (spo2 <= 83)
                    {
                        spo2Score = 1;
                    }               //score: 3

                }
                else
                if (Breathing == breathing.air)
                {
                    if (spo2 >= 93)
                    {
                        spo2Score = 0;
                    }               //score: 0
                    if (spo2 == 86 || spo2 == 87)
                    {
                        spo2Score = 1;
                    } //score: 1
                    if (spo2 == 84 || spo2 == 85)
                    {
                        spo2Score = 2;
                    } //score: 2
                    if (spo2 <= 83)
                    {
                        spo2Score = 1;
                    }               //score: 3
                }

            }


            //calculate score for if supplementary oxygen is in use 
            if (Breathing == breathing.oxygen)
            {
                oxygenScore = 2;
            } //score: 2
            else
            if (Breathing == breathing.air)
            {
                oxygenScore = 0;
            } //score: 0


            //calculate score for blood pressure
            if (bp <= 90)
            {
                bpScore = 3;
            }                //score: 3
            else
            if (bp >= 91 && bp <= 100)
            {
                bpScore = 2;
            }   //score: 2
            else
            if (bp >= 101 && bp <= 110)
            {
                bpScore = 1;
            }  //score: 1
            else
            if (bp >= 111 && bp <= 219)
            {
                bpScore = 0;
            }  //score: 0
            else
            if (bp >= 220)
            {
                bpScore = 3;
            }               //score: 3


            //calculate pulse score
            if (pulse <= 40)
            {
                pulseScore = 3;
            }                  //score: 3
            else
            if (pulse >= 41 && pulse <= 50)
            {
                pulseScore = 1;
            }   //score: 1
            else
            if (pulse >= 51 && pulse <= 90)
            {
                pulseScore = 0;
            }   //score: 0
            else
            if (pulse >= 91 && pulse <= 110)
            {
                pulseScore = 1;
            }  //score: 1
            else
            if (pulse >= 111 && pulse <= 130)
            {
                pulseScore = 2;
            } //score: 2
            else
            if (pulse >= 131)
            {
                pulseScore = 3;
            }                 //score: 3


            //calculate consciousness score
            if (Conscious == conscious.alert)
            {
                consciousnessScore = 0;
            } //score: 0
            else
            if (Conscious == conscious.CVPU)
            {
                consciousnessScore = 3;
            }  //score: 3


            //calculate temperature score
            if (tempereture <= 35)
            {
                temperetureScore = 3;
            }                     //score: 3
            else
            if (tempereture > 35 && tempereture <= 36)
            {
                temperetureScore = 1;
            } //score: 1
            else
            if (tempereture > 36 && tempereture <= 38)
            {
                temperetureScore = 0;
            } //score: 0
            else
            if (tempereture > 38 && tempereture <= 39)
            {
                temperetureScore = 0;
            } //score: 1
            else
            if (tempereture >= 39.1)
            {
                temperetureScore = 3;
            }                   //score: 2


            uint aggregateScore = respirationScore + spo2Score + oxygenScore + bpScore + pulseScore + consciousnessScore + temperetureScore;

            scoreScore.Text = "Score: " + aggregateScore.ToString();

            if (aggregateScore >= 7)
            {
                //news score high
                return newsscore.high;
            }
            else
            if (aggregateScore >= 5 && aggregateScore <= 6)
            {
                //news score medium
                return newsscore.medium;
            }
            else
            if (respirationScore == 3 || spo2Score == 3 || oxygenScore == 3 || bpScore == 3 || pulseScore == 3 || consciousnessScore == 3 || temperetureScore == 3)
            {
                //news score low-medium
                return newsscore.lowmedium;
            }
            else
            if (aggregateScore >= 0 && aggregateScore <= 4)
            {
                //news score low
                return newsscore.low;
            }
            return newsscore.low;
        }

        private void sqlupload(int spo2, int pulse, int bp, float tempereture, int respiration, spo2scale spo2Scale, breathing breathing, conscious conscious)
        {
            string conntectionString = GetConnectionString();
            System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conntectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("", con);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText =
            @"
                INSERT INTO news
                           (patientID
                           ,datetime
                           ,respirations
                           ,spo2scale1
                           ,spo2
                           ,oxygen
                           ,bp
                           ,pulse
                           ,conscious
                           ,tempereture)
                     VALUES
                           (@patientID
                           ,@datetime
                           ,@respirations
                           ,@spo2scale1
                           ,@spo2
                           ,@oxygen
                           ,@bp
                           ,@pulse
                           ,@conscious
                           ,@tempereture)
                ";
            try
            {


                DateTime myDateTime = DateTime.Now;
                string sqlFormattedDate = myDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

                cmd.Parameters.AddWithValue("@patientID", PatientID);
                cmd.Parameters.AddWithValue("@datetime", sqlFormattedDate);
                cmd.Parameters.AddWithValue("@respirations", respiration);
                cmd.Parameters.AddWithValue("@spo2", spo2);
                cmd.Parameters.AddWithValue("@bp", bp);
                cmd.Parameters.AddWithValue("@pulse", pulse);
                cmd.Parameters.AddWithValue("@tempereture", tempereture);

                if (conscious == conscious.alert)
                    cmd.Parameters.AddWithValue("@conscious", "ALERT");
                else
                if (conscious == conscious.CVPU)
                    cmd.Parameters.AddWithValue("@conscious", "CVPU");


                if (spo2Scale == spo2scale.spo2_1)
                    cmd.Parameters.AddWithValue("@spo2scale1", "1");
                else
                if (spo2Scale == spo2scale.spo2_2)
                    cmd.Parameters.AddWithValue("@spo2scale1", "2");

                if (breathing == breathing.air)
                    cmd.Parameters.AddWithValue("@oxygen", "AIR");
                else
                if (breathing == breathing.oxygen)
                    cmd.Parameters.AddWithValue("@oxygen", "OXYGEN");
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch
            {
                con.Close();
                ToastNotification.TostMessage("error connecting to database");
            }

        }

        private bool sqlCheck()
        {
            string conntectionString = GetConnectionString();
            System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(conntectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT * FROM patients WHERE ([patientID] = @ID)", con);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue("@ID", PatientID);

            con.Open();
            System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                con.Close();
                return true;
            }
            else
            {
                con.Close();
                return false;
            }
        }

        static private string GetConnectionString()
        {
            return "Server=tcp:newsscore.database.windows.net,1433;Initial Catalog=news;Persist Security Info=False;User ID=KieranHodgson;Password=oVvFE0p4Oh2o;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

