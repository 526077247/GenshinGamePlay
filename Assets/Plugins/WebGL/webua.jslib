mergeInto(LibraryManager.library, {

    GetWebGLUA: function () {
        var uA = navigator.userAgent.toLowerCase();
        var ipad = uA.match(/ipad/i) == "ipad";
        var iphone = uA.match(/iphone os/i) == "iphone os";
        var midp = uA.match(/midp/i) == "midp";
        var uc7 = uA.match(/rv:1.2.3.4/i) == "rv:1.2.3.4";
        var uc = uA.match(/ucweb/i) == "ucweb";
        var android = uA.match(/android/i) == "android";
        var windowsce = uA.match(/windows ce/i) == "windows ce";
        var windowsmd = uA.match(/windows mobile/i) == "windows mobile";
        if (ipad || iphone){
            // iOS 端  返回3
            return 3;
        }
        else if (!(midp || uc7 || uc || android || windowsce || windowsmd)) {
            // PC 端  返回1
            return 1;
        }else{
            // 安卓端  返回2
            return 2;
        }
    },

});