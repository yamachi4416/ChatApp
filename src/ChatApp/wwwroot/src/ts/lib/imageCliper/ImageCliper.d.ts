declare interface ImageCliperOptions {
    zoomStep?: number
    imageType?: string
    imageQuality?: number
    minRatio?: number
    maxRatio?: number
    parentSelector?: string
    dragWrapperTemplate?: string
    focusOnly?: boolean
}

declare interface ImageCliperClientRect {
    top: number
    left: number
    width: number
    height: number
    naturalWidth?: number
    minWidth?: number
    maxWidth?: number
}

declare interface JQueryImageCliper extends JQuery {
    start(): JQueryImageCliper
    openFileDialog(): JQueryDeferred<FileList>
    loadFile(file: File): JQueryDeferred<ImageCliperClientRect>
    toBlob(): JQueryDeferred<Blob>
    postImage(url: string, name: string, options: JQueryAjaxSettings, blob: Blob): JQueryDeferred<any>
    zoom(step?: number),
    setSrc(src: string, def?: JQueryDeferred<{}>): JQueryDeferred<{}>
    getSrc(): string
    imageInfo(): ImageCliperClientRect
}

declare interface JQuery {
    imageCliper: (options: ImageCliperOptions) => JQueryImageCliper
}