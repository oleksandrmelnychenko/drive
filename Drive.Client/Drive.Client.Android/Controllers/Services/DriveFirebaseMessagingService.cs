﻿using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Drive.Client.Droid.Models.Notiifactions;
using Drive.Client.Helpers;
using Firebase.Messaging;
using System;
using System.Diagnostics;
using System.Linq;

namespace Drive.Client.Droid.Controllers.Services {
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class DriveFirebaseMessagingService : FirebaseMessagingService {

        public override void OnMessageReceived(RemoteMessage message) {
            base.OnMessageReceived(message);

            Console.WriteLine("Received: {0}", message);

            try {
                ///
                /// TODO: handle incomming message parse message and invoke appropriate `shared project service`... 
                /// 

                try {
                    string jsonNotificationMessage = message.Data.Values.FirstOrDefault().ToString();
                    NotificationMessage notificationMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<NotificationMessage>(jsonNotificationMessage);

                    if (BaseSingleton<GlobalSetting>.Instance.UserProfile?.NetId == notificationMessage.UserNetId) {
                        ReleaseResidentVehicleNotification(jsonNotificationMessage);
                    }
                }
                catch (Exception exc) {
                    string messageExc = exc.Message;
                    Debugger.Break();
                    TODO_RELEASE_TEMPORARY_NOTIFICATION_FOR_DEV(exc.Message);
                }
            }
            catch (Exception exc) {
                Console.WriteLine("Error occured while receiving push-message. {0}", exc.Message);
                Debugger.Break();
            }
        }

        private void ReleaseResidentVehicleNotification(string jsonNotificationMessage) {
            try {
                NotificationCompat.Builder mBuilder = new Android.Support.V4.App.NotificationCompat.Builder(this)
                    .SetPriority(NotificationCompat.PriorityHigh)
                    .SetVibrate(new long[] { 1000, 2000, 3000 })
                    .SetSmallIcon(Resource.Mipmap.im_logo_d)
                    .SetContentTitle("Запит по фізичній особі")
                    .SetContentText("Оброблено")
                    .SetContentIntent(PendingIntent.GetActivity(this, 0, MainActivity.GetIntentWithParsedResidentVehicleDetail(this, jsonNotificationMessage), PendingIntentFlags.UpdateCurrent))
                    .SetAutoCancel(true);

                NotificationManager mNotificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
                mNotificationManager.Notify(0, mBuilder.Build());
            }
            catch (Exception exc) {
                Console.WriteLine("Cant build bar notification. {0}", exc.Message);
                Debugger.Break();
            }
        }

        private void TODO_RELEASE_TEMPORARY_NOTIFICATION_FOR_DEV(string contentText) {
            try {
                NotificationCompat.Builder mBuilder = new Android.Support.V4.App.NotificationCompat.Builder(this)
                    .SetPriority(NotificationCompat.PriorityHigh)
                    .SetVibrate(new long[] { 1000 })
                    .SetSmallIcon(Resource.Mipmap.im_logo_d)
                    .SetContentTitle(contentText)
                    .SetContentIntent(PendingIntent.GetActivity(this, 0, MainActivity.GetIntentWithParsedResidentVehicleDetail(this, "{ }"), PendingIntentFlags.UpdateCurrent))
                    .SetAutoCancel(true);

                NotificationManager mNotificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
                mNotificationManager.Notify(0, mBuilder.Build());
            }
            catch (Exception exc) {
                Console.WriteLine("TODO_RELEASE_TEMPORARY_NOTIFICATION_FOR_DEV cant build bar notification. {0}", exc.Message);
                Debugger.Break();
            }
        }
    }
}