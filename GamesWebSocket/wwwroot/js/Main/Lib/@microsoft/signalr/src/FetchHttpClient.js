var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
import { AbortError, HttpError, TimeoutError } from "./Errors";
import { HttpClient, HttpResponse } from "./HttpClient";
import { LogLevel } from "./ILogger";
import { Platform, getGlobalThis, isArrayBuffer } from "./Utils";
export class FetchHttpClient extends HttpClient {
    constructor(logger) {
        super();
        this._logger = logger;
        if (typeof fetch === "undefined" || Platform.isNode) {
            const requireFunc = typeof __webpack_require__ === "function" ? __non_webpack_require__ : require;
            this._jar = new (requireFunc("tough-cookie")).CookieJar();
            if (typeof fetch === "undefined") {
                this._fetchType = requireFunc("node-fetch");
            }
            else {
                this._fetchType = fetch;
            }
            this._fetchType = requireFunc("fetch-cookie")(this._fetchType, this._jar);
        }
        else {
            this._fetchType = fetch.bind(getGlobalThis());
        }
        if (typeof AbortController === "undefined") {
            const requireFunc = typeof __webpack_require__ === "function" ? __non_webpack_require__ : require;
            this._abortControllerType = requireFunc("abort-controller");
        }
        else {
            this._abortControllerType = AbortController;
        }
    }
    send(request) {
        return __awaiter(this, void 0, void 0, function* () {
            if (request.abortSignal && request.abortSignal.aborted) {
                throw new AbortError();
            }
            if (!request.method) {
                throw new Error("No method defined.");
            }
            if (!request.url) {
                throw new Error("No url defined.");
            }
            const abortController = new this._abortControllerType();
            let error;
            if (request.abortSignal) {
                request.abortSignal.onabort = () => {
                    abortController.abort();
                    error = new AbortError();
                };
            }
            let timeoutId = null;
            if (request.timeout) {
                const msTimeout = request.timeout;
                timeoutId = setTimeout(() => {
                    abortController.abort();
                    this._logger.log(LogLevel.Warning, `Timeout from HTTP request.`);
                    error = new TimeoutError();
                }, msTimeout);
            }
            if (request.content === "") {
                request.content = undefined;
            }
            if (request.content) {
                request.headers = request.headers || {};
                if (isArrayBuffer(request.content)) {
                    request.headers["Content-Type"] = "application/octet-stream";
                }
                else {
                    request.headers["Content-Type"] = "text/plain;charset=UTF-8";
                }
            }
            let response;
            try {
                response = yield this._fetchType(request.url, {
                    body: request.content,
                    cache: "no-cache",
                    credentials: request.withCredentials === true ? "include" : "same-origin",
                    headers: Object.assign({ "X-Requested-With": "XMLHttpRequest" }, request.headers),
                    method: request.method,
                    mode: "cors",
                    redirect: "follow",
                    signal: abortController.signal,
                });
            }
            catch (e) {
                if (error) {
                    throw error;
                }
                this._logger.log(LogLevel.Warning, `Error from HTTP request. ${e}.`);
                throw e;
            }
            finally {
                if (timeoutId) {
                    clearTimeout(timeoutId);
                }
                if (request.abortSignal) {
                    request.abortSignal.onabort = null;
                }
            }
            if (!response.ok) {
                const errorMessage = yield deserializeContent(response, "text");
                throw new HttpError(errorMessage || response.statusText, response.status);
            }
            const content = deserializeContent(response, request.responseType);
            const payload = yield content;
            return new HttpResponse(response.status, response.statusText, payload);
        });
    }
    getCookieString(url) {
        let cookies = "";
        if (Platform.isNode && this._jar) {
            this._jar.getCookies(url, (e, c) => cookies = c.join("; "));
        }
        return cookies;
    }
}
function deserializeContent(response, responseType) {
    let content;
    switch (responseType) {
        case "arraybuffer":
            content = response.arrayBuffer();
            break;
        case "text":
            content = response.text();
            break;
        case "blob":
        case "document":
        case "json":
            throw new Error(`${responseType} is not supported.`);
        default:
            content = response.text();
            break;
    }
    return content;
}
