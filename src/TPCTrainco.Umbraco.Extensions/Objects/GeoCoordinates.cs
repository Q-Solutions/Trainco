using GoogleMaps.LocationServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TPCTrainco.Umbraco.Extensions.Helpers;
using TPCTrainco.Umbraco.Extensions.Models;
using MaxMind.GeoIP2;
using System.Web;
using System.Configuration;
using Umbraco.Core.Persistence;

namespace TPCTrainco.Umbraco.Extensions.Objects
{
    public class GeoCoordinates
    {
        /// <summary>
        /// Get coordinate details (CoordinateDetails) from IP address
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static CoordinateDetails GetCoordinateDetailsFromIp(string ipAddress)
        {
            CoordinateDetails coordinateDetails = null;
            GeoLocationLookup geoLocation = null;

            using (var objClient = new WebDownload())
            {
                string json = null;

                objClient.Timeout = 8000;

                try
                {
                    // DEBUG
                    if (false == string.IsNullOrEmpty(ipAddress) && ipAddress.Length > 3 && ipAddress.Substring(0, 3) == "10.")
                    {
                        geoLocation = new GeoLocationLookup();

                        geoLocation.City = "Englewood";
                        geoLocation.RegionCode = "CO";
                        geoLocation.RegionName = "Colorado";
                        geoLocation.Ip = "38.109.197.34";
                        geoLocation.ZipCode = "80112";
                        geoLocation.Latitude = 39.538984;
                        geoLocation.Longitude = -104.8615189;
                        geoLocation.CountryCode = "US";
                        geoLocation.CountryName = "United States";
                    }
                    else if (ipAddress != "::1")
                    {
                        try
                        {
                            using (var reader = new DatabaseReader(HttpContext.Current.Server.MapPath("/ip-db/GeoLite2-City.mmdb")))
                            {
                                MaxMind.GeoIP2.Responses.CityResponse cityResponse = reader.City(ipAddress);

                                if (cityResponse != null)
                                {
                                    geoLocation = new GeoLocationLookup();

                                    geoLocation.City = cityResponse.City.Name;
                                    geoLocation.RegionCode = cityResponse.MostSpecificSubdivision.IsoCode;
                                    geoLocation.RegionName = cityResponse.MostSpecificSubdivision.Name;
                                    geoLocation.Ip = ipAddress;
                                    geoLocation.ZipCode = cityResponse.Postal.Code;
                                    geoLocation.Latitude = cityResponse.Location.Latitude ?? 0;
                                    geoLocation.Longitude = cityResponse.Location.Longitude ?? 0;
                                    geoLocation.CountryCode = cityResponse.Country.IsoCode;
                                    geoLocation.CountryName = cityResponse.Country.Name;
                                }
                            }
                        }
                        catch { }

                        if (geoLocation == null || true == string.IsNullOrEmpty(geoLocation.City))
                        {
                            json = objClient.DownloadString("http://api.db-ip.com/addrinfo?addr=" + ipAddress + "&api_key=723b0198c9093910bdcf5f0d91e1a845a2a38d97");
                            DbIpLookup dbIpLookup = JsonConvert.DeserializeObject<DbIpLookup>(json);

                            if (dbIpLookup != null)
                            {
                                geoLocation = new GeoLocationLookup();

                                geoLocation.City = dbIpLookup.City;
                                geoLocation.RegionCode = dbIpLookup.StateProv;
                                geoLocation.RegionName = dbIpLookup.StateProv;

                                if (false == string.IsNullOrEmpty(dbIpLookup.StateProv))
                                {
                                    if (dbIpLookup.StateProv.Length != 2)
                                    {
                                        State state = CacheObjects.GetStateList().Where(p => p.StateName == dbIpLookup.StateProv).FirstOrDefault();

                                        if (state != null)
                                        {
                                            geoLocation.RegionCode = state.StateAbbreviation;
                                        }
                                    }
                                    else
                                    {
                                        State state = CacheObjects.GetStateList().Where(p => p.StateAbbreviation == dbIpLookup.StateProv).FirstOrDefault();

                                        if (state != null)
                                        {
                                            geoLocation.RegionName = state.StateName;
                                        }
                                    }
                                }


                                geoLocation.Ip = dbIpLookup.Address;
                                geoLocation.CountryCode = dbIpLookup.Country;

                                if (false == string.IsNullOrEmpty(dbIpLookup.City))
                                {
                                    CoordinateDetails coordDetails = GetCoordinateDetailsFromCityState(dbIpLookup.City + "," + dbIpLookup.StateProv);

                                    if (coordDetails != null)
                                    {
                                        geoLocation.Latitude = coordDetails.Latitude;
                                        geoLocation.Longitude = coordDetails.Longitude;
                                    }
                                }
                            }
                        }

                        if (geoLocation == null || true == string.IsNullOrEmpty(geoLocation.City))
                        {
                            json = objClient.DownloadString("http://freegeoip.net/json/" + ipAddress);

                            geoLocation = JsonConvert.DeserializeObject<GeoLocationLookup>(json);
                        }
                    }
                }
                catch { }
            }

            if (geoLocation != null)
            {
                if (geoLocation.Latitude != 0)
                {
                    coordinateDetails = new CoordinateDetails();

                    MapPoint mapPoint = new MapPoint();

                    mapPoint.Latitude = geoLocation.Latitude;
                    mapPoint.Longitude = geoLocation.Longitude;

                    coordinateDetails = GetCoordinateDetailsFromLatLong(mapPoint);
                    coordinateDetails.City = geoLocation.City;
                    coordinateDetails.State = geoLocation.RegionName;
                    coordinateDetails.StateCode = geoLocation.RegionCode;

                    if (true == string.IsNullOrWhiteSpace(coordinateDetails.City) || true == string.IsNullOrWhiteSpace(coordinateDetails.State))
                    {
                        coordinateDetails = null;
                    }
                }
            }


            return coordinateDetails;
        }


