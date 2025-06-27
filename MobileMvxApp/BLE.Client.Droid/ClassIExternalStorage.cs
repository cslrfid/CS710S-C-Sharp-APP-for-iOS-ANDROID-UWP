using Android.App;
using Android.Media;
using Java.IO;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using Android.Content;
using Java.IO;
using Android.Provider;
using Android.OS;
using MvvmCross.Platforms.Android;
using MvvmCross;

[assembly: Xamarin.Forms.Dependency(typeof(BLE.Client.Droid.AndroidImplementation))]
namespace BLE.Client.Droid
{

    public class AndroidImplementation : IExternalStorage
    {
        public string GetPath()
        {
            Android.Content.Context context = Android.App.Application.Context;
            var filePath = context.GetExternalFilesDir("");
            return filePath.Path;
        }

        public void SaveTextFileToDocuments(string fileName, string content, int fileType)
        {
            var topActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                var resolver = topActivity.ContentResolver;
                ContentValues values = new ContentValues();
                values.Put(MediaStore.IMediaColumns.DisplayName, fileName);
                switch (fileType)
                {
                    case 0:
                        values.Put(MediaStore.IMediaColumns.MimeType, "application/json");
                        break;
                    case 1:
                    case 2:
                        values.Put(MediaStore.IMediaColumns.MimeType, "text/csv");
                        break;
                    default:
                        values.Put(MediaStore.IMediaColumns.MimeType, "text/plain");
                        break;
                }
                values.Put(MediaStore.IMediaColumns.RelativePath, "Documents/");

                Android.Net.Uri collection = MediaStore.Files.GetContentUri("external");
                Android.Net.Uri fileUri = resolver.Insert(collection, values);

                using (var outputStream = resolver.OpenOutputStream(fileUri))
                using (var writer = new StreamWriter(outputStream))
                {
                    writer.Write(content);
                    writer.Flush();
                }
            }
            else
            {
                var documentsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
                var filePath = Path.Combine(documentsPath, fileName);

                if (!Directory.Exists(documentsPath))
                    Directory.CreateDirectory(documentsPath);

                System.IO.File.WriteAllText(filePath, content);
            }
        }
    }
}