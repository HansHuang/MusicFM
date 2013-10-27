using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public static Weahter GetWeather(string cityCode = "")
        {
            var weatherObject = GetWeatherInfo(cityCode);
            if (weatherObject == null) return new Weahter();
            var info = weatherObject["weatherinfo"];
            var date = DateTime.Now;
            //var jsonIndex = HttpWebDealer.GetHtml("http://data.weather.com.cn/zsLenovo/" + cityCode + ".html", Encoding.UTF8);
            var weather = new Weahter
                {
                    CityName = info["city"],
                    CityEnName = info["city_en"],
                    CityID = info["cityid"],
                    PublishDate = info["date_y"],
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
            weather.LifeIndexes.Add(new LifeIndex
                {
                    Name = "48H 穿衣指数",
                    Hint = info["index48"],
                    Description = info["index48_d"]
                });
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
            return weather;
        } 
        #endregion

        #region GetWeatherInfo
        /// <summary>
        /// Get city weather info json dictionary
        /// </summary>
        /// <param name="cityCode">manually set value or get info from public IP automaticly</param>
        /// <returns></returns>
        public  static dynamic GetWeatherInfo(string cityCode = "")
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
            if (string.IsNullOrEmpty(cityCode.Trim())) cityCode = GetCityCode().Trim();
            if (!(new Regex(@"^\d{9}$").IsMatch(cityCode))) return null;

            //2. Get json data from web
            var urlInfo = "http://m.weather.com.cn/data/" + cityCode + ".html";
            var urlIndex = "http://data.weather.com.cn/zsLenovo/" + cityCode + ".html";
            string weatherInfo = "", weatherIndex = "";
            Task.Factory.StartNew(() =>
                {
                    weatherInfo = HttpWebDealer.GetHtml(urlInfo, Encoding.UTF8).Trim();
                });
            Task.Factory.StartNew(() =>
                {
                    weatherIndex = HttpWebDealer.GetHtml(urlIndex, Encoding.UTF8).Trim();
                });
            while (string.IsNullOrEmpty(weatherInfo) || string.IsNullOrEmpty(weatherIndex))
            {
                Thread.Sleep(50);
            }
            //3. Group the two jsons
            if (!weatherInfo.StartsWith("{") && !weatherInfo.EndsWith("}")) return null;
            if (!weatherIndex.StartsWith("{") && !weatherIndex.EndsWith("}")) return weatherInfo;
            var weatherJson = weatherInfo.Substring(0, weatherInfo.Length - 1) + "," +
                              weatherIndex.Substring(1, weatherIndex.Length - 1);

            return weatherJson;
        }
        #endregion

        #region GetCityCode(Private)
        /// <summary>
        /// Get City weather code from city name
        /// </summary>
        /// <param name="cityName">manually set value or get info from public IP automaticly</param>
        /// <returns></returns>
        private static string GetCityCode(string cityName = "")
        {
            if (string.IsNullOrEmpty(cityName.Trim())) cityName = CityIPHelper.GetCurrentCity();
            return ChinaWeatherCityCode.GetCode(cityName);
        }

        #endregion

    }

    #region Weahter Model class

    public class Weahter : WeatherBase
    {
        public string CityName { get; set; }
        public string CityEnName { get; set; }
        public string CityID { get; set; }
        public string PublishDate { get; set; }
        public string PublishTime { get; set; }
        public string PublishWeek { get; set; }
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

    public class LifeIndex
    {
        public string Name { get; set; }
        public string Hint { get; set; }
        public string Description { get; set; }
    }

    #endregion
}
