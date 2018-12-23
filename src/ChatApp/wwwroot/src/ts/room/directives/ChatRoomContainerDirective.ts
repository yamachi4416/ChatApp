import { IDirective, IDirectiveFactory, ILocationService, IScope, IWindowService, IDeferred, IAugmentedJQuery, IAttributes, ITranscludeFunction } from "angular";
import { ITimeoutService, IRootScopeService, IQService } from "angular";
import { RoomServiceModel } from "../services/RoomServiceModel";
import { IRoomPromise } from "../services/HttpServiceBase";

interface IChatRoomContainerScope {
    chatPrefix: string;
    chatRoom: RoomServiceModel;
    chatRoomChange: (any?) => IRoomPromise<any | void>;
    oldMessages: (any?) => IRoomPromise<any | void>;
    chatContent: string;
    loadingTemplate: string;
}

export class ChatRoomContainerOperator {

    private isForceScrollBottom = true;

    private getOldMessagesEnable = true;

    private saveScrollMap = new Map<string, number>();

    public ajustMargin = 10;

    constructor(
        private scope: IChatRoomContainerScope,
        private $timeout: ITimeoutService,
        private $location: ILocationService,
        private $window: IWindowService,
        private $rootScope: IRootScopeService
    ) { }

    init() {
        this.postform.on('click', 'button', () => this.isForceScrollBottom = true);
        this.container.css({ 'opacity': '0' });
        this.registerHandles();
    }

    private get container() {
        return angular.element(`#${this.scope.chatPrefix}`);
    }

    private get content() {
        return this.container.find(this.scope.chatContent);
    }

    private get postform() {
        return angular.element(`#${this.scope.chatPrefix}-form`);
    }

    private justMessageSize() {
        this.$timeout(() => {
            var cm = this.container;
            var cf = this.postform;
            cm.css({ 'overflow-y': 'scroll' });
            cm.height(angular.element(this.$window).height() - cf.height() - cm.offset().top - 30);
        });
    }

    private restoreScroll(roomid: string, promise: IRoomPromise<any>) {
        if (!promise) return;
        const top = this.saveScrollMap.get(roomid);
        this.container.css({ 'opacity': '0' });
        return promise.finally(() => {
            this.justMessageSize();
            this.$timeout(() => {
                this.container
                    .scrollTop(top == null ? this.content.height() : top)
                    .animate({ 'opacity': '1' });
            });
        });
    }

    private keepScrollTop(promise: IRoomPromise<any>) {
        return promise && promise.then(() => {
            const oldHeight = this.content.height();
            return this.$timeout(() => {
                this.container.scrollTop(this.content.height() - oldHeight);
            });
        });
    }

    private getOldMessages() {
        if (this.getOldMessagesEnable && this.container.scrollTop() === 0) {
            const promise = this.scope.oldMessages();
            if (promise && !promise.rejected) {
                const loading = angular.element(this.scope.loadingTemplate);
                this.getOldMessagesEnable = false;
                this.container.prepend(loading as JQuery);
                return promise.finally(() => {
                    loading.remove();
                    this.getOldMessagesEnable = true;
                });
            }
        }
    }

    private registerHandles() {
        this.$rootScope.$on('$locationChangeSuccess', () => {
            return this.restoreScroll(this.$location.hash(), this.scope.chatRoomChange());
        });

        this.$rootScope.$on('chatRoomMessageReady', () => {
            angular.element(this.$window)
                .bind('resize', () => this.justMessageSize())
                .trigger('resize');

            this.container.bind('scroll', () => {
                if (!this.scope.chatRoom) return;
                this.saveScrollMap.set(this.scope.chatRoom.id, this.container.scrollTop());
                return this.keepScrollTop(this.getOldMessages());
            });

            this.restoreScroll(this.$location.hash(), this.scope.chatRoomChange());
        });

        this.$rootScope.$on('chatRoomMessageScrollBottom', (_, opts) => {
            const opt = angular.extend({}, opts);
            const oldTop = this.container.scrollTop();
            const oldHeight = this.content.height();

            if (opt.ifShowBottom && !this.isForceScrollBottom) {
                if (oldHeight - oldTop - this.ajustMargin > this.container.height()) {
                    return;
                }
            }

            this.isForceScrollBottom = false;
            this.$timeout(() => this.container.scrollTop(this.content.height() - this.container.height()));
        });
    }
}

export class ChatRoomContainerDirective implements IDirective {
    restrict = 'A';
    scope = {
        chatPrefix: '@',
        chatRoom: '<',
        chatRoomChange: '&',
        oldMessages: '&',
        chatContent: '@',
        loadingTemplate: '<'
    };
    link = this.linkfn.bind(this);

    constructor(
        private $timeout: ITimeoutService,
        private $location: ILocationService,
        private $rootScope: IRootScopeService,
        private $window: IWindowService,
    ) { }

    private linkfn(
        scope: IChatRoomContainerScope,
        instanceElement: IAugmentedJQuery,
        instanceAttributes: IAttributes,
        controller: {},
        transclude: ITranscludeFunction
    ): void {
        const operator = new ChatRoomContainerOperator(
            scope, this.$timeout, this.$location, this.$window, this.$rootScope
        );
        operator.init();
    }

    static get Factory(): IDirectiveFactory {
        const factory: IDirectiveFactory =
            ($timeout, $location, $rootScope, $window) =>
                new ChatRoomContainerDirective($timeout, $location, $rootScope, $window);
        factory.$inject = ['$timeout', '$location', '$rootScope', '$window'];
        return factory;
    }
}