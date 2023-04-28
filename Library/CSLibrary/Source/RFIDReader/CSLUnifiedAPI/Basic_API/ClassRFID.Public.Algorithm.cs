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
    using static RFIDDEVICE;
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
        /// RFID_ERROR_INVALID_PARAMETER is returned. </param>m
        public Result SetCurrentSingulationAlgorithm(SingulationAlgorithm SingulationAlgorithm)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetCurrentSingulationAlgorithm_CS108(SingulationAlgorithm);
                    break;

                case MODEL.CS710S:
                    return SetCurrentSingulationAlgorithm_CS710S(SingulationAlgorithm);
                    break;
            }

            return Result.FAILURE;
        }

        /// <summary>
        /// Get Current Singulation Algorithm
        /// </summary>
        /// <param name="SingulationAlgorithm"></param>
        /// <returns></returns>
        public Result GetCurrentSingulationAlgorithm(ref SingulationAlgorithm SingulationAlgorithm)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return GetCurrentSingulationAlgorithm_CS108(ref SingulationAlgorithm);
                    break;

                case MODEL.CS710S:
                    return GetCurrentSingulationAlgorithm_CS710S(ref SingulationAlgorithm);
                    break;
            }

            return Result.FAILURE;
        }

        /// <summary>
        /// SetSingulationAlgorithmParms
        /// </summary>
        /// <param name="alg"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public Result SetSingulationAlgorithmParms(SingulationAlgorithm alg, SingulationAlgorithmParms parms)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetSingulationAlgorithmParms_CS108(alg, parms);
                    break;

                case MODEL.CS710S:
                    return SetSingulationAlgorithmParms_CS710S(alg, parms);
                    break;
            }

            return Result.FAILURE;
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
        public Result SetFixedQParms(uint QValue, uint ToggleTarget)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetFixedQParms_CS108(QValue, 0, ToggleTarget, 0);
                    //return SetFixedQParms_CS108(QValue, ToggleTarget);
                    break;

                case MODEL.CS710S:
                    return SetFixedQParms_CS710S(QValue, ToggleTarget);
                    break;
            }

            return Result.FAILURE;
        }
        /// <summary>
        /// The  parameters  for  the  fixed-Q  algorithm,  MAC  singulation  algorithm  0
        /// If running a same operation, it only need to config once times
        /// </summary>
        /// <returns></returns>
        public Result SetFixedQParms(FixedQParms fixedQParm)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetFixedQParms_CS108(fixedQParm);
                    break;

                case MODEL.CS710S:
                    return SetFixedQParms_CS710S(fixedQParm);
                    break;
            }

            return Result.FAILURE;
        }
        /// <summary>
        /// The  parameters  for  the  fixed-Q  algorithm,  MAC  singulation  algorithm  0
        /// If running a same operation, it only need to config once times
        /// </summary>
        /// <returns></returns>
        public Result SetFixedQParms()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetFixedQParms_CS108();
                    break;

                case MODEL.CS710S:
                    return SetFixedQParms_CS710S();
                    break;
            }

            return Result.FAILURE;
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
        public Result SetDynamicQParms(uint StartQValue, uint MinQValue, uint MaxQValue, uint ToggleTarget)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetDynamicQParms_CS108(StartQValue, MinQValue, MaxQValue, 0, 0, ToggleTarget);
                    //return SetDynamicQParms_CS108(StartQValue, MinQValue, MaxQValue, ToggleTarget);
                    break;

                case MODEL.CS710S:
                    return SetDynamicQParms_CS710S(StartQValue, MinQValue, MaxQValue, ToggleTarget);
                    break;
            }

            return Result.FAILURE;
        }
        /// <summary>
        /// The parameters for the dynamic-Q algorithm with application-controlled Q-adjustment-threshold
        /// </summary>
        /// <returns></returns>
        public Result SetDynamicQParms(DynamicQParms dynParm)
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetDynamicQParms_CS108(dynParm);
                    break;

                case MODEL.CS710S:
                    return SetDynamicQParms_CS710S(dynParm);
                    break;
            }

            return Result.FAILURE;
        }
        /// <summary>
        /// The parameters for the dynamic-Q algorithm with application-controlled Q-adjustment-threshold
        /// </summary>
        /// <returns></returns>
        public Result SetDynamicQParms()
        {
            switch (_deviceType)
            {
                case MODEL.CS108:
                    return SetDynamicQParms_CS108();
                    break;

                case MODEL.CS710S:
                    return SetDynamicQParms_CS710S();
                    break;
            }

            return Result.FAILURE;
        }
    }
}
