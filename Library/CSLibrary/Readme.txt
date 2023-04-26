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

CSLibrary is CS108 RFID reader framework

CSLibrary type
--------------
1. CSLibrary-NETStandard is a .netstandard library for iOS and Android MVVM apps
2. CSLibrary-DESKTOP is a DLL library for Widnows 10 Desktop apps (WINFORMS and WPF) 
3. CSLibrary-UWP is a link project for Widnows 10 UWP apps


CLibrary structure
------------------
CSLibrary				: Main library
CSLibrary.Net			: Reader search functions
CSLibrary.notification	: Reader inforation (volatage and blue key)
CSLibrary.rfid			: RFID reader Class
CSLibrary.barcode		: Barcode scan Class
CSLibrary.bluetoothIC	: for internal uses
CSLibrary.siliconlabIC	: for internal uses


API List
--------

CSLibrary.notification functions
--------------------------------
uint GetCurrentBatteryLevel()			// Get battery level
void ClearEventHandler()				// Clear callback event

CSLibrary.notification callback events
--------------------------------------
EventHandler<VoltageEventArgs> OnVoltageEvent	// Battery level callback event
EventHandler<HotKeyEventArgs> OnKeyEvent		// Key callback event

CSLibrary.bluetoothIC functions
-------------------------------
UInt32 GetFirmwareVersion ()			// Get bluetooth IC firmware version
string GetSerialNumberSync()			// Get serial number
string GetPCBVersion ()					// Get PCB version
void ClearEventHandler()				// Clean callback event

CSLibrary.siliconlabIC functions
--------------------------------
uint GetFirmwareVersion()				// Get siliconlab IC firmware version
string GetDeviceName ()					// Get device name
bool SetDeviceName (string deviceName)	// Set device name

CSLibrary callback event
-------------------------
public event EventHandler<CSLibrary.Events.OnReaderStateChangedEventArgs> OnReaderStateChanged;	// Return Reader Status

CSLibrary functions
-------------------
public async Task<bool> ConnectAsync(IAdapter adapter, IDevice device)							// Connect to reader
public async void DisconnectAsync()																// Disconnect from reader

CSLibrary.barcode callback events
---------------------------------
public event EventHandler<CSLibrary.Barcode.BarcodeEventArgs> OnCapturedNotify;					// Return barcode data
public event EventHandler<CSLibrary.Barcode.BarcodeStateEventArgs> OnStateChanged;				// Return barcode module status
	Event Type
	----------
	CSLibrary.Barcode.Constants.BarcodeState.IDLE												// In idle mode and ready to receive command
	CSLibrary.Barcode.Constants.BarcodeState.BUSY												// Barcode sacnning

CSLibrary.barcode functions
---------------------------
public bool Start()																				// Start scanning
public bool Stop()																				// Stop scanning
public void FactoryReset()																		// Initialize Barcode setting to Factory Reset

CSLibrary.barcode callback events
---------------------------------
public event EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs> OnAsyncCallback;			// Return inventory / searching tags data
	Event Type
	----------
	CSLibrary.Constants.CallbackType.TAG_RANGING												// inventory data
	CSLibrary.Constants.CallbackType.TAG_SEARCHING												// searching tag data

public event EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs> OnAccessCompleted;		// Return read/write/lock result
public event EventHandler<CSLibrary.Events.OnStateChangedEventArgs> OnStateChanged;				// Return RFID reader state
	Event Type
	----------
	CSLibrary.Constants.RFState.INITIALIZATION_COMPLETE											// RFID initialization complete
	CSLibrary.Constants.RFState.IDLE															// RFID reader in idle mode and ready to receive command
	CSLibrary.Constants.RFState.BUSY															// RFID reader busy


CSLibrary.rfid properties
-------------------------
uint SelectedChannel																		// Currect selected channel
RegionCode SelectedRegionCode															// Current selected region code
bool IsHoppingChannelOnly																// Is reader only hopping channel
bool IsFixedChannelOnly																	// Is reader only fixed channel
bool IsFixedChannel																		// Is current selected fixed channel
Machine DeviceType																		// Reader Type
ChipSetID ChipSetID																		// Reader chipset ID
uint CountryCode																			// Reader Counter Code


