using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary
{
    using Constants;
    using Structures;
    using Events;
    using Tools;
    public class ClassASYN
    {
        public class TagCallbackInfo : CSLibrary.Structures.TagCallbackInfo
        {
            public TagCallbackInfo()
            {
            }
        }

        //public class OnAsyncCallbackEventArgs() : CSLibrary.Events.OnAsyncCallbackEventArgs
        //{
        //}

        private HighLevelInterface _deviceHandler;

        public event EventHandler<OnAsyncCallbackEventArgs> OnAsyncCallback;

        internal ClassASYN(HighLevelInterface handler)
        {
            _deviceHandler = handler;
        }

        void TagInventoryEvent(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e)
        {
            if (e.type != CSLibrary.Constants.CallbackType.TAG_RANGING)
                return;

            CSLibrary.Constants.CallbackType type = CSLibrary.Constants.CallbackType.TAG_RANGING;
            //CSLibrary.Events.OnAsyncCallbackEventArgs callBackData = new Events.OnAsyncCallbackEventArgs(info, type);

            //if (OnAsyncCallback != null)
            //    OnAsyncCallback(_deviceHandler, callBackData);
        }

        public Result ClearEventHandler()
        {
            this.OnAsyncCallback = null;
            return Result.OK;
        }

        public Result StartInventory ()
        {
            _deviceHandler.rfid.ClearOnAsyncCallback();
            _deviceHandler.rfid.OnAsyncCallback += new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);
            _deviceHandler.rfid.Options.TagRanging.multibanks = 2;
            _deviceHandler.rfid.Options.TagRanging.bank1 = MemoryBank.USER ;
            _deviceHandler.rfid.Options.TagRanging.offset1 = 0;
            _deviceHandler.rfid.Options.TagRanging.count1 = 1;
            _deviceHandler.rfid.Options.TagRanging.bank2 = MemoryBank.USER;
            _deviceHandler.rfid.Options.TagRanging.offset2 = 0;
            _deviceHandler.rfid.Options.TagRanging.count2 = 1;

            return _deviceHandler.rfid.StartOperation(Operation.TAG_RANGING);
        }

        public void StopInventory ()
        {
            _deviceHandler.rfid.StopOperation();
        }

        public Result Configuration(S_EPC tagid)
        {
            return _deviceHandler.rfid.StartOperation(Operation.TAG_RANGING);
        }


    }
}
