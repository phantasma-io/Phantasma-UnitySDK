using System;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using PhantasmaPhoenix.Unity.Core.Logging;

namespace Phantasma.SDK
{
    public static class WebClient
    {
        public static int NoTimeout = 0;
        public static int DefaultTimeout = 30;
        public static int DefaultRetries = 0;
        private static long requestNumber = 0;
        private static object requestNumberLock = new object();
        private static long GetNextRequestNumber()
        {
            lock (requestNumberLock)
            {
                if (requestNumber == Int64.MaxValue)
                    requestNumber = 0;
                else
                    requestNumber++;
            }

            return requestNumber;
        }

        public class JsonRpcRequest
        {
            public string jsonrpc = "2.0";
            public string method;
            public string id;
            public object[] @params;

            public JsonRpcRequest(string method, object[] parameters, string id)
            {
                this.method = method;
                this.@params = parameters;
                this.id = id;
            }
        }
        public class JsonRpcError
        {
            public int code;
            public string message;
        }
        public class JsonRpcResponse<T>
        {
            public string jsonrpc;
            public string id;
            public T result;
            public JsonRpcError error;
        }
        
        private static string FormatError(UnityWebRequest request, string url, JsonRpcError parsedError = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine(request.error ?? "Unknown error");
            sb.AppendLine($"URL: {url}");
            sb.AppendLine($"Is connection error: {request.result == UnityWebRequest.Result.ConnectionError}");
            sb.AppendLine($"Is protocol error: {request.result == UnityWebRequest.Result.ProtocolError}");
            sb.AppendLine($"Is data processing error: {request.result == UnityWebRequest.Result.DataProcessingError}");
            sb.AppendLine($"Response code: {request.responseCode}");

            if (parsedError != null)
            {
                sb.AppendLine($"Error code: {parsedError.code}");
                sb.AppendLine($"Error message: {parsedError.message}");
            }

            return sb.ToString();
        }

        private static JsonRpcError TryParseJsonRpcError(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<JsonRpcResponse<object>>(json)?.error;
            }
            catch { return null; }
        }

