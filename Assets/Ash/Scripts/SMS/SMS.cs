using System;

public class Sms
{
    [SQLite4Unity3d.PrimaryKey]
    public string _id { get; set; }
    public string _address { get; set; }
    public string _msgType { get; set; }
    public double _changeAmt { get; set; }
    public double _balance { get; set; }
    public string _beneficiaryName { get; set; }
    public DateTime _dateTime { get; set; }
}