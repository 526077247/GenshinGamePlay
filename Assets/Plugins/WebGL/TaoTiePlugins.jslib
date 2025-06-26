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
    StringReturnValueFunction: function ()
    {
        var returnStr = window.location.href;
        var title = decodeURI(returnStr);
        var bufferSize = lengthBytesUTF8(title) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(title, buffer, bufferSize);
        return buffer;
    },
    Vibrate: function ()
    {
        if ("vibrate" in navigator)
        {
            return navigator.vibrate(50)
        }
        else if("mozVibrate" in navigator)
        {
            return navigator.mozVibrate(50)
        }
        return false;
    },
    NativeDialogPrompt:function (title , defaultValue){
        defaultValue = UTF8ToString(defaultValue);
        title = UTF8ToString(title);
        var result = window.prompt( title , defaultValue );
        if( !result ){
        result = defaultValue;
        }
        var bufferSize = lengthBytesUTF8(result) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(result, buffer, bufferSize);
        return buffer;
    },
    SetupOverlayDialogHtml:function(title,defaultValue,okBtnText,cancelBtnText){
        title = UTF8ToString(title);
        defaultValue = UTF8ToString(defaultValue);
        okBtnText = UTF8ToString(okBtnText);
        cancelBtnText = UTF8ToString(cancelBtnText);

        if( !document.getElementById("nativeInputDialogInput" ) ){
            // setup css
            var style = document.createElement( 'style' );
            style.setAttribute('id' , 'inputDialogTextSelect');
            style.appendChild( document.createTextNode( '#nativeInputDialogInput::-moz-selection { background-color:#00ffff;}' ) );
            style.appendChild( document.createTextNode( '#nativeInputDialogInput::selection { background-color:#00ffff;}' ) );
            document.head.appendChild( style );
        }
        if( !document.getElementById("nativeInputDialog" ) ){
            // setup html
            var html = '<div id="nativeInputDialog" style="background:#000000;opacity:0.9;width:100%;height:100%;position:fixed;top:0%;z-index:2147483647;">' + 
                    '  <div style="position:relative;top:30%;" align="center" vertical-align="middle">' + 
                    '    <div id="nativeInputDialogTitle" style="color:#ffffff;">Here is title</div>' + 
                    '    <div>' + 
                    '      <input id="nativeInputDialogInput" type="text" size="40" onsubmit="">' + 
                    '    </div>' + 
                    '    <div style="margin-top:10px">' + 
                    '      <input id="nativeInputDialogOkBtn" type="button" value="OK" onclick="" >' + 
                    '      <input id="nativeInputDialogCancelBtn" type="button" value="Cancel" onclick ="">' + 
                    '      <input id="nativeInputDialogCheck" type="checkBox" style="display:none;">' + 
                    '    </div>' + 
                    '  </div>' + 
                    '</div>';
            var element = document.createElement('div');
            element.innerHTML = html;
            // write to html
            document.body.appendChild( element );
    
            // set Event
            var okFunction = 
                'document.getElementById("nativeInputDialog" ).style.display = "none";' + 
                'document.getElementById("nativeInputDialogCheck").checked = false;' +
                'document.getElementById("unity-canvas").style.display="";';
            var cancelFunction = 
                'document.getElementById("nativeInputDialog" ).style.display = "none";'+ 
                'document.getElementById("nativeInputDialogCheck").checked = true;'+
                'document.getElementById("unity-canvas").style.display="";';
    
            var inputField = document.getElementById("nativeInputDialogInput");
            inputField.setAttribute( "onsubmit" , okFunction );
            var okBtn = document.getElementById("nativeInputDialogOkBtn");
            okBtn.setAttribute( "onclick" , okFunction );
            var cancelBtn = document.getElementById("nativeInputDialogCancelBtn");
            cancelBtn.setAttribute( "onclick" , cancelFunction );
        }
        document.getElementById("nativeInputDialogTitle").innerText = title;
        document.getElementById("nativeInputDialogInput").value= defaultValue;

        document.getElementById("nativeInputDialogOkBtn").value = okBtnText;
        document.getElementById("nativeInputDialogCancelBtn").value = cancelBtnText;
        document.getElementById("nativeInputDialog" ).style.display = "";
    },
    HideUnityScreenIfHtmlOverlayCant:function(){
        if( navigator.userAgent.indexOf("Chrome") < 0 && navigator.userAgent.indexOf("Firefox") < 0 ){
            document.getElementById("unity-canvas").style.display="none";
        }
    },
    IsRunningOnEdgeBrowser:function(){
        if( navigator.userAgent.indexOf("Edge/") < 0 ){
            return false;
        }
        return true;
    },
    IsOverlayDialogHtmlActive:function(){
        var nativeDialog = document.getElementById("nativeInputDialog" );
        if( !nativeDialog ){
            return false;
        }
        return ( nativeDialog.style.display != 'none' );
    },
    IsOverlayDialogHtmlCanceled:function(){
        var check = document.getElementById("nativeInputDialogCheck");
        if( !check ){ return false; }
        return check.checked;
    },
    GetOverlayHtmlInputFieldValue:function(){
        var inputField = document.getElementById("nativeInputDialogInput");
        var result = "";
        if( inputField && inputField.value ){
            result = inputField.value;
        }
        var bufferSize = lengthBytesUTF8(result) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(result, buffer, bufferSize);
        return buffer;
    },
    OpenUploader: function(){
        var uploader = document.getElementById("unity-uploader");
        if(!uploader){
            // setup html
            var html = '<input type="file" id="unity-uploader" accept="image/*" style="display:none">';
            var element = document.createElement('div');
            element.innerHTML = html;
            // write to html
            document.body.appendChild( element );
            // set Event
            var onchangeFunction = 
            '    const file = this.files[0];' +
            '    if(file) {' +
            '       const fileSizeInMB = file.size / (1024 * 1024);' +
            '       if (fileSizeInMB > 5) {' +
            '           alert("< 5M");return;' +
            '       }' +
            '       var reader = new FileReader();' +
            '       reader.onload = function(e) {' +
            '            if (reader.readyState === 2) {' +
            '                window.imgData = e.target.result;' +
            '           }' +
            '       };' +
            '       reader.readAsDataURL(this.files[0]);' +
            '    }';
            uploader = document.getElementById("unity-uploader");
            uploader.setAttribute("onchange" , onchangeFunction );
        }
        window.imgData = null;
        uploader.click();
    },
    GetImgData: function(){
        var uploader = document.getElementById("unity-uploader");
        if(uploader && window.imgData){
            var result = window.imgData;
            var bufferSize = lengthBytesUTF8(result) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(result, buffer, bufferSize);
            window.imgData = null;
            return buffer;
        }
        return null;
    },
});