        private static T ParseResponse<T>(string response)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)response;
            }

            return JsonConvert.DeserializeObject<T>(response);
        }

        public static IEnumerator RPCRequest<T>(string url, string method, int timeout, int retriesOnNetworkError, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback,
                                            Action<T> callback, params object[] parameters)
        {
            var requestNumber = GetNextRequestNumber().ToString();
            var rpcRequest = new JsonRpcRequest(method, parameters, requestNumber);
            var json = JsonConvert.SerializeObject(rpcRequest);

            Log.Write($"RPC request [{requestNumber}]\nurl: {url}\njson: {json}", Log.Level.Networking);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            DateTime startTime = DateTime.Now;

            UnityWebRequest request;
            for (; ; )
            {
                request = new UnityWebRequest(url, "POST");
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                if (timeout > 0)
                    request.timeout = timeout;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success || retriesOnNetworkError == 0)
                {
                    // success
                    break;
                }

                Log.Write($"RPC network error [{requestNumber}], {retriesOnNetworkError} retries left.", Log.Level.Networking);
                yield return new WaitForSeconds(1f);
                retriesOnNetworkError--;
            }

            TimeSpan responseTime = DateTime.Now - startTime;

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                var parsedRpcError = TryParseJsonRpcError(request.downloadHandler.text);
                var error = FormatError(request, url, parsedRpcError);
                Log.Write($"RPC error [{requestNumber}]\nResponse time: {responseTime.Seconds}.{responseTime.Milliseconds} sec\n{error}", Log.Level.Networking);
                errorHandlingCallback?.Invoke(EPHANTASMA_SDK_ERROR_TYPE.WEB_REQUEST_ERROR, error);
            }
            else
            {
                Log.Write($"RPC response [{requestNumber}]\nurl: {url}\nResponse time: {responseTime.Seconds}.{responseTime.Milliseconds} sec\n{request.downloadHandler.text}", Log.Level.Networking);

                try
                {
                    var stringResponse = request.downloadHandler.text;

                    var rpcResponse = JsonConvert.DeserializeObject<JsonRpcResponse<T>>(stringResponse);
                    if (rpcResponse != null && rpcResponse.result != null && rpcResponse.error == null)
                    {
                        callback?.Invoke(rpcResponse.result);
                    }
                    else
                    {
                        if (rpcResponse?.error != null)
                        {
                            Log.Write($"RPC response [{requestNumber}]\nurl: {url}\nError node found: {rpcResponse.error.message}", Log.Level.Networking);
                            errorHandlingCallback?.Invoke(EPHANTASMA_SDK_ERROR_TYPE.API_ERROR, rpcResponse.error.message);
                        }
                        else
                        {
                            errorHandlingCallback?.Invoke(EPHANTASMA_SDK_ERROR_TYPE.FAILED_PARSING_JSON, "Invalid or null response");
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Write($"RPC response [{requestNumber}]\nurl: {url}\nFailed to parse JSON: " + e.ToString(), Log.Level.Networking);
                    errorHandlingCallback?.Invoke(EPHANTASMA_SDK_ERROR_TYPE.FAILED_PARSING_JSON, "Failed to parse RPC response: \"" + e.Message + "\"");
                    yield break;
                }
            }

            yield break;
        }

        public static IEnumerator RESTGet<T>(string url, int timeout, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback, Action<T> callback)
        {
            UnityWebRequest request;

            var requestNumber = GetNextRequestNumber();
            Log.Write($"REST request [{requestNumber}]\nurl: {url}", Log.Level.Networking);

            request = new UnityWebRequest(url, "GET");
            request.downloadHandler = new DownloadHandlerBuffer();

            DateTime startTime = DateTime.Now;

            if (timeout > 0)
                request.timeout = timeout;
            
            yield return request.SendWebRequest();
            
            TimeSpan responseTime = DateTime.Now - startTime;

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                var error = FormatError(request, url);
                Log.Write($"REST error [{requestNumber}]\nResponse time: {responseTime.Seconds}.{responseTime.Milliseconds} sec\n{error}", Log.Level.Networking);
                errorHandlingCallback?.Invoke(EPHANTASMA_SDK_ERROR_TYPE.WEB_REQUEST_ERROR, error);
            }
            else
            {
                T response = default;
                try
                {
                    Log.Write($"REST response [{requestNumber}]\nurl: {url}\nResponse time: {responseTime.Seconds}.{responseTime.Milliseconds} sec\n{request.downloadHandler.text}", Log.Level.Networking);
                    response = ParseResponse<T>(request.downloadHandler.text);
                }
                catch(Exception e)
                {
                    Log.Write(e.Message);
                }
                callback(response);
            }

            yield break;
        }

        public static IEnumerator RESTPost<T>(string url, string serializedJson, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback, Action<T> callback)
        {
            UnityWebRequest request;

            var requestNumber = GetNextRequestNumber();
            Log.Write($"REST request (POST) [{requestNumber}]\nurl: {url}", Log.Level.Networking);

            Log.Write($"REST request (POST) [{requestNumber}]\nserializedJson: {serializedJson}", Log.Level.Debug1);

            request = new UnityWebRequest(url, "POST");

            byte[] data = Encoding.UTF8.GetBytes(serializedJson);
            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            DateTime startTime = DateTime.Now;
            yield return request.SendWebRequest();
            TimeSpan responseTime = DateTime.Now - startTime;

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                var error = FormatError(request, url);
                Log.Write($"REST error [{requestNumber}]\nResponse time: {responseTime.Seconds}.{responseTime.Milliseconds} sec\n{error}", Log.Level.Networking);
                errorHandlingCallback?.Invoke(EPHANTASMA_SDK_ERROR_TYPE.WEB_REQUEST_ERROR, error);
            }
            else
            {
                Log.Write($"REST response [{requestNumber}]\nurl: {url}\nResponse time: {responseTime.Seconds}.{responseTime.Milliseconds} sec\n{request.downloadHandler.text}", Log.Level.Networking);

                T response = default;
                try
                {
                    response = ParseResponse<T>(request.downloadHandler.text);
                }
                catch(Exception e)
                {
                    Log.Write(e.Message);
                }
                callback(response);
            }

            yield break;
        }

        public static IEnumerator Ping(string url, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback, Action<TimeSpan> callback)
        {
            UnityWebRequest request;

            var requestNumber = GetNextRequestNumber();
            Log.Write($"Ping url [{requestNumber}]: {url}", Log.Level.Networking);

            request = new UnityWebRequest(url, "GET");
            request.downloadHandler = new DownloadHandlerBuffer();

            DateTime startTime = DateTime.Now;
            yield return request.SendWebRequest();
            TimeSpan responseTime = DateTime.Now - startTime;

            // TODO return proper check later when PHA RPC would return something instead of 405 error code.
            // if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.DataProcessingError)
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                var error = FormatError(request, url);
                Log.Write($"Ping error error [{requestNumber}]\nResponse time: {responseTime.Seconds}.{responseTime.Milliseconds} sec\n{error}", Log.Level.Networking);
                errorHandlingCallback?.Invoke(EPHANTASMA_SDK_ERROR_TYPE.WEB_REQUEST_ERROR, error);
            }
            else
            {
                Log.Write($"Ping response [{requestNumber}]\nurl: {url}\nResponse time: {responseTime.Seconds}.{responseTime.Milliseconds} sec\n{request.downloadHandler.text}", Log.Level.Networking);
                callback(responseTime);
            }

            yield break;
        }
    }
}