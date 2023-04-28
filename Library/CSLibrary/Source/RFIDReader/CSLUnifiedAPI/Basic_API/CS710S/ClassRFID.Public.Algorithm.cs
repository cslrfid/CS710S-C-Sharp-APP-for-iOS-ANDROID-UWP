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

using CSLibrary.Constants;
using CSLibrary.Structures;

namespace CSLibrary
{
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
        /// RFID_ERROR_INVALID_PARAMETER is returned. </param>m
        public Result SetCurrentSingulationAlgorithm_CS710S(SingulationAlgorithm SingulationAlgorithm)
        {
            if (SingulationAlgorithm == SingulationAlgorithm.FIXEDQ)
                RFIDRegister.AntennaPortConfig.EnableFixedQ();
            else
                RFIDRegister.AntennaPortConfig.EnableDynamicQ();

            return Result.OK;
        }

        /// <summary>
        /// Get Current Singulation Algorithm
        /// </summary>
        /// <param name="SingulationAlgorithm"></param>
        /// <returns></returns>
        public Result GetCurrentSingulationAlgorithm_CS710S(ref SingulationAlgorithm SingulationAlgorithm)
        {
            if (RFIDRegister.AntennaPortConfig.GetCurrentAlgorithm() == 0)
                SingulationAlgorithm = SingulationAlgorithm.DYNAMICQ;
            else
                SingulationAlgorithm = SingulationAlgorithm.FIXEDQ;

            return Result.OK;
        }

        /// <summary>
        /// SetSingulationAlgorithmParms
        /// </summary>
        /// <param name="alg"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public Result SetSingulationAlgorithmParms_CS710S(SingulationAlgorithm alg, SingulationAlgorithmParms parms)
        {
            if (alg == SingulationAlgorithm.UNKNOWN)
                return Result.INVALID_PARAMETER;

            try
            {
                switch (alg)
                {
                    case SingulationAlgorithm.FIXEDQ:
                        {
                            FixedQParms p = (FixedQParms)parms;
                            RFIDRegister.AntennaPortConfig.EnableFixedQ(p.qValue, 0); // Query Target alway A
                            RFIDRegister.AntennaPortConfig.SetTargetToggle(p.toggleTarget > 0);
                        }
                        break;

                    case SingulationAlgorithm.DYNAMICQ:
                        {
                            DynamicQParms p = (DynamicQParms)parms;
                            RFIDRegister.AntennaPortConfig.EnableDynamicQ(p.minQValue, p.maxQValue, p.startQValue, p.MinQCycles, p.QDecreaseUseQuery, p.QIncreaseUseQuery, 0); // Query Target alway A
                            RFIDRegister.AntennaPortConfig.MaxQSinceValidEpc(p.NoEPCMaxQ);
                            RFIDRegister.AntennaPortConfig.SetTargetToggle(p.toggleTarget > 0);
                        }
                        break;

                    default:
                        return Result.INVALID_PARAMETER;
                } // switch (algorithm)

            }
            catch (Exception ex)
            {

            }

            return (m_Result = Result.OK);
            //return (m_Result = SetCurrentSingulationAlgorithm(alg));
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
        public Result SetFixedQParms_CS710S(uint QValue, uint ToggleTarget)
        {
            FixedQParms FixedQParm = new FixedQParms();
            FixedQParm.qValue = QValue;      //if only 1 tag read and write, otherwise use 7
            FixedQParm.toggleTarget = ToggleTarget;

            return (m_Result = SetSingulationAlgorithmParms_CS710S(SingulationAlgorithm.FIXEDQ, FixedQParm));
        }
        /// <summary>
        /// The  parameters  for  the  fixed-Q  algorithm,  MAC  singulation  algorithm  0
        /// If running a same operation, it only need to config once times
        /// </summary>
        /// <returns></returns>
        public Result SetFixedQParms_CS710S(FixedQParms fixedQParm)
        {
            return (m_Result = SetSingulationAlgorithmParms_CS710S(SingulationAlgorithm.FIXEDQ, fixedQParm));
        }
        /// <summary>
        /// The  parameters  for  the  fixed-Q  algorithm,  MAC  singulation  algorithm  0
        /// If running a same operation, it only need to config once times
        /// </summary>
        /// <returns></returns>
        public Result SetFixedQParms_CS710S()
        {
            FixedQParms FixedQParm = new FixedQParms();
            FixedQParm.qValue = 7;      //if only 1 tag read and write, otherwise use 7
            FixedQParm.toggleTarget = 1;

            return (m_Result = SetSingulationAlgorithmParms_CS710S(SingulationAlgorithm.FIXEDQ, FixedQParm));
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
        /// <param name="ToggleTarget">A flag that indicates if, after performing the inventory cycle for the 
        /// specified target (i.e., A or B), if the target should be toggled (i.e., 
        /// A to B or B to A) and another inventory cycle run.  A non-zero 
        /// value indicates that the target should be toggled.  A zero value 
        /// indicates that the target should not be toggled.  Note that if the 
        /// target is toggled, retryCount and maxQueryRepCount will 
        /// also apply to the new target. </param>
        public Result SetDynamicQParms_CS710S(uint StartQValue, uint MinQValue, uint MaxQValue, uint ToggleTarget)
        {
            DynamicQParms dynParm = new DynamicQParms();
            dynParm.startQValue = StartQValue;
            dynParm.maxQValue = MaxQValue;      //if only 1 tag read and write, otherwise use 7
            dynParm.minQValue = MinQValue;
            dynParm.toggleTarget = ToggleTarget;

            return (m_Result = SetSingulationAlgorithmParms_CS710S(SingulationAlgorithm.DYNAMICQ, dynParm));
        }
        /// <summary>
        /// The parameters for the dynamic-Q algorithm with application-controlled Q-adjustment-threshold
        /// </summary>
        /// <returns></returns>
        public Result SetDynamicQParms_CS710S(DynamicQParms dynParm)
        {
            return (m_Result = SetSingulationAlgorithmParms_CS710S(SingulationAlgorithm.DYNAMICQ, dynParm));
        }
        /// <summary>
        /// The parameters for the dynamic-Q algorithm with application-controlled Q-adjustment-threshold
        /// </summary>
        /// <returns></returns>
        public Result SetDynamicQParms_CS710S()
        {
            DynamicQParms dynParm = new DynamicQParms();
            dynParm.startQValue = 7;
            dynParm.maxQValue = 15;      //if only 1 tag read and write, otherwise use 7
            dynParm.minQValue = 0;
            dynParm.toggleTarget = 1;

            return (m_Result = SetSingulationAlgorithmParms_CS710S(SingulationAlgorithm.DYNAMICQ, dynParm));
        }
    }
}
