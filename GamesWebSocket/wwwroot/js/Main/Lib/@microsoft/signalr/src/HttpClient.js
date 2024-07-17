export class HttpResponse {
    constructor(statusCode, statusText, content) {
        this.statusCode = statusCode;
        this.statusText = statusText;
        this.content = content;
    }
}
export class HttpClient {
    get(url, options) {
        return this.send(Object.assign(Object.assign({}, options), { method: "GET", url }));
    }
    post(url, options) {
        return this.send(Object.assign(Object.assign({}, options), { method: "POST", url }));
    }
    delete(url, options) {
        return this.send(Object.assign(Object.assign({}, options), { method: "DELETE", url }));
    }
    getCookieString(url) {
        return "";
    }
}
