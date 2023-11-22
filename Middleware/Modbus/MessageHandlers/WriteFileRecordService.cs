﻿using Net.Middleware.Modbus.Data;
using Net.Middleware.Modbus.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Middleware.Modbus.MessageHandlers
{
    public class WriteFileRecordService
        : ModbusFunctionServiceBase<WriteFileRecordRequest>
    {
        public WriteFileRecordService()
            : base(ModbusFunctionCodes.WriteFileRecord)
        {
        }

        public override IModbusMessage CreateRequest(byte[] frame)
        {
            return CreateModbusMessage<WriteFileRecordRequest>(frame);
        }

        public override int GetRtuRequestBytesToRead(byte[] frameStart)
        {
            return frameStart[2] + 1;
        }

        public override int GetRtuResponseBytesToRead(byte[] frameStart)
        {
            return frameStart[2] + 1;
        }

        protected override IModbusMessage Handle(WriteFileRecordRequest request, ISlaveDataStore dataStore)
        {
            throw new NotImplementedException("WriteFileRecordService::Handle");
        }
    }
}
