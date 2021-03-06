﻿namespace PushSharp.Sample
{
    using System;
    using System.IO;

    using PushSharp.Android;
    using PushSharp.Apple;
    using PushSharp.Windows;
    using PushSharp.WindowsPhone;

    class Program
	{
		static void Main(string[] args)
		{		
			var push = new PushService();

			HookUpAllEvents(push);

		    //Configure and start Apple APNS
			// IMPORTANT: Make sure you use the right Push certificate.  Apple allows you to generate one for connecting to Sandbox,
			//   and one for connecting to Production.  You must use the right one, to match the provisioning profile you build your
			//   app with!
            var appleCert = File.ReadAllBytes("C:\\Certs\\DevelopmentCertificate.p12");

            //IMPORTANT: If you are using a Development provisioning Profile, you must use the Sandbox push notification server 
            //  (so you would leave the first arg in the ctor of ApplePushChannelSettings as 'false')
            //  If you are using an AdHoc or AppStore provisioning profile, you must use the Production push notification server
            //  (so you would change the first arg in the ctor of ApplePushChannelSettings to 'true')
		    push.StartApplePushService(new ApplePushChannelSettings(false, appleCert, null));

            //Configure and start Android GCM
            //IMPORTANT: The SENDER_ID is your Google API Console App Project ID.
            //  Be sure to get the right Project ID from your Google APIs Console.  It's not the named project ID that appears in the Overview,
            //  but instead the numeric project id in the url: eg: https://code.google.com/apis/console/?pli=1#project:785671162406:overview
            //  where 785671162406 is the project id, which is the SENDER_ID to use!
            //push.StartGoogleCloudMessagingPushService(new GcmPushChannelSettings("785671162406", "AIzaSyC2PZNXQDVaUpZGmtsF_Vp8tHtIABVjazI", "com.pushsharp.test"));

            //Configure and start Windows Phone Notifications
            //push.StartWindowsPhonePushService(new WindowsPhonePushChannelSettings());

			//Configure and start Windows Notifications
			//push.StartWindowsPushService(new WindowsPushChannelSettings("677AltusApps.PushSharpTest",
				//"ms-app://s-1-15-2-397915024-884168245-3562497613-3307968140-4074292843-797285123-433377759", "ei5Lott1HEbbZBv2wGDTUsrCjU++Pj8Z"));

            //Fluent construction of a Windows Toast Notification
            //push.QueueNotification(NotificationFactory.Windows().Toast().AsToastText01("This is a test").ForChannelUri("YOUR_CHANNEL_URI_HERE"));

            //Fluent construction of a Windows Phone Toast notification
            //IMPORTANT: For Windows Phone you MUST use your own Endpoint Uri here that gets generated within your Windows Phone app itself!
            /*push.QueueNotification(NotificationFactory.WindowsPhone().Toast()
                .ForEndpointUri(new Uri("http://sn1.notify.live.net/throttledthirdparty/01.00/AAFCoNoCXidwRpn5NOxvwSxPAgAAAAADAgAAAAQUZm52OkJCMjg1QTg1QkZDMkUxREQ"))
                .ForOSVersion(WindowsPhone.WindowsPhoneDeviceOSVersion.MangoSevenPointFive)
                .WithBatchingInterval(WindowsPhone.BatchingInterval.Immediate)
                .WithNavigatePath("/MainPage.xaml")
                .WithText1("PushSharp")
                .WithText2("This is a Toast"));*/

            //Fluent construction of an iOS notification
            //IMPORTANT: For iOS you MUST MUST MUST use your own DeviceToken here that gets generated within your iOS app itself when the Application Delegate
            //  for registered for remote notifications is called, and the device token is passed back to you
		    push.QueueNotification(
		        NotificationFactory.Apple()
		                           .ForDeviceToken("813a3e60dc1655baaa09cc78fe584fa18419a133f6d271f5d79272ad644b4a18")
		                           .WithAlert("1 Alert Text!")
		                           .WithSound("default")
		                           .WithBadge(7));
		    

		    //Fluent construction of an Android GCM Notification
            //IMPORTANT: For Android you MUST use your own RegistrationId here that gets generated within your Android app itself!
            /*push.QueueNotification(NotificationFactory.AndroidGcm()
                .ForDeviceRegistrationId("APA91bG7J-cZjkURrqi58cEd5ain6hzi4i06T0zg9eM2kQAprV-fslFiq60hnBUVlnJPlPV-4K7X39aHIe55of8fJugEuYMyAZSUbmDyima5ZTC7hn4euQ0Yflj2wMeTxnyMOZPuwTLuYNiJ6EREeI9qJuJZH9Zu9g")
                .WithCollapseKey("NONE")
                .WithJson("{\"alert\":\"Alert Text!\",\"badge\":\"7\"}"));

			push.QueueNotification(NotificationFactory.Windows()
				.Toast()
				.ForChannelUri("https://bn1.notify.windows.com/?token=AgUAAACC2u7flXAmaevcggrLenaSdExjVfIHvr6KSZrg0KeuGrcz877rPJprPL9bEuQH%2bacmmm%2beUyXNXEM8oRNit%2bzPoigksDOq6bIFyV3XGmhUmXadysLokl5rlmTscvHGAbs%3d")
				.WithRequestForStatus(true)
				.AsToastText01("This is a test!"));*/

			Console.WriteLine("Waiting for Queue to Finish...");

            Console.ReadLine();			
			//Stop and wait for the queues to drains
			push.StopAllServices(true);

			Console.WriteLine("Queue Finished, press return to exit...");
			Console.ReadLine();			
		}

        private static void HookUpAllEvents(PushService push)
        {
            push.Events.OnDeviceSubscriptionExpired += Events_OnDeviceSubscriptionExpired;
            push.Events.OnDeviceSubscriptionIdChanged +=
                new Common.ChannelEvents.DeviceSubscriptionIdChanged(Events_OnDeviceSubscriptionIdChanged);
            push.Events.OnChannelException += new Common.ChannelEvents.ChannelExceptionDelegate(Events_OnChannelException);
            push.Events.OnNotificationSendFailure +=
                new Common.ChannelEvents.NotificationSendFailureDelegate(Events_OnNotificationSendFailure);
            push.Events.OnNotificationSent += new Common.ChannelEvents.NotificationSentDelegate(Events_OnNotificationSent);
            push.Events.OnChannelCreated += new Common.ChannelEvents.ChannelCreatedDelegate(Events_OnChannelCreated);
            push.Events.OnChannelDestroyed += new Common.ChannelEvents.ChannelDestroyedDelegate(Events_OnChannelDestroyed);
        }

        static void Events_OnDeviceSubscriptionIdChanged(Common.PlatformType platform, string oldDeviceInfo, string newDeviceInfo, Common.Notification notification)
		{
			//Currently this event will only ever happen for Android GCM
			Console.WriteLine("Device Registration Changed:  Old-> " + oldDeviceInfo + "  New-> " + newDeviceInfo);
		}

		static void Events_OnNotificationSent(Common.Notification notification)
		{
			Console.WriteLine("Sent: " + notification.Platform.ToString() + " -> " + notification.ToString());
		}

		static void Events_OnNotificationSendFailure(Common.Notification notification, Exception notificationFailureException)
		{
			Console.WriteLine("Failure: " + notification.Platform.ToString() + " -> " + notificationFailureException.Message + " -> " + notification.ToString());
		}

		static void Events_OnChannelException(Exception exception, Common.PlatformType platformType, Common.Notification notification)
		{
			Console.WriteLine("Channel Exception: " + platformType.ToString() + " -> " + exception.ToString());
		}

		static void Events_OnDeviceSubscriptionExpired(Common.PlatformType platform, string deviceInfo, Common.Notification notification)
		{
			Console.WriteLine("Device Subscription Expired: " + platform.ToString() + " -> " + deviceInfo);
		}

		static void Events_OnChannelDestroyed(Common.PlatformType platformType, int newChannelCount)
		{
			Console.WriteLine("Channel Destroyed for: " + platformType.ToString() + " Channel Count: " + newChannelCount);
		}

		static void Events_OnChannelCreated(Common.PlatformType platformType, int newChannelCount)
		{
			Console.WriteLine("Channel Created for: " + platformType.ToString() + " Channel Count: " + newChannelCount);
		}
	}
}
