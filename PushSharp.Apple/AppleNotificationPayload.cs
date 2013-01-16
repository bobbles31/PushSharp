namespace PushSharp.Apple
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;

    using Newtonsoft.Json.Linq;

    public class AppleNotificationPayload
	{
        private const int MAX_PAYLOAD_SIZE = 256;

		public AppleNotificationAlert Alert { get; set; }

		public int? ContentAvailable { get; set; }

		public int? Badge { get; set; }

		public string Sound { get; set; }

		public bool HideActionButton { get; set; }


		public Dictionary<string, object[]> CustomItems
		{
			get;
			private set;
		}

		public AppleNotificationPayload()
		{
			HideActionButton = false;
			Alert = new AppleNotificationAlert();
			CustomItems = new Dictionary<string, object[]>();
		}

		public AppleNotificationPayload(string alert)
		{
			HideActionButton = false;
			Alert = new AppleNotificationAlert() { Body = alert };
			CustomItems = new Dictionary<string, object[]>();
		}

		public AppleNotificationPayload(string alert, int badge)
		{
			HideActionButton = false;
			Alert = new AppleNotificationAlert() { Body = alert };
			Badge = badge;
			CustomItems = new Dictionary<string, object[]>();
		}

		public AppleNotificationPayload(string alert, int badge, string sound)
		{
			HideActionButton = false;
			Alert = new AppleNotificationAlert() { Body = alert };
			Badge = badge;
			Sound = sound;
			CustomItems = new Dictionary<string, object[]>();
		}

		public void AddCustom(string key, params object[] values)
		{
			if (values != null)
				this.CustomItems.Add(key, values);
		}

        public string ToJson()
		{
			var json = new JObject();

			var aps = new JObject();

			if (!this.Alert.IsEmpty)
			{
				if (!string.IsNullOrEmpty(this.Alert.Body)
					&& Alert.IsNotLocalized
					&& !this.HideActionButton)
				{
					aps["alert"] = new JValue(this.Alert.Body);
				}
				else
				{
					JObject jsonAlert = new JObject();

					if (!string.IsNullOrEmpty(this.Alert.LocalizedKey))
						jsonAlert["loc-key"] = new JValue(this.Alert.LocalizedKey);

					if (this.Alert.LocalizedArgs != null && this.Alert.LocalizedArgs.Count > 0)
						jsonAlert["loc-args"] = new JArray(this.Alert.LocalizedArgs.ToArray());

					if (!string.IsNullOrEmpty(this.Alert.Body))
						jsonAlert["body"] = new JValue(this.Alert.Body);

					if (this.HideActionButton)
						jsonAlert["action-loc-key"] = new JValue((string)null);
					else if (!string.IsNullOrEmpty(this.Alert.ActionLocalizedKey))
						jsonAlert["action-loc-key"] = new JValue(this.Alert.ActionLocalizedKey);

					aps["alert"] = jsonAlert;
				}
			}

			if (this.Badge.HasValue)
				aps["badge"] = new JValue(this.Badge.Value);

			if (!string.IsNullOrEmpty(this.Sound))
				aps["sound"] = new JValue(this.Sound);

			if (this.ContentAvailable.HasValue)
				aps["content-available"] = new JValue(this.ContentAvailable.Value);

			if (aps.Count > 0)
				json["aps"] = aps;

			foreach (string key in this.CustomItems.Keys)
			{
				if (this.CustomItems[key].Length == 1)
					json[key] = new JValue(this.CustomItems[key][0]);
				else if (this.CustomItems[key].Length > 1)
					json[key] = new JArray(this.CustomItems[key]);
			}

			string rawString = json.ToString(Newtonsoft.Json.Formatting.None, null);

			StringBuilder encodedString = new StringBuilder();
			foreach (char c in rawString)
			{
				if ((int)c < 32 || (int)c > 127)
					encodedString.Append("\\u" + String.Format("{0:x4}", Convert.ToUInt32(c)));
				else
					encodedString.Append(c);
			}
			return rawString;// encodedString.ToString();
		}

	    public byte[] AsByteArray()
	    {
            byte[] payload = Encoding.UTF8.GetBytes(ToJson());

            if (payload.Length > MAX_PAYLOAD_SIZE)
            {
                int newSize = Alert.Body.Length - (payload.Length - MAX_PAYLOAD_SIZE);
                if (newSize > 0)
                {
                    Alert.Body = Alert.Body.Substring(0, newSize);
                    payload = Encoding.UTF8.GetBytes(ToString());
                }
                else
                {
                    do
                    {
                        Alert.Body = Alert.Body.Remove(Alert.Body.Length - 1);
                        payload = Encoding.UTF8.GetBytes(ToString());
                    }
                    while (payload.Length > MAX_PAYLOAD_SIZE && !string.IsNullOrEmpty(Alert.Body));
                }

                if (payload.Length > MAX_PAYLOAD_SIZE)
                {
                    throw new AppleNotificationToPayLoadConversionException();
                }
            }

	        return payload;
	    }

	    public override string ToString()
		{
			return ToJson();
		}

        public byte[] SizeInBytes()
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(ToString().Length)));
        }
	}
}



