using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write coil functions/requests.
    /// </summary>
    public class WriteSingleCoilFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleCoilFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters p = this.CommandParameters as ModbusWriteCommandParameters;
            byte[] packet = new byte[12];

            packet[0] = (byte)(p.TransactionId >> 8);
            packet[1] = (byte)(p.TransactionId);
            packet[2] = (byte)(p.ProtocolId >> 8);
            packet[3] = (byte)(p.ProtocolId);
            packet[4] = (byte)(p.Length >> 8);
            packet[5] = (byte)(p.Length);
            packet[6] = p.UnitId;
            packet[7] = p.FunctionCode;
            packet[8] = (byte)(p.OutputAddress >> 8);
            packet[9] = (byte)(p.OutputAddress);
            packet[10] = (byte)(p.Value == 1 ? 0xFF : 0x00);
            packet[11] = 0x00;

            return packet;
        }

        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusWriteCommandParameters p = this.CommandParameters as ModbusWriteCommandParameters;
            var result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] == 0x85)
            {
                HandeException(response[8]);
                return result;
            }

            ushort value = p.Value;
            result.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, p.OutputAddress), value);

            return result;
        }
    }
}