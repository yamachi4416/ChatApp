import { IHttpService, IQService, IDeferred, IPromise, IRequestShortcutConfig } from "angular";

interface IRoomPromise<T> extends IPromise<T> {
    rejected?: boolean;
}

abstract class HttpServiceBase {
    protected baseUrl: string;

    constructor(protected $http : IHttpService, protected $q : IQService) {

    }

    get defer() : IDeferred<any> {
        return this.$q.defer();
    }

    get promise() : IRoomPromise<any> {
        return this.defer.promise;
    }

    get reject(): IRoomPromise<any> {
        const d = this.defer;
        d.reject();
        const def : IRoomPromise<any> = d.promise.then(() => {}, () => {});
        def.rejected = true;
        return def;
    }

    all(...args: any[]) : IRoomPromise<any> {
        return this.$q.all(args);
    }

    httpGet(path: string, config?: IRequestShortcutConfig) : IRoomPromise<any> {
        return this.$http.get(`${this.baseUrl}${path}`, angular.extend({}, config))
            .then((res) => res.data);
    }

    httpPost(path: string, data?: any, config?: IRequestShortcutConfig) : IRoomPromise<any> {
        return this.$http.post(`${this.baseUrl}${path}`, data, angular.extend({}, config))
            .then((res) => res.data);
    }
}

export {
    IRoomPromise,
    HttpServiceBase
}
