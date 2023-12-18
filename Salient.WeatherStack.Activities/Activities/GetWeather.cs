using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using System.Security;
using System.Collections.Generic;
using Salient.WeatherStack.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Salient.WeatherStack.Activities
{
    [LocalizedDisplayName(nameof(Resources.GetWeather_DisplayName))]
    [LocalizedDescription(nameof(Resources.GetWeather_Description))]
    public class GetWeather : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.Timeout_DisplayName))]
        [LocalizedDescription(nameof(Resources.Timeout_Description))]
        public InArgument<int> TimeoutMS { get; set; } = 60000;

        [LocalizedDisplayName(nameof(Resources.GetWeather_APIKey_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetWeather_APIKey_Description))]
        [LocalizedCategory(nameof(Resources.Authentication_Category))]
        public InArgument<SecureString> APIKey { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetWeather_City_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetWeather_City_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> City { get; set; }

        [LocalizedDisplayName(nameof(Resources.GetWeather_Weather_DisplayName))]
        [LocalizedDescription(nameof(Resources.GetWeather_Weather_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<Dictionary<string, string>> Weather { get; set; }

        #endregion


        #region Constructors

        public GetWeather()
        {
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (APIKey == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(APIKey)));
            if (City == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(City)));

            base.CacheMetadata(metadata);
        }

        public string ToInsecureString(SecureString secureString)
        {
            if (secureString == null)
                throw new ArgumentNullException(nameof(secureString));

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }

        public static Dictionary<string, string> ConvertToDictionary(string jsonResponse) {
            
            dynamic json = JsonConvert.DeserializeObject(jsonResponse);

            string temperature = json.current.temperature.ToString();
            string humidity = json.current.humidity.ToString();
            Dictionary<string, string> weather = new Dictionary<string, string>
            {
                { "temperature", temperature },
                { "humidity", humidity }
            };

            return weather;
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Inputs
            var timeout = TimeoutMS.Get(context);
            var apiKey = APIKey.Get(context);
            var city = City.Get(context);

            string key = ToInsecureString(apiKey);
            // Set a timeout on the execution
            var task = ExecuteWithTimeout(context, cancellationToken);
            if (await Task.WhenAny(task, Task.Delay(timeout, cancellationToken)) != task) throw new TimeoutException(Resources.Timeout_Error);

            string response = await HttpGetAsync(key, city);

            Dictionary<string, string> output = ConvertToDictionary(response);

            // Outputs
            return (ctx) => {
                Weather.Set(ctx, output);
            };
        }


        public async static Task<string> HttpGetAsync(string apiKey, string city)
        {
            string content = null;

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"http://api.weatherstack.com/current?access_key={apiKey}&query={city}&units=f");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            //Console.WriteLine(await response.Content.ReadAsStringAsync());

            if (response.IsSuccessStatusCode)
            {
                content = await response.Content.ReadAsStringAsync();
            }

            return content;
        }

        private async Task ExecuteWithTimeout(AsyncCodeActivityContext context, CancellationToken cancellationToken = default)
        {
            ///////////////////////////
            // Add execution logic HERE
            ///////////////////////////

        }

        #endregion
    }
}

