using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;

namespace NEWS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {   
        
        Button buttonSubmit, buttonRefresh; //initiate submit button
        RadioButton RadioSpo2_1, RadioSpo2_2, RadioAir_Air, RadioAir_Oxygen, RadioAlert_Alert, RadioAlert_CVPU;


        protected override void OnCreate(Bundle savedInstanceState) //run on startup.
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            RadioSpo2_1 = FindViewById<RadioButton>(Resource.Id.RadioSpo2_1);
            RadioSpo2_2 = FindViewById<RadioButton>(Resource.Id.RadioSpo2_2);

            RadioAir_Air = FindViewById<RadioButton>(Resource.Id.RadioAir_Air);
            RadioAir_Oxygen = FindViewById<RadioButton>(Resource.Id.RadioAir_Oxygen);

            RadioAlert_Alert = FindViewById<RadioButton>(Resource.Id.RadioAlert_Alert);
            RadioAlert_CVPU = FindViewById<RadioButton>(Resource.Id.RadioAlert_CVPU);

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

            buttonSubmit = FindViewById<Button>(Resource.Id.buttonSubmit);
            buttonSubmit.Click += SubmitClicked;

            buttonRefresh = FindViewById<Button>(Resource.Id.buttonRefresh);
            buttonRefresh.Click += RefreshClicked;
            
        }

        

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
        public static class ToastNotification
        {
            public static void TostMessage(string message)
            {
                var context = Android.App.Application.Context;
                var tostMessage = message;
                var durtion = ToastLength.Long;

                Toast.MakeText(context, tostMessage, durtion).Show();
            }
        }
        spo2scale Spo2Scale = spo2scale.spo2_1;
        breathing Breathing = breathing.air;
        conscious Conscious = conscious.alert;


        private void RefreshClicked(object sender, EventArgs e)
        {
            
        }


        private void SubmitClicked(object sender, EventArgs e)
        {
            string spo2 = FindViewById<TextView>(Resource.Id.TextEditSpo2).Text;
            string pulse = FindViewById<TextView>(Resource.Id.TextEditPulse).Text;
            string bp = FindViewById<TextView>(Resource.Id.TextEditBP).Text;
            string tempereture = FindViewById<TextView>(Resource.Id.TextEditTempereture).Text;
            string respiration = FindViewById<TextView>(Resource.Id.TextEditRespiration).Text;
            try
            {
                newscalc(int.Parse(spo2), int.Parse(pulse), int.Parse(bp), float.Parse(tempereture), int.Parse(respiration), Spo2Scale, Breathing, Conscious);
            }
            catch
            {
                ToastNotification.TostMessage("error");
            }
        }

        private void newscalc(int spo2, int pulse, int bp, float tempereture, int respiration, spo2scale spo2Scale, breathing breathing, conscious conscious)
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



            if (aggregateScore >=7)
            {
                //news score high
                ToastNotification.TostMessage("high (" + aggregateScore +")");
            }
            else
            if (aggregateScore >= 5 && aggregateScore <= 6)
            {
                //news score medium
                ToastNotification.TostMessage("medium (" + aggregateScore + ")");
            }
            else
            if (respirationScore == 3 || spo2Score == 3 || oxygenScore == 3 || bpScore == 3 || pulseScore == 3 || consciousnessScore == 3 || temperetureScore == 3)
            {
                //news score low-medium
                ToastNotification.TostMessage("low-medium (" + aggregateScore + ")");
            }
            else
            if(aggregateScore >= 0 && aggregateScore <= 4)
            {
                //news score low
                ToastNotification.TostMessage("low (" + aggregateScore + ")");
            }
        }




        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

    }
}

