﻿using System;
using System.Threading.Tasks;
using eZet.EveLib.Core.Serializers;
using eZet.EveLib.Modules.Models.Entities;
using eZet.EveLib.Modules.Models.Resources;
using eZet.EveLib.Modules.RequestHandlers;

namespace eZet.EveLib.Modules {


    /// <summary>
    /// Enum Crest Access Mode
    /// </summary>
    public enum CrestMode {
        /// <summary>
        /// Public CREST
        /// </summary>
        Public,
        /// <summary>
        /// Authenticated CREST. This requires a valid AccessToken
        /// </summary>
        Authenticated
    }


    /// <summary>
    ///     Provides access to the Eve Online CREST API.
    /// </summary>
    public class EveCrest {
        /// <summary>
        ///     The default URI used to access the public CREST API. This can be overridded by setting the BasePublicUri.
        /// </summary>
        public const string DefaultPublicUri = "https://public-crest.eveonline.com/";


        /// <summary>
        ///     The default URI used to access the authenticated CREST API. This can be overridded by setting the BaseAuthUri.
        /// </summary>
        public const string DefaultAuthUri = "https://crest-tq.eveonline.com/";

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="publicUri"></param>
        /// <param name="requestHandler"></param>
        protected EveCrest(string publicUri, ICrestRequestHandler requestHandler) {
            RequestHandler = requestHandler;
            BasePublicUri = publicUri;
        }

        /// <summary>
        ///     Creates a new EveCrest object with a default request handler
        /// </summary>
        public EveCrest() {
            RequestHandler = new CrestRequestHandler(new JsonSerializer());
            BasePublicUri = DefaultPublicUri;
            BaseAuthUri = DefaultAuthUri;
        }

        /// <summary>
        ///     Gets or sets the base URI used to access the public CREST API. This should include a trailing backslash.
        /// </summary>
        public string BasePublicUri { get; set; }

        /// <summary>
        ///     Gets or sets the base URI used to access the authed CREST API. This should include a trailing backslash.
        /// </summary>
        public string BaseAuthUri { get; set; }

        /// <summary>
        /// Gets or sets the CREST Access Token
        /// The Access Token is the final token acquired through the SSO login process, and should be managed by client code.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the CREST access mode.
        /// </summary>
        public CrestMode Mode { get; set; }

        /// <summary>
        ///     Gets or sets the request handler used by this instance
        /// </summary>
        public ICrestRequestHandler RequestHandler { get; set; }


        /// <summary>
        ///     Gets or sets the relative path to the API base.
        /// </summary>
        public string ApiPath { get; set; }

