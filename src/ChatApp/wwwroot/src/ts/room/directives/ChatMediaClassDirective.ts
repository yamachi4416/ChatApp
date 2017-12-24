import { IDirective, IScope, IAugmentedJQuery, IAttributes, ITranscludeFunction, IWindowService, IDirectiveFactory } from "angular";

interface IChatMediaClassScope extends IScope {
    media: { xs: string, sm: string, md: string }
}

export class ChatMediaClassDirective implements IDirective {
    restrict = 'A';
    scope = {
        media: '<mediaClass'
    };
    link = this.linkfn.bind(this);

    constructor(
        private $window: IWindowService
    ) { }

    private linkfn(
        scope: IChatMediaClassScope,
        element: IAugmentedJQuery,
        attributes: IAttributes,
        controller: {},
        transclude: ITranscludeFunction
    ): void {
        const xs = 480;
        const sm = 768;
        const md = 992;

        const xsClass = scope.media.xs;
        const smClass = scope.media.sm;
        const mdClass = scope.media.md;

        const watcher = () => this.$window.innerWidth;

        const handler = (newVal, oldVal?) => {
            xsClass && element.removeClass(xsClass);
            smClass && element.removeClass(smClass);
            mdClass && element.removeClass(mdClass);
            if (newVal <= xs) {
                xsClass && element.addClass(xsClass);
            } else if (newVal <= sm) {
                smClass && element.addClass(smClass);
            } else {
                mdClass && element.addClass(mdClass);
            }
        };

        scope.$watch(watcher, handler);

        handler(watcher());
    }

    static get Factory(): IDirectiveFactory {
        const factory: IDirectiveFactory = ($window) => new ChatMediaClassDirective($window);
        factory.$inject = ['$window'];
        return factory;
    }
}