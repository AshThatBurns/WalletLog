using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultsTable : MonoBehaviour
{
    public GameObject tableCellPrefab;
    public Transform tableContent;

    public static ResultsTable instance;

    private void Awake()
    {
        instance = this;
    }

    public void GenerateResultsTable(IEnumerable<Sms> queryResults) 
    {
        // delete child objs
        foreach (Transform child in tableContent) Destroy(child.gameObject);

        // Generate category headers
        Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = "Id";
        Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = "Sender";
        Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = "Time";
        Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = "Transaction Type";
        Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = "Change Amount";
        Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = "Balance";
        Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = "Beneficiary Name";

        //for (int s = 0; s < numberOfEntries; s++)
        foreach (var sms in queryResults)
        {
            Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = sms._id;
            Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = sms._address;
            Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = sms._dateTime.ToString();
            Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = sms._msgType;
            Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = sms._changeAmt.ToString();
            Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = sms._balance.ToString();
            Instantiate(tableCellPrefab, tableContent).GetComponent<Text>().text = sms._beneficiaryName;
        }
    }

}
