using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class ArduinoManager : MonoBehaviour
{
    public static ArduinoManager Instance;
    void Awake(){
        if(Instance == null) Instance = this;
        else if(Instance != this){
            Destroy(this.gameObject);
            Destroy(this);
        }
        DontDestroyOnLoad(this);
    } 
    #region PORT
    public string PortName = "COM8";
    public int BaudRate = 115200;
    public int DataBits = 8;
    #endregion

    private SerialPort sp;
    string writng = "s";
    #region Unity Event
    void Start()
    {
        sp = new SerialPort();
        sp.PortName = PortName;     // 여기에는 아두이노 포트 넣어주면 됩니다.
        sp.BaudRate = BaudRate;      // 아두이노 보레이트랑 맞춰주시면 됩니다.
        sp.DataBits = DataBits;
        sp.Parity = Parity.None;
        sp.StopBits = StopBits.One;
        sp.Open();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        switch (Input.inputString)
        {
            case "W":
            case "w":
                Debug.Log("press w");
                sp.WriteLine("w");
                writng = "w";
                break;

            case "A":
            case "a":
              Debug.Log("press a");
                sp.WriteLine("a");
                writng = "a";
                break;

            case "S":
            case "s":
                Debug.Log("press s");
                sp.WriteLine("s");
                writng = "s";
                break;

            case "D":
            case "d":
                Debug.Log("press d");
                sp.WriteLine("d");
                writng = "d";
                break;

        }

        sp.WriteLine(writng);
        */
    }
    void OnApplicationQuit()
    {
        sp.Close();    //꺼질때 소켓을 닫아줍니다.
    }
    #endregion
    #region Arduino Serial
    public void SpinClockwise(){
        sp.WriteLine("q");
    }
    public void SpinCounterClockwise(){
        sp.WriteLine("e");
    }
    public void StopSpinning(){
        sp.WriteLine("n");
    }
    #endregion
}