import { IDirective } from "angular";
import { IDirectiveLinkFn } from "angular";
import { IScope } from "angular";
import { IAugmentedJQuery } from "angular";
import { IAttributes } from "angular";
import { ITranscludeFunction } from "angular";
import { IDirectiveFactory } from "angular";

export class ChatMessageImageDirective implements IDirective {
    restrict = 'A';
    link = this.linkfn.bind(this);

    private errorImageUrls = new Map<string, boolean>();

    constructor(
        private noImageUrl: string
    ) { }

    private linkfn(
        scope: IScope,
        instanceElement: IAugmentedJQuery,
        instanceAttributes: IAttributes,
        controller: {},
        transclude: ITranscludeFunction
    ): void {
        if (this.errorImageUrls.has(instanceElement.attr('src'))) {
            this.errorHandle(instanceElement);
        } else {
            instanceElement
                .on('load', () => instanceElement.unwrap())
                .on('error', () => this.errorHandle(instanceElement))
                .wrap(angular.element('<div>').addClass('chat-img-loading'));
        }
    }

    errorHandle(elem: IAugmentedJQuery) {
        const src = elem.attr('src');
        if (src) {
            this.errorImageUrls.set(src, true);
        }

        const anchor = angular.element('<a>').attr({
            target: '_blank',
            title: elem.attr('alt'),
            href: elem.attr('src')
        });

        elem.attr('src', this.noImageUrl).unbind('load').unwrap().wrap(anchor);
    }

    static get Factory(): IDirectiveFactory {
        const factory: IDirectiveFactory = (errorImageUrls) => {
            return new ChatMessageImageDirective(errorImageUrls);
        };
        factory.$inject = ['chatMessageNoImage'];
        return factory;
    }
}