using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read discrete inputs functions/requests.
    /// </summary>
    public class ReadDiscreteInputsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadDiscreteInputsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadDiscreteInputsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters p = this.CommandParameters as ModbusReadCommandParameters;
            byte[] packet = new byte[12];

            packet[0] = (byte)(p.TransactionId >> 8);
            packet[1] = (byte)(p.TransactionId);
            packet[2] = (byte)(p.ProtocolId >> 8);
            packet[3] = (byte)(p.ProtocolId);
            packet[4] = (byte)(p.Length >> 8);
            packet[5] = (byte)(p.Length);
            packet[6] = p.UnitId;
            packet[7] = p.FunctionCode;
            packet[8] = (byte)(p.StartAddress >> 8);
            packet[9] = (byte)(p.StartAddress);
            packet[10] = (byte)(p.Quantity >> 8);
            packet[11] = (byte)(p.Quantity);

            return packet;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusReadCommandParameters p = this.CommandParameters as ModbusReadCommandParameters;
            var result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] == 0x82)
            {
                HandeException(response[8]);
                return result;
            }

            int byteCount = response[8];
            for (int i = 0; i < p.Quantity; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                ushort value = (ushort)((response[9 + byteIndex] >> bitIndex) & 1);
                result.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_INPUT, (ushort)(p.StartAddress + i)), value);
            }

            return result;
        }
    }
}