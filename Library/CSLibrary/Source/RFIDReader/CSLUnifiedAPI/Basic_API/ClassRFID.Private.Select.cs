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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSLibrary
{
    using static RFIDDEVICE;
    using Constants;

    public partial class RFIDReader
    {
        private Structures.TagSelectedParms _tagSelectedParms;

        private void TagSelected()
        {
            switch (_deviceType)
            {
                case MODEL.CS710S:
                    TagSelected_CS710S();
                    break;
            }
        }

        private void TagSelectedDYNQ()
        {
            switch (_deviceType)
            {
                case MODEL.CS710S:
                    //TagSelectedDYNQ_CS710S();
                    break;
            }
        }

        /// <summary>
        /// Only set first EPC ID and length (register 0x804-0x807)
        /// </summary>
        private void FastTagSelected()
        {
            switch (_deviceType)
            {
                case MODEL.CS710S:
                    FastTagSelected_CS710S();
                    break;
            }
        }

        private void PreFilter()
        {
            switch (_deviceType)
            {
                case MODEL.CS710S:
                    PreFilter_CS710S();
                    break;
            }
        }

        private void SetMaskThreadProc()
        {
            switch (_deviceType)
            {
                case MODEL.CS710S:
                    SetMaskThreadProc_CS710S();
                    break;
            }
        }

        private void PostFilter()
        {
            switch (_deviceType)
            {
                case MODEL.CS710S:
                    PostFilter_CS710S();
                    break;
            }
        }

    }
}
