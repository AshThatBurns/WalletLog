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
    public DBService ds;

    public static SQLManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void StartSync()
    {
        ds = new DBService(dbName);
        ds.CreateTable();
    }

    public void CreateNewEntry(Sms sms)
    {
        ds.CreateNewEntry(sms);
    }

    public IEnumerable<Sms> GetSMSFromDatabase(int limit)
    {
        return ds.CreateQuery("select * from Sms Limit ?", limit);
    }

    public void DumpSMSData()
    {
        //DataService(dbName);
    }

    
}
