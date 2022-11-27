namespace NuitrackSDK
{
    [System.Serializable]
    public class LicenseInfo
    {
        public int LicenseVersion;
        public string Signature;
        public int SignatureVersion;
        public int Serial;
        public int OSType;
        public string SensorName = "NONE";
        public bool Trial;
    }
}