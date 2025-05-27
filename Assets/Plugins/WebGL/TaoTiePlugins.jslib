mergeInto(LibraryManager.library, {
    IsMobileWebGL: function () {
        return /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
    },
    CloseWindow:function() {
        if (navigator.userAgent.indexOf("Firefox") != -1 || navigator.userAgent.indexOf("Chrome") != -1) {
            window.location.href = "about:blank";
            window.close();
        } else {
            window.opener = null;
            window.open("", "_self");
            window.close();
        }
    },
});