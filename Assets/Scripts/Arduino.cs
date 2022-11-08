using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class Arduino : MonoBehaviour
{

    SerialPort sp = new SerialPort();
    string writng = "s";

    void Start()
    {
        sp.PortName = "COM8";     // 여기에는 아두이노 포트 넣어주면 됩니다.
        sp.BaudRate = 115200;      // 아두이노 보레이트랑 맞춰주시면 됩니다.
        sp.DataBits = 8;
        sp.Parity = Parity.None;
        sp.StopBits = StopBits.One;

        sp.Open();
    }

    // Update is called once per frame
    void Update()
    {
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

        void OnApplicationQuit()
        {
            sp.Close();    //꺼질때 소켓을 닫아줍니다.
        }

        sp.WriteLine(writng);
    }

    public void Clock()
    {
        writng = "a";
    }

    public void UnClock()
    {
        writng = "d";
    }

    public void Stop()
    {
        writng = "s";
    }

}