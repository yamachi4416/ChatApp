declare interface JQueryImageCliper extends JQuery {
    start(): JQueryImageCliper
    openFileDialog(): JQueryDeferred<FileList>
    loadFile(file: File): JQueryDeferred<ImageCliperClientRect>
    toBlob(): JQueryDeferred<Blob>
    postImage(url: string, name: string, options: JQueryAjaxSettings, blob: Blob): JQueryDeferred<any>
    zoom(step?: number),
    setSrc(src: string, def?: JQueryDeferred<{}>): JQueryDeferred<{}>
    getSrc(): string
}

declare interface JQuery {
    imageCliper: (options: ImageCliperOptions) => JQueryImageCliper
}