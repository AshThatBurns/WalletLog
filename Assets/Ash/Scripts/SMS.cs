public class Sms
{
    private string _id;
    private string _address;
    private string _msg;
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
    public string getMsg()
    {
        return _msg;
    }
    public string getReadState()
    {
        return _readState;
    }
    public string getTime()
    {
        return _time;
    }
    public string getFolderName()
    {
        return _folderName;
    }


    public void setId(string id)
    {
        _id = id;
    }
    public void setAddress(string address)
    {
        _address = address;
    }
    public void setMsg(string msg)
    {
        _msg = msg;
    }
    public void setReadState(string readState)
    {
        _readState = readState;
    }
    public void setTime(string time)
    {
        _time = time;
    }
    public void setFolderName(string folderName)
    {
        _folderName = folderName;
    }

}