﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE.Client
{
    public interface IExternalStorage
    {
        string GetPath();

        public void SaveTextFileToDocuments(string fileName, string content, int fileType);
    }
}
