namespace CSLibrary
{
    public static class Debug
    {
        public static void WriteBytes(string header, byte[] data)
        {
            string str = "";
            for (int cnt = 0; cnt < data.Length; cnt++)
                str += data[cnt].ToString("X2") + " ";
            WriteLine ("CSLibrary : " + header + " {0}:{1}", data.Length, str);
        }

        public static void WriteLine(string format, params object[] args)
        {
            System.Console.WriteLine(string.Format(format, args));
            //Plugin.BLE.Abstractions.Trace.Message(string.Format(format, args));
        }

        public static void Write(string format, params object[] args)
        {
            System.Console.WriteLine(string.Format(format, args));
            //Plugin.BLE.Abstractions.Trace.Message(string.Format(format, args));
        }
    }
}