        /// <summary>
        /// Get coordinate details (CoordinateDetails) from lat/long
        /// </summary>
        /// <param name="mapPoint"></param>
        /// <returns></returns>
        public static CoordinateDetails GetCoordinateDetailsFromLatLong(MapPoint mapPoint)
        {
            CoordinateDetails coordinateDetails = null;

            if (mapPoint != null)
            {
                coordinateDetails = new CoordinateDetails();

                coordinateDetails.Latitude = mapPoint.Latitude;
                coordinateDetails.Longitude = mapPoint.Longitude;

                coordinateDetails.DbGeography = DbGeography.FromText("POINT(" + coordinateDetails.Longitude + " " + coordinateDetails.Latitude + ")", 4326);
            }

            return coordinateDetails;
        }


        /// <summary>
        /// Get coordinate details (CoordinateDetails) from lat/long (or zip)
        /// </summary>
        /// <param name="cityState"></param>
        /// <returns></returns>
        public static CoordinateDetails GetCoordinateDetailsFromCityState(string cityState)
        {
            CoordinateDetails coordinateDetails = null;

            coordinateDetails = GetCoordinatesByCityStateLookup(cityState);

            if (coordinateDetails == null)
            {
                MapPoint mapPoint = GetCoordinatesFromCityState(cityState);

                coordinateDetails = GetCoordinateDetailsFromLatLong(mapPoint);
            }

            if (coordinateDetails == null)
            {
                coordinateDetails = GetCoordinateDetailsFromIp(HttpContext.Current.Request.UserHostAddress);
            }


            return coordinateDetails;
        }


        /// <summary>
        /// Get coordinates (MapPoint) from city/state
        /// </summary>
        /// <param name="cityState"></param>
        /// <returns></returns>
        public static MapPoint GetCoordinatesFromCityState(string cityState)
        {
            GoogleLocationService locationService = new GoogleLocationService();
            MapPoint mapPoint = null;
            string debug = "";

            try
            {
                mapPoint = locationService.GetLatLongFromAddress(cityState);

                if (mapPoint != null)
                {
                    debug += "mapPoint.Latitude: " + mapPoint.Latitude + "\r\n";
                    debug += "mapPoint.Longitude: " + mapPoint.Longitude + "\r\n";
                }
                else
                {
                    debug += "mapPoint IS NULL!\r\n";
                }
                //SendDebugEmail(debug, "TPCTrainco.com Google Location Service Debug");
            }
            catch (Exception ex)
            {
                SendDebugEmail(ex.ToString(), "TPCTrainco.com Google Location Service Error");
            }


            return mapPoint;
        }


        public static CoordinateDetails GetCoordinatesByCityStateLookup(string cityState)
        {
            CoordinateDetails coordinateDetails = null;

            string city = "";
            string state = "";
            string stateCode = "";

            if (cityState.IndexOf(",") >= 0)
            {
                List<string> cityStateArray = cityState.Split(',').ToList();

                city = cityStateArray[0].Trim();

                if (cityStateArray[1].Trim().Length == 2)
                {
                    stateCode = cityStateArray[1].Trim();
                }
                else
                {
                    state = cityStateArray[1].Trim();
                }
            }

            if (false == string.IsNullOrWhiteSpace(city))
            {
                LocationLookup locationLookup = null;

                using (Database db = new Database("umbracoDbDSN"))
                {
                    string sql = "";

                    if (false == string.IsNullOrWhiteSpace(stateCode))
                    {
                        sql = "SELECT * FROM CacheLocationLookup WHERE City = @city AND StateCode = @stateCode";

                        locationLookup = db.Query<LocationLookup>(sql, new { city = city, stateCode = stateCode }).FirstOrDefault();
                    }
                    else if (false == string.IsNullOrWhiteSpace(state))
                    {
                        sql = "SELECT * FROM CacheLocationLookup WHERE City = @city AND StateCode = @state";

                        locationLookup = db.Query<LocationLookup>(sql, new { city = city, state = state }).FirstOrDefault();
                    }

                    if (locationLookup == null)
                    {
                        sql = "SELECT * FROM CacheLocationLookup WHERE City = @city";

                        locationLookup = db.Query<LocationLookup>(sql, new { city = city }).FirstOrDefault();
                    }
                }

                if (locationLookup != null)
                {
                    coordinateDetails = new CoordinateDetails();

                    coordinateDetails.City = locationLookup.City;
                    coordinateDetails.DbGeography = DbGeography.FromText("POINT(" + locationLookup.Longitude + " " + locationLookup.Latitude + ")", 4326);
                    coordinateDetails.Latitude = locationLookup.Latitude;
                    coordinateDetails.Longitude = locationLookup.Longitude;
                    coordinateDetails.State = locationLookup.State;
                    coordinateDetails.StateCode = locationLookup.StateCode;
                }
            }

            return coordinateDetails;
        }


