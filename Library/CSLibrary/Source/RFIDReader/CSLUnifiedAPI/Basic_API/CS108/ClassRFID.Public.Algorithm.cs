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
    using Constants;
    using Structures;

    public partial class RFIDReader
    {
        /// <summary>
        /// Allows the application to set the currently-active singulation 
        /// algorithm (i.e., the one that is used when performing a tag-
        /// protocol operation (e.g., inventory, tag read, etc.)).  The 
        /// currently-active singulation algorithm may not be changed while a 
        /// radio module is executing a tag-protocol operation. 
        /// </summary>
        /// <param name="SingulationAlgorithm">
        /// The singulation algorithm that is to be used for 
        /// subsequent tag-access operations.  If this 
        /// parameter does not represent a valid 
        /// singulation algorithm, 
        /// RFID_ERROR_INVALID_PARAMETER is returned. </param>
        public Result SetCurrentSingulationAlgorithm_CS108(SingulationAlgorithm SingulationAlgorithm)
        {
            UInt32 value = 0;

            if (SingulationAlgorithm == SingulationAlgorithm.UNKNOWN)
                return Result.INVALID_PARAMETER;

            MacReadRegister(MACREGISTER.HST_INV_CFG, ref value);

            value &= ~0x3fU;
            value |= (UInt32)SingulationAlgorithm;

            MacWriteRegister(MACREGISTER.HST_INV_CFG, value);

            return Result.OK;
        }

        /// <summary>
        /// Get Current Singulation Algorithm
        /// </summary>
        /// <param name="SingulationAlgorithm"></param>
        /// <returns></returns>
        public Result GetCurrentSingulationAlgorithm_CS108(ref SingulationAlgorithm SingulationAlgorithm)
        {
            UInt32 value = 0;

            MacReadRegister(MACREGISTER.HST_INV_CFG, ref value);
            value &= 0x3fU;
            SingulationAlgorithm = (SingulationAlgorithm)value;
            return Result.OK;
        }

        /// <summary>
        /// SetSingulationAlgorithmParms
        /// </summary>
        /// <param name="alg"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public Result SetSingulationAlgorithmParms_CS108(SingulationAlgorithm alg, SingulationAlgorithmParms parms)
        {
            const uint RFID_18K6C_SINGULATION_ALGORITHM_FIXEDQ = 0;
            const uint RFID_18K6C_SINGULATION_ALGORITHM_DYNAMICQ = 3;

            if (alg == SingulationAlgorithm.UNKNOWN)
                return Result.INVALID_PARAMETER;

            try
            {
                switch (alg)
                {
                    case SingulationAlgorithm.FIXEDQ:
                        {
                            FixedQParms p = (FixedQParms)parms;
                            // Write the inventory algorithm parameter registers
                            MacWriteRegister(MACREGISTER.HST_INV_SEL, RFID_18K6C_SINGULATION_ALGORITHM_FIXEDQ);
                            MacWriteRegister(MACREGISTER.HST_INV_ALG_PARM_0, p.qValue);
                            MacWriteRegister(MACREGISTER.HST_INV_ALG_PARM_1, p.retryCount);
                            MacWriteRegister(MACREGISTER.HST_INV_ALG_PARM_2,
                                (uint)(p.toggleTarget != 0 ? 1 : 0) |
                                (uint)(p.repeatUntilNoTags != 0 ? 2 : 0));
                            MacWriteRegister(MACREGISTER.HST_INV_ALG_PARM_3, 0);
                        }
                        break;

                    case SingulationAlgorithm.DYNAMICQ:
                        {
                            DynamicQParms p = (DynamicQParms)parms;
                            // Write the inventory algorithm parameter registers.  For register
                            // zero, remember to preserve values that we aren't exposing
                            MacWriteRegister(MACREGISTER.HST_INV_SEL, RFID_18K6C_SINGULATION_ALGORITHM_DYNAMICQ);
                            MacWriteRegister(MACREGISTER.HST_INV_ALG_PARM_0, p.startQValue | (p.maxQValue << 4) | (p.minQValue << 8) | (p.thresholdMultiplier << 12));
                            MacWriteRegister(MACREGISTER.HST_INV_ALG_PARM_1, p.retryCount);
                            MacWriteRegister(MACREGISTER.HST_INV_ALG_PARM_2, (uint)(p.toggleTarget != 0 ? 1 : 0));
                            MacWriteRegister(MACREGISTER.HST_INV_ALG_PARM_3, 0);
                        }
                        break;

                    default:
                        return Result.INVALID_PARAMETER;
                } // switch (algorithm)
            }
            catch (Exception ex)
            {

            }

            return (m_Result = SetCurrentSingulationAlgorithm_CS108(alg));
        }

        /// <summary>
        /// GetSingulationAlgorithmParms
        /// </summary>
        /// <param name="alg"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public Result GetSingulationAlgorithmParms_CS108(SingulationAlgorithm alg, SingulationAlgorithmParms parms)
        {
            const int RFID_18K6C_SINGULATION_ALGORITHM_FIXEDQ = 0;
            const int RFID_18K6C_SINGULATION_ALGORITHM_DYNAMICQ = 3;
            UInt32 parm0Register = 0;
            UInt32 parm1Register = 0;
            UInt32 parm2Register = 0;

            switch (alg)
            {
                case SingulationAlgorithm.FIXEDQ:
                    {
                        FixedQParms m_fixedQ = (FixedQParms)parms;

                        // Tell the MAC which singulation algorithm selector to use and then
                        // read the singulation algorithm registers
                        MacWriteRegister(MACREGISTER.HST_INV_SEL, RFID_18K6C_SINGULATION_ALGORITHM_FIXEDQ);
                        MacReadRegister(MACREGISTER.HST_INV_ALG_PARM_0, ref parm0Register);
                        MacReadRegister(MACREGISTER.HST_INV_ALG_PARM_1, ref parm1Register);
                        MacReadRegister(MACREGISTER.HST_INV_ALG_PARM_2, ref parm2Register);

                        // Set up the fixed Q singulation algorithm structure
                        //m_fixedQ.length = sizeof(FixedQParms);
                        m_fixedQ.qValue = parm0Register & 0x0f;
                        m_fixedQ.retryCount = parm1Register & 0xff;
                        m_fixedQ.toggleTarget = (parm2Register & 0x01) != 0 ? (uint)1 : (uint)0;
                        m_fixedQ.repeatUntilNoTags = (parm2Register & 0x02) != 0 ? (uint)1 : (uint)0;
                    }
                    break;

                case SingulationAlgorithm.DYNAMICQ:
                    {
                        DynamicQParms m_dynQ = (DynamicQParms)parms;

                        // Tell the MAC which singulation algorithm selector to use and then
                        // read the singulation algorithm registers
                        MacWriteRegister(MACREGISTER.HST_INV_SEL, RFID_18K6C_SINGULATION_ALGORITHM_DYNAMICQ);

                        MacReadRegister(MACREGISTER.HST_INV_ALG_PARM_0, ref parm0Register);
                        MacReadRegister(MACREGISTER.HST_INV_ALG_PARM_1, ref parm1Register);
                        MacReadRegister(MACREGISTER.HST_INV_ALG_PARM_2, ref parm2Register);

                        // Extract the dynamic-Q with Q-adjustment threshold singulation algorithm
                        // parameters
                        //m_dynQ.length = sizeof(DynamicQParms);
                        m_dynQ.startQValue = parm0Register & 0x0f;
                        m_dynQ.minQValue = (parm0Register >> 8) & 0x0f;
                        m_dynQ.maxQValue = (parm0Register >> 4) & 0x0f;
                        m_dynQ.thresholdMultiplier = (parm0Register >> 12) & 0x3f;
                        m_dynQ.retryCount = parm1Register;
                        m_dynQ.toggleTarget = (parm2Register & 0x01) != 0 ? (uint)1 : (uint)0;
                    }
                    break;

                default:
                    return Result.INVALID_PARAMETER;
            }

            return Result.OK;
        }
        /// <summary>
        /// Get FixedQ Singulation Algorithm
        /// </summary>
        /// <param name="fixedQ"></param>
        /// <returns></returns>
        public Result GetFixedQParms_CS108(FixedQParms fixedQ)
        {
            return (m_Result = GetSingulationAlgorithmParms_CS108(SingulationAlgorithm.FIXEDQ, fixedQ));
        }
        /// <summary>
        /// The  parameters  for  the  fixed-Q  algorithm,  MAC  singulation  algorithm  0
        /// If running a same operation, it only need to config once times
        /// </summary>
        /// <param name="QValue">The Q value to use.  Valid values are 0-15, inclusive.</param>
        /// <param name="RetryCount">Specifies the number of times to try another execution 
        /// of the singulation algorithm for the specified 
        /// session/target before either toggling the target (if 
        /// toggleTarget is non-zero) or terminating the 
        /// inventory/tag access operation.  Valid values are 0-
        /// 255, inclusive. Valid values are 0-255, inclusive.</param>
        /// <param name="ToggleTarget"> A non-zero value indicates that the target should
        /// be toggled.A zero value indicates that the target should not be toggled.
        /// Note that if the target is toggled, retryCount and repeatUntilNoTags will also apply
        /// to the new target. </param>
        /// <param name="RepeatUnitNoTags">A flag that indicates whether or not the singulation 
        /// algorithm should continue performing inventory rounds 
        /// until no tags are singulated.  A non-zero value indicates 
        /// that, for each execution of the singulation algorithm, 
        /// inventory rounds should be performed until no tags are 
        /// singulated.  A zero value indicates that a single 
        /// inventory round should be performed for each 
        /// execution of the singulation algorithm.</param>
        public Result SetFixedQParms_CS108(uint QValue, uint RetryCount, uint ToggleTarget, uint RepeatUnitNoTags)
        {
            FixedQParms FixedQParm = new FixedQParms();
            FixedQParm.qValue = QValue;      //if only 1 tag read and write, otherwise use 7
            FixedQParm.retryCount = RetryCount;
            FixedQParm.toggleTarget = ToggleTarget;
            FixedQParm.repeatUntilNoTags = RepeatUnitNoTags;

            return (m_Result = SetSingulationAlgorithmParms_CS108(SingulationAlgorithm.FIXEDQ, FixedQParm));
        }
        /// <summary>
        /// The  parameters  for  the  fixed-Q  algorithm,  MAC  singulation  algorithm  0
        /// If running a same operation, it only need to config once times
        /// </summary>
        /// <returns></returns>
        public Result SetFixedQParms_CS108(FixedQParms fixedQParm)
        {
            return (m_Result = SetSingulationAlgorithmParms_CS108(SingulationAlgorithm.FIXEDQ, fixedQParm));
        }
        /// <summary>
        /// The  parameters  for  the  fixed-Q  algorithm,  MAC  singulation  algorithm  0
        /// If running a same operation, it only need to config once times
        /// </summary>
        /// <returns></returns>
        public Result SetFixedQParms_CS108()
        {
            FixedQParms FixedQParm = new FixedQParms();
            FixedQParm.qValue = 7;      //if only 1 tag read and write, otherwise use 7
            FixedQParm.retryCount = 0;
            FixedQParm.toggleTarget = 1;
            FixedQParm.repeatUntilNoTags = 1;

            return (m_Result = SetSingulationAlgorithmParms_CS108(SingulationAlgorithm.FIXEDQ, FixedQParm));
        }

        /// <summary>
        /// Get DynamicQ Singulation Algorithm
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        public Result GetDynamicQParms_CS108(DynamicQParms parms)
        {
            return (m_Result = GetSingulationAlgorithmParms_CS108(SingulationAlgorithm.DYNAMICQ, parms));
        }
        /// <summary>
        /// The parameters for the dynamic-Q algorithm with application-controlled Q-adjustment-threshold, MAC singulation algorithm 3
        /// </summary>
        /// <param name="StartQValue">The starting Q value to use.  Valid values are 0-15, inclusive.  
        /// startQValue must be greater than or equal to minQValue and 
        /// less than or equal to maxQValue. </param>
        /// <param name="MinQValue">The minimum Q value to use.  Valid values are 0-15, inclusive.  
        /// minQValue must be less than or equal to startQValue and 
        /// maxQValue. </param>
        /// <param name="MaxQValue">The maximum Q value to use.  Valid values are 0-15, inclusive.  
        /// maxQValue must be greater than or equal to startQValue and 
        /// minQValue. </param>
        /// <param name="RetryCount">Specifies the number of times to try another execution of 
        /// the singulation algorithm for the specified session/target 
        /// before either toggling the target (if toggleTarget is non-
        /// zero) or terminating the inventory/tag access operation.  
        /// Valid values are 0-255, inclusive. </param>
        /// <param name="ThresholdMultiplier">The multiplier, specified in units of fourths (i.e., 0.25), that will be 
        /// applied to the Q-adjustment threshold as part of the dynamic-Q 
        /// algorithm.  For example, a value of 7 represents a multiplier of 
        /// 1.75.  See [MAC-EDS] for specifics on how the Q-adjustment 
        /// threshold is used in the dynamic Q algorithm.  Valid values are 0-
        /// 255, inclusive. </param>
        /// <param name="ToggleTarget">A flag that indicates if, after performing the inventory cycle for the 
        /// specified target (i.e., A or B), if the target should be toggled (i.e., 
        /// A to B or B to A) and another inventory cycle run.  A non-zero 
        /// value indicates that the target should be toggled.  A zero value 
        /// indicates that the target should not be toggled.  Note that if the 
        /// target is toggled, retryCount and maxQueryRepCount will 
        /// also apply to the new target. </param>
        public Result SetDynamicQParms_CS108(uint StartQValue, uint MinQValue, uint MaxQValue, uint RetryCount, uint ThresholdMultiplier, uint ToggleTarget)
        {
            DynamicQParms dynParm = new DynamicQParms();
            dynParm.startQValue = StartQValue;
            dynParm.maxQValue = MaxQValue;      //if only 1 tag read and write, otherwise use 7
            dynParm.minQValue = MinQValue;
            dynParm.retryCount = RetryCount;
            dynParm.thresholdMultiplier = ThresholdMultiplier;
            dynParm.toggleTarget = ToggleTarget;

            return (m_Result = SetSingulationAlgorithmParms_CS108(SingulationAlgorithm.DYNAMICQ, dynParm));
        }
        /// <summary>
        /// The parameters for the dynamic-Q algorithm with application-controlled Q-adjustment-threshold, MAC singulation algorithm 3
        /// </summary>
        /// <returns></returns>
        public Result SetDynamicQParms_CS108(DynamicQParms dynParm)
        {
            return (m_Result = SetSingulationAlgorithmParms_CS108(SingulationAlgorithm.DYNAMICQ, dynParm));
        }
        /// <summary>
        /// The parameters for the dynamic-Q algorithm with application-controlled Q-adjustment-threshold, MAC singulation algorithm 3
        /// </summary>
        /// <returns></returns>
        public Result SetDynamicQParms_CS108()
        {
            DynamicQParms dynParm = new DynamicQParms();
            dynParm.startQValue = 7;
            dynParm.maxQValue = 15;      //if only 1 tag read and write, otherwise use 7
            dynParm.minQValue = 0;
            dynParm.retryCount = 0;
            dynParm.thresholdMultiplier = 4;
            dynParm.toggleTarget = 1;

            return (m_Result = SetSingulationAlgorithmParms_CS108(SingulationAlgorithm.DYNAMICQ, dynParm));
        }
    }
}
