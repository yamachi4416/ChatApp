declare interface ImageCliperState {
    px: number
    py: number
    drag: boolean
    dragStart: boolean
    wrapper?: JQuery
    touchStart?: TouchList
}

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