using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Media;
using Flurl.Http;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using DocumentFormat.OpenXml.Wordprocessing;

namespace MyApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Camera : ContentPage
    {
        public Camera()
        {
            InitializeComponent();
        }

        private async void CameraButton_Clicked(object sender, EventArgs e)
        {
            var photo = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions() { });

            if (photo != null)
                PhotoImage.Source = ImageSource.FromStream(() => { return photo.GetStream(); });

            byte[] photoByteArr;

            // Convert photo to array of bytes
            using (var memoryStream = new MemoryStream())
            {
                photo.GetStream().CopyTo(memoryStream);
                photo.Dispose();
                photoByteArr = memoryStream.ToArray();
            }

            Task<HttpResponseMessage> response = UploadAsFormDataContent("https://kyf-cloud-api.herokuapp.com/", photoByteArr);
            HttpResponseMessage httpResponse = response.Result;
            String responseBodyAsText = await httpResponse.Content.ReadAsStringAsync();
            responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine); // todo remove
            Console.WriteLine(responseBodyAsText);
        }

        public Task<HttpResponseMessage> UploadAsFormDataContent(string url, byte[] image)
        {
            MultipartFormDataContent form = new MultipartFormDataContent
            {
                { new ByteArrayContent(image, 0, image.Length), "image", "pic.jpeg" }
            };

            HttpClient client = new HttpClient();
            return client.PostAsync(url, form);
        }

    }
}