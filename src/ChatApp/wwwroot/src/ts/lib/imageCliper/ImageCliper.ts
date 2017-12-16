class ImageCliper {
    public options: ImageCliperOptions
    
    private clip: JQuery
    private image: JQuery
    private state: ImageCliperState
    private disableHandle = (e: Event) => { e.preventDefault(); };

    constructor(
        options: ImageCliperOptions,
        element: JQuery
    ) {
        this.options = $.extend({
            zoomStep: 0.5,
            imageType: 'image/png',
            imageQuality: 1,
            minRatio: 0.5,
            maxRatio: 5,
            parentSelector: 'body',
            dragWrapperTemplate: '<div id="imgCliperDragWrapper" style="z-index: 999;">'
        }, options)

        this.state = {
            px: 0,
            py: 0,
            drag: false,
            dragStart: false
        }

        this.initElements(element)
    }

    private imageNode() {
        return this.image[0] as HTMLImageElement
    }

    private initElements(element: JQuery) {
        this.clip = $(element)
        this.image = this.clip.find("img")

        this.clip.css({
            position: "relative",
            width: this.clip.width(),
            height: this.clip.height()
        }).on("contextmenu", this.disableHandle)

        this.image.css({
            position: "absolute"
        }).on("dragstart", this.disableHandle)
    }

    get src(): string {
        return this.image.attr("src")
    }

    get isFocus(): boolean {
        return !this.options.focusOnly || this.clip.is(":focus")
    }

    get isDragEnable(): boolean {
        return this.state.drag && this.isFocus
    }

    dragStart() {
        this.state.drag = true
        this.state.dragStart = true
        this.state.wrapper && this.state.wrapper.remove()
        this.state.wrapper = $(this.options.dragWrapperTemplate)
            .css({ position: "fixed", top: 0, left: 0, width: "100%", height: "100%" })
            .bind('mousemove', this.drag.bind(this))
            .bind('mouseup', this.dragStop.bind(this))
            .bind('touchmove', this.touchMove.bind(this))
            .bind('touchend touchcancel', this.dragStop.bind(this))
            .appendTo(this.options.parentSelector);
    }

    dragStop(e: JQueryEventObject) {
        this.disableHandle(e)
        this.state.dragStart = false
        if (this.isDragEnable) {
            this.state.drag = false
            this.state.touchStart = null
        }
        if (this.state.wrapper) {
            this.state.wrapper.remove()
            this.state.wrapper = null
        }
    }

    move(x: number, y: number) {
        const pos = this.image.offset()
        this.image.offset({
            left: Math.floor(pos.left + x),
            top: Math.floor(pos.top + y)
        })
    }

    drag(e: JQueryEventObject) {
        this.disableHandle(e)
        if (!this.isDragEnable) {
            return
        } else if (this.state.dragStart) {
            this.state.dragStart = false
        } else {
            this.move(e.pageX - this.state.px, e.pageY - this.state.py)
        }

        this.state.px = e.pageX
        this.state.py = e.pageY
    }

    rectInfo(element: JQuery): ImageCliperClientRect {
        return {
            width: parseFloat(element.css('width')),
            height: parseFloat(element.css('height')),
            top: parseFloat(element.css('top')),
            left: parseFloat(element.css('left')),
        }
    }

    imageInfo() {
        const iInfo = this.rectInfo(this.image)
        const cInfo = this.rectInfo(this.clip)
        iInfo.minWidth = cInfo.width * this.options.minRatio;
        iInfo.naturalWidth = this.imageNode().naturalWidth;
        iInfo.maxWidth = iInfo.naturalWidth * this.options.maxRatio;
        return iInfo;
    }

    zoom(step: number) {
        const c = this.rectInfo(this.clip)
        const p = this.rectInfo(this.image)
        const cw = c.width
        const ch = c.height
        const ow = this.imageNode().naturalWidth

        if ((p.width + step < cw * this.options.minRatio && step < 0)
            || (p.width + step > ow * this.options.maxRatio && step > 0)) {
            return
        }

        this.image.css('width', p.width + step);
        const n = this.rectInfo(this.image);
        this.image.css({
            left: p.left - (n.width - p.width) * (cw / 2 - p.left) / p.width,
            top: p.top - (n.height - p.height) * (ch / 2 - p.top) / p.height
        });
        this.clip.trigger('cliper.zoomed', this.imageInfo());
    }

    zoomIn(step?: number) {
        this.zoom(step || this.options.zoomStep)
    }

    zoomOut(step?: number) {
        this.zoom(step || -this.options.zoomStep)
    }

    zoomInHandle(e: JQueryEventObject) {
        if (!this.isFocus)
            return
        this.disableHandle(e)
        this.zoomIn()
    }

    zoomOutHandle(e: JQueryEventObject) {
        if (!this.isFocus) {
            return
        }

        this.disableHandle(e)
        this.zoomOut()
    }

    drawImageContext() {
        const canvas = document.createElement('canvas')
        const context = canvas.getContext('2d')
        const sw = this.imageNode().naturalWidth
        const sh = this.imageNode().naturalHeight
        const i = this.rectInfo(this.image)
        const c = this.rectInfo(this.clip)
        canvas.width = c.width
        canvas.height = c.height
        context.drawImage(this.imageNode(), 0, 0, sw, sh, i.left, i.top, i.width, i.height)
        return context
    }

    dataUrlToBlob(dataUrl: string) {
        const b64 = dataUrl.replace(/^data:image\/[^;]+;base64,/, '')
        const binary = atob(b64)
        const array = new Uint8Array(binary.length)
        for (let i = 0, l = binary.length; i < l; i++) {
            array[i] = binary.charCodeAt(i)
        }
        return new Blob([array], { type: this.options.imageType })
    }

    toBlob(callback: (blob: Blob) => any | void) {
        const context = this.drawImageContext();
        const canvas = context.canvas;
        if (typeof canvas.toBlob === 'function') {
            canvas.toBlob(callback, this.options.imageType, this.options.imageQuality);
        } else {
            const dataUrl = canvas.toDataURL(this.options.imageType, this.options.imageQuality);
            callback(this.dataUrlToBlob(dataUrl));
        }
    }

    touchRect(ts: TouchList) {
        if (!(ts[0] && ts[1])) {
            return
        }
        var x1 = ts[0].clientX;
        var y1 = ts[0].clientY;
        var x2 = ts[1].clientX;
        var y2 = ts[1].clientY;
        return Math.abs(x1 - x2) * Math.abs(y1 - y2);
    }

    touchStart(evt: JQueryEventObject) {
        this.disableHandle(evt)
        evt.stopImmediatePropagation()

        const e = evt.originalEvent as TouchEvent
        this.state.touchStart = e.touches
        this.dragStart()
    }

    touchMove(evt: JQueryEventObject) {
        this.disableHandle(evt)
        evt.stopImmediatePropagation()

        if (!this.isDragEnable) {
            return
        }

        const e = evt.originalEvent as TouchEvent
        const ts = e.touches
        if (ts.length == 1) {
            $.extend(evt, {
                pageX: ts[0].clientX,
                pageY: ts[0].clientY
            })
            this.state.touchStart = ts
            this.drag(evt)
        } else {
            const rs = this.state.touchStart
            const r1 = this.touchRect(rs)
            const r2 = this.touchRect(ts)
            this.state.touchStart = ts
            if (Math.abs(Math.abs(r1) - Math.abs(r2)) >= this.options.zoomStep) {
                r1 < r2 ? this.zoomInHandle(evt) : this.zoomOutHandle(evt)
            }
        }
    }

    loadFile(file: File) {
        const df = $.Deferred();

        if (!file) {
            return df.reject()
        }

        const reader = new FileReader()

        reader.onload = (e) => {
            const buff = reader.result
            const blob = new Blob([buff], { type: file.type })
            const image = new Image()

            image.onload = (e) => {
                const $img = this.image
                    .attr('src', image.src)
                    .css({ width: image.width, height: 'unset' })
                    .one('load', () => {
                        const c = this.rectInfo(this.clip);
                        const i = this.rectInfo($img);
                        $img.css({
                            left: (c.width - i.width) / 2,
                            top: (c.height - i.height) / 2
                        });
                        df.resolve(this.imageInfo());
                    });
            }

            image.onerror = (e) => df.reject(e)
            image.src = URL.createObjectURL(blob);
        }

        reader.onerror = (e) => df.reject(e)
        reader.readAsArrayBuffer(file);
        return df.promise();
    }

    wheelHandle(e: JQueryEventObject) {
        this.disableHandle(e)

        const oe = e.originalEvent as WheelEvent;
        if (oe.deltaY < 0) {
            this.zoomOutHandle(e);
        } else if (oe.deltaY > 0) {
            this.zoomInHandle(e);
        }
    }

    start() {
        this.clip
            .bind('mousedown', this.dragStart.bind(this))
            .bind('touchstart', this.touchStart.bind(this))
            .bind('touchmove', this.touchMove.bind(this))
            .bind('touchend touchcancel', this.dragStop.bind(this))
            .bind('wheel', this.wheelHandle.bind(this));
    }
}

export {
    ImageCliper
}