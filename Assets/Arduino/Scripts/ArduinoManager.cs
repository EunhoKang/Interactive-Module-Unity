using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.IO;

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

    private SerialPortUnit serialPortUnit;
    string writng = "s";
    #region Unity Event
    void Start()
    {
        serialPortUnit = new SerialPortUnit(PortName, BaudRate, DataBits);
    }
/*
    void Update(){
        if(Input.GetKeyDown("q")) SpinClockwise();
        if(Input.GetKeyDown("e")) SpinCounterClockwise();
    }
*/  
    void OnApplicationQuit()
    {
        serialPortUnit.ClosePort();
    }
    #endregion
    #region Arduino Serial
    public void SpinClockwise(){
        serialPortUnit.WriteLine("a");
    }
    public void SpinCounterClockwise(){
        serialPortUnit.WriteLine("d");
    }
    public void StopSpinning(){
        serialPortUnit.WriteLine("n");
    }
    #endregion
}

public class SerialPortUnit {
    private SerialPort serialPort;
    private string portName; 
    private int baudRate; 
    private int dataBits;
    private bool isPortOpen;
    public SerialPortUnit(){
        this.portName = "COM8";
        this.baudRate = 115200;
        this.dataBits = 8;
        isPortOpen = false;
        initializePort();
    }
    public SerialPortUnit(string portName, int baudRate, int dataBits){
        this.portName = portName;
        this.baudRate = baudRate;
        this.dataBits = dataBits;
        isPortOpen = false;
        initializePort();
    }
    private void initializePort() {
        try {
            serialPort = new SerialPort();
            serialPort.PortName = portName;
            serialPort.BaudRate = baudRate;
            serialPort.DataBits = dataBits;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
            serialPort.Open();
            isPortOpen = true;
        }
        catch ( IOException IOException ) {
            Debug.LogError($"Arduino not linked : {IOException}");
            isPortOpen = false;
        }
    }
    public void WriteLine(string line){
        if(isPortOpen)
            serialPort.WriteLine(line);
    }
    public void ClosePort(){
        serialPort.Close();
    }
}