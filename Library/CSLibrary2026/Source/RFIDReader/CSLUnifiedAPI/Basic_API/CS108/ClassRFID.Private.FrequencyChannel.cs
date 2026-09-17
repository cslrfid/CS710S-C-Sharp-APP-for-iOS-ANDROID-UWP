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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSLibrary
{
    using static FrequencyBand;
    using Constants;

    public partial class RFIDReader
    {
        private uint[] currentInventoryFreqRevIndex = null;

        #region ====================== Frequency Table ======================

        #region FCC (-2)
        /// <summary>
        /// FCC Frequency Table
        /// </summary>
        private readonly double[] FCCTableOfFreq = new double[]
            {
                902.75,//0
                903.25,//1
                903.75,
                904.25,
                904.75,
                905.25,//5
                905.75,
                906.25,
                906.75,
                907.25,
                907.75,//10
                908.25,
                908.75,
                909.25,
                909.75,
                910.25,//15
                910.75,
                911.25,
                911.75,
                912.25,
                912.75,//20
                913.25,
                913.75,
                914.25,
                914.75,
                915.25,//25
                915.75,
                916.25,
                916.75,
                917.25,
                917.75,//30
                918.25,
                918.75,
                919.25,
                919.75,
                920.25,//35
                920.75,
                921.25,
                921.75,
                922.25,
                922.75,//40
                923.25,
                923.75,
                924.25,
                924.75,
                925.25,//45
                925.75,
                926.25,
                926.75,
                927.25,//49
            };
        /*OK*/
        private uint[] fccFreqTable = new uint[]
        {
            0x00180E1F, /*903.75 MHz  2 */
            0x00180E41, /*912.25 MHz  19 */
            0x00180E2F, /*907.75 MHz  10 */
            0x00180E39, /*910.25 MHz  15 */
            0x00180E6B, /*922.75 MHz  40 */
            0x00180E6D, /*923.25 MHz  41 */
            0x00180E6F, /*923.75 MHz  42 */
            0x00180E4D, /*915.25 MHz  25 */
            0x00180E35, /*909.25 MHz  13 */
            0x00180E43, /*912.75 MHz  20 */
            0x00180E3B, /*910.75 MHz  16 */
            0x00180E47, /*913.75 MHz  22 */
            0x00180E37, /*909.75 MHz  14 */
            0x00180E25, /*905.25 MHz  5 */
            0x00180E3F, /*911.75 MHz  18 */
            0x00180E1B, /*902.75 MHz  0 */
            0x00180E49, /*914.25 MHz  23 */
            0x00180E59, /*918.25 MHz  31 */
            0x00180E79, /*926.25 MHz  47 */
            0x00180E77, /*925.75 MHz  46 */
            0x00180E63, /*920.75 MHz  36 */
            0x00180E61, /*920.25 MHz  35 */
            0x00180E2D, /*907.25 MHz  9 */
            0x00180E4B, /*914.75 MHz  24 */
            0x00180E5F, /*919.75 MHz  34 */
            0x00180E69, /*922.25 MHz  39 */
            0x00180E1D, /*903.25 MHz  1 */
            0x00180E29, /*906.25 MHz  7 */
            0x00180E27, /*905.75 MHz  6 */
            0x00180E7B, /*926.75 MHz  48 */
            0x00180E71, /*924.25 MHz  43 */
            0x00180E23, /*904.75 MHz  4 */
            0x00180E75, /*925.25 MHz  45 */
            0x00180E73, /*924.75 MHz  44 */
            0x00180E5D, /*919.25 MHz  33 */
            0x00180E53, /*916.75 MHz  28 */
            0x00180E3D, /*911.25 MHz  17 */
            0x00180E65, /*921.25 MHz  37 */
            0x00180E31, /*908.25 MHz  11 */
            0x00180E33, /*908.75 MHz  12 */
            0x00180E45, /*913.25 MHz  21 */
            0x00180E51, /*916.25 MHz  27 */
            0x00180E21, /*904.25 MHz  3 */
            0x00180E2B, /*906.75 MHz  8 */
            0x00180E57, /*917.75 MHz  30 */
            0x00180E67, /*921.75 MHz  38 */
            0x00180E55, /*917.25 MHz  29 */
            0x00180E7D, /*927.25 MHz  49 */
            0x00180E5B, /*918.75 MHz  32 */
            0x00180E4F, /*915.75 MHz  26 */
        };
        /// <summary>
        /// FCC Frequency Channel number
        /// </summary>
        private const uint FCC_CHN_CNT = 50;
        private readonly uint[] fccFreqSortedIdx = new uint[]{
            2, 19, 10, 15, 40,
            41, 42, 25, 13, 20,
            16, 22, 14, 5, 18,
            0, 23, 31, 47, 46,
            36, 35, 9, 24, 34,
            39, 1, 7, 6, 48,
            43, 4, 45, 44, 33,
            28, 17, 37, 11, 12,
            21, 27, 3, 8, 30,
            38, 29, 49, 32, 26
        };

        private uint[] fccFreqTable_Ver20170001 = new uint[]
        {
            0x00180E4D, /*915.25 MHz  25 */
            0x00180E63, /*920.75 MHz  36 */
            0x00180E35, /*909.25 MHz  13 */
            0x00180E41, /*912.25 MHz  19 */
            0x00180E59, /*918.25 MHz  31 */
            0x00180E61, /*920.25 MHz  35 */
            0x00180E37, /*909.75 MHz  14 */
            0x00180E39, /*910.25 MHz  15 */
            0x00180E5F, /*919.75 MHz  34 */
            0x00180E6B, /*922.75 MHz  40 */
            0x00180E33, /*908.75 MHz  12 */
            0x00180E47, /*913.75 MHz  22 */
            0x00180E1F, /*903.75 MHz  2 */
            0x00180E5D, /*919.25 MHz  33 */
            0x00180E69, /*922.25 MHz  39 */
            0x00180E2F, /*907.75 MHz  10 */
            0x00180E3F, /*911.75 MHz  18 */
            0x00180E6F, /*923.75 MHz  42 */
            0x00180E53, /*916.75 MHz  28 */
            0x00180E79, /*926.25 MHz  47 */
            0x00180E31, /*908.25 MHz  11 */
            0x00180E43, /*912.75 MHz  20 */
            0x00180E71, /*924.25 MHz  43 */
            0x00180E51, /*916.25 MHz  27 */
            0x00180E7D, /*927.25 MHz  49 */
            0x00180E2D, /*907.25 MHz  9 */
            0x00180E3B, /*910.75 MHz  16 */
            0x00180E1D, /*903.25 MHz  1 */
            0x00180E57, /*917.75 MHz  30 */
            0x00180E7B, /*926.75 MHz  48 */
            0x00180E25, /*905.25 MHz  5 */
            0x00180E3D, /*911.25 MHz  17 */
            0x00180E73, /*924.75 MHz  44 */
            0x00180E55, /*917.25 MHz  29 */
            0x00180E77, /*925.75 MHz  46 */
            0x00180E2B, /*906.75 MHz  8 */
            0x00180E49, /*914.25 MHz  23 */
            0x00180E23, /*904.75 MHz  4 */
            0x00180E5B, /*918.75 MHz  32 */
            0x00180E6D, /*923.25 MHz  41 */
            0x00180E1B, /*902.75 MHz  0 */
            0x00180E4B, /*914.75 MHz  24 */
            0x00180E27, /*905.75 MHz  6 */
            0x00180E4F, /*915.75 MHz  26 */
            0x00180E75, /*925.25 MHz  45 */
            0x00180E29, /*906.25 MHz  7 */
            0x00180E65, /*921.25 MHz  37 */
            0x00180E45, /*913.25 MHz  21 */
            0x00180E67, /*921.75 MHz  38 */
            0x00180E21, /*904.25 MHz  3 */
        };

        private readonly uint[] fccFreqSortedIdx_Ver20170001 = new uint[]{
            25, 36, 13, 19, 31,
            35, 14, 15, 34, 40,
            12, 22, 2, 33, 39,
            10, 18, 42, 28, 47,
            11, 20, 43, 27, 49,
            9, 16, 1, 30, 48,
            5, 17, 44, 29, 46,
            8, 23, 4, 32, 41,
            0, 24, 6, 26, 45,
            7, 37, 21, 38, 3
        };

        #endregion

        #region South Africa
        /// <summary>
        /// South Africa Frequency Table
        /// </summary>
        private readonly double[] ZATableOfFreq = new double[]
            {
                915.7,
                915.9,
                916.1,
                916.3,
                916.5,
                916.7,
                916.9,
                917.1,
                917.3,
                917.5,
                917.7,
                917.9,
                918.1,
                918.3,
                918.5,
                918.7,
            };
        /*OK*/
        private uint[] zaFreqTable = new uint[]
        {
            0x003C23C5, /*915.7 MHz   */ 
            0x003C23C7, /*915.9 MHz   */
            0x003C23C9, /*916.1 MHz   */
            0x003C23CB, /*916.3 MHz   */
            0x003C23CD, /*916.5 MHz   */
            0x003C23CF, /*916.7 MHz   */
            0x003C23D1, /*916.9 MHz   */
            0x003C23D3, /*917.1 MHz   */
            0x003C23D5, /*917.3 MHz   */
            0x003C23D7, /*917.5 MHz   */
            0x003C23D9, /*917.7 MHz   */
            0x003C23DB, /*917.9 MHz   */
            0x003C23DD, /*918.1 MHz   */
            0x003C23DF, /*918.3 MHz   */
            0x003C23E1, /*918.5 MHz   */
            0x003C23E3, /*918.7 MHz   */
        };
        /// <summary>
        /// FCC Frequency Channel number
        /// </summary>
        private const uint ZA_CHN_CNT = 16;
        private readonly uint[] zaFreqSortedIdx = new uint[]{
            0,1,2,3,
            4,5,6,7,
            8,9,10,11,
            12,13,14,15
        };
        #endregion

        #region ETSI, G800
        /// <summary>
        /// ETSI, G800 and India Frequency Table
        /// </summary>
        private readonly double[] ETSITableOfFreq = new double[]
        {
            865.70,
            866.30,
            866.90,
            867.50,
        };

        /*OK*/
        private readonly uint[] etsiFreqTable = new uint[]
        {
            0x003C21D1, /*865.700MHz   */
            0x003C21D7, /*866.300MHz   */
            0x003C21DD, /*866.900MHz   */
            0x003C21E3, /*867.500MHz   */
        };
        /// <summary>
        /// ETSI Frequency Channel number
        /// </summary>
        private const uint ETSI_CHN_CNT = 4;
        private readonly uint[] etsiFreqSortedIdx = new uint[]{
            0,
            1,
            2,
            3
        };

        #endregion

        #region India
        /// <summary>
        /// India Frequency Table
        /// </summary>
        private readonly double[] IDATableOfFreq = new double[]
        {
            865.70,
            866.30,
            866.90,
        };

        /*OK*/
        private readonly uint[] indiaFreqTable = new uint[]
        {
            0x003C21D1, /*865.700MHz   */
            0x003C21D7, /*866.300MHz   */
            0x003C21DD, /*866.900MHz   */
        };
        /// <summary>
        /// India Frequency Channel number
        /// </summary>
        private const uint IDA_CHN_CNT = 3;
        private readonly uint[] indiaFreqSortedIdx = new uint[]{
            0,
            1,
            2,
        };

        #endregion

        #region Australia
        /// <summary>
        /// Australia Frequency Table
        /// </summary>
        private readonly double[] AUSTableOfFreq = new double[]
        {
            920.75,
            921.25,
            921.75,
            922.25,
            922.75,
            923.25,
            923.75,
            924.25,
            924.75,
            925.25,
        };
        /*OK*/
        private readonly uint[] AusFreqTable = new uint[]
        {
            0x00180E63, /* 920.75MHz   */
            0x00180E69, /* 922.25MHz   */
            0x00180E6F, /* 923.75MHz   */
            0x00180E73, /* 924.75MHz   */
            0x00180E65, /* 921.25MHz   */
            0x00180E6B, /* 922.75MHz   */
            0x00180E71, /* 924.25MHz   */
            0x00180E75, /* 925.25MHz   */
            0x00180E67, /* 921.75MHz   */
            0x00180E6D, /* 923.25MHz   */
        };
        /// <summary>
        /// Australia Frequency Channel number
        /// </summary>
        private const uint AUS_CHN_CNT = 10;

        private readonly uint[] ausFreqSortedIdx = new uint[]{
                                                    0, 3, 6, 8, 1,
                                                    4, 7, 9, 2, 5,};
        #endregion

        #region China
        /// <summary>
        /// China Frequency Table
        /// </summary>
        private readonly double[] CHNTableOfFreq = new double[]
        {
            920.625,
            920.875,
            921.125,
            921.375,
            921.625,
            921.875,
            922.125,
            922.375,
            922.625,
            922.875,
            923.125,
            923.375,
            923.625,
            923.875,
            924.125,
            924.375,
        };
        /*OK*/
        private readonly uint[] cnFreqTable = new uint[]
        {
            0x00301CD3, /*922.375MHz   */
            0x00301CD1, /*922.125MHz   */
            0x00301CCD, /*921.625MHz   */
            0x00301CC5, /*920.625MHz   */
            0x00301CD9, /*923.125MHz   */
            0x00301CE1, /*924.125MHz   */
            0x00301CCB, /*921.375MHz   */
            0x00301CC7, /*920.875MHz   */
            0x00301CD7, /*922.875MHz   */
            0x00301CD5, /*922.625MHz   */
            0x00301CC9, /*921.125MHz   */
            0x00301CDF, /*923.875MHz   */
            0x00301CDD, /*923.625MHz   */
            0x00301CDB, /*923.375MHz   */
            0x00301CCF, /*921.875MHz   */
            0x00301CE3, /*924.375MHz   */
        };
        /// <summary>
        /// China Frequency Channel number
        /// </summary>
        private const uint CN_CHN_CNT = 16;
        private readonly uint[] cnFreqSortedIdx = new uint[]{
                                                7, 6, 4, 0,
                                                10, 14, 3, 1,
                                                9, 8, 2, 13,
                                                12, 11, 5, 15,
                                                };
        #endregion

        #region China1
        /// <summary>
        /// China Frequency Table
        /// </summary>
        private readonly double[] CHN1TableOfFreq = new double[]
        {
            920.625,
            920.875,
            921.125,
            921.375,
        };
        private readonly uint[] cn1FreqTable = new uint[]
        {
            0x00301CC5, /*920.625MHz   */
            0x00301CC7, /*920.875MHz   */
            0x00301CC9, /*921.125MHz   */
            0x00301CCB, /*921.375MHz   */
        };
        /// <summary>
        /// China Frequency Channel number
        /// </summary>
        private const uint CN1_CHN_CNT = 4;
        private readonly uint[] cn1FreqSortedIdx = new uint[]{
                                                0,1,2,3
                                                };
        #endregion
        #region China2
        /// <summary>
        /// China Frequency Table
        /// </summary>
        private readonly double[] CHN2TableOfFreq = new double[]
        {
            921.625,
            921.875,
            922.125,
            922.375,
        };
        private readonly uint[] cn2FreqTable = new uint[]
        {
            0x00301CCD, /*921.625MHz   */
            0x00301CCF, /*921.875MHz   */
            0x00301CD1, /*922.125MHz   */
			0x00301CD3, /*922.375MHz   */
        };
        #endregion
        #region China3
        /// <summary>
        /// China Frequency Table
        /// </summary>
        private readonly double[] CHN3TableOfFreq = new double[]
        {
            922.625,
            922.875,
            923.125,
            923.375,
        };
        private readonly uint[] cn3FreqTable = new uint[]
        {
            0x00301CD5, /*922.625MHz   */
            0x00301CD7, /*922.875MHz   */
            0x00301CD9, /*923.125MHz   */
            0x00301CDB, /*923.375MHz   */
        };
        #endregion
        #region China4
        /// <summary>
        /// China Frequency Table
        /// </summary>
        private readonly double[] CHN4TableOfFreq = new double[]
        {
            923.625,
            923.875,
            924.125,
            924.375,
        };
        private readonly uint[] cn4FreqTable = new uint[]
        {
            0x00301CDD, /*923.625MHz   */
            0x00301CDF, /*923.875MHz   */
            0x00301CE1, /*924.125MHz   */
            0x00301CE3, /*924.375MHz   */
        };
        #endregion
        #region China5
        /// <summary>
        /// China Frequency Table
        /// </summary>
        private readonly double[] CHN5TableOfFreq = new double[]
        {
            920.625,
            921.625,
            922.625,
            923.625,
        };
        private readonly uint[] cn5FreqTable = new uint[]
        {
            0x00301CC5, /*920.625MHz   */
            0x00301CCD, /*921.625MHz   */
            0x00301CD5, /*922.625MHz   */
            0x00301CDD, /*923.625MHz   */
        };
        #endregion
        #region China6
        /// <summary>
        /// China Frequency Table
        /// </summary>
        private readonly double[] CHN6TableOfFreq = new double[]
        {
            920.875,
            921.875,
            922.875,
            923.875,
        };
        private readonly uint[] cn6FreqTable = new uint[]
        {
            0x00301CC7, /*920.875MHz   */
            0x00301CCF, /*921.875MHz   */
            0x00301CD7, /*922.875MHz   */
            0x00301CDF, /*923.875MHz   */
        };
        #endregion
        #region China7
        /// <summary>
        /// China Frequency Table
        /// </summary>
        private readonly double[] CHN7TableOfFreq = new double[]
        {
            921.125,
            922.125,
            923.125,
            924.125,
        };
        private readonly uint[] cn7FreqTable = new uint[]
        {
             0x00301CC9, /*921.125MHz   */
             0x00301CD1, /*922.125MHz   */
             0x00301CD9, /*923.125MHz   */
             0x00301CE1, /*924.125MHz   */
        };
        #endregion
        #region China8
        /// <summary>
        /// China Frequency Table
        /// </summary>
        private readonly double[] CHN8TableOfFreq = new double[]
        {
            921.375,
            922.375,
            923.375,
            924.375,
        };
        private readonly uint[] cn8FreqTable = new uint[]
        {
            0x00301CCB, /*921.375MHz   */
            0x00301CD3, /*922.375MHz   */
            0x00301CDB, /*923.375MHz   */
            0x00301CE3, /*924.375MHz   */
        };
        #endregion
        #region China9
        /// <summary>
        /// China Frequency Table
        /// </summary>
        private readonly double[] CHN9TableOfFreq = new double[]
        {
            920.625,
            920.875,
            921.125,
        };
        private readonly uint[] cn9FreqTable = new uint[]
        {
            0x00301CC5, /*920.625MHz   */
            0x00301CC7, /*920.875MHz   */
            0x00301CC9, /*921.125MHz   */
        };
        /// <summary>
        /// China Frequency Channel number
        /// </summary>
        private const uint CN9_CHN_CNT = 3;
        private readonly uint[] cn9FreqSortedIdx = new uint[]{
                                                0,1,2
                                                };
        #endregion
        #region China10
        /// <summary>
        /// China Frequency Table
        /// </summary>
        private readonly double[] CHN10TableOfFreq = new double[]
        {
            921.625,
            921.875,
            922.125,
        };
        private readonly uint[] cn10FreqTable = new uint[]
        {
            0x00301CCD, /*921.625MHz   */
            0x00301CCF, /*921.875MHz   */
            0x00301CD1, /*922.125MHz   */
        };
        #endregion
        #region China11
        /// <summary>
        /// China Frequency Table
        /// </summary>
        private readonly double[] CHN11TableOfFreq = new double[]
        {
            922.625,
            922.875,
            923.125,
        };
        private readonly uint[] cn11FreqTable = new uint[]
        {
            0x00301CD5, /*922.625MHz   */
            0x00301CD7, /*922.875MHz   */
            0x00301CD9, /*923.125MHz   */
        };
        #endregion
        #region China12
        /// <summary>
        /// China Frequency Table
        /// </summary>
        private readonly double[] CHN12TableOfFreq = new double[]
        {
            923.625,
            923.875,
            924.125,
        };
        private readonly uint[] cn12FreqTable = new uint[]
        {
            0x00301CDD, /*923.625MHz   */
            0x00301CDF, /*923.875MHz   */
            0x00301CE1, /*924.125MHz   */
        };
        #endregion
        #region Singapo
        /// <summary>
        /// Hong Kong and Singapo Frequency Table
        /// </summary>
        private readonly double[] HKTableOfFreq = new double[]
        {
            920.75,
            921.25,
            921.75,
            922.25,
            922.75,
            923.25,
            923.75,
            924.25,
        };
        /*OK*/
        private readonly uint[] hkFreqTable = new uint[]
        {
            0x00180E63, /*920.75MHz   */
            0x00180E69, /*922.25MHz   */
            0x00180E71, /*924.25MHz   */
            0x00180E65, /*921.25MHz   */
            0x00180E6B, /*922.75MHz   */
            0x00180E6D, /*923.25MHz   */
            0x00180E6F, /*923.75MHz   */
            0x00180E67, /*921.75MHz   */
        };
        /// <summary>
        /// Hong Kong Frequency Channel number
        /// </summary>
        private const uint HK_CHN_CNT = 8;
        private readonly uint[] hkFreqSortedIdx = new uint[]{
            0, 3, 7, 1,
            4, 5, 6, 2,
        };
        #endregion

        #region OFCA (Hong Kong)
        /// <summary>
        /// Hong Kong and Singapo Frequency Table
        /// </summary>
        private readonly double[] OFCATableOfFreq = new double[]
        {
 920.416, // CH1
 920.500, // CH2
 920.583, // CH3	
 920.666, // CH4
 920.750, // CH5
 920.833, // CH6
 920.916, // CH7
 921.000, // CH8
 921.083, // CH9
 921.166, // CH10
 921.250, // CH11	
 921.333, // CH12
 921.416, // CH13	
 921.500, // CH14	
 921.583, // CH15	
 921.666, // CH16 	
 921.750, // CH17
 921.833, // CH18
 921.916, // CH19
 922.000, // CH20	
 922.083, // CH21
 922.166, // CH22
 922.250, // CH23	
 922.333, // CH24
 922.416, // CH25
 922.500, // CH26 
 922.583, // CH27
 922.666, // CH28
 922.750, // CH29
 922.833, // CH30
 922.916, // CH31
 923.000, // CH32 	
 923.083, // CH33
 923.166, // CH34	
 923.250, // CH35	
 923.333, // CH36 	
 923.416, // CH37	
 923.500, // CH38
 923.583, // CH39
 923.666, // CH40	
 923.750, // CH41	
 923.833, // CH42
 923.916, // CH43
 924.000, // CH44
 924.083, // CH45
 924.166, // CH46
 924.250, // CH47
 924.333, // CH48
 924.416, // CH49
 924.500 // CH50
        };
        /*OK*/
        private readonly uint[] ofcaFreqTable = new uint[]
        {
0x00482B3E,// 922.500 MHz CH26 
0x00482B49,// 923.416 MHz CH37	
0x00482B32,// 921.500 MHz CH14	
0x00482B38,// 922.000 MHz CH20	
0x00482B44,// 923.000 MHz CH32 	
0x00482B48,// 923.333 MHz CH36 	
0x00482B33,// 921.583 MHz CH15	
0x00482B34,// 921.666 MHz CH16 	
0x00482B47,// 923.250 MHz CH35	
0x00482B4D,// 923.750 MHz CH41	
0x00482B31,// 921.416 MHz CH13	
0x00482B3B,// 922.250 MHz CH23	
0x00482B27,// 920.583 MHz CH3	
0x00482B46,// 923.166 MHz CH34	
0x00482B4C,// 923.666 MHz CH40	
0x00482B2F,// 921.250 MHz CH11	
0x00482B37,// 921.916 MHz CH19
0x00482B4F,// 923.916 MHz CH43
0x00482B41,// 922.750 MHz CH29
0x00482B54,// 924.333 MHz CH48
0x00482B30,// 921.333 MHz CH12
0x00482B39,// 922.083 MHz CH21
0x00482B50,// 924.000 MHz CH44
0x00482B40,// 924.666 MHz CH28
0x00482B56,// 924.500 MHz CH50
0x00482B2E,// 921.166 MHz CH10
0x00482B35,// 921.750 MHz CH17
0x00482B26,// 920.500 MHz CH2
0x00482B43,// 922.916 MHz CH31
0x00482B55,// 924.416 MHz CH49
0x00482B2A,// 920.833 MHz CH6
0x00482B36,// 921.833 MHz CH18
0x00482B51,// 924.083 MHz CH45
0x00482B42,// 922.833 MHz CH30
0x00482B53,// 924.250 MHz CH47
0x00482B2D,// 921.083 MHz CH9
0x00482B3C,// 922.333 MHz CH24
0x00482B29,// 920.750 MHz CH5
0x00482B45,// 923.083 MHz CH33
0x00482B4E,// 923.833 MHz CH42
0x00482B25,// 920.416 MHz CH1
0x00482B3D,// 922.416 MHz CH25
0x00482B2B,// 920.916 MHz CH7
0x00482B3F,// 922.583 MHz CH27
0x00482B52,// 924.166 MHz CH46
0x00482B2C,// 921.000 MHz CH8
0x00482B4A,// 923.500 MHz CH38
0x00482B3A,// 922.166 MHz CH22
0x00482B4B,// 923.583 MHz CH39
0x00482B28 // 920.666 MHz CH4
        };
        /// <summary>
        /// Hong Kong Frequency Channel number
        /// </summary>
        private const uint OFCA_CHN_CNT = 50;
        private readonly uint[] ofcaFreqSortedIdx = new uint[]{
25, 36, 13, 19, 31,
35, 14, 15, 34, 40,
12, 22, 2, 33, 39,
10, 18, 42, 28, 47,
11, 20, 43, 27, 49,
9, 16, 1, 30, 48,
5, 17, 44, 29, 46,
8, 23, 4, 32, 41,
0, 24, 6, 26, 45,
7, 37, 21, 38, 3
        };
        #endregion

        #region Japan
        /// <summary>
        /// Japan Frequency Table
        /// </summary>
        private readonly double[] JPNTableOfFreq = new double[]
        {
            952.20,
            952.40,
            952.60,
            952.80,
            953.00,
            953.20,
            953.40,
            953.60,
            953.80,
        };
        private readonly double[] JPNTableOfFreq28 = new double[]
        {
            //952.20,
            952.40,
            952.60,
            952.80,
            953.00,
            953.20,
            953.40,
            953.60,
            //953.80,
        };
        /// <summary>
        /// Japan Frequency Table
        /// </summary>
        private readonly double[] JPNTableOfFreq29 = new double[]
        {
            //952.20,
            952.40,
            952.60,
            952.80,
            953.00,
            953.20,
            953.40,
            953.60,
            953.80,
        };
        /*OK*/
        private readonly uint[] jpnFreqTable = new uint[]
        {
            0x003C2534, /*952.400MHz   Channel 2*/
            0x003C2542, /*953.800MHz   Channel 9*/
            0x003C253A, /*953.000MHz   Channel 5*/
            0x003C2540, /*953.600MHz   Channel 8*/
            0x003C2536, /*952.600MHz   Channel 3*/
            0x003C253C, /*953.200MHz   Channel 6*/
            0x003C2538, /*952.800MHz   Channel 4*/
            0x003C253E, /*953.400MHz   Channel 7*/
            0x003C2532, /*952.200MHz   Channel 1*/
        };
        private readonly uint[] jpnFreqTable28 = new uint[]
        {
            0x003C2534, /*952.400MHz   Channel 2*/
            0x003C253A, /*953.000MHz   Channel 5*/
            0x003C2540, /*953.600MHz   Channel 8*/
            0x003C2536, /*952.600MHz   Channel 3*/
            0x003C253C, /*953.200MHz   Channel 6*/
            0x003C2538, /*952.800MHz   Channel 4*/
            0x003C253E, /*953.400MHz   Channel 7*/
        };
        private readonly uint[] jpnFreqTable29 = new uint[]
        {
            0x003C2534, /*952.400MHz   Channel 2*/
            0x003C2542, /*953.800MHz   Channel 9*/
            0x003C253A, /*953.000MHz   Channel 5*/
            0x003C2540, /*953.600MHz   Channel 8*/
            0x003C2536, /*952.600MHz   Channel 3*/
            0x003C253C, /*953.200MHz   Channel 6*/
            0x003C2538, /*952.800MHz   Channel 4*/
            0x003C253E, /*953.400MHz   Channel 7*/
        };

        /// <summary>
        /// Japan Frequency Channel number
        /// </summary>
        private const uint JPN_CHN_CNT = 9;
        private const uint JPN_CHN_CNT28 = 7;
        private const uint JPN_CHN_CNT29 = 8;
        private readonly uint[] jpnFreqSortedIdx = new uint[]{
            0, 4, 6, 2, 5, 7, 3, 1, 8
        };
        private readonly uint[] jpnFreqSortedIdx28 = new uint[]{
            0, 4, 6, 2, 5, 3, 1
        };
        private readonly uint[] jpnFreqSortedIdx29 = new uint[]{
            0, 4, 6, 2, 5, 7, 3, 1
        };

#if nouse
        private readonly uint[] jpnFreqTable = new uint[]
        {
            //0x003C2532, /*952.200MHz   */
            0x003C2534, /*952.400MHz   */
            0x003C2536, /*952.600MHz   */
            0x003C2538, /*952.800MHz   */
            0x003C253A, /*953.000MHz   */
            0x003C253C, /*953.200MHz   */
            0x003C253E, /*953.400MHz   */
            0x003C2540, /*953.600MHz   */
            //0x003C2542, /*953.800MHz   */
        };

        /// <summary>
        /// Japan Frequency Channel number
        /// </summary>
        private const uint JPN_CHN_CNT = 7;// CS203 is not supported channel 1 and 9;
        private readonly uint[] jpnFreqSortedIdx = new uint[]{
	        0, 1, 2,
            3, 4, 5,
            6, //7, 8,
        };
#endif
        #endregion

        #region Japan
        /// <summary>
        /// Japan 2012 Frequency Table
        /// </summary>
        private readonly double[] JPN2012TableOfFreq = new double[]
        {
            916.80,
            918.00,
            919.20,
            920.40,
            920.60,
            920.80,
        };
        /*OK*/
        private readonly uint[] jpn2012FreqTable = new uint[]
        {
            0x003C23D0, /*916.800MHz   Channel 1*/
            0x003C23DC, /*918.000MHz   Channel 2*/
            0x003C23E8, /*919.200MHz   Channel 3*/
            0x003C23F4, /*920.400MHz   Channel 4*/
            0x003C23F6, /*920.600MHz   Channel 5*/
            0x003C23F8, /*920.800MHz   Channel 6*/
        };
        /// <summary>
        /// Japan Frequency Channel number
        /// </summary>
        private const uint JPN2012_CHN_CNT = 6;
        private readonly uint[] jpn2012FreqSortedIdx = new uint[]{
            0, 1, 2, 3, 4, 5
        };

        private readonly double[] JPN2019TableOfFreq = new double[]
        {
            916.80,
            918.00,
            919.20,
            920.40,
        };

        private readonly uint[] jpn2019FreqTable = new uint[]
        {
            0x003C23D0, /*916.800MHz   Channel 1*/
            0x003C23DC, /*918.000MHz   Channel 2*/
            0x003C23E8, /*919.200MHz   Channel 3*/
            0x003C23F4, /*920.400MHz   Channel 4*/
        };
        /// <summary>
        /// Japan Frequency 2019 Channel number
        /// </summary>
        private const uint JPN2019_CHN_CNT = 4;
        private readonly uint[] jpn2019FreqSortedIdx = new uint[]{
            0, 1, 2, 3
        };

        #endregion

        #region Korea
        /// <summary>
        /// Korea Frequency Table
        /// </summary>
        private double[] KRTableOfFreq = new double[]
        {
            917.30,
            917.90,
            918.50,
            919.10,
            919.70,
            920.30
        };

        /*Not same as CS101???*/
        private uint[] krFreqTable = new uint[]
        {
            0x003C23E7, /*919.1 MHz   */
            0x003C23D5, /*917.3 MHz   */
            0x003C23F3, /*920.3 MHz   */
            0x003C23DB, /*917.9 MHz   */
            0x003C23ED, /*919.7 MHz   */
            0x003C23E1, /*918.5 MHz   */
        };

        /// <summary>
        /// Korea Frequency Channel number
        /// </summary>
        private const uint KR_CHN_CNT = 6;
        private readonly uint[] krFreqSortedIdx = new uint[]{
            3, 0, 5, 1, 4, 2
        };

#if oldvalue
        /// <summary>
        /// Korea Frequency Table
        /// </summary>
        private double[] KRTableOfFreq = new double[]
        {
            910.20,
            910.40,
            910.60,
            910.80,
            911.00,
            911.20,
            911.40,
            911.60,
            911.80,
            912.00,
            912.20,
            912.40,
            912.60,
            912.80,
            913.00,
            913.20,
            913.40,
            913.60,
            913.80,
        };

        /*Not same as CS101???*/
        private uint[] krFreqTable = new uint[]
        {
            0x003C23A8, /*912.8MHz   13*/
            0x003C23A0, /*912.0MHz   9*/
            0x003C23AC, /*913.2MHz   15*/
            0x003C239E, /*911.8MHz   8*/
            0x003C23A4, /*912.4MHz   11*/
            0x003C23B2, /*913.8MHz   18*/
            0x003C2392, /*910.6MHz   2*/
            0x003C23B0, /*913.6MHz   17*/
            0x003C2390, /*910.4MHz   1*/
            0x003C239C, /*911.6MHz   7*/
            0x003C2396, /*911.0MHz   4*/
            0x003C23A2, /*912.2MHz   10*/
            0x003C238E, /*910.2MHz   0*/
            0x003C23A6, /*912.6MHz   12*/
            0x003C2398, /*911.2MHz   5*/
            0x003C2394, /*910.8MHz   3*/
            0x003C23AE, /*913.4MHz   16*/
            0x003C239A, /*911.4MHz   6*/
            0x003C23AA, /*913.0MHz   14*/
        };

        /// <summary>
        /// Korea Frequency Channel number
        /// </summary>
        private const uint KR_CHN_CNT = 19;
        private readonly uint[] krFreqSortedIdx = new uint[]{
            13, 9, 15, 8, 11,
            18, 2, 17, 1, 7,
            4, 10, 0, 12, 5,
            3, 16, 6, 14
        };
#endif

        /*private const uint VIRTUAL_KR_DIVRAT = 0x001E0000;
        private const uint VIRTUAL_KR_CHN_CNT = 19;
        private readonly uint[] Virtual_krFreqMultRat = new uint[]{ // with 0x001E as DIVRat
            0x11CA,0x11CE,0x11D2,0x11CD,0x11D6,
            0x11D8,0x11D3,0x11CF,0x11CB,0x11C9,
            0x11C7,0x11D1,0x11D4,0x11D9,0x11D7,
            0x11D5,0x11D0,0x11CC,0x11C8
        };
        private readonly uint[] Virtual_krFreqSortedIdx = new uint[]{
            10, 18, 9, 0, 8,
            17, 3, 1, 7, 16,
            11, 2, 6, 12, 15,
            4, 14, 5, 13
        };*/


        #endregion

        #region Malaysia
        /// <summary>
        /// Malaysia Frequency Table
        /// </summary>
        private double[] MYSTableOfFreq = new double[]
        {
            919.75,
            920.25,
            920.75,
            921.25,
            921.75,
            922.25,
        };

        private uint[] mysFreqTable = new uint[]
        {
            0x00180E5F, /*919.75MHz   */
            0x00180E65, /*921.25MHz   */
            0x00180E61, /*920.25MHz   */
            0x00180E67, /*921.75MHz   */
            0x00180E63, /*920.75MHz   */
            0x00180E69, /*922.25MHz   */
        };

        /// <summary>
        /// Malaysia Frequency Channel number
        /// </summary>
        private const uint MYS_CHN_CNT = 6;
        private readonly uint[] mysFreqSortedIdx = new uint[]{
                                                    0, 3, 1,
                                                    4, 2, 5,
                                                    };

#endregion

#region Taiwan
        /// <summary>
        /// Taiwan Frequency Table
        /// </summary>
        private double[] TWTableOfFreq = new double[]
        {
#if oldtwfreq
            922.25,
            922.75,
            923.25,
            923.75,
            924.25,
            924.75,
            925.25,
            925.75,
            926.25,
            926.75,
            927.25,
            927.75,
#endif
            922.875,
            923.250,
            923.625,
            924.000,
            924.375,
            924.750,
            925.125,
            925.500,
            925.875,
            926.250,
            926.625,
            927.000
        };

        /*Not same as CS101*/
        private uint[] twFreqTable = new uint[]
        {
#if oldtwfreq
            0x00180E7D, /*927.25MHz   10*/
            0x00180E73, /*924.75MHz   5*/
            0x00180E6B, /*922.75MHz   1*/
            0x00180E75, /*925.25MHz   6*/
            0x00180E7F, /*927.75MHz   11*/
            0x00180E71, /*924.25MHz   4*/
            0x00180E79, /*926.25MHz   8*/
            0x00180E6D, /*923.25MHz   2*/
            0x00180E7B, /*926.75MHz   9*/
            0x00180E69, /*922.25MHz   0*/
            0x00180E77, /*925.75MHz   7*/
            0x00180E6F, /*923.75MHz   3*/
#endif
            0x001009a7, /* 926.625 10 */
            0x001009a2, /* 924.750 5 */
            0x0010099e, /* 923.250 1 */
            0x001009a3, /* 925.125 6 */
            0x001009a8, /* 927.000 11 */
            0x001009a1, /* 924.375 4 */
            0x001009a5, /* 925.875 8 */
            0x0010099f, /* 923.625 2 */
            0x001009a6, /* 926.250 9 */
            0x0010099d, /* 922.875 0 */
            0x001009a4, /* 925.500 7 */
            0x001009a0, /* 924.000 3 */
        };

        /// <summary>
        /// Taiwan Frequency Channel number
        /// </summary>
        private const uint TW_CHN_CNT = 12;
        private readonly uint[] twFreqSortedIdx = new uint[]{
            10, 5, 1, 6,
            11, 4, 8, 2,
            9, 0, 7, 3,
        };
#endregion

#region Brazil

        private double[] BR1TableOfFreq = new double[]
            {
                /*902.75,
                903.25,
                903.75,
                904.25,
                904.75,
                905.25,
                905.75,
                906.25,
                906.75,
                907.25,
                907.75,
                908.25,
                908.75,
                909.25,
                909.75,
                910.25,
                910.75,
                911.25,
                911.75,
                912.25,
                912.75,
                913.25,
                913.75,
                914.25,
                914.75,
                915.25,*/
                915.75,
                916.25,
                916.75,
                917.25,
                917.75,
                918.25,
                918.75,
                919.25,
                919.75,
                920.25,
                920.75,
                921.25,
                921.75,
                922.25,
                922.75,
                923.25,
                923.75,
                924.25,
                924.75,
                925.25,
                925.75,
                926.25,
                926.75,
                927.25,
            };
        private uint[] br1FreqTable = new uint[]
        {
            0x00180E4F, /*915.75 MHz   */
            //0x00180E4D, /*915.25 MHz   */
            //0x00180E1D, /*903.25 MHz   */
            0x00180E7B, /*926.75 MHz   */
            0x00180E79, /*926.25 MHz   */
            //0x00180E21, /*904.25 MHz   */
            0x00180E7D, /*927.25 MHz   */
            0x00180E61, /*920.25 MHz   */
            0x00180E5D, /*919.25 MHz   */
            //0x00180E35, /*909.25 MHz   */
            0x00180E5B, /*918.75 MHz   */
            0x00180E57, /*917.75 MHz   */
            //0x00180E25, /*905.25 MHz   */
            //0x00180E23, /*904.75 MHz   */
            0x00180E75, /*925.25 MHz   */
            0x00180E67, /*921.75 MHz   */
            //0x00180E4B, /*914.75 MHz   */
            //0x00180E2B, /*906.75 MHz   */
            //0x00180E47, /*913.75 MHz   */
            0x00180E69, /*922.25 MHz   */
            //0x00180E3D, /*911.25 MHz   */
            //0x00180E3F, /*911.75 MHz   */
            //0x00180E1F, /*903.75 MHz   */
            //0x00180E33, /*908.75 MHz   */
            //0x00180E27, /*905.75 MHz   */
            //0x00180E41, /*912.25 MHz   */
            //0x00180E29, /*906.25 MHz   */
            0x00180E55, /*917.25 MHz   */
            //0x00180E49, /*914.25 MHz   */
            //0x00180E2D, /*907.25 MHz   */
            0x00180E59, /*918.25 MHz   */
            0x00180E51, /*916.25 MHz   */
            //0x00180E39, /*910.25 MHz   */
            //0x00180E3B, /*910.75 MHz   */
            //0x00180E2F, /*907.75 MHz   */
            0x00180E73, /*924.75 MHz   */
            //0x00180E37, /*909.75 MHz   */
            0x00180E5F, /*919.75 MHz   */
            0x00180E53, /*916.75 MHz   */
            //0x00180E45, /*913.25 MHz   */
            0x00180E6F, /*923.75 MHz   */
            //0x00180E31, /*908.25 MHz   */
            0x00180E77, /*925.75 MHz   */
            //0x00180E43, /*912.75 MHz   */
            0x00180E71, /*924.25 MHz   */
            0x00180E65, /*921.25 MHz   */
            0x00180E63, /*920.75 MHz   */
            0x00180E6B, /*922.75 MHz   */
            //0x00180E1B, /*902.75 MHz   */
            0x00180E6D, /*923.25 MHz   */
        };
        /// <summary>
        /// Brazil1 Frequency Channel number
        /// </summary>
        private const uint BR1_CHN_CNT = 24;
        private readonly uint[] br1FreqSortedIdx = new uint[]{
            0, 22, 21, 23,
            9, 7, 6, 4,
            19, 12, 13, 3,
            5, 1, 18, 8,
            2, 16, 20, 17,
            11, 10, 14, 15,
        };

        private double[] BR2TableOfFreq = new double[]
            {
                902.75,
                903.25,
                903.75,
                904.25,
                904.75,
                905.25,
                905.75,
                906.25,
                906.75,
                /*907.25,
                907.75,
                908.25,
                908.75,
                909.25,
                909.75,
                910.25,
                910.75,
                911.25,
                911.75,
                912.25,
                912.75,
                913.25,
                913.75,
                914.25,
                914.75,
                915.25,*/
                915.75,
                916.25,
                916.75,
                917.25,
                917.75,
                918.25,
                918.75,
                919.25,
                919.75,
                920.25,
                920.75,
                921.25,
                921.75,
                922.25,
                922.75,
                923.25,
                923.75,
                924.25,
                924.75,
                925.25,
                925.75,
                926.25,
                926.75,
                927.25,
            };
        private uint[] br2FreqTable = new uint[]
            {
                0x00180E4F, /*915.75 MHz   */
                //0x00180E4D, /*915.25 MHz   */
                0x00180E1D, /*903.25 MHz   */
                0x00180E7B, /*926.75 MHz   */
                0x00180E79, /*926.25 MHz   */
                0x00180E21, /*904.25 MHz   */
                0x00180E7D, /*927.25 MHz   */
                0x00180E61, /*920.25 MHz   */
                0x00180E5D, /*919.25 MHz   */
                //0x00180E35, /*909.25 MHz   */
                0x00180E5B, /*918.75 MHz   */
                0x00180E57, /*917.75 MHz   */
                0x00180E25, /*905.25 MHz   */
                0x00180E23, /*904.75 MHz   */
                0x00180E75, /*925.25 MHz   */
                0x00180E67, /*921.75 MHz   */
                //0x00180E4B, /*914.75 MHz   */
                0x00180E2B, /*906.75 MHz   */
                //0x00180E47, /*913.75 MHz   */
                0x00180E69, /*922.25 MHz   */
                //0x00180E3D, /*911.25 MHz   */
                //0x00180E3F, /*911.75 MHz   */
                0x00180E1F, /*903.75 MHz   */
                //0x00180E33, /*908.75 MHz   */
                0x00180E27, /*905.75 MHz   */
                //0x00180E41, /*912.25 MHz   */
                0x00180E29, /*906.25 MHz   */
                0x00180E55, /*917.25 MHz   */
                //0x00180E49, /*914.25 MHz   */
                //0x00180E2D, /*907.25 MHz   */
                0x00180E59, /*918.25 MHz   */
                0x00180E51, /*916.25 MHz   */
                //0x00180E39, /*910.25 MHz   */
                //0x00180E3B, /*910.75 MHz   */
                //0x00180E2F, /*907.75 MHz   */
                0x00180E73, /*924.75 MHz   */
                //0x00180E37, /*909.75 MHz   */
                0x00180E5F, /*919.75 MHz   */
                0x00180E53, /*916.75 MHz   */
                //0x00180E45, /*913.25 MHz   */
                0x00180E6F, /*923.75 MHz   */
                //0x00180E31, /*908.25 MHz   */
                0x00180E77, /*925.75 MHz   */
                //0x00180E43, /*912.75 MHz   */
                0x00180E71, /*924.25 MHz   */
                0x00180E65, /*921.25 MHz   */
                0x00180E63, /*920.75 MHz   */
                0x00180E6B, /*922.75 MHz   */
                0x00180E1B, /*902.75 MHz   */
                0x00180E6D, /*923.25 MHz   */
            };
        /// <summary>
        /// Brazil2 Frequency Channel number
        /// </summary>
        private const uint BR2_CHN_CNT = 33;
        private readonly uint[] br2FreqSortedIdx = new uint[]{
            9, 1, 31,
            30, 3, 32,
            18, 16, 15,
            13, 5, 4,
            28, 21, 8,
            22, 2, 6,
            7, 12, 14,
            10, 27, 17,
            11, 25, 29,
            26, 20, 19,
            23, 0, 24,
        };

        private double[] BR3TableOfFreq = new double[]
            {
                902.75, // 0
                903.25, // 1
                903.75, // 2
                904.25, // 3
                904.75, // 4
                905.25, // 5
                905.75, // 6
                906.25, // 7
                906.75, // 8
            };
        private uint[] br3FreqTable = new uint[]
            {
                0x00180E1D, /*903.25 MHz   */
                0x00180E21, /*904.25 MHz   */
                0x00180E25, /*905.25 MHz   */
                0x00180E23, /*904.75 MHz   */
                0x00180E2B, /*906.75 MHz   */
                0x00180E1F, /*903.75 MHz   */
                0x00180E27, /*905.75 MHz   */
                0x00180E29, /*906.25 MHz   */
                0x00180E1B, /*902.75 MHz   */
            };
        /// <summary>
        /// Brazil3 Frequency Channel number
        /// </summary>
        private const uint BR3_CHN_CNT = 9;
        private readonly uint[] br3FreqSortedIdx = new uint[]{
            1, 3, 5, 4, 8, 2, 6, 7, 0
        };

        private double[] BR4TableOfFreq = new double[]
            {
                902.75,
                903.25,
                903.75,
                904.25,
            };
        private uint[] br4FreqTable = new uint[]
            {
                0x00180E1D, /*903.25 MHz   */
                0x00180E21, /*904.25 MHz   */
                0x00180E1F, /*903.75 MHz   */
                0x00180E1B, /*902.75 MHz   */
            };
        /// <summary>
        /// Brazil2 Frequency Channel number
        /// </summary>
        private const uint BR4_CHN_CNT = 4;
        private readonly uint[] br4FreqSortedIdx = new uint[]{
            1, 3, 2, 0
        };

        private double[] BR5TableOfFreq = new double[]
            {
                917.75, // 0
                918.25, // 1
                918.75, // 2
                919.25, // 3
                919.75, // 4
                920.25, // 5
                920.75, // 6
                921.25, // 7
                921.75, // 8
                922.25, // 9
                922.75, // 10
                923.25, // 11
                923.75, // 12
                924.25, // 13
            };
        private uint[] br5FreqTable = new uint[]
            {
                0x00180E61, /*920.25 MHz   */
                0x00180E5D, /*919.25 MHz   */
                0x00180E5B, /*918.75 MHz   */
                0x00180E57, /*917.75 MHz   */
                0x00180E67, /*921.75 MHz   */
                0x00180E69, /*922.25 MHz   */
                0x00180E59, /*918.25 MHz   */
                0x00180E5F, /*919.75 MHz   */
                0x00180E6F, /*923.75 MHz   */
                0x00180E71, /*924.25 MHz   */
                0x00180E65, /*921.25 MHz   */
                0x00180E63, /*920.75 MHz   */
                0x00180E6B, /*922.75 MHz   */
                0x00180E6D, /*923.25 MHz   */

            };
        /// <summary>
        /// Brazil2 Frequency Channel number
        /// </summary>
        private const uint BR5_CHN_CNT = 14;
        private readonly uint[] br5FreqSortedIdx = new uint[]{
            5, 3, 2, 0, 8, 9, 1, 4, 12, 13, 7, 6, 10, 11
        };

#endregion

#region Indonesia
        /// <summary>
        /// Indonesia Frequency Table
        /// </summary>
        private readonly double[] IDTableOfFreq = new double[]
        {
            923.25,
            923.75,
            924.25,
            924.75,
        };

        /*OK*/
        private readonly uint[] indonesiaFreqTable = new uint[]
        {
            0x00180E6D, /*923.25 MHz    */
            0x00180E6F, /*923.75 MHz    */
            0x00180E71, /*924.25 MHz    */
            0x00180E73, /*924.75 MHz    */

        };
        /// <summary>
        /// Indonesia Frequency Channel number
        /// </summary>
        private const uint ID_CHN_CNT = 4;
        private readonly uint[] indonesiaFreqSortedIdx = new uint[]{
            0,
            1,
            2,
            3
        };

#region UH1
        /// <summary>
        /// FCC UH Frequency Table 915-920
        /// </summary>
        private readonly double[] UH1TableOfFreq = new double[]
            {
                915.25,     // 0
                915.75,     // 1
                916.25,     // 2
                916.75,     // 3
                917.25,     // 4
                917.75,     // 5
                918.25,     // 6
                918.75,     // 7
                919.25,     // 8
                919.75,     // 9
            };
        /*OK*/
        private uint[] uh1FreqTable = new uint[]
        {
            0x00180E4F, /*915.75 MHz   */
            0x00180E4D, /*915.25 MHz   */
            0x00180E5D, /*919.25 MHz   */
            0x00180E5B, /*918.75 MHz   */
            0x00180E57, /*917.75 MHz   */
            0x00180E55, /*917.25 MHz   */
            0x00180E59, /*918.25 MHz   */
            0x00180E51, /*916.25 MHz   */
            0x00180E5F, /*919.75 MHz   */
            0x00180E53, /*916.75 MHz   */
        };
        /// <summary>
        /// FCC UH Frequency Channel number
        /// </summary>
        private const uint UH1_CHN_CNT = 10;
        private readonly uint[] uh1FreqSortedIdx = new uint[]{
            1, 0, 8, 7, 5, 4, 6, 2, 9, 3
        };
#endregion

#region UH2
        /// <summary>
        /// FCC UH Frequency Table 920-928
        /// </summary>
        private readonly double[] UH2TableOfFreq = new double[]
            {
                920.25,   // 0
                920.75,   // 1
                921.25,   // 2
                921.75,   // 3
                922.25,   // 4
                922.75,   // 5
                923.25,   // 6
                923.75,   // 7
                924.25,   // 8
                924.75,   // 9
                925.25,   // 10
                925.75,   // 11
                926.25,   // 12
                926.75,   // 13
                927.25,   // 14
            };
        /*OK*/
        private uint[] uh2FreqTable = new uint[]
        {
            0x00180E7B, /*926.75 MHz   */
            0x00180E79, /*926.25 MHz   */
            0x00180E7D, /*927.25 MHz   */
            0x00180E61, /*920.25 MHz   */
            0x00180E75, /*925.25 MHz   */
            0x00180E67, /*921.75 MHz   */
            0x00180E69, /*922.25 MHz   */
            0x00180E73, /*924.75 MHz   */
            0x00180E6F, /*923.75 MHz   */
            0x00180E77, /*925.75 MHz   */
            0x00180E71, /*924.25 MHz   */
            0x00180E65, /*921.25 MHz   */
            0x00180E63, /*920.75 MHz   */
            0x00180E6B, /*922.75 MHz   */
            0x00180E6D, /*923.25 MHz   */
        };
        /// <summary>
        /// FCC UH Frequency Channel number
        /// </summary>
        private const uint UH2_CHN_CNT = 15;
        private readonly uint[] uh2FreqSortedIdx = new uint[]{
            13, 12, 14, 0, 10, 3, 4, 9, 7, 11, 8, 2, 1, 5, 6,
        };
#endregion

#region LH

        private double[] LHTableOfFreq = new double[]
            {
                902.75, // 0
                903.25, // 1
                903.75, // 2
                904.25, // 3
                904.75, // 4
                905.25, // 5
                905.75, // 6
                906.25, // 7
                906.75, // 8
                907.25, // 9
                907.75, // 10
                908.25, // 11
                908.75, // 12
                909.25, // 13
                909.75, // 14
                910.25, // 15
                910.75, // 16
                911.25, // 17
                911.75, // 18
                912.25, // 19
                912.75, // 20
                913.25, // 21
                913.75, // 22
                914.25, // 23
                914.75, // 24
                915.25, // 25
                
                /*915.75,
                916.25,
                916.75,
                917.25,
                917.75,
                918.25,
                918.75,
                919.25,
                919.75,
                920.25,
                920.75,
                921.25,
                921.75,
                922.25,
                922.75,
                923.25,
                923.75,
                924.25,
                924.75,
                925.25,
                925.75,
                926.25,
                926.75,
                927.25,*/
            };
        private uint[] lhFreqTable = new uint[]
        {
            0x00180E1B, /*902.75 MHz   */
            0x00180E35, /*909.25 MHz   */
            0x00180E1D, /*903.25 MHz   */
            0x00180E37, /*909.75 MHz   */
            0x00180E1F, /*903.75 MHz   */
            0x00180E39, /*910.25 MHz   */
            0x00180E21, /*904.25 MHz   */
            0x00180E3B, /*910.75 MHz   */
            0x00180E23, /*904.75 MHz   */
            0x00180E3D, /*911.25 MHz   */
            0x00180E25, /*905.25 MHz   */
            0x00180E3F, /*911.75 MHz   */
            0x00180E27, /*905.75 MHz   */
            0x00180E41, /*912.25 MHz   */
            0x00180E29, /*906.25 MHz   */
            0x00180E43, /*912.75 MHz   */
            0x00180E2B, /*906.75 MHz   */
            0x00180E45, /*913.25 MHz   */
            0x00180E2D, /*907.25 MHz   */
            0x00180E47, /*913.75 MHz   */
            0x00180E2F, /*907.75 MHz   */
            0x00180E49, /*914.25 MHz   */
            0x00180E31, /*908.25 MHz   */
            0x00180E4B, /*914.75 MHz   */
            0x00180E33, /*908.75 MHz   */
            0x00180E4D, /*915.25 MHz   */


            //0x00180E4F, /*915.75 MHz   */
            //0x00180E7B, /*926.75 MHz   */
            //0x00180E79, /*926.25 MHz   */
            //0x00180E7D, /*927.25 MHz   */
            //0x00180E61, /*920.25 MHz   */
            //0x00180E5D, /*919.25 MHz   */
            //0x00180E5B, /*918.75 MHz   */
            //0x00180E57, /*917.75 MHz   */
            //0x00180E75, /*925.25 MHz   */
            //0x00180E67, /*921.75 MHz   */
            //0x00180E69, /*922.25 MHz   */
            //0x00180E55, /*917.25 MHz   */
            //0x00180E59, /*918.25 MHz   */
            //0x00180E51, /*916.25 MHz   */
            //0x00180E73, /*924.75 MHz   */
            //0x00180E5F, /*919.75 MHz   */
            //0x00180E53, /*916.75 MHz   */
            //0x00180E6F, /*923.75 MHz   */
            //0x00180E77, /*925.75 MHz   */
            //0x00180E71, /*924.25 MHz   */
            //0x00180E65, /*921.25 MHz   */
            //0x00180E63, /*920.75 MHz   */
            //0x00180E6B, /*922.75 MHz   */
            //0x00180E6D, /*923.25 MHz   */
        };
        /// <summary>
        /// Brazil1 Frequency Channel number
        /// </summary>
        private const uint LH_CHN_CNT = 26;
        private readonly uint[] lhFreqSortedIdx = new uint[]{
        0, 13, 1, 14, 2, 15, 3, 16, 4, 17, 5, 18, 6, 19, 7, 20, 8, 21, 9, 22, 10, 23, 11, 24, 12, 25 
            /*
 * 0, 22, 21, 23,
            9, 7, 6, 4,
            19, 12, 13, 3,
            5, 1, 18, 8,
            2, 16, 20, 17,
            11, 10, 14, 15,
*/
        };


        private double[] LH1TableOfFreq = new double[]
            {
                902.75, // 0
                903.25, // 1
                903.75, // 2
                904.25, // 3
                904.75, // 4
                905.25, // 5
                905.75, // 6
                906.25, // 7
                906.75, // 8
                907.25, // 9
                907.75, // 10
                908.25, // 11
                908.75, // 12
                909.25, // 13
            };
        private uint[] lh1FreqTable = new uint[]
        {
            0x00180E1B, /*902.75 MHz   */
            0x00180E35, /*909.25 MHz   */
            0x00180E1D, /*903.25 MHz   */
            0x00180E1F, /*903.75 MHz   */
            0x00180E21, /*904.25 MHz   */
            0x00180E23, /*904.75 MHz   */
            0x00180E25, /*905.25 MHz   */
            0x00180E27, /*905.75 MHz   */
            0x00180E29, /*906.25 MHz   */
            0x00180E2B, /*906.75 MHz   */
            0x00180E2D, /*907.25 MHz   */
            0x00180E2F, /*907.75 MHz   */
            0x00180E31, /*908.25 MHz   */
            0x00180E33, /*908.75 MHz   */
        };
        /// <summary>
        /// Brazil1 Frequency Channel number
        /// </summary>
        private const uint LH1_CHN_CNT = 14;
        private readonly uint[] lh1FreqSortedIdx = new uint[]{
        0, 13, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 
        };


        private double[] LH2TableOfFreq = new double[]
            {
                909.75, // 0
                910.25, // 1
                910.75, // 2
                911.25, // 3
                911.75, // 4
                912.25, // 5
                912.75, // 6
                913.25, // 7
                913.75, // 8
                914.25, // 9
                914.75, // 10
            };

        private uint[] lh2FreqTable = new uint[]
        {
            0x00180E37, /*909.75 MHz   */
            0x00180E39, /*910.25 MHz   */
            0x00180E3B, /*910.75 MHz   */
            0x00180E3D, /*911.25 MHz   */
            0x00180E3F, /*911.75 MHz   */
            0x00180E41, /*912.25 MHz   */
            0x00180E43, /*912.75 MHz   */
            0x00180E45, /*913.25 MHz   */
            0x00180E47, /*913.75 MHz   */
            0x00180E49, /*914.25 MHz   */
            0x00180E4B, /*914.75 MHz   */
        };
        /// <summary>
        /// Brazil1 Frequency Channel number
        /// </summary>
        private const uint LH2_CHN_CNT = 11;
        private readonly uint[] lh2FreqSortedIdx = new uint[]{
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10
        };



#endregion

#endregion

#region JE

        private double[] JETableOfFreq = new double[]
        {
                915.25, // 0
                915.5,  // 1
                915.75, // 2
                916.0,  // 3
                916.25, // 4
                916.5,  // 5
                916.75, // 6
            };
        private uint[] jeFreqTable = new uint[]
        {
            0x00180E4D, /*915.25 MHz   */
            0x00180E51, /*916.25 MHz   */
            0x00180E4E, /*915.5 MHz   */
            0x00180E52, /*916.5 MHz   */
            0x00180E4F, /*915.75 MHz   */
            0x00180E53, /*916.75 MHz   */
            0x00180E50, /*916.0 MHz   */
        };
        /// <summary>
        /// Brazil1 Frequency Channel number
        /// </summary>
        private const uint JE_CHN_CNT = 7;
        private readonly uint[] jeFreqSortedIdx = new uint[]{
        0, 4, 1, 5, 2, 6, 3
        };

#endregion

#region BackoffTable
        private UInt32[] etsiBackoffTable = new uint[]
        {
            0x0000175a, /*       5978 usecs */
            0x000016d5, /*       5845 usecs */
            0x0000225a, /*       8794 usecs */
            0x0000219f, /*       8607 usecs */
            0x00001fdd, /*       8157 usecs */
            0x00001cb4, /*       7348 usecs */
            0x000026c9, /*       9929 usecs */
            0x000026c6, /*       9926 usecs */
            0x00001e66, /*       7782 usecs */
            0x0000140d, /*       5133 usecs */
            0x00001ead  /*       7853 usecs */
        };

        private UInt32[] japanBackoffTable = new uint[]
        {
            0x00001388, /*       5978 usecs */
            0x00001388, /*       5845 usecs */
            0x00001388, /*       8794 usecs */
            0x00001388, /*       8607 usecs */
            0x00001388, /*       8157 usecs */
            0x00001388, /*       7348 usecs */
            0x00001388, /*       9929 usecs */
            0x00001388, /*       9926 usecs */
            0x00001388, /*       7782 usecs */
            0x00001388, /*       5133 usecs */
            0x00001388  /*       7853 usecs */
        };
#endregion

#region PH

        private double[] PHTableOfFreq = new double[]
            {
                918.125, // 0
                918.375, // 1
                918.625, // 2
                918.875, // 3
                919.125, // 5
                919.375, // 6
                919.625, // 7
                919.875, // 8
            };
        private uint[] phFreqTable = new uint[]
        {
            0x00301CB1, /*918.125MHz   Channel 0*/
            0x00301CBB, /*919.375MHz   Channel 5*/
            0x00301CB7, /*918.875MHz   Channel 3*/
            0x00301CBF, /*919.875MHz   Channel 7*/
            0x00301CB3, /*918.375MHz   Channel 1*/
            0x00301CBD, /*919.625MHz   Channel 6*/
            0x00301CB5, /*918.625MHz   Channel 2*/
            0x00301CB9, /*919.125MHz   Channel 4*/
        };
        /// <summary>
        /// Brazil1 Frequency Channel number
        /// </summary>
        private const uint PH_CHN_CNT = 8;
        private readonly uint[] phFreqSortedIdx = new uint[]{
            0, 5, 3, 7, 1, 6, 2, 4
        };

#region ETSIUPPERBAND

        private double[] ETSIUPPERBANDTableOfFreq = new double[]
        {
            916.3,
            917.5,
            918.7,
            //919.9,
        };
        private uint[] etsiupperbandFreqTable = new uint[]
        {
            0x003C23CB, /*916.3 MHz   */
            0x003C23D7, /*917.5 MHz   */
            0x003C23E3, /*918.7 MHz   */
            0x003C23E3, /*918.7 MHz   */
            //0x003C23EF, /*919.9 MHz   */
        };
        /// <summary>
        /// Brazil1 Frequency Channel number
        /// </summary>
        private const uint ETSIUPPERBAND_CHN_CNT = 3;
        private readonly uint[] etsiupperbandFreqSortedIdx = new uint[]{
            0, 1, 2//, 3
        };

#endregion

#region NZ

        private double[] NZTableOfFreq = new double[]
        {
            920.25,
            920.75,
            921.25,
            921.75,
            922.25,
            922.75,
            923.25,
            923.75,
            924.25,
            924.75,
            925.25,
            925.75,
            926.25,
            926.75,
            927.25,
            927.75
        };
        private uint[] nzFreqTable = new uint[]
        {
    0x00180E65, /*921.25 MHz   2*/
    0x00180E73, /*924.75MHz   9*/
    0x00180E69, /*922.25MHz   4*/
    0x00180E79, /*926.25MHz   12*/
    0x00180E6B, /*922.75MHz   5*/
    0x00180E6F, /*923.75MHz   7*/
    0x00180E67, /*921.75 MHz   3*/
    0x00180E7B, /*926.75MHz   13*/
    0x00180E77, /*925.75MHz   11*/
    0x00180E71, /*924.25MHz   8*/
    0x00180E7D, /*927.25MHz   14*/
    0x00180E61, /*920.25 MHz   0*/
    0x00180E7F, /*927.75MHz   15*/
    0x00180E75, /*925.25MHz   10*/
    0x00180E63, /*920.75 MHz   1*/
    0x00180E6D, /*923.25MHz   6*/
        };
        /// <summary>
        /// Brazil1 Frequency Channel number
        /// </summary>
        private const uint NZ_CHN_CNT = 16;
        private readonly uint[] nzFreqSortedIdx = new uint[]{
            2, 9, 4, 12, 5, 7, 3, 13, 11, 8, 14, 0, 15, 10, 1, 6
        };

#endregion


#region VE

        private readonly double[] VETableOfFreq = new double[]
        {
            922.75,// 0
            923.25,
            923.75,
            924.25,
            924.75,
            925.25,// 5
            925.75,
            926.25,
            926.75,
            927.25,// 9
        };

        private uint[] veFreqTable = new uint[]
        {
            0x00180E77, /*925.75 MHz  6 */
            0x00180E6B, /*922.75 MHz  0 */
            0x00180E7D, /*927.25 MHz  9 */
            0x00180E75, /*925.25 MHz  5 */
            0x00180E6D, /*923.25 MHz  1 */
            0x00180E7B, /*926.75 MHz  8 */
            0x00180E73, /*924.75 MHz  4 */
            0x00180E6F, /*923.75 MHz  2 */
            0x00180E79, /*926.25 MHz  7 */
            0x00180E71, /*924.25 MHz  3 */
};
        /// <summary>
        /// FCC Frequency Channel number
        /// </summary>
        private const uint VE_CHN_CNT = 10;
        private readonly uint[] veFreqSortedIdx = new uint[]{
            6, 0, 9, 5, 1,
            8, 4, 2, 7, 3
        };

#endregion

#region BA

        private readonly double[] BATableOfFreq = new double[]
        {
            925.25,
            925.75,
            926.25,
            926.75
        };

        private uint[] baFreqTable = new uint[]
        {
            0x00180E75, /*925.25 MHz  0 */
            0x00180E77, /*925.75 MHz  1 */
            0x00180E79, /*926.25 MHz  2 */
            0x00180E7B, /*926.75 MHz  3 */
        };

        /// <summary>
        /// BA Frequency Channel number
        /// </summary>
        private const uint BA_CHN_CNT = 4;
        private readonly uint[] baFreqSortedIdx = new uint[]{
            0, 1, 2, 3
        };

#endregion
        
#endregion

#endregion


        /// <summary>
        /// Set Frequency Band - Basic Function
        /// </summary>
        /// <param name="m_radioIndex"></param>
        /// <param name="frequencySelector"></param>
        /// <param name="config"></param>
        /// <param name="multdiv"></param>
        /// <param name="pllcc"></param>
        /// <returns></returns>
        private Result SetFrequencyBand (UInt32 frequencySelector, BandState config, UInt32 multdiv, UInt32 pllcc)
        {
            MacWriteRegister(MACREGISTER.HST_RFTC_FRQCH_SEL /*SELECTOR_ADDRESS*/, frequencySelector);

            MacWriteRegister(MACREGISTER.HST_RFTC_FRQCH_CFG /*CONFIG_ADDRESS*/, (uint)config);

            if (config == BandState.ENABLE)
            {
                MacWriteRegister(MACREGISTER.HST_RFTC_FRQCH_DESC_PLLDIVMULT /*MULTDIV_ADDRESS*/, multdiv);

                MacWriteRegister(MACREGISTER.HST_RFTC_FRQCH_DESC_PLLDACCTL /*PLLCC_ADDRESS*/, pllcc);
            }

            return Result.OK;
        }

        private uint FreqChnCnt(RegionCode prof)
        {
            switch (prof)
            {
                case RegionCode.AR:
                case RegionCode.CL:
                case RegionCode.CO:
                case RegionCode.CR:
                case RegionCode.DO:
                case RegionCode.PA:
                case RegionCode.UY:
                case RegionCode.FCC:
                    return FCC_CHN_CNT;
                case RegionCode.CN:
                    return CN_CHN_CNT;
                case RegionCode.TW:
                    return TW_CHN_CNT;
                case RegionCode.KR:
                    return KR_CHN_CNT;
                case RegionCode.HK:
                    return OFCA_CHN_CNT;
                case RegionCode.SG:
                case RegionCode.TH:
                case RegionCode.VI:
                    return HK_CHN_CNT;
                case RegionCode.AU:
                    return AUS_CHN_CNT;
                case RegionCode.MY:
                    return MYS_CHN_CNT;
                case RegionCode.G800:
                case RegionCode.ETSI:
                    return ETSI_CHN_CNT;
                case RegionCode.IN:
                    return IDA_CHN_CNT;
                case RegionCode.JP:
                    if (m_oem_special_country_version == 0x2A4A5036)
                        return JPN2012_CHN_CNT;
                    else
                        return JPN2019_CHN_CNT;
                case RegionCode.ZA:
                    return ZA_CHN_CNT;
                case RegionCode.BR1:
                    return BR1_CHN_CNT;
                case RegionCode.PE:
                case RegionCode.BR2:
                    return BR2_CHN_CNT;
                case RegionCode.BR3:
                    return BR3_CHN_CNT;
                case RegionCode.BR4:
                    return BR4_CHN_CNT;
                case RegionCode.BR5:
                    return BR5_CHN_CNT;
                case RegionCode.ID:
                    return ID_CHN_CNT;
                case RegionCode.JE:
                    return JE_CHN_CNT;
                case RegionCode.PH:
                    return PH_CHN_CNT;
                case RegionCode.ETSIUPPERBAND:
                    return ETSIUPPERBAND_CHN_CNT;
                case RegionCode.NZ:
                    return NZ_CHN_CNT;
                case RegionCode.UH1:
                    return UH1_CHN_CNT;
                case RegionCode.UH2:
                    return UH2_CHN_CNT;
                case RegionCode.LH:
                    return LH_CHN_CNT;
                case RegionCode.LH1:
                    return LH1_CHN_CNT;
                case RegionCode.LH2:
                    return LH2_CHN_CNT;
                case RegionCode.VE:
                    return VE_CHN_CNT;
                case RegionCode.BA:
                    return BA_CHN_CNT;
                default:
                    return 0;
                    //break;
            }
            //return 0;
        }

        private uint[] FreqTable(RegionCode prof)
        {
            switch (prof)
            {
                case RegionCode.AR:
                case RegionCode.CL:
                case RegionCode.CO:
                case RegionCode.CR:
                case RegionCode.DO:
                case RegionCode.PA:
                case RegionCode.UY:
                case RegionCode.FCC:
                    switch (m_oem_table_version)
                    {
                        default:
                            return fccFreqTable;

                        case 0x20170001:
                            return fccFreqTable_Ver20170001;
                    }
                case RegionCode.CN:
                    return cnFreqTable;
                    return cn12FreqTable;
                case RegionCode.TW:
                    return twFreqTable;
                case RegionCode.KR:
                    return krFreqTable;
                case RegionCode.HK:
                    return ofcaFreqTable;
                case RegionCode.SG:
                case RegionCode.TH:
                case RegionCode.VI:
                    return hkFreqTable;
                case RegionCode.AU:
                    return AusFreqTable;
                case RegionCode.MY:
                    return mysFreqTable;
                case RegionCode.G800:
                case RegionCode.ETSI:
                    return etsiFreqTable;
                case RegionCode.IN:
                    return indiaFreqTable;
                case RegionCode.JP:
                    if (m_oem_special_country_version == 0x2A4A5036)
                        return jpn2012FreqTable;
                    else
                        return jpn2019FreqTable;
                case RegionCode.ZA:
                    return zaFreqTable;
                case RegionCode.BR1:
                    return br1FreqTable;
                case RegionCode.PE:
                case RegionCode.BR2:
                    return br2FreqTable;
                case RegionCode.BR3:
                    return br3FreqTable;
                case RegionCode.BR4:
                    return br4FreqTable;
                case RegionCode.BR5:
                    return br5FreqTable;
                case RegionCode.ID:
                    return indonesiaFreqTable;
                case RegionCode.JE:
                    return jeFreqTable;
                case RegionCode.PH:
                    return phFreqTable;
                case RegionCode.ETSIUPPERBAND:
                    return etsiupperbandFreqTable;
                case RegionCode.NZ:
                    return nzFreqTable;
                case RegionCode.UH1:
                    return uh1FreqTable;
                case RegionCode.UH2:
                    return uh2FreqTable;
                case RegionCode.LH:
                    return lhFreqTable;
                case RegionCode.LH1:
                    return lh1FreqTable;
                case RegionCode.LH2:
                    return lh2FreqTable;
                case RegionCode.VE:
                    return veFreqTable;
                case RegionCode.BA:
                    return baFreqTable;
                default:
                    return null;
                    //break;
            }
            //return null;
        }

        private bool FreqChnWithinRange(uint Channel, RegionCode region)
        {
            uint ChnCnt = FreqChnCnt(region);
            if (ChnCnt < 0)
                return false;
            if (Channel >= 0 && Channel < ChnCnt)
            {
                return true;
            }
            return false;
        }

        private int FreqSortedIdxTbls(RegionCode Prof, uint Channel)
        {
            uint TotalCnt = FreqChnCnt(Prof);
            uint[] freqIndex = FreqIndex(Prof);
            if (!FreqChnWithinRange(Channel, Prof) || freqIndex == null)
                return -1;
            for (int i = 0; i < TotalCnt; i++)
            {
                if (freqIndex[i] == Channel)
                {
                    return i;
                }
            }
            return -1;
        }

        private uint GetPllcc(RegionCode prof)
        {
            switch (prof)
            {
                case RegionCode.G800:
                case RegionCode.ETSI:
                case RegionCode.IN:
                    return 0x14070400;
            }

            return 0x14070200;
            //return pllvalue;
        }

        private uint[] FreqIndex(RegionCode prof)
        {
            switch (prof)
            {
                case RegionCode.AR:
                case RegionCode.CL:
                case RegionCode.CO:
                case RegionCode.CR:
                case RegionCode.DO:
                case RegionCode.PA:
                case RegionCode.UY:
                case RegionCode.FCC:
                    switch (m_oem_table_version)
                    {
                        default:
                            return fccFreqSortedIdx;

                        case 0x20170001:
                            return fccFreqSortedIdx_Ver20170001;
                    }
                case RegionCode.CN:
                    return cnFreqSortedIdx;
                case RegionCode.TW:
                    return twFreqSortedIdx;
                case RegionCode.KR:
                    return krFreqSortedIdx;
                case RegionCode.HK:
                    return ofcaFreqSortedIdx;
                case RegionCode.SG:
                case RegionCode.TH:
                case RegionCode.VI:
                    return hkFreqSortedIdx;
                case RegionCode.AU:
                    return ausFreqSortedIdx;
                case RegionCode.MY:
                    return mysFreqSortedIdx;
                case RegionCode.G800:
                case RegionCode.ETSI:
                    return etsiFreqSortedIdx;
                case RegionCode.IN:
                    return indiaFreqSortedIdx;
                case RegionCode.JP:
                    if (m_oem_special_country_version == 0x2A4A5036)
                        return jpn2012FreqSortedIdx;
                    else
                        return jpn2019FreqSortedIdx;
                case RegionCode.ZA:
                    return zaFreqSortedIdx;
                case RegionCode.BR1:
                    return br1FreqSortedIdx;
                case RegionCode.PE:
                case RegionCode.BR2:
                    return br2FreqSortedIdx;
                case RegionCode.BR3:
                    return br3FreqSortedIdx;
                case RegionCode.BR4:
                    return br4FreqSortedIdx;
                case RegionCode.BR5:
                    return br5FreqSortedIdx;
                case RegionCode.ID:
                    return indonesiaFreqSortedIdx;
                case RegionCode.JE:
                    return jeFreqSortedIdx;
                case RegionCode.PH:
                    return phFreqSortedIdx;
                case RegionCode.ETSIUPPERBAND:
                    return etsiupperbandFreqSortedIdx;
                case RegionCode.NZ:
                    return nzFreqSortedIdx;
                case RegionCode.UH1:
                    return uh1FreqSortedIdx;
                case RegionCode.UH2:
                    return uh2FreqSortedIdx;
                case RegionCode.LH:
                    return lhFreqSortedIdx;
                case RegionCode.LH1:
                    return lh1FreqSortedIdx;
                case RegionCode.LH2:
                    return lh2FreqSortedIdx;
                case RegionCode.VE:
                    return veFreqSortedIdx;
                case RegionCode.BA:
                    return baFreqSortedIdx;
            }

            return null;
        }

        private Result SetRadioLBT(LBT enable)
        {
            //ushort Reg = 0x0301; // HST_PROTSCH_SMCFG
            uint Val = 0;
            MacReadRegister(MACREGISTER.HST_PROTSCH_SMCFG /*Reg*/, ref Val);

            if (enable == LBT.ON) /* Bit 0 */
                Val |= 0x00000001;
            else
                Val &= 0xFFFFFFFE;

            MacWriteRegister(MACREGISTER.HST_PROTSCH_SMCFG /*Reg*/, Val);

            //m_save_enable_lbt = enable;
            return Result.OK;
        }

        /// <summary>
        /// Get frequency table on specific region
        /// </summary>
        /// <param name="region">Region Code</param>
        /// <returns></returns>
        public double[] GetAvailableFrequencyTable(FrequencyBand.RegionCode region)
        {
            switch (region)
            {
                case RegionCode.AU:
                    return AUSTableOfFreq;
                case RegionCode.CN:
                    return CHNTableOfFreq;
                case RegionCode.ETSI:
                case RegionCode.G800:
                    return ETSITableOfFreq;
                case RegionCode.IN:
                    return IDATableOfFreq;
                case RegionCode.AR:
                case RegionCode.CL:
                case RegionCode.CO:
                case RegionCode.CR:
                case RegionCode.DO:
                case RegionCode.PA:
                case RegionCode.UY:
                case RegionCode.FCC:
                    return FCCTableOfFreq;
                case RegionCode.HK:
                    return OFCATableOfFreq;
                case RegionCode.SG:
                case RegionCode.TH:
                case RegionCode.VI:
                    return HKTableOfFreq;
                case RegionCode.JP:
                    if (m_oem_special_country_version == 0x2A4A5036)
                        return JPN2012TableOfFreq;
                    else
                        return JPN2019TableOfFreq;
                case RegionCode.KR:
                    return KRTableOfFreq;
                case RegionCode.MY:
                    return MYSTableOfFreq;
                case RegionCode.TW:
                    return TWTableOfFreq;
                case RegionCode.ZA:
                    return ZATableOfFreq;
                case RegionCode.BR1:
                    return BR1TableOfFreq;
                case RegionCode.PE:
                case RegionCode.BR2:
                    return BR2TableOfFreq;
                case RegionCode.BR3:
                    return BR3TableOfFreq;
                case RegionCode.BR4:
                    return BR4TableOfFreq;
                case RegionCode.BR5:
                    return BR5TableOfFreq;
                case RegionCode.ID:
                    return IDTableOfFreq;
                case RegionCode.JE:
                    return JETableOfFreq;
                case RegionCode.PH:
                    return PHTableOfFreq;
                case RegionCode.ETSIUPPERBAND:
                    return ETSIUPPERBANDTableOfFreq;
                case RegionCode.NZ:
                    return NZTableOfFreq;
                case RegionCode.UH1:
                    return UH1TableOfFreq;
                case RegionCode.UH2:
                    return UH2TableOfFreq;
                case RegionCode.LH:
                    return LHTableOfFreq;
                case RegionCode.LH1:
                    return LH1TableOfFreq;
                case RegionCode.LH2:
                    return LH2TableOfFreq;
                case RegionCode.VE:
                    return VETableOfFreq;
                case RegionCode.BA:
                    return BATableOfFreq;
                default:
                    return new double[0];
            }
        }


        private static uint[] BasicDIVRAT = new uint[] { 0x0010, 0x0018, 0x0030, 0x003c, 0x0048 };


        static public UInt32 FrequenceToPll(double freq)
        {
            UInt32 pll;

            foreach (uint divrat in BasicDIVRAT)
            {
                pll = FrequenceToPll(freq, divrat);

                if (pll != 0)
                    return pll;
            }

            throw new InvalidOperationException("can not cal pll value, please check BasicDIVRAT");
        }

        static public UInt32 FrequenceToPll(double freq, uint divrat)
        {
            if (divrat == 0)
                return FrequenceToPll(freq);

            double fd = freq * divrat;

            if ((fd % 6) == 0)
            {
                UInt32 pllHight = divrat << 16;
                UInt32 pllLow = (UInt32)(fd / 6);
                return (pllHight | pllLow);
            }

            return 0;
        }

        static public UInt32[] GetPllValues(double[] FrequencyTable)
        {
            UInt32[] pllTable = new UInt32[FrequencyTable.Length];
            UInt32 divrat = 0;

            try
            {
                pllTable[0] = FrequenceToPll(FrequencyTable[0]);
                divrat = pllTable[0] >> 16;

                for (int i = 1; i < FrequencyTable.Length; i++)
                    pllTable[i] = FrequenceToPll(FrequencyTable[i], divrat);

                return pllTable;
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("ERROR : can not get pll table");
            }
            return null;
        }

    }
}

#if new
{
    public class FREQUENCYDATA
    {
        int channel;
        double frequency;
        UInt16 pplvalue;
    }

    public class FREQUENCYSET
    {
        public string country;
        public FREQUENCYDATA [] frequencyData;
    }

    // Israeli
    FREQUENCYSET israeliFrequencySet {
        country = "Israeli";
        frequencyData = new FREQUENCYDATA()[]{ { 0, 915.5, 0x00180E4E },
                                               { 1, 915.7, 0x003C23C5 },
                                               { 2, 915.9, 0x003C23C7 },
                                               { 3, 916.1, 0x003C23C9 },
                                               { 4, 916.3, 0x003C23CB },
                                               { 5, 916.5, 0x003C23CD } }

}
#endif
