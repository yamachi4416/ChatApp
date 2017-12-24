import { IDirective, IScope, IAugmentedJQuery, IAttributes, ITranscludeFunction, ITimeoutService } from "angular";
import { IDirectiveFactory, INgModelController, IWindowService } from "angular";

export class ChatAutoresizeScopeWatcher {
    private watchStoper: () => any;
    constructor(
        private scope: IScope,
        private watcher: () => any,
        private handler: () => void
    ) { }

    start(isCall: boolean) {
        this.stop();
        isCall && this.handler();
        this.watchStoper = this.scope.$watch(this.watcher, this.handler);
    }

    stop() {
        if (this.watchStoper) {
            this.watchStoper();
            this.watchStoper = null;
        }
    }
}

export class ChatAutoresizeDirective implements IDirective {

    restrict = 'A';
    require = 'ngModel';
    scope = {};
    link = this.linkfn.bind(this);

    constructor(
        private $timeout: ITimeoutService,
        private $window: IWindowService
    ) { }

    private resize(element: IAugmentedJQuery, parent: IAugmentedJQuery, diff): void {
        const oldHeight = element.height();
        element.height(0);
        const newHeight = element[0].scrollHeight - diff;
        const maxHeight = this.getMaxHeight(element, parent);

        if (maxHeight && newHeight > maxHeight) {
            element.height(maxHeight);
        } else {
            element.height(newHeight);
        }

        parent.scrollTop(parent.height());
    }

    private getMaxHeight(element: IAugmentedJQuery, parent: IAugmentedJQuery): number {
        const maxHeight = element.css('max-height');
        if (maxHeight == 'none' || !maxHeight) {
            return null;
        } else if (/^[0-9.]+%$/.test(maxHeight)) {
            return parent.height() * parseFloat(maxHeight) / 100;
        } else if (/^[0-9.]+$/.test(maxHeight)) {
            return parseFloat(maxHeight);
        }

        return null;
    }

    private linkfn(
        scope: IScope,
        element: IAugmentedJQuery,
        attributes: IAttributes,
        controller: INgModelController,
        transclude: ITranscludeFunction
    ): void {
        const initHeight = element.height();
        const heightDiff = parseInt(element.css('padding-bottom')) + parseInt(element.css('padding-top'));
        const modelWatcher = new ChatAutoresizeScopeWatcher(
            scope,
            () => controller.$viewValue,
            () => this.resize(element, angular.element(this.$window), heightDiff)
        );
        const restoreHeight = () => {
            element.height(initHeight);
            angular.element(this.$window).trigger('resize');
        };
        const blurHandle = () => {
            const focusElements = element.closest('form').find(':focus');
            if (!focusElements.length) {
                modelWatcher.stop();
                restoreHeight();
            } else {
                focusElements.one('blur', restoreHeight);
            }
        };

        element
            .bind('focus', () => modelWatcher.start(true))
            .bind('blur', () => this.$timeout(() => blurHandle()))
            .height(element[0].scrollHeight - heightDiff)
            .css({ opacity: 0 });

        scope.$on('chatRoomMessageReady', () => element.css({ opacity: 1 }));
        scope.$on('$destroy', () => modelWatcher.stop());
    }

    static get Factory(): IDirectiveFactory {
        const factory: IDirectiveFactory = ($timeout, $window) => {
            return new ChatAutoresizeDirective($timeout, $window);
        };
        factory.$inject = ['$timeout', '$window'];
        return factory;
    }
}