        /// <summary>
        /// Loads a CREST resource.
        /// </summary>
        /// <typeparam name="T">The resource type, usually inferred from the parameter</typeparam>
        /// <param name="uri">The Href that should be loaded</param>
        /// <returns></returns>
        public async Task<T> Load<T>(CrestHref<T> uri) where T : ICrestResource {
            return await RequestHandler.RequestAsync<T>(new Uri(uri.Uri), AccessToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads a crest resource
        /// </summary>
        /// <typeparam name="T">The resource type, usually inferred from the parameter</typeparam>
        /// <param name="entity">The entity that should be loaded</param>
        /// <returns></returns>
        public async Task<T> Load<T>(ICrestLinkedEntity<T> entity) where T : ICrestResource {
            return await RequestHandler.RequestAsync<T>(new Uri(entity.Href.Uri), AccessToken).ConfigureAwait(false);
        }


        /// <summary>
        ///     Returns the CREST root
        ///     Path: /
        /// </summary>
        /// <returns></returns>
        public Task<CrestRoot> GetRootAsync() {
            const string relPath = "";
            return requestAsync<CrestRoot>(relPath);
        }

        /// <summary>
        ///     Returns the CREST root
        ///     Path: /
        /// </summary>
        /// <returns></returns>
        public CrestRoot GetRoot() {
            return GetRootAsync().Result;
        }

        /// <summary>
        ///     Returns data on the specified killmail.
        ///     Path: /killmails/$warId/$hash/
        /// </summary>
        /// <param name="id">Killmail ID</param>
        /// <param name="hash">Killmail hash</param>
        /// <returns>Returns data for the specified killmail.</returns>
        public Task<CrestKillmail> GetKillmailAsync(long id, string hash) {
            string relPath = "killmails/" + id + "/" + hash + "/";
            return requestAsync<CrestKillmail>(relPath);
        }

        /// <summary>
        ///     Returns data on the specified killmail.
        ///     Path: /killmails/$warId/$hash/
        /// </summary>
        /// <param name="id">Killmail ID</param>
        /// <param name="hash">Killmail hash</param>
        /// <returns>Returns data for the specified killmail.</returns>
        public CrestKillmail GetKillmail(long id, string hash) {
            return GetKillmailAsync(id, hash).Result;
        }

        /// <summary>
        ///     Returns a list of all active incursions.
        ///     Path: /incursions/
        /// </summary>
        /// <returns>A list of all active incursions.</returns>
        public Task<CrestIncursionCollection> GetIncursionsAsync() {
            const string relPath = "incursions/";
            return requestAsync<CrestIncursionCollection>(relPath);
        }

        /// <summary>
        ///     Returns a list of all active incursions.
        ///     Path: /incursions/
        /// </summary>
        /// <returns>A list of all active incursions.</returns>
        public CrestIncursionCollection GetIncursions() {
            return GetIncursionsAsync().Result;
        }

        /// <summary>
        ///     Returns a list of all alliances.
        ///     Path: /alliances/
        /// </summary>
        /// <param name="page">The 1-indexed page to return. Number of total pages is available in the repsonse.</param>
        /// <returns>A list of all alliances.</returns>
        public Task<CrestAllianceCollection> GetAlliancesAsync(int page = 1) {
            string relPath = "alliances/?page=" + page;
            return requestAsync<CrestAllianceCollection>(relPath);
        }

        /// <summary>
        ///     Returns a list of all alliances.
        ///     Path: /alliances/
        /// </summary>
        /// <param name="page">The 1-indexed page to return. Number of total pages is available in the repsonse.</param>
        /// <returns>A list of all alliances.</returns>
        public CrestAllianceCollection GetAlliances(int page = 1) {
            return GetAlliancesAsync(page).Result;
        }

        /// <summary>
        ///     Returns data about a specific alliance.
        ///     Path: /alliances/$allianceId/
        /// </summary>
        /// <param name="allianceId">A valid alliance ID</param>
        /// <returns>Data for specified alliance</returns>
        public Task<CrestAlliance> GetAllianceAsync(long allianceId) {
            string relPath = "alliances/" + allianceId + "/";
            return requestAsync<CrestAlliance>(relPath);
        }

        /// <summary>
        ///     Returns data about a specific alliance.
        ///     Path: /alliances/$allianceId/
        /// </summary>
        /// <param name="allianceId">A valid alliance ID</param>
        /// <returns>Data for specified alliance</returns>
        public CrestAlliance GetAlliance(long allianceId) {
            return GetAllianceAsync(allianceId).Result;
        }

        /// <summary>
        ///     Returns daily price and volume history for a specific region and item type.
        ///     Path: /market/$regionId/types/$typeId/history/
        /// </summary>
        /// <param name="regionId">Region ID</param>
        /// <param name="typeId">Type ID</param>
        /// <returns>Market history for the specified region and type.</returns>
        public Task<CrestMarketHistory> GetMarketHistoryAsync(int regionId, int typeId) {
            string relPath = "market/" + regionId + "/types/" + typeId + "/history/";
            return requestAsync<CrestMarketHistory>(relPath);
        }

        /// <summary>
        ///     Returns daily price and volume history for a specific region and item type.
        ///     Path: /market/$regionId/types/$typeId/history/
        /// </summary>
        /// <param name="regionId">Region ID</param>
        /// <param name="typeId">Type ID</param>
        /// <returns>Market history for the specified region and type.</returns>
        public CrestMarketHistory GetMarketHistory(int regionId, int typeId) {
            return GetMarketHistoryAsync(regionId, typeId).Result;
        }

        /// <summary>
        ///     Returns the average and adjusted values for all items
        ///     Path: /market/prices/
        /// </summary>
        /// <returns></returns>
        public Task<CrestMarketTypePriceCollection> GetMarketPricesAsync() {
            const string relpath = "market/prices/";
            return requestAsync<CrestMarketTypePriceCollection>(relpath);
        }

        /// <summary>
        ///     Returns the average and adjusted values for all items
        ///     Path: /market/prices/
        /// </summary>
        /// <returns></returns>
        public CrestMarketTypePriceCollection GetMarketPrices() {
            return GetMarketPricesAsync().Result;
        }

        /// <summary>
        ///     Returns a list of all wars.
        ///     Path: /wars/
        /// </summary>
        /// <param name="page">The 1-indexed page to return. Number of total pages is available in the repsonse.</param>
        /// <returns>A list of all wars.</returns>
        public Task<CrestWarCollection> GetWarsAsync(int page = 1) {
            string relPath = "/wars/?page=" + page;
            return requestAsync<CrestWarCollection>(relPath);
        }

        /// <summary>
        ///     Returns a list of all wars.
        ///     Path: /wars/
        /// </summary>
        /// <param name="page">The 1-indexed page to return. Number of total pages is available in the repsonse.</param>
        /// <returns>A list of all wars.</returns>
        public CrestWarCollection GetWars(int page = 1) {
            return GetWarsAsync(page).Result;
        }

        /// <summary>
        ///     Returns data for a specific war.
        ///     Path: /wars/$warId/
        /// </summary>
        /// <param name="warId">CrestWar ID</param>
        /// <returns>Data for the specified war.</returns>
        public Task<CrestWar> GetWarAsync(int warId) {
            string relPath = "wars/" + warId + "/";
            return requestAsync<CrestWar>(relPath);
        }

        /// <summary>
        ///     Returns data for a specific war.
        ///     Path: /wars/$warId/
        /// </summary>
        /// <param name="warId">CrestWar ID</param>
        /// <returns>Data for the specified war.</returns>
        public CrestWar GetWar(int warId) {
            return GetWarAsync(warId).Result;
        }

        /// <summary>
        ///     Returns a list of all killmails related to a specified war.
        ///     Path: /wars/$warId/killmails/all/
        /// </summary>
        /// <param name="warId">CrestWar ID</param>
        /// <returns>A list of all killmails related to the specified war.</returns>
        public Task<CrestKillmailCollection> GetWarKillmailsAsync(int warId) {
            string relPath = "wars/" + warId + "/killmails/all/";
            return requestAsync<CrestKillmailCollection>(relPath);
        }

        /// <summary>
        ///     Returns a list of all killmails related to a specified war.
        ///     Path: /wars/$warId/killmails/all/
        /// </summary>
        /// <param name="warId">CrestWar ID</param>
        /// <returns>A list of all killmails related to the specified war.</returns>
        public CrestKillmailCollection GetWarKillmails(int warId) {
            return GetWarKillmailsAsync(warId).Result;
        }

        /// <summary>
        ///     Returns a list of all industry specialities
        ///     Path: /industry/specialities/
        /// </summary>
        /// <returns>A list of all industry specialities</returns>
        public Task<CrestIndustrySpecialityCollection> GetSpecialitiesAsync() {
            const string relPath = "industry/specialities/";
            return requestAsync<CrestIndustrySpecialityCollection>(relPath);
        }

        /// <summary>
        ///     Returns a list of all industry specialities
        ///     Path: /industry/specialities/
        /// </summary>
        /// <returns>A list of all industry specialities</returns>
        public CrestIndustrySpecialityCollection GetSpecialities() {
            return GetSpecialitiesAsync().Result;
        }

        /// <summary>
        ///     Returns details for the requested speciality
        /// </summary>
        /// <param name="specialityId">Speciality ID</param>
        /// <returns></returns>
        public Task<CrestIndustrySpeciality> GetSpecialityAsync(int specialityId) {
            string relPath = "industry/specialities/" + specialityId + "/";
            return requestAsync<CrestIndustrySpeciality>(relPath);
        }

        /// <summary>
        ///     Returns details for the requested speciality
        /// </summary>
        /// <param name="specialityId">Speciality ID</param>
        /// <returns></returns>
        public CrestIndustrySpeciality GetSpeciality(int specialityId) {
            return GetSpecialityAsync(specialityId).Result;
        }


        /// <summary>
        ///     Returns a list of all industry teams
        /// </summary>
        /// <returns>A list of all industry teams</returns>
        public Task<CrestIndustryTeamCollection> GetIndustryTeamsAsync() {
            const string relPath = "industry/teams/";
            return requestAsync<CrestIndustryTeamCollection>(relPath);
        }

        /// <summary>
        ///     Returns a list of all industry teams
        /// </summary>
        /// <returns>A list of all industry teams</returns>
        public CrestIndustryTeamCollection GetIndustryTeams() {
            return GetIndustryTeamsAsync().Result;
        }

        /// <summary>
        ///     Returns data for the specified industry team
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <returns></returns>
        public Task<CrestIndustryTeam> GetIndustryTeamAsync(int teamId) {
            string relPath = "industry/teams/" + teamId + "/";
            return requestAsync<CrestIndustryTeam>(relPath);
        }

        /// <summary>
        ///     Returns data for the specified industry team
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <returns></returns>
        public CrestIndustryTeam GetIndustryTeam(int teamId) {
            return GetIndustryTeamAsync(teamId).Result;
        }


        /// <summary>
        ///     Returns a list of industry systems and prices
        /// </summary>
        /// <returns></returns>
        public Task<CrestIndustrySystemCollection> GetIndustrySystemsAsync() {
            const string relPath = "industry/systems/";
            return requestAsync<CrestIndustrySystemCollection>(relPath);
        }

        /// <summary>
        ///     Returns a list of industry systems and prices
        /// </summary>
        /// <returns></returns>
        public CrestIndustrySystemCollection GetIndustrySystems() {
            return GetIndustrySystemsAsync().Result;
        }

        /// <summary>
        ///     Returns a list of all current industry team auctions
        /// </summary>
        /// <returns>A list of all current industry team auctions</returns>
        public Task<CrestIndustryTeamAuction> GetIndustryTeamAuctionsAsync() {
            const string relPath = "industry/teams/auction/";
            return requestAsync<CrestIndustryTeamAuction>(relPath);
        }

        /// <summary>
        ///     Returns a list of all current industry team auctions
        /// </summary>
        /// <returns>A list of all current industry team auctions</returns>
        public CrestIndustryTeamAuction GetIndustryTeamAuction() {
            return GetIndustryTeamAuctionsAsync().Result;
        }

        /// <summary>
        ///     Returns a collection of all industry facilities
        /// </summary>
        /// <returns></returns>
        public Task<CrestIndustryFacilityCollection> GetIndustryFacilitiesAsync() {
            const string relPath = "industry/facilities/";
            return requestAsync<CrestIndustryFacilityCollection>(relPath);
        }

        /// <summary>
        ///     Returns a collection of all industry facilities
        /// </summary>
        /// <returns></returns>
        public CrestIndustryFacilityCollection GetIndustryFacilities() {
            return GetIndustryFacilitiesAsync().Result;
        }

        /// <summary>
        ///     Performs a request using the request handler.
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="relPath">Relative path</param>
        /// <returns></returns>
        protected Task<T> requestAsync<T>(string relPath) where T : ICrestResource {
            if (Mode == CrestMode.Authenticated) {
                return RequestHandler.RequestAsync<T>(new Uri(BaseAuthUri + ApiPath + relPath), AccessToken);
            }
            return RequestHandler.RequestAsync<T>(new Uri(BasePublicUri + ApiPath + relPath), null);
        }
    }
}