using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CommonHelperLibrary.WEB
{
    /// <summary>
    /// Author : Hans.Huang
    /// Date : 2013-03-16
    /// Class : CityWeatherHelper
    /// Discription : Helper class for one China city real-time weather
    /// </summary>
    public class CityWeatherHelper
    {
        #region GetWeather
        /// <summary>
        /// Get weather(if failed to get weather, I will return null)
        /// </summary>
        /// <param name="cityCode">manually set value or get info from public IP automaticly</param>
        /// <returns></returns>
        public static Weather GetWeather(string cityCode = "")
        {
            var weatherObject = GetWeatherInfo(cityCode);
            if (weatherObject == null) return new Weather();
            var info = weatherObject["weatherinfo"];
            var date = DateTime.Now;

            var weather = new Weather
                {
                    CityName = info["city"],
                    CityEnName = info["city_en"].ToUpper(),
                    CityID = info["cityid"],
                    PublishDate = info["date_y"],
                    PublishChinaData = info["date"],
                    PublishTime = info["fchh"],
                    PublishWeek = info["week"],
                    WeatherImage = info["img_single"],
                    WeatherImageTitle = info["img_title_single"],
                    DaytimeWindDirection = info["fx1"],
                    NightWindDirection = info["fx2"],
                    //DressingIndex = info["index"],
                    //DressingDetail = info["index_d"],
                    //DressingIndexOfTomorrow = info["index48"],
                    //DressingDetailOfTomorrow = info["index48_d"],
                    LifeIndexes=new Collection<LifeIndex>(),

                    Temperature = info["temp1"],
                    TemperatureF = info["tempF1"],
                    WeatherDetail = info["weather1"],
                    DaytimeWeatherImage = info["img1"],
                    NightWehtherImage = info["img2"],
                    DaytimeWeatherImageTitle = info["img_title1"],
                    NightWehtherImageTitle = info["img_title2"],
                    Wind = info["wind1"],
                    //Date = date.Day + "th " + date.DayOfWeek,
                    Date="Today",

                    TomorrowWeather = new WeatherBase
                        {
                            Temperature = info["temp2"],
                            TemperatureF = info["tempF2"],
                            WeatherDetail = info["weather2"],
                            DaytimeWeatherImage = info["img3"],
                            NightWehtherImage = info["img4"],
                            DaytimeWeatherImageTitle = info["img_title3"],
                            NightWehtherImageTitle = info["img_title4"],
                            Wind = info["wind2"],
                            //Date = date.AddDays(1).Day + "th " + date.AddDays(1).DayOfWeek,
                            Date = date.AddDays(1).DayOfWeek.ToString(),
                        },
                    NextTwoDayWeather = new WeatherBase
                        {
                            Temperature = info["temp3"],
                            TemperatureF = info["tempF3"],
                            WeatherDetail = info["weather3"],
                            DaytimeWeatherImage = info["img5"],
                            NightWehtherImage = info["img6"],
                            DaytimeWeatherImageTitle = info["img_title5"],
                            NightWehtherImageTitle = info["img_title6"],
                            Wind = info["wind3"],
                            //Date = date.AddDays(2).Day + "th " + date.AddDays(2).DayOfWeek,
                            Date = date.AddDays(2).DayOfWeek.ToString(),
                        },
                    NextThreeDayWeather = new WeatherBase
                        {
                            Temperature = info["temp4"],
                            TemperatureF = info["tempF4"],
                            WeatherDetail = info["weather4"],
                            DaytimeWeatherImage = info["img7"],
                            NightWehtherImage = info["img8"],
                            DaytimeWeatherImageTitle = info["img_title7"],
                            NightWehtherImageTitle = info["img_title8"],
                            Wind = info["wind4"],
                            //Date = date.AddDays(3).Day + "th " + date.AddDays(3).DayOfWeek,
                            Date = date.AddDays(3).DayOfWeek.ToString(),
                        },
                    NextFourDayWeather = new WeatherBase
                        {
                            Temperature = info["temp5"],
                            TemperatureF = info["tempF5"],
                            WeatherDetail = info["weather5"],
                            DaytimeWeatherImage = info["img9"],
                            NightWehtherImage = info["img10"],
                            DaytimeWeatherImageTitle = info["img_title9"],
                            NightWehtherImageTitle = info["img_title10"],
                            Wind = info["wind5"],
                            //Date = date.AddDays(4).Day + "th " + date.AddDays(4).DayOfWeek,
                            Date = date.AddDays(4).DayOfWeek.ToString(),
                        },
                    NextFiveDayWeather = new WeatherBase
                        {
                            Temperature = info["temp6"],
                            TemperatureF = info["tempF6"],
                            WeatherDetail = info["weather6"],
                            DaytimeWeatherImage = info["img11"],
                            NightWehtherImage = info["img12"],
                            DaytimeWeatherImageTitle = info["img_title11"],
                            NightWehtherImageTitle = info["img_title12"],
                            Wind = info["wind6"],
                            //Date = date.AddDays(5).Day + "th " + date.AddDays(5).DayOfWeek,
                            Date = date.AddDays(5).DayOfWeek.ToString(),
                        }
                };

            //Add Life index
            weather.LifeIndexes.Add(new LifeIndex {Name = "穿衣指数", Hint = info["index"], Description = info["index_d"]});
            if (weatherObject.ContainsKey("zs"))
            {
                foreach (var index in weatherObject["zs"])
                {
                    weather.LifeIndexes.Add(new LifeIndex
                    {
                        Name = index["name"],
                        Hint = index["hint"],
                        Description = index["des"]
                    });
                }
            }
            if (weatherObject.ContainsKey("k")) 
            {
                string[] indexArr = weatherObject["k"]["k3"].Split(new[] { '?', '|' }, StringSplitOptions.RemoveEmptyEntries);
                weather.CurrentAirIndex = indexArr[indexArr.Length - 1];
            }
            return weather;
        } 
        #endregion

        #region GetWeatherInfo
        /// <summary>
        /// Get city weather info json dictionary
        /// </summary>
        /// <param name="cityCode">manually set value or get info from public IP automaticly</param>
        /// <returns></returns>
        public static dynamic GetWeatherInfo(string cityCode = "")
        {
            var jsonStr = GetWeatherJson(cityCode);
            if (string.IsNullOrEmpty(jsonStr)) return null;
            var jsonSerializer = new JavaScriptSerializer();
            return jsonSerializer.Deserialize<dynamic>(jsonStr);
        } 
        #endregion

        #region GetWeatherJson
        /// <summary>
        /// Get city weather info json string
        /// </summary>
        /// <param name="cityCode">manually set value or get info from public IP automaticly</param>
        /// <returns></returns>
        public static string GetWeatherJson(string cityCode = "")
        {
            //1. Get city code and its validation
            if (string.IsNullOrWhiteSpace(cityCode)) cityCode = GetCityCode();
            if (!(new Regex(@"^\d{9}$").IsMatch(cityCode))) return null;

            //2. Get json data from web
            var urlInfo = "http://data.weather.com.cn/forecast/" + cityCode + ".html";
            var urlIndex = "http://data.weather.com.cn/zsLenovo/" + cityCode + ".html";
            var urlAir = "http://mobile.weather.com.cn/data/air/" + cityCode + ".html";
            var urlAirRefer = "http://mobile.weather.com.cn/air/" + cityCode + ".html";
            string weatherInfo = "", weatherIndex = "", airIndex = "";
            var t1 = Task.Run(() =>
            {
                weatherInfo = HttpWebDealer.GetHtml(urlInfo, null, Encoding.UTF8).Trim();
            });
            var t2 = Task.Run(() =>
            {
                weatherIndex = HttpWebDealer.GetHtml(urlIndex, null, Encoding.UTF8).Trim();
            });
            var t3 = Task.Run(() =>
            {
                var header = new WebHeaderCollection {{"Referer", urlAirRefer}};
                airIndex = HttpWebDealer.GetHtml(urlAir, header, Encoding.UTF8).Trim();
            });
            Task.WaitAll(t1, t2, t3);
            //3. Group the three json string
            var weatherJson = weatherInfo;
            if (!weatherJson.StartsWith("{") || !weatherJson.EndsWith("}")) return null;
            if (weatherIndex.StartsWith("{") && weatherIndex.EndsWith("}")) {
                weatherJson = weatherJson.Substring(0, weatherJson.Length - 1) + "," +
                              weatherIndex.Substring(1, weatherIndex.Length - 1);
            }
            if (airIndex.StartsWith("{") && airIndex.EndsWith("}")) {
                weatherJson = weatherJson.Substring(0, weatherJson.Length - 1) + "," +
                              airIndex.Substring(1, airIndex.Length - 1);
            }
            return weatherJson;
        }
        #endregion

        #region GetCityCode(Private)
        /// <summary>
        /// Get City weather code from by IP automaticly
        /// </summary>
        /// <returns></returns>
        private static string GetCityCode()
        {
            const string url = "http://61.4.185.48:81/g/";
            var input = HttpWebDealer.GetHtml(url);
            //The input should be like: var ip="116.207.25.21";var id=101020100;
            //var ip = new Regex("ip=\"([^\"]+)\"").Groups[1].Value;
            return new Regex("id=(\\d{9})").Match(input).Groups[1].Value;
        }

        #endregion

    }

    #region Weahter Model class
    [Serializable]
    public class Weather : WeatherBase
    {
        public string CityName { get; set; }
        public string CityEnName { get; set; }
        public string CityID { get; set; }
        public string PublishDate { get; set; }
        public string PublishChinaData { get; set; }
        public string PublishTime { get; set; }
        public string PublishWeek { get; set; }
        public string CurrentAirIndex { get; set; }
        public string WeatherImage { get; set; }
        public string WeatherImageTitle { get; set; }
        public string DaytimeWindDirection { get; set; }
        public string NightWindDirection { get; set; }
        public ICollection<LifeIndex> LifeIndexes { get; set; }

        public WeatherBase TomorrowWeather { get; set; }
        public WeatherBase NextTwoDayWeather { get; set; }
        public WeatherBase NextThreeDayWeather { get; set; }
        public WeatherBase NextFourDayWeather { get; set; }
        public WeatherBase NextFiveDayWeather { get; set; }
    }

    [Serializable]
    public class WeatherBase
    {
        public string Temperature { get; set; }
        public string TemperatureF { get; set; }
        public string WeatherDetail { get; set; }
        public string DaytimeWeatherImage { get; set; }
        public string NightWehtherImage { get; set; }
        public string DaytimeWeatherImageTitle { get; set; }
        public string NightWehtherImageTitle { get; set; }
        public string Wind { get; set; }
        public string Date { get; set; }
    }

    [Serializable]
    public class LifeIndex
    {
        public string Name { get; set; }
        public string Hint { get; set; }
        public string Description { get; set; }
    }

    #endregion
}
