using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMSReader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ImportFromIntent()
    {
        try
        {
            // Get the current activity
            Debug.Log("Getting the current activity...");
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activityObject = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");

            // Get the current intent
            Debug.Log("Getting the current intent...");
            AndroidJavaObject intent = activityObject.Call<AndroidJavaObject>("getIntent");

            // Get the intent data using AndroidJNI.CallObjectMethod so we can check for null
            IntPtr method_getData = AndroidJNIHelper.GetMethodID(intent.GetRawClass(), "getData", "()Ljava/lang/Object;");
            IntPtr getDataResult = AndroidJNI.CallObjectMethod(intent.GetRawObject(), method_getData, AndroidJNIHelper.CreateJNIArgArray(new object[0]));
            if (getDataResult.ToInt32() != 0)
            //if (true/*getDataResult.ToInt32() != 0*/)
            {
                Debug.Log("getdataresult != 0");

                // Now actually get the data. We should be able to get it from the result of AndroidJNI.CallObjectMethod, but I don't now how so just call again
                AndroidJavaObject intentURI = intent.Call<AndroidJavaObject>("getData");

                // ASH CODE START
                // Attempt to read inbox smses
                AndroidJavaObject contentResolver = activityObject.Call<AndroidJavaObject>("getContentResolver");
                AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
                AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "content://sms/inbox");
                AndroidJavaObject cursor = contentResolver.Call<AndroidJavaObject>("query", uriObject, null, null, null, null);


                if (cursor.Call<bool>("moveToFirst"))
                {
                    do
                    {
                        string msgData = "";
                        for (int idx = 0; idx < cursor.Call<int>("getColumnCount"); idx++)
                        {
                            msgData += " " + cursor.Call<string>("getColumnName", idx) + ":" + cursor.Call<string>("getString", idx);
                        }
                        // use msgData
                        Debug.Log(msgData);
                    } while (cursor.Call<bool>("moveToNext"));
                }
                else
                {
                    // empty box, no SMS
                    Debug.Log("No sms!");
                }
                // ASH CODE END

            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
            // Handle error
        }
    }

}
