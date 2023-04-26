/*
Copyright (c) 2018 Convergence Systems Limited

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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
            Plugin.BLE.Abstractions.Trace.Message(string.Format(format, args));
        }

        public static void Write(string format, params object[] args)
        {
            Plugin.BLE.Abstractions.Trace.Message(string.Format(format, args));
        }
    }
}