        /// <summary>
        /// Update all city coordinates in database
        /// </summary>
        /// <returns></returns>
        public static string UpdateCityCoordinates()
        {
            string result = null;
            StringBuilder sb = new StringBuilder();

            GoogleLocationService locationService = new GoogleLocationService();

            List<City> cityList = null;
            List<State> stateList = null;

            sb.AppendLine("Getting city list...");

            using (var db = new americantraincoEntities())
            {
                cityList = db.Set<City>().Where(p => p.CityName != "WEBINAR").ToList();
                stateList = db.Set<State>().ToList();
            }

            if (cityList != null && stateList != null && cityList.Count > 0)
            {
                sb.AppendLine("Cities found: " + cityList.Count);

                foreach (City city in cityList)
                {
                    sb.AppendLine("City: " + city.CityName);

                    if (city.Latitude == null || city.Longitude == null)
                    {
                        sb.AppendLine(" - Coordinates NOT found.");

                        State state = stateList.Where(p => p.StateID == city.StateID).FirstOrDefault();

                        if (state != null)
                        {
                            string address = city.CityName + ", " + state.StateName;

                            sb.AppendLine(" - Looking for coordinates for: " + address);

                            MapPoint mapPoint = GetCoordinatesFromCityState(address);

                            if (mapPoint != null)
                            {
                                CoordinateDetails coordinateDetails = GetCoordinateDetailsFromLatLong(mapPoint);

                                if (coordinateDetails != null)
                                {
                                    city.Latitude = coordinateDetails.Latitude;
                                    city.Longitude = coordinateDetails.Longitude;

                                    sb.AppendLine(" - Coordinates - Lat: " + city.Latitude + " Lon: " + city.Longitude);

                                    city.Coordinates = coordinateDetails.DbGeography;

                                    using (var dbContext = new americantraincoEntities())
                                    {
                                        dbContext.Entry(city).State = System.Data.Entity.EntityState.Modified;

                                        sb.AppendLine(" - Saving record...");

                                        dbContext.SaveChanges();

                                        sb.AppendLine(" - Saved.");
                                    }
                                }
                                else
                                {
                                    sb.AppendLine(" - Coordinate lookup failed.");
                                }
                            }
                            else
                            {
                                sb.AppendLine(" - Coordinate lookup failed.");
                            }
                        }
                    }
                    else
                    {
                        sb.AppendLine(" - Coordinates found.");
                    }

                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("No cities found.");
            }

            Debug.WriteLine(sb.ToString());

            return result;
        }


        private static void SendDebugEmail(string emailBody, string subject = "")
        {
            List<string> emailToList = null;

            if (ConfigurationManager.AppSettings["LogToEmail:CCError"] != null && ConfigurationManager.AppSettings.Get("LogToEmail:CCError").Length > 0)
            {
                emailToList = new List<string>();

                emailToList = ConfigurationManager.AppSettings.Get("LogToEmail:CCError").Split(';').ToList();

                Helpers.Email email = new Email();

                email.EmailFrom = "website@tpctrainco.com";
                email.EmailToList = emailToList;
                if (false == string.IsNullOrEmpty(subject))
                {
                    email.Subject = subject;
                }
                else
                {
                    email.Subject = "TPCTrainco.com Registration Error";
                }
                email.IsBodyHtml = false;

                string body = emailBody;

                body += "\r\n\r\n";
                body += HttpContext.Current.Request.Url.AbsoluteUri;
                body += "\r\n\r\n";
                body += HttpContext.Current.Request.UserHostAddress;
                body += "\r\n\r\n";
                body += HttpContext.Current.Request.UserAgent;
                body += "\r\n\r\n";

                email.Body = body;

                email.SendEmail();
            }
        }
    }
}
