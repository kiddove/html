(function (window, document, undefined) {
    function getParameterByName(name, url) {
        if (!url) url = window.location.href;
        name = name.replace(/[\[\]]/g, "\\$&");
        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    }
    function get_browser_info() {
        var ua = navigator.userAgent, tem, M = ua.match(/(opera|chrome|safari|firefox|msie|trident|CriOS(?=\/))\/?\s*(\d+)/i) || [];
        if (/trident/i.test(M[1])) {
            tem = /\brv[ :]+(\d+)/g.exec(ua) || [];
            return { name: 'IE ', version: (tem[1] || '') };
        }
        if (M[1] === 'Chrome') {
            tem = ua.match(/\bOPR\/(\d+)/)
            if (tem != null) { return { name: 'Opera', version: tem[1] }; }
        }
        M = M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, '-?'];
        if ((tem = ua.match(/version\/(\d+)/i)) != null) { M.splice(1, 1, tem[1]); }
        return {
            name: M[0],
            version: M[1]
        };
    }
    $(document).ready(function () {
        var bi = get_browser_info();
        var ua = detect.parse(navigator.userAgent);
        var fp = new Fingerprint2();
        var distributor = getParameterByName("id");
        fp.get(function (result) {
            var uploadObj = {};
            // ip, token, agent, language, color_depth, screen_resolution, time_zone, platform, device, os, visit_time(*), country, province, city, province_code, alias(*), distributor, refer, page, type(*)
            // asp.net
            uploadObj.token = result;
            uploadObj.agent = ua.browser.name;
            uploadObj.language = fp.languageKey();
            uploadObj.color_depth = fp.colorDepthKey();
            uploadObj.screen_resolution = fp.screenResolutionKey();
            uploadObj.time_zone = fp.timezoneOffsetKey();
            uploadObj.platform = fp.platformKey().toString().replace("navigatorPlatform: ", "");
            uploadObj.device = ua.device.type;
            uploadObj.os = ua.os.name;

            uploadObj.distributor = distributor;
            uploadObj.refer = document.referrer;
            uploadObj.page = window.location.pathname;

            if (uploadObj.distributor) {
                var params = JSON.stringify(uploadObj);

                $.ajax({
                    type: "POST",
                    url: "http://206.190.131.92:6009/SampleAnalytics.ashx",
                    data: {
                        update: new Date().getTime(),
                        t: 'u',
                        j: params
                    },
                    success: function (data, status) {
                        // todo what if failed, when alias already exists.
                        //console.log(data);
                    }
                });
            }
        });

        if ($('#dlLImgList img').length > 0) {
            $('#dlLImgList img').click(function () {
                sendAdStat($(this)[0].src, distributor);
            });
        }
        if ($('#dlRImgList img').length > 0) {
            $('#dlRImgList img').click(function () {
                sendAdStat($(this)[0].src, distributor);
            });
        }

        function sendAdStat(src, distributor) {
            var obj = {};
            obj.url = src;
            obj.distributor = distributor;
            obj.page = window.location.pathname;
            var params = JSON.stringify(obj);
            if (distributor) {
                $.ajax({
                    type: "POST",
                    url: "http://206.190.131.92:6009/SampleAnalytics.ashx",
                    data: {
                        update: new Date().getTime(),
                        t: 'c',
                        j: params
                    },
                    success: function (data, status) {
                        // todo what if failed, when alias already exists.
                        //console.log(data);
                    }
                });
            }
        }
    });
})(window, document);