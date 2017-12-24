import { IDirective, IScope, IAugmentedJQuery, IAttributes, ITranscludeFunction } from "angular";
import { ITimeoutService } from "angular";
import { IDirectiveFactory } from "angular";

export interface CliperComponant {
    cliper: JQueryImageCliper,
    range: { min: number, val: number, max: number }
}

interface IChatImageCliperScope extends IScope {
    cCtrl: {}
    cAssign: string
    cOption: ImageCliperOptions
    cSrc: string
}

export class ChatImageCliperDirective implements IDirective {
    restrict = 'A';
    scope = {
        cCtrl: '=',
        cAssign: '@',
        cOption: '<',
        cSrc: '@'
    };
    link = this.linkfn.bind(this);

    constructor(
        private $timeout: ITimeoutService
    ) { }

    private linkfn(
        scope: IChatImageCliperScope,
        element: IAugmentedJQuery,
        attributes: IAttributes,
        controller: {},
        transclude: ITranscludeFunction
    ): void {
        const cliper = angular.element(element).append(
            angular.element('<img>') as JQLite
        ).imageCliper(scope.cOption);

        const range = { min: 0, val: 50, max: 100 };

        const setRangeVal = (_, info) => this.$timeout(() => range.val = info.width);
        const updateRange = (_, info) => {
            this.$timeout(() => {
                range.max = info.maxWidth;
                range.min = info.minWidth;
                setRangeVal(_, info);
            });
        };

        scope.cCtrl[scope.cAssign] = { cliper: cliper, range: range };

        cliper.on('cliper.srcChanged', updateRange);
        cliper.on('cliper.zoomed', setRangeVal);

        cliper.setSrc(scope.cSrc);
        cliper.start();
    }

    static get Factory(): IDirectiveFactory {
        const factory: IDirectiveFactory = ($timeout) => {
            return new ChatImageCliperDirective($timeout);
        };
        factory.$inject = ['$timeout'];
        return factory;
    }
}