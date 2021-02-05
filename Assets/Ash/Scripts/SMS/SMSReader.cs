using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SMSReader : MonoBehaviour
{

    string[] desiredContacts = { "EmiratesNBD" };
    /// <summary>
    /// 1: _id
    /// 2: address, 
    /// 4: date, 
    /// 12: body
    /// </summary>
    string[] desiredMsgInfo = { "_id", "address", "date", "body" };
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

    List<List<string>> msgs;

    /// <summary>
    /// Order from newest -> oldest
    /// </summary>
    List<Sms> processedMessages;

    public GameObject tableCellPrefab;
    public Transform tableContent;

    public static SMSReader instance;

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

        sms.setMsgType(currMsgType.ToString());
        sms.setChangeAmount(changeAmt);
        sms.setBalance(balanceAmt);
        sms.setBeneficiaryName(beneficiaryName);
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

                msgs = new List<List<string>>();
                if (cursor.Call<bool>("moveToFirst"))
                {
                    do      // Loop through all messages
                    {
                        bool isDesiredContact = false;
                        List<string> msgData = new List<string>();

                        int columnCount = cursor.Call<int>("getColumnCount");

                        // Loop through all elements of current message
                        for (int idx = 0; idx < columnCount; idx++) 
                        {

                            bool isDesiredInfo = false;

                            // Retrieve type and data
                            string valueType = cursor.Call<string>("getColumnName", idx).Trim();
                            string value = cursor.Call<string>("getString", idx);

                            // Check if message came from desired contacts
                            if (valueType == "address")
                            {
                                for (int i = 0; i < desiredContacts.Length; i++)
                                {
                                    isDesiredContact = value == desiredContacts[i];
                                }
                            }

                            msgData.Add(valueType + ":" + value);
                        }

                        // Save msgData only if its from a desired contact
                        if (isDesiredContact) msgs.Add(msgData);

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

        #region Store data in sms objects
        int msgInfo = msgs[0].Count;    // Number of categories
        //Debug.Log(msgs.Count);  // Number of messages

        progress = 0;

        processedMessages = new List<Sms>();

        for (int i = 0; i < msgs.Count - 1; i++)    // Loop through messages
        {
            Sms sms = new Sms();
            for (int j = 0; j < msgInfo - 1; j++)   // loop through message info
            {
                string[] items = msgs[i][j].Split(':');
                string valueType = items[0].Trim();
                string value = items[1].Trim();

                for (int k = 0; k < desiredMsgInfo.Length; k++) // loop through desired info
                {
                    if (valueType == desiredMsgInfo[k])
                    {
                        if (valueType == "_id") sms.setId(value);
                        if (valueType == "address") sms.setAddress(value);
                        if (valueType == "date") sms.setTime(value);
                        if (valueType == "body") parseBodyText(sms, value);
                    }

                    // calculate progress
                    int totalProgress = msgInfo * msgs.Count * desiredMsgInfo.Length;
                    int currProgress = i * j * k;
                    progress = (currProgress / totalProgress) * 100;
                }
            }
            processedMessages.Add(sms);
        }
        msgs.Clear();
        #endregion

        #region Create a table

        for (int s = 0; s < 5/*processedMessages.Count-1*/; s++)
        {
            Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = processedMessages[s].getId();
            Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = processedMessages[s].getAddress();
            Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = processedMessages[s].getTime();
            Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = processedMessages[s].getMsgType();
            Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = processedMessages[s].getChangeAmount().ToString();
            Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = processedMessages[s].getBalance().ToString();
            Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = processedMessages[s].getBeneficiaryName();
        }
        #endregion

        loadingProgressPanel.SetActive(false);
        finishLoadingPanel.SetActive(true);

        isLoading = false;
        yield return null;
    }

}
