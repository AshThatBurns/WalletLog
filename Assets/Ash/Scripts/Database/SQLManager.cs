using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SQLite4Unity3d;
#if !UNITY_EDITOR
using System.IO;
#endif

public class SQLManager : MonoBehaviour
{
    
    string dbName = "tempDatabase.db";

    public static SQLManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void SubmitQuery()
    {
        //_connection.Query<SMS>(sqlDebugPanel.inputField.text, null);
    }

    public void DumpSMSData()
    {
        //DataService(dbName);
    }

    
}
