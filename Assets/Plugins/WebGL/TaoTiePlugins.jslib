mergeInto(LibraryManager.library, {
    IsMobileWebGL: function () {
        return /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
    }
});