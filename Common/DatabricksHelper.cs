using Azure;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ResumableFileTransfer.Common
{
    public class DatabricksHelper
    {
        private static readonly string _clsFullName = typeof(DatabricksHelper).FullName;

        private readonly HttpClient _httpClient;
        private readonly HttpClient _sasHttpClient;
        private readonly int _timeOut;
        private readonly ILogHelper _log;

        public DatabricksHelper(HttpClient httpClient, HttpClient sasHttpClient, int timeOut, ILogHelper log)
        {
            this._httpClient = httpClient;
            this._sasHttpClient = sasHttpClient;
            this._timeOut = timeOut;
            this._log = log;
        }

        public async Task<long> ImportAsync(string sqlConnString, SqlBulkCopySetting sqlBulkCopySetting, string warehouseId, string sql, string catalog, string schema, string format, bool isMultiThread, int multiThreadCount = 1, bool convertType = false, int? rowLimit = null)
        {
            long rowCount = 0;
            try
            {
                var importStart = DateTime.Now;
                // Step 1: Run Databricks SQL
                var hasRowLimit = rowLimit.HasValue;
                if (hasRowLimit)
                {
                    sql = sql.TrimEnd(';', ' ');

                    if (sql.IndexOf("LIMIT", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        sql += " LIMIT :row_limit";
                    }
                }

                var payload = new
                {
                    warehouse_id = warehouseId,
                    catalog,
                    schema,
                    format = format,
                    disposition = "EXTERNAL_LINKS",
                    statement = sql,
                    parameters = hasRowLimit
                        ? new[]
                          {
                        new { name = "row_limit", value = rowLimit.Value.ToString(), type = "INT" }
                          }
                        : Array.Empty<object>()
                };

                this._log.WriteTextLog("\tCall API start");
                var jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                using (var postResp = await this._httpClient.PostAsync("/api/2.0/sql/statements/", content))
                {
                    postResp.EnsureSuccessStatusCode();
                    var json = JsonConvert.DeserializeObject<dynamic>(await postResp.Content.ReadAsStringAsync());
                    string statementId = json.statement_id ?? throw BaseHelper.CreateException(_clsFullName, new Exception("Missing statement_id"));

                    // Step 2: Loop Until SUCCEEDED
                    string state;
                    dynamic statusDoc = null;
                    var timeout = TimeSpan.FromSeconds(this._timeOut);
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                    do
                    {
                        await Task.Delay(1000);

                        if (stopwatch.Elapsed > timeout)
                        {
                            throw BaseHelper.CreateException(_clsFullName,
                                new TimeoutException($"Databricks query timeout after {timeout.TotalMinutes} minutes."));
                        }

                        using (var statusResp = await _httpClient.GetAsync($"/api/2.0/sql/statements/{statementId}"))
                        {
                            statusResp.EnsureSuccessStatusCode();
                            var statusJson = await statusResp.Content.ReadAsStringAsync();
                            statusDoc = JsonConvert.DeserializeObject<dynamic>(statusJson);
                            state = statusDoc.status?.state ?? "UNKNOWN";
                        }                            

                    } while (state == "PENDING" || state == "RUNNING");

                    stopwatch.Stop();

                    if (state != "SUCCEEDED")
                    {
                        throw BaseHelper.CreateException(_clsFullName,
                            new Exception($"Databricks query failed: {state}"));
                    }

                    // Step 3: Get external_links and schema
                    var resultNode = statusDoc.result ?? throw BaseHelper.CreateException(_clsFullName, new Exception("Missing result node"));
                    var externalLinks = resultNode.external_links;

                    var manifestNode = statusDoc.manifest ?? throw BaseHelper.CreateException(_clsFullName, new Exception("Missing manifest node"));
                    var schemaNode = manifestNode.schema ?? throw BaseHelper.CreateException(_clsFullName, new Exception("Missing schema node"));
                    var chunksNode = manifestNode.chunks ?? throw BaseHelper.CreateException(_clsFullName, new Exception("Missing chunks node"));

                    rowCount = manifestNode.total_row_count ?? throw BaseHelper.CreateException(_clsFullName, new Exception("Missing total_row_count"));

                    if (isMultiThread)
                    {
                        List<ChunkModel> chunkModels = await this.HandleChunksAsync(chunksNode, statementId);
                        this._log.WriteTextLog("\tCall API end");
                        if (chunkModels.Count > 0)
                        {                            
                            this._log.WriteTextLog($"\tMultithread download stream and convert to DataTable start");

                            var semaphore = new SemaphoreSlim(multiThreadCount);
                            try
                            {
                                var tasks = chunkModels.Select(async chunkModel =>
                                {
                                    await semaphore.WaitAsync();
                                    try
                                    {
                                        var start = DateTime.Now;
                                        DataTable dt = null;

                                        using (var response = await this._sasHttpClient.GetAsync(chunkModel.ExternalLink, HttpCompletionOption.ResponseHeadersRead))
                                        {
                                            response.EnsureSuccessStatusCode();
                                            using (var stream = await response.Content.ReadAsStreamAsync())
                                            {
                                                dt = await BaseHelper.ConvertStreamToDataTableAsync(stream, schemaNode, format, convertType);
                                                var end = DateTime.Now;
                                                chunkModel.ConvertSeconds = (end - start).TotalSeconds;
                                            }
                                        }

                                        chunkModel.ChunkDataTable = dt;
                                    }
                                    catch (Exception e)
                                    {
                                        throw BaseHelper.CreateException(_clsFullName, e);
                                    }
                                    finally
                                    {
                                        semaphore.Release();
                                    }
                                });

                                await Task.WhenAll(tasks);

                                #region Parallel.ForEachAsync is not supported here, because it's .net standard 2.0
                                //var parallelOptions = new ParallelOptions
                                //{
                                //    MaxDegreeOfParallelism = multiThreadCount
                                //};
                                //Parallel.ForEachAsync(chunkModels, parallelOptions, async (chunkModel, ct) =>
                                //{
                                //    var start = DateTime.Now;
                                //    DataTable dt = null;

                                //    //When you use the EXTERNAL_LINKS disposition, a shared access signature (SAS) URL is generated,
                                //    //which can be used to download the results directly from Azure storage.
                                //    //As a short-lived SAS token is embedded within this SAS URL, you should protect both the SAS URL and the SAS token.
                                //    //Because SAS URLs are already generated with embedded temporary SAS tokens, you must not set an Authorization header in the download requests.
                                //    using (var response = await this._sasHttpClient.GetAsync(chunkModel.ExternalLink, HttpCompletionOption.ResponseHeadersRead))
                                //    {
                                //        response.EnsureSuccessStatusCode();
                                //        using (System.IO.Stream stream = await response.Content.ReadAsStreamAsync())
                                //        {
                                //            dt = await BaseHelper.ConvertStreamToDataTableAsync(stream, schemaNode, format, convertType);
                                //            var end = DateTime.Now;
                                //            chunkModel.ConvertSeconds = (end - start).TotalSeconds;
                                //        }
                                //    }
                                //    chunkModel.ChunkDataTable = dt;

                                //}); 
                                #endregion

                                this._log.WriteTextLog($"\tMultithread download stream and convert to DataTable end");

                                this._log.WriteTextLog($"\tDataTable into DB start {DateTime.Now}");
                                await SqlBulkCopyHelper.WriteToServerAsync(chunkModels.Select(x => x.ChunkDataTable).ToList(), sqlConnString, sqlBulkCopySetting, this._timeOut, this._log);
                                this._log.WriteTextLog($"\tDataTable into DB end {DateTime.Now}");

                                this._log.WriteTextLog($"\ttotal_chunk_count: {chunkModels.Count};total_row_count: {chunkModels.Sum(x => x.RowCount)};total_byte_count: {chunkModels.Sum(x => x.ByteCount)};Timming: {(DateTime.Now - importStart).TotalMinutes}");
                            }
                            catch (Exception e)
                            {
                                throw BaseHelper.CreateException(_clsFullName, e);
                            }
                            finally
                            {
                                semaphore.Dispose();
                                //release memory
                                foreach (var cm in chunkModels)
                                {
                                    cm.ChunkDataTable?.Clear();
                                    cm.ChunkDataTable?.Dispose();
                                    cm.ChunkDataTable = null;
                                }
                                chunkModels.Clear();
                            }                            
                        }
                    }
                    else
                    {
                        // Step 4: Handle external_links
                        this._log.WriteTextLog($"\tDownload Stream and insert to DB start");
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConnString, SqlBulkCopyOptions.UseInternalTransaction))
                        {
                            bulkCopy.BulkCopyTimeout = this._timeOut;
                            bulkCopy.EnableStreaming = true;
                            bulkCopy.BatchSize = 10000;

                            SqlBulkCopyHelper.SetColumnMappings(bulkCopy, sqlBulkCopySetting);

                            await this.HandleExternalLinkAsync(externalLinks, schemaNode, format, bulkCopy);
                        }
                        this._log.WriteTextLog($"\tDownload Stream and insert to DB end");
                        this._log.WriteTextLog($"\tTiming: {(DateTime.Now - importStart).TotalMinutes}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }            

            return rowCount;
        }

        private async Task<List<ChunkModel>> HandleChunksAsync(dynamic chunks, string statementId)
        {
            List<ChunkModel> chunkModels = new List<ChunkModel>();
            try
            {
                foreach (var chunk in chunks)
                {
                    var chunkModel = new ChunkModel
                    {
                        ChunkIndex = chunk.chunk_index,
                        RowCount = chunk.row_count,
                        ByteCount = chunk.byte_count,
                        ConvertSeconds = 0
                    };
                    using (var currChunkResult = await this._httpClient.GetAsync($"/api/2.0/sql/statements/{statementId}/result/chunks/{chunkModel.ChunkIndex}"))
                    {
                        currChunkResult.EnsureSuccessStatusCode();
                        var currChunkJsonStr = await currChunkResult.Content.ReadAsStringAsync();
                        var currChunkJson = JsonConvert.DeserializeObject<dynamic>(currChunkJsonStr);
                        chunkModel.ExternalLink = currChunkJson.external_links[0].external_link;
                        chunkModels.Add(chunkModel);
                    }
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
            return chunkModels;
        }

        private async Task HandleExternalLinkAsync(dynamic externalLinks, dynamic schemaNode, string format, SqlBulkCopy bulkCopy)
        {
            if (externalLinks == null)
            {
                Console.WriteLine("No external links found. Query returned no data?");
                return;
            }
            try
            {
                foreach (var chunk in externalLinks)
                {
                    string link = chunk.external_link;
                    if (!string.IsNullOrEmpty(link))
                    {
                        //When you use the EXTERNAL_LINKS disposition, a shared access signature (SAS) URL is generated,
                        //which can be used to download the results directly from Azure storage.
                        //As a short-lived SAS token is embedded within this SAS URL, you should protect both the SAS URL and the SAS token.
                        //Because SAS URLs are already generated with embedded temporary SAS tokens, you must not set an Authorization header in the download requests.
                        using (var response = await this._sasHttpClient.GetAsync(link, HttpCompletionOption.ResponseHeadersRead))
                        {
                            response.EnsureSuccessStatusCode();
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                using (IDataReader dataReader = await DataReaderFactory.CreateAsync(format, stream, schemaNode))
                                {
                                    await SqlBulkCopyHelper.WriteToServerAsync(dataReader, bulkCopy, this._log);
                                }
                            }
                        }
                    }

                    string nextChunkInternalLink = chunk.next_chunk_internal_link;
                    if (!string.IsNullOrEmpty(nextChunkInternalLink))
                    {
                        using (var nextChunkResult = await this._httpClient.GetAsync(nextChunkInternalLink))
                        {
                            nextChunkResult.EnsureSuccessStatusCode();
                            var nextChunkJsonStr = await nextChunkResult.Content.ReadAsStringAsync();
                            var nextChunkJson = JsonConvert.DeserializeObject<dynamic>(nextChunkJsonStr);
                            var nextChunkExternalLinks = nextChunkJson.external_links;
                            await this.HandleExternalLinkAsync(nextChunkExternalLinks, schemaNode, format, bulkCopy);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw BaseHelper.CreateException(_clsFullName, ex);
            }
        }
    }

    public class ChunkModel
    {
        public int ChunkIndex { get; set; }
        public int RowCount { get; set; }
        public long ByteCount { get; set; }
        public double ConvertSeconds { get; set; }
        public string ExternalLink { get; set; }
        public DataTable ChunkDataTable { get; set; }
    }
}
