using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class SMSReader : MonoBehaviour
{
    #region VARIABLES
    string[] desiredContacts = { "EmiratesNBD" };
    /// <summary>
    /// 1: _id
    /// 2: address, 
    /// 4: date, 
    /// 12: body
    /// </summary>
    string[] desiredMsgInfo = { "_id", "address", /*"date",*/ "body" };
    //int[] desiredMsgInfo = { 2, 4, 12};

    public GameObject startLoadingPanel;
    public GameObject loadingProgressPanel;
    public Image loadingBar;
    public Text loadingProgressPercent;
    public GameObject finishLoadingPanel;

    int progress = 0;
    bool isLoading = false;

    /// <summary>
    /// None: Non-applicable
    /// Purchase
    /// TransferOut: "Deducted"
    /// TransferIn: "Deposited"
    /// </summary>
    public enum msgType { None, Purchase, TransferOut, TransferIn }

    /// <summary>
    /// Order from newest -> oldest
    /// </summary>
    //public List<Sms> processedMessages;

    public static SMSReader instance;
    #endregion

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Show Start Loading Panel
        startLoadingPanel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (isLoading)
        {
            loadingProgressPercent.text = progress + "%";
            loadingBar.fillAmount = (progress / 100);
        }
    }

    public void CloseLoadingPanel()
    {
        startLoadingPanel.SetActive(false);
        loadingProgressPanel.SetActive(false);
        finishLoadingPanel.SetActive(false);
    }

    public void RetrieveFromInbox()
    {
        // Show Loading Progress panel
        startLoadingPanel.SetActive(false);
        loadingProgressPanel.SetActive(true);
        loadingProgressPercent.text = "0%";

        // Load SMS Inbox
        StartCoroutine(ImportFromIntent());
    }

    public void parseBodyText(Sms sms, string body)
    {
        msgType currMsgType = msgType.None;
        Double changeAmt = 0.0;
        bool Balance_LookForNumber = false;
        Double balanceAmt = 0.0;
        bool AED_LookForNumber = false;
        string beneficiaryName = "";
        bool lookForName = false;

        string[] words = body.Split(' ');
        foreach (string w in words)
        {
            string word = w.Trim();

            // If looking for the beneficiary name, concatinate words until fullstop is reached
            if (lookForName)
            {
                beneficiaryName += " " + word;
                if (word.EndsWith(".")) lookForName = false;
            }

            // Check msg type
            if (word == "Purchase") { if (currMsgType == msgType.None) currMsgType = msgType.Purchase; }
            else if (word == "deducted") { if (currMsgType == msgType.None) currMsgType = msgType.TransferOut; }
            else if (word == "deposited" || word == "credited" || word == "Withdrawal") { if (currMsgType == msgType.None) currMsgType = msgType.TransferIn; }
            else if (word == "Balance" || word == "balance") Balance_LookForNumber = true;

            // If word is "at" start looking for beneficiary name
            else if (word == "at") lookForName = true;

            // If looking for a money value, try checking data type
            else if (AED_LookForNumber)
            {
                if (Balance_LookForNumber)   // Check if we are looking for a balance number
                {
                    if (Double.TryParse(word, out balanceAmt))
                    {
                        Balance_LookForNumber = false;
                        AED_LookForNumber = false;
                    }
                }
                else    // just save it as regular change number
                {
                    if (Double.TryParse(word, out changeAmt))
                    {
                        AED_LookForNumber = false;
                    }
                }
            }

            // If found AED, start looking for a money value
            else if (word == "AED") AED_LookForNumber = true;
        }

        if (currMsgType == msgType.None)
            sms._msgType = ("");
        else
            sms._msgType = (currMsgType.ToString());
        sms._changeAmt = (changeAmt);
        sms._balance = (balanceAmt);
        sms._beneficiaryName = (beneficiaryName);
    }

    public DateTime formatDateTime(string value)
    {
        //AndroidJavaObject formatter = new AndroidJavaObject("java.text.SimpleDateFormat", "dd/MM/yyyy hh:mm:ss.SSS");
        AndroidJavaClass calendarClass = new AndroidJavaClass("java.util.Calendar");
        AndroidJavaObject calendar = calendarClass.CallStatic<AndroidJavaObject>("getInstance");

        long number = long.Parse(value);
        calendar.Call("setTimeInMillis", number);
        int day = calendar.Call<int>("get", 5);   //  5 = Calendar.DATE_OF_MONTH
        int month = calendar.Call<int>("get", 2); //  2 = Calendar.MONTH
        int year = calendar.Call<int>("get", 1);  //  1 = Calendar.YEAR
        int hour = calendar.Call<int>("get", 10); // 10 = Calendar.HOUR
        int mins = calendar.Call<int>("get", 12); // 12 = Calendar.MINUTE
        int secs = calendar.Call<int>("get", 13); // 12 = Calendar.SECOND
        int ampm = calendar.Call<int>("get", 9);  //  9 = Calendar.AM_PM
        //return (day+"/"+month+"/"+year+" "+hour+":"+mins+" "+ampm);
        return new DateTime(year, month, day, hour, mins, secs);
    }

    IEnumerator ImportFromIntent()
    {
        yield return new WaitForSeconds(1f);

        isLoading = true;
        try
        {
            #region Gobboldeegook you don't need to touch
            // Get the current activity
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activityObject = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            // Get the current intent
            AndroidJavaObject intent = activityObject.Call<AndroidJavaObject>("getIntent");
            // Get the intent data using AndroidJNI.CallObjectMethod so we can check for null
            IntPtr method_getData = AndroidJNIHelper.GetMethodID(intent.GetRawClass(), "getData", "()Ljava/lang/Object;");
            IntPtr getDataResult = AndroidJNI.CallObjectMethod(intent.GetRawObject(), method_getData, AndroidJNIHelper.CreateJNIArgArray(new object[0]));
            #endregion

            //if (getDataResult.ToInt32() != 0)
            if (true)
            {
                // Now actually get the data. We should be able to get it from the result of AndroidJNI.CallObjectMethod, but I don't now how so just call again
                AndroidJavaObject intentURI = intent.Call<AndroidJavaObject>("getData");

                // ASH CODE START
                // Attempt to read inbox smses
                AndroidJavaObject contentResolver = activityObject.Call<AndroidJavaObject>("getContentResolver");
                AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
                AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "content://sms/inbox");
                AndroidJavaObject cursor = contentResolver.Call<AndroidJavaObject>("query", uriObject, null, null, null, null);

                SQLManager.instance.StartSync();

                //msgs = new List<List<string>>();
                if (cursor.Call<bool>("moveToFirst"))
                {
                    do      // Loop through all messages
                    {
                        bool isDesiredContact = false;
                        //List<string> msgData = new List<string>();

                        int columnCount = cursor.Call<int>("getColumnCount");

                        Sms sms = new Sms();

                        // Loop through all elements of current message
                        for (int idx = 0; idx < columnCount; idx++) 
                        {
                            bool isDesiredInfo = false;

                            // Retrieve type and data
                            string valueType = cursor.Call<string>("getColumnName", idx).Trim();
                            string value = cursor.Call<string>("getString", idx);

                            // Check if message came from desired contacts
                            if (valueType == "_id") sms._id = value;
                            if (valueType == "address")
                            {
                                for (int i = 0; i < desiredContacts.Length; i++)
                                {
                                    isDesiredContact = value == desiredContacts[i];
                                    if (isDesiredContact) sms._address = value;
                                }
                            }
                            if (valueType == "body") parseBodyText(sms, value);
                            if (valueType == "date")
                            {
                                //1612343119703
                                //1612345977808
                                //1612275113595

                                // Attempt to parse date string
                                //long lon = cursor.Call<long>("getLong", 0);

                                //msgData.Add(valueType + ":" + formatDateTime(value));
                            }
                        }

                        // Save msgData only if its from a desired contact and msg type
                        if (isDesiredContact && sms._msgType != msgType.None.ToString())
                        {
                            SQLManager.instance.CreateNewEntry(sms);
                        }

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

        //processedMessages = convertStringListToSMSList(msgs);
        //msgs.Clear();

        // Generate table contents
        ResultsTable.instance.GenerateResultsTable(SQLManager.instance.ds._connection.Query<Sms>("Select * from Sms Limit ?", 20));

        loadingProgressPanel.SetActive(false);
        finishLoadingPanel.SetActive(true);

        isLoading = false;
        yield return null;
    }

    /*List<Sms> convertStringListToSMSList(List<List<string>> msgs)
    {
        int msgInfo = msgs[0].Count;    // Number of categories
        //Debug.Log(msgs.Count);  // Number of messages

        //processedMessages = new List<Sms>();

        for (int i = 0; i < msgs.Count - 1; i++)    // Loop through messages
        {
            Sms sms = new Sms();
            for (int j = 0; j < msgInfo - 1; j++)   // loop through message info
            {
                string[] items = msgs[i][j].Split(':');
                string valueType = items[0].Trim();
                string value = items[1];

                for (int k = 0; k < desiredMsgInfo.Length; k++) // loop through desired info
                {
                    if (valueType == desiredMsgInfo[k])
                    {
                        if (valueType == "_id") sms._id = value;
                        if (valueType == "address") sms._address = (value);
                        if (valueType == "date")
                        {
                            //for (int p = 1; p > items.Length - 1; p++)    // add other pieces together
                                //value += items[p];
                            //sms._time = (value);
                        }
                        if (valueType == "body") parseBodyText(sms, value);
                    }
                }
            }
            if (sms._msgType.Length > 0)
                processedMessages.Add(sms);
        }
        return processedMessages;
    }*/

}
