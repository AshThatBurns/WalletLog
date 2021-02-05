public class Sms
{
    private string _id;
    private string _address;
    private string _msgType;
    private double _changeAmt;
    private double _balance;
    private string _beneficiaryName;
    private string _readState; //"0" for have not read sms and "1" for have read sms
    private string _time;
    private string _folderName;

    public string getId()
    {
        return _id;
    }
    public string getAddress()
    {
        return _address;
    }
    public string getTime()
    {
        return _time;
    }
    public string getMsgType()
    {
        return _msgType;
    }
    public double getChangeAmount()
    {
        return _changeAmt;
    }
    public double getBalance()
    {
        return _balance;
    }
    public string getBeneficiaryName()
    {
        return _beneficiaryName;
    }

    public void setId(string id)
    {
        _id = id;
    }
    public void setAddress(string address)
    {
        _address = address;
    }
    public void setTime(string time)
    {
        _time = time;
    }
    public void setMsgType(string msgType)
    {
        _msgType = msgType;
    }
    public void setChangeAmount(double changeAmt)
    {
        _changeAmt = changeAmt;
    }
    public void setBalance(double balance)
    {
        _balance = balance;
    }
    public void setBeneficiaryName(string benName)
    {
        _beneficiaryName = benName;
    }
}