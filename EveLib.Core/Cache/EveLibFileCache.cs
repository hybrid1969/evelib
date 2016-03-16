﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using eZet.EveLib.Core.Util;

namespace eZet.EveLib.Core.Cache {
    /// <summary>
    ///     Simple plain file cache implementation
    /// </summary>
    public class EveLibFileCache : IEveLibCache, IDisposable {
        private static readonly SHA1CryptoServiceProvider Sha1 = new SHA1CryptoServiceProvider();

        private readonly IDictionary<string, DateTime> _register = new Dictionary<string, DateTime>();

        private readonly ReaderWriterLockSlim _registerLock = new ReaderWriterLockSlim();

        private readonly TraceSource _trace = new TraceSource("EveLib");

        private bool _isInitialized;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EveLibFileCache" /> class.
        /// </summary>
        /// <param name="cachePath">The relative cache path.</param>
        /// <param name="cacheRegisterName">Name of the cache register.</param>
        public EveLibFileCache(string cachePath, string cacheRegisterName) {
            CachePath = cachePath;
            CacheRegister = Path.Combine(CachePath, cacheRegisterName);
        }

        /// <summary>
        ///     Gets the cache path.
        /// </summary>
        /// <value>The cache path.</value>
        public string CachePath { get; }

        /// <summary>
        ///     Gets the cache register.
        /// </summary>
        /// <value>The cache register.</value>
        public string CacheRegister { get; }

        /// <summary>
        ///     Stores data to the cache
        /// </summary>
        /// <param name="uri">The uri this caches</param>
        /// <param name="cacheTime">The cache expiry time</param>
        /// <param name="data">The data to cache</param>
        /// <returns></returns>
        public async Task StoreAsync(Uri uri, DateTime cacheTime, string data) {
            _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache.StoreAsync:Start");
            _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache:Uri: {0}", uri);
            _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache:Cache Expiry: {0}", cacheTime);

            var key = getHash(uri);
            _register[key] = cacheTime;
            if (!Directory.Exists(CachePath)) {
                _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache:Creating cache directory");
                Directory.CreateDirectory(CachePath);
            }
            try {
                var cacheTask = writeCacheDataToDiskAsync(uri, data);
                var registerTask = writeRegisterToDiskAsync();
                await Task.WhenAll(cacheTask, registerTask).ConfigureAwait(false);
            }
            catch (Exception) {
                _trace.TraceEvent(TraceEventType.Error, 0, "EveLibFileCache:An error occured while writing to cache");
            }
            _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache.StoreAsync:Complete");
        }

        /// <summary>
        ///     Loads data from cache
        /// </summary>
        /// <param name="uri">The uri to load cache for</param>
        /// <returns>The cached data</returns>
        public async Task<string> LoadAsync(Uri uri) {
            _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache.LoadAsync:Start");
            await initAsync().ConfigureAwait(false);
            string data = null;
            _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache:CacheRegisterLookupUri: {0}", uri);
            var hash = getHash(uri);
            _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache:CacheRegisterLookupHash: {0}", hash);
            DateTime cacheExpirationTime;
            var found = _register.TryGetValue(hash, out cacheExpirationTime);
            _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache:CacheRegisterHit: {0}", found);
            if (found) {
                var validCache = DateTime.UtcNow < cacheExpirationTime;
                _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache:CacheIsValid: {0} ({1})", validCache,
                    cacheExpirationTime);
                if (validCache) {
                    var filePath = Path.Combine(CachePath, hash);
                    var fileExist = File.Exists(filePath);
                    _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache:CacheDataFound: {0}", fileExist);
                    if (File.Exists(filePath)) {
                        try {
                            data =
                                await
                                    AsyncFileUtilities.ReadAllTextAsync(Path.Combine(CachePath, getHash(uri)))
                                        .ConfigureAwait(false);
                            _trace.TraceEvent(TraceEventType.Verbose, 0,
                                "EveLibFileCache:Data successfully loaded from cache: {0}",
                                filePath);
                        }
                        catch (Exception) {
                            _trace.TraceEvent(TraceEventType.Error, 0,
                                "EveLibFileCache:Cache data could not be loaded: {0}", filePath);
                        }
                    }
                }
            }
            _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache.LoadAsync:Complete");
            return data;
        }