CSLibrary.rfid functions
------------------------
uint GetActiveMaxPowerLevel()															// Get Max Power level
Result GetPowerLevel(ref uint pwrlvl)													// Get Current Power level
void SetPowerLevel(UInt32 pwrlevel)														// Set Power level

uint[] GetActiveLinkProfile()															// Get active profile list
Result SetCurrentLinkProfile(uint profile)												// Set profile

Result SetPostMatchCriteria(SingulationCriterion[] postmatch)							// Set Post Filter

Result SetTagGroup(TagGroup tagGroup)													// Set TagGroup parameters
Result GetTagGroup(ref TagGroup tagGroup)												// Get TagGroup parameters

Result SetCurrentSingulationAlgorithm(SingulationAlgorithm SingulationAlgorithm)			// Set current algorithm
Result GetCurrentSingulationAlgorithm(ref SingulationAlgorithm SingulationAlgorithm)		// Get current algorithm
				
Result SetFixedQParms(FixedQParms fixedQParm)											// Set fixed Q parameters
Result GetFixedQParms(ref FixedQParms fixedQ)											// Get current fixed Q parameters 

Result SetDynamicQParms(DynamicQParms dynParm)											// Set Dynamic Q parameters
Result GetDynamicQParms(ref DynamicQParms parms)											// Get Dynamic Q parameters

Result SetOperationMode(RadioOperationMode mode)											// Set operation mode CONTINUOUS/NON-CONTINUOUS
void GetOperationMode(ref RadioOperationMode mode)										// Get current operation mode

Result SetOperationMode(ushort cycles)													// Set inventory antenna cycle
Result SetInventoryDuration(uint duration)												// Set inventory duration (dwell time)
Result SetInventoryCycle(uint cycle)														// Set inventory cycles count

Result SetFixedChannel(RegionCode prof = RegionCode.CURRENT, uint channel = 0)			// Set to fixed channel with region
Result SetHoppingChannels(RegionCode prof)												// Set to hopping with region
Result SetHoppingChannels()																// Set to hopping with current selected region
Result SetAgileChannels(RegionCode prof)													// Set to agile channel with region

Result GetCountryCode(ref uint code)														// Get reader country code
List<RegionCode> GetActiveRegionCode()													// Get vaild region code list

double[] GetAvailableFrequencyTable(RegionCode region)									// Get Available frequency table with region code
double[] GetCurrentFrequencyTable()														// Get frequency table on current selected region

Result StartOperation(Operation opertion)												// Start special function (see below table)
void StopOperation()																		// Stop CONTINUOUS inventory 


Operation value
---------------
Operation.TAG_RANGING																			// Start Inventory
Operation.TAG_PRERANGING																		// Inventory pre-setting
Operation.TAG_EXERANGING																		// Execute inventory command
Operation.TAG_SEARCHING																			// Start search Tag
Operation.TAG_SELECTED																			// Set Selected Tag parameter
Operation.TAG_GENERALSELECTED																	// Set Selected Tag parameter
Operation.TAG_PREFILTER																			// Set Pre-Filter
Operation.TAG_READ																				// Start to read
Operation.TAG_READ_PC																			// Start to read PC
Operation.TAG_READ_EPC																			// Start to read EPC
Operation.TAG_READ_ACC_PWD																		// Start to read access password
Operation.TAG_READ_KILL_PWD																		// Start to read kill password
Operation.TAG_READ_TID																			// Start to read TID bank data
Operation.TAG_READ_USER																			// Start to reader USER bank data
Operation.TAG_WRITE																				// Start to Write
Operation.TAG_WRITE_PC																			// Start to Write PC
Operation.TAG_WRITE_EPC																			// Start to Write EPC
Operation.TAG_WRITE_ACC_PWD																		// Start to Write access password
Operation.TAG_WRITE_KILL_PWD																	// Start to Write kill password
Operation.TAG_WRITE_USER																		// Start to Write USER bank data
Operation.TAG_LOCK																				// Set Tag Lock
Operation.TAG_BLOCK_PERMALOCK																	// Set Tag Block Permalock
Operation.TAG_KILL																				// Kill Tag
