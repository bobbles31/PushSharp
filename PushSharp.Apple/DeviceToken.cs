namespace PushSharp.Apple
{
    using System;
    using System.Net;

    public class DeviceToken
    {
        private const int DEVICE_TOKEN_BINARY_SIZE = 32;

        private const int DEVICE_TOKEN_STRING_SIZE = 64;

        private readonly string deviceToken;

        public DeviceToken(string deviceToken)
        {
            if (!string.IsNullOrEmpty(deviceToken) && deviceToken.Length != DEVICE_TOKEN_STRING_SIZE)
            {
                throw new InvalidDeviceTokenException();
            }

            this.deviceToken = deviceToken;
        }

        public int TokenSizeAsBytesLength
        {
            get
            {
                return TokenSizeAsBytes.Length;
            }
        }

        public byte[] TokenSizeAsBytes
        {
            get
            {
                return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(deviceToken.Length)));
            }
        }

        public int RawStringLength
        {
            get
            {
                return deviceToken.Length;
            }
        }

        public string AsString
        {
            get
            {
                return deviceToken;
            }
        }

        public byte[] AsByteArray
        {
            get
            {
                var tokenBytes = new byte[this.deviceToken.Length / 2];
                for (int i = 0; i < this.deviceToken.Length; i++)
                {
                    try
                    {
                        tokenBytes[i] = byte.Parse(this.deviceToken.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                    }
                    catch (Exception)
                    {
                        throw new DeviceTokenStringToByteArrayParseException();
                    }
                }

                if (this.deviceToken.Length != DEVICE_TOKEN_BINARY_SIZE)
                {
                    throw new InvalidDeviceTokenBinarySizeException();
                }

                return tokenBytes;
            }
        }
    }
}