        /// <summary>
        ///     Gets the cache expiry time for specified uri
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool TryGetExpirationDate(Uri uri, out DateTime value) {
            var key = getHash(uri);
            return _register.TryGetValue(key, out value);
        }

        /// <summary>
        ///     initialize as an asynchronous operation.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task initAsync() {
            if (_isInitialized) return;
            await loadRegisterFromDiskAsync().ConfigureAwait(false);
            _isInitialized = true;
        }

        private async Task writeRegisterToDiskAsync() {
            _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache:Writing cache register to disk");
            _registerLock.EnterWriteLock();
            try {
                await AsyncFileUtilities.WriteAllLinesAsync(CacheRegister,
                    _register.Select(x => x.Key + "," + x.Value.ToString(CultureInfo.InvariantCulture)))
                    .ConfigureAwait(false);
            }
            finally {
                if (_registerLock.IsWriteLockHeld) _registerLock.ExitWriteLock();
            }
        }

        private Task writeCacheDataToDiskAsync(Uri uri, string data) {
            _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache:Writing cache data to disk: {0}", uri);
            return AsyncFileUtilities.WriteAllTextAsync(Path.Combine(CachePath, getHash(uri)),
                data);
        }

        private static string getHash(Uri uri) {
            //return uri.AbsoluteUri;
            var fileName = uri.AbsoluteUri;
            var hash = Sha1.ComputeHash(Encoding.Unicode.GetBytes(fileName));
            return BitConverter.ToString(hash).Replace("-", "");
        }

        private async Task loadRegisterFromDiskAsync() {
            _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache:Loading cache register: {0}",
                CacheRegister);
            if (!Directory.Exists(CachePath)) {
                _trace.TraceEvent(TraceEventType.Warning, 0, "EveLibFileCache:Cache directory not found: {0}",
                    CachePath);
                return;
            }
            if (!File.Exists(CacheRegister)) {
                _trace.TraceEvent(TraceEventType.Warning, 0, "EveLibFileCache:Cache register not found: {0}",
                    CacheRegister);
                return;
            }
            //_registerLock.EnterReadLock();
            try {
                // read all lines
                var data = await
                    AsyncFileUtilities.ReadAllLinesAsync(CacheRegister).ConfigureAwait(false);
                foreach (var entry in data) {
                    var split = entry.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    var cacheValidUntil = DateTime.Parse(split[1], CultureInfo.InvariantCulture);
                    var fileName = split[0];
                    // if cache is still valid we register it
                    if (cacheValidUntil > DateTime.UtcNow)
                        _register[fileName] = cacheValidUntil;
                    else {
                        // if cache is out of date we delete the data
                        var file = Path.Combine(CachePath, fileName);
                        if (File.Exists(file)) {
                            File.Delete(file);
                        }
                    }
                }
                // delete all files that aren't listed in the register file
                var files = Directory.EnumerateFiles(CachePath);
                foreach (
                    var file in
                        files.Where(
                            file =>
                                !_register.ContainsKey(file.Replace(CachePath + Config.Separator, "")) &&
                                !file.Equals(CacheRegister))) {
                    File.Delete(file);
                }
                _trace.TraceEvent(TraceEventType.Verbose, 0, "EveLibFileCache:CacheRegisterLoaded");
            }
            catch (Exception) {
                _trace.TraceEvent(TraceEventType.Warning, 0, "EveLibFileCache:Could not load cache register");
            }
            finally {
                if (_registerLock.IsReadLockHeld) _registerLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="finalize"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool finalize) {
            _registerLock.Dispose();
        }
    }
}