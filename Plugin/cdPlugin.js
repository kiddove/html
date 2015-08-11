// use videojs.ads.js
/**
 * Example ad integration using the videojs-ads plugin.
 *
 * For each content video, this plugin plays one preroll and one midroll.
 * Ad content is chosen randomly from the URLs listed in inventory.json.
 */
function ChangePlayerSource(src, type) {
    var player = videojs("example_video_1");
    player.change(src, type);
}

function ActionStat(act) {
    var player = videojs("example_video_1");
    player.stat(act);
}

function SetPageinfo(pinfo) {
    var player = videojs("example_video_1");
    player.setinfo(pinfo);
    // quality selector
    player.resolutionSelector();
}

(function (window, document, vjs, undefined) {
    "use strict";
    function EncryptByDES(message, key) {
        var encrypted = CryptoJS.DES.encrypt(CryptoJS.enc.Utf8.parse(message), CryptoJS.enc.Utf8.parse(key), { mode: CryptoJS.mode.ECB, padding: CryptoJS.pad.ZeroPadding });

        var str = encrypted.toString();
        // replace + -> %2B
        str = str.split('+').join('%2B');
        //console.log(str);
        return str;
    }
    function DecryptByDES(message, key) {
        message = message.split('%2B').join('+');
        var decrypted = CryptoJS.DES.decrypt({
            ciphertext: CryptoJS.enc.Base64.parse(message)
        }, CryptoJS.enc.Utf8.parse(key), {
            mode: CryptoJS.mode.ECB,
            padding: CryptoJS.pad.ZeroPadding
        });

        //console.log(decrypted.toString(CryptoJS.enc.Utf8));
        return decrypted.toString(CryptoJS.enc.Utf8);
    }

    // for cookie
    function setCookie(cname, cvalue, exdays) {
        var d = new Date();
        d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
        var expires = "expires=" + d.toGMTString();
        document.cookie = cname + "=" + cvalue + "; " + expires;
    }
    function getCookie(cname) {
        var name = cname + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') c = c.substring(1);
            if (c.indexOf(name) == 0) {
                return c.substring(name.length, c.length);
            }
        }
        return "";
    }

    // https://developer.chrome.com/multidevice/user-agent
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
    //EncryptByDES("rtmp://206.190.133.140/liveH3Camera/demo_wyslink_com_ch1", "phpWVnet");
    //DecryptByDES("/XpIMmZkJJIUF1vCVQWuvdJNtKrPasb1S4vEVoX%2BNWq/5MTVXN0dVyjG%2BqE7ixs0rtvEwerd0iM=", "phpWVnet");
    ////videojs.BigPlayButton.prototype.u = function () {
    //videojs.BigPlayButton.prototype.onClick = function () {
    //    if (state.adPlaying && curAd !== undefined) {
    //        if (curAd.link)
    //            window.open(curAd.link);
    //    }
    //    else
    //        this.player_.play();
    //};
    function fireClick(node) {
        if (document.createEvent) {
            var evt = document.createEvent('MouseEvents');
            evt.initEvent('click', true, false);
            node.dispatchEvent(evt);
        } else if (document.createEventObject) {
            node.fireEvent('onclick');
        } else if (typeof node.onclick == 'function') {
            node.onclick();
        }
        // send to server
        ActionStat(1);
    }
    // reload the click event when playing ad jump to link
    //videojs.MediaTechController.prototype.onClick = function (event) {
    videojs.MediaTechController.prototype.u = function (event) {
        // We're using mousedown to detect clicks thanks to Flash, but mousedown
        // will also be triggered with right-clicks, so we need to prevent that
        if (event.button !== 0) return;
        //this.player().change("http://vjs.zencdn.net/v/oceans.mp4", "video/mp4");
        // HAVE_NOTHING -- 0
        if (this.player().readyState() === 0) return;
        //console.log("ready state: " + this.player().readyState());
        if (state.adPlaying && curAd !== undefined) {
            if (curAd.link)
                //window.open(curAd.link, '_blank');
            {
                // the ONLY way to bypass the popup blocker is through the onclick event
                var evLink = document.createElement('acdbb');
                document.body.appendChild(evLink);

                evLink.onclick = function () { window.open(curAd.link, '_blank'); };
                fireClick(evLink);
            }
        }
        else {
            // When controls are disabled a click should not toggle playback because
            // the click is considered a control
            if (this.player().controls()) {
                if (this.player().paused()) {
                    this.player().play();
                } else {
                    this.player().pause();
                }
            }
        }
    };

    // try to forbid right click
    videojs.oncontextmenu = function () {
        return false;
    }

    /**
     * Add a skip button for the Ads Plugin
     * enable/disable count down then skip
     */
    // create the button
    videojs.SkipButton = videojs.Button.extend({
        init: function (player, options) {
            // Initialize the button using the default constructor
            videojs.Button.call(this, player, options);
            this.on('click', this.onClick);
        }
    });

    // Note the we're not doing this in prototype.createEl() because
    // it won't be called by Component.init(due to name obfuscation)
    var createSkipButton = function () {
        var props = {
            className: 'vjs-skip-button vjs-control',
            innerHTML: '<div class="vjs-control-content">' + ('Skip Ad.') + '</div>',
            role: 'button',
            'aria-live': 'polite',  // let the screen reader user know that the text of the button
            tabIndex: 0
        };
        return videojs.Component.prototype.createEl(null, props);
    };

    // videojs.Button already sets up the onclick handler, override it.
    videojs.SkipButton.prototype.onClick = function (e) {
        // count down?
        // skip?
        var player = videojs("example_video_1");
        //player.src({ src: DecryptByDES("/XpIMmZkJJIUF1vCVQWuvdJNtKrPasb1S4vEVoX%2BNWq/5MTVXN0dVyjG%2BqE7ixs0rtvEwerd0iM=", "phpWVnet"), type: "rtmp/flv" });
        //player.play();

        //ChangePlayerSource("/XpIMmZkJJIUF1vCVQWuvdJNtKrPasb1S4vEVoX%2BNWq/5MTVXN0dVyjG%2BqE7ixs0rtvEwerd0iM=", "rtmp/flv");
        //player.change("http://vjs.zencdn.net/v/oceans.mp4", "video/mp4");
        player.ads.endLinearAdMode();
        //state.adPlaying = false;
        // replace(/(?:^|\s)MyClass(?!\S)/g , '');vjs-ad-enable-skip
        //player.el().className = player.el().className.replace(/(?:^|\s)vjs-ad-enable-skip(?!\S)/g, "");
        //console.log("*** skip button clicked, disable skip: " + player.el().className);

        //ActionStat(2);
        player.stat(2);
    };

    var state = {};
    state.adPlaying = false;
    var userToken;
    var curAd = {};

    /**
     * Register the ad integration plugin.
     * To initialize for a player, call player.regAds().
     *
     * @param {mixed} options Hash of obtions for the videojs-ads plugin.
     */
    vjs.plugin('regAds', function (options) {

        // check cookie
        // if not exist..
        userToken = getCookie("kec_token");


        var bi = get_browser_info();
        var ua = detect.parse(navigator.userAgent);
        //console.log(bi.name.toString() + '-' + bi.version.toString());
        // for finger print
        //var strBrowserInfo = JSON.stringify(Browsers);
        var fp = new Fingerprint2();
        fp.get(function (result) {
            var uploadObj = {};
            //uploadObj.user_token = result;
            ////uploadObj.user_agent = fp.userAgentKey();
            //uploadObj.user_agent = bi.name + '-' + bi.version;
            //uploadObj.user_language = fp.languageKey();
            //uploadObj.user_color_depth = fp.colorDepthKey();
            //uploadObj.user_screen_resolution = fp.screenResolutionKey();
            //uploadObj.user_time_zone = fp.timezoneOffsetKey();
            //uploadObj.user_platform = fp.platformKey().toString().replace("navigatorPlatform: ", "");

            // asp.net
            uploadObj.token = result;
            //uploadObj.agent = uploadObj.user_agent = bi.name + '-' + bi.version;
            uploadObj.agent = ua.browser.name;
            uploadObj.language = fp.languageKey();
            uploadObj.color_depth = fp.colorDepthKey();
            uploadObj.screen_resolution = fp.screenResolutionKey();
            uploadObj.time_zone = fp.timezoneOffsetKey();
            uploadObj.platform = fp.platformKey().toString().replace("navigatorPlatform: ", "");
            //uploadObj.device = ua.device.name == undefined ? ua.device.type : ua.device.name;
            uploadObj.device = ua.device.type;
            uploadObj.os = ua.os.name;

            if (userToken !== result) {
                userToken = result;
                setCookie("kec_token", userToken, 30);
            }
            //var xxx = JSON.stringify(uploadObj);
            // console.log(xxx.length);
            // http://localhost/fingerprint/index.php?myParam=xxxxx
            // result is token
            // send a http request to server to save user info

            var xhr = new XMLHttpRequest();
            //var params = "myParam=" + JSON.stringify(uploadObj);
            var params = JSON.stringify(uploadObj);
            //xhr.open("GET", "/player/demo/samples/inventory.json?src=" + encodeURIComponent(player.currentSrc()));
            //xhr.open("GET", "http://localhost/testFingerPrint/index.php?myParam=" + JSON.stringify(uploadObj));
            //xhr.open("GET", "http://173.236.36.10/cds/index.php?myParam=" + JSON.stringify(uploadObj));
            //xhr.send(null);
            // try post
            //xhr.open("POST", "http://localhost/testFingerPrint/index.php", true);
            //xhr.open("POST", "http://173.236.36.10/cds/index.php", true);
            //xhr.open("POST", "http://localhost:51209/FingerPrint.ashx", true);
            xhr.open("POST", "http://206.190.131.92:6008/FingerPrint.ashx", true);
            xhr.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
            //xhr.setRequestHeader("Content-length", params.length);
            //xhr.setRequestHeader("Connection", "close");
            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4 && xhr.status === 200) {
                    try {
                        // request ad inventory whenever the player gets new content to play
                        player.on('contentupdate', requestAds);
                        //player.one('contentupdate', requestAds);
                        // if there's already content loaded, request an add immediately
                        if (player.currentSrc()) {
                            requestAds();
                        }
                    } catch (err) {
                        throw new Error('Couldn\'t parse inventory response as JSON');
                    }
                }
            };
            xhr.send(params);
        });



        // add skip button
        var btn_options = {
            'el': createSkipButton()
        };
        var btnSkip = new videojs.SkipButton(this, btn_options);
        var skipbutton = this.addChild(btnSkip);
        var

        player = this,

          // example plugin state, may have any of these properties:
          //  - inventory - hypothetical ad inventory, list of URLs to ads
          //  - lastTime - the last time observed during content playback
          //  - adPlaying - whether a linear ad is currently playing
          //  - prerollPlayed - whether we've played a preroll
          //  - midrollPlayed - whether we've played a midroll
          //state = {},
        // asynchronous method for requesting ad inventory
        requestAds = function () {

            // reset plugin state
            //state = {};
            // fetch ad inventory
            // the 'src' parameter is ignored by the example inventory.json flat file,
            // but this shows how you might send player information along to the ad server.
            var xhr = new XMLHttpRequest();
            //xhr.open("GET", "http://173.236.36.10/cds/getlist.php?type=stream", true);

            // try post
            var adi = {};
            adi.type_ads = [];
            adi.tgt_network = {};
            adi.tgt_network.network = [];
            adi.type_ads.push(1);
            adi.type_ads.push(2);
            adi.token = [];
            adi.token.push(userToken);
            //var adi = {};
            //adi.type_ads = 1;
            
            // network... 
            for (var idx = 0; idx < player.pageObj.whitelist.length; idx++) {
                adi.tgt_network.network.push(player.pageObj.whitelist[idx]);
            }
            
            var params = JSON.stringify(adi);
            //xhr.open("POST", "http://localhost:51209/GetList.ashx", true);
            xhr.open("POST", "http://206.190.131.92:6008/GetList.ashx", true);
            xhr.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4 && xhr.status === 200) {
                    try {
                        state.inventory = JSON.parse(xhr.responseText);
                        if (!state.inventory.pre)
                            return;
                        player.trigger('adsready');
                    } catch (err) {
                        throw new Error('Couldn\'t parse inventory response as JSON');
                    }
                }
            };
            //xhr.send(null);
            xhr.send(params);
        },

        // play an ad, given an opportunity
        playAd = function () {

            // short-circuit if we don't have any ad inventory to play
            if (!state.inventory || (!state.preroll && !state.midroll)) {
                return;
            }

            if (state.preroll) {
                if (state.inventory.pre === undefined || state.inventory.pre.length === 0)
                    return;
            }

            if (state.midroll) {
                if (state.inventory.mid === undefined || state.inventory.midroll.length === 0)
                    return;
            }

            // tell ads plugin we're ready to play our ad
            player.ads.startLinearAdMode();
            state.adPlaying = true;

            if (state.preroll) {
                curAd = state.inventory.pre[Math.floor(Math.random() * state.inventory.pre.length)];
            }
            //else {
            //    // curAd already got in 'timeupdate'
            //}
            // tell videojs to load the ad

            if (curAd === undefined) {
                console.log('Couldn\'t parse inventory response as JSON');
                return;
            }
            player.src(curAd);

            // when the video metadata is loaded, play it!
            player.one('durationchange', function () {
                player.play();
            });

            // when it's finished
            player.one('ended', function () {
                // play your linear ad content, then when it's finished ...
                player.ads.endLinearAdMode();
                //player.el().className = player.el().className.replace(/(?:^|\s)vjs-ad-enable-skip(?!\S)/g, "");
                //console.log("*** ads end, disable skip: " + player.el().className);
                //state.adPlaying = false;
                //console.log("adPlaying set to false in event 'ended'.");
            });
            player.stat(3);
        };

        // by paul decode the url
        if (options instanceof Object) {
            if (options.src) {
                options.src = DecryptByDES(options.src, "phpWVnet");
                player.src(options);
            }
        }

        // initialize the ads plugin, passing in any relevant options
        player.ads(options);

        var changesrc = function (s, t) {
            if (s && t) {
                console.log("change src.");
                var player = videojs("example_video_1");
                player.src({ src: DecryptByDES(s, "phpWVnet"), type: t });
                //player.src({ src: s, type: t });
                changestatus(false);
                player.play();
            }
        };

        var changestatus = function (s) {
            state.adPlaying = s;
            if (s === false) {
                player.el().className = player.el().className.replace(/(?:^|\s)vjs-ad-enable-skip(?!\S)/g, "");
            }
        }

        var actstat = function (act) {
            if (curAd === undefined || userToken === undefined)
                return;
            var statObj = {};
            statObj.id = curAd.id;
            statObj.token = userToken;
            statObj.action = act;
            var strStat = JSON.stringify(statObj);
            // post to server
            var xhr = new XMLHttpRequest();
            xhr.open("POST", "http://206.190.131.92:6008/Actstat.ashx", true);
            xhr.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
            ////xhr.setRequestHeader("Content-length", params.length);
            ////xhr.setRequestHeader("Connection", "close");
            //xhr.onreadystatechange = function () {
            //    if (xhr.readyState === 4 && xhr.status === 200) {
            //        try {
            //            // request ad inventory whenever the player gets new content to play
            //            player.on('contentupdate', requestAds);
            //            //player.one('contentupdate', requestAds);
            //            // if there's already content loaded, request an add immediately
            //            if (player.currentSrc()) {
            //                requestAds();
            //            }
            //        } catch (err) {
            //            throw new Error('Couldn\'t parse inventory response as JSON');
            //        }
            //    }
            //};
            xhr.send(strStat);
        }

        var setpageinfo = function (pageinfo) {
            try {
            	var pageObj;
            	if (typeof pageinfo == "string")
                player.pageObj = JSON.parse(pageinfo);
                else if (typeof pageinfo == "object")
                player.pageObj = pageinfo;
		
		// to be continued..
            } catch (err) {
                throw new Error('Couldn\'t parse inventory response as JSON');
            }
        }

        // vjs.plugin('ads', adFramework);
        vjs.plugin('change', changesrc);
        vjs.plugin('changestatus', changestatus);
        vjs.plugin('stat', actstat);
	vjs.plugin('setinfo', setpageinfo);

        //// request ad inventory whenever the player gets new content to play
        //player.on('contentupdate', requestAds);
        ////player.one('contentupdate', requestAds);
        //// if there's already content loaded, request an add immediately
        //if (player.currentSrc()) {
        //    requestAds();
        //}

        // play an ad the first time there's a preroll opportunity
        player.on('readyforpreroll', function () {
            //// check if this invetory contains preroll ads
            //if (state.inventory.pre)
            //if (!state.prerollPlayed) {
            //    state.prerollPlayed = true;
            //    playAd();
            //}
            state.preroll = true;
            state.midroll = false;
            playAd();
        });

        // watch for time to pass 15 seconds, then play an ad
        // if we haven't played a midroll already
        var curs;
        player.on('timeupdate', function (event) {
            var currentTime = player.currentTime(), opportunity_show_skip = false;
            //console.log("currentTime: " + currentTime + " ###");
            if ('lastTime' in state) {
                //opportunity = currentTime > 15 && state.lastTime < 15;
                if (state.adPlaying)
                    opportunity_show_skip = currentTime > 3;
            }
            else
                state.lastTime = 0;
            if (state.adPlaying) {
                if (opportunity_show_skip) {
                    var sName = player.el().className.toString();
                    if (sName.indexOf("vjs-ad-enable-skip") == -1) {
                        player.el().className += ' vjs-ad-enable-skip';
                        //console.log("*** enable skip: " + player.el().className);
                    }
                }
                //else
                //    //player.ads.removeClass(player.el(), 'vjs-ad-enable-skip');
                //    player.el().className = player.el().className.replace(" vjs-ad-enable-skip", "");
                //console.log("ads curtime: " + currentTime + " skip: " + opportunity_show_skip + " ###");
            }
            //else
            //    console.log("not ads curtime: " + currentTime + " & state.lasttime: " + state.lastTime + " ***");
            if (!state.adPlaying && Math.floor(currentTime) > state.lastTime) {

                // after end or skip adPlaying will be false, but src may not be changed due to event async.
                if (player.currentSrc() === curAd.src)
                    return;
                if (curs === undefined || curs != player.currentSrc()) {
                    //console.log("not ads curSrc: pre --- " + curs + "    after --- " + player.currentSrc() + " currentTime: " + currentTime + " state.lastTime: " + state.lastTime);
                    curs = player.currentSrc();
                }

                state.lastTime = Math.floor(currentTime);
                if (state.lastTime === 0)
                    console.log("state.lastTime: " + state.lastTime + " ***");
                //console.log("state.lastTime: " + state.lastTime + " ***");
                var i = 0;
		if (!state.inventory || !state.inventory.mid)
                    return;
                for (i = 0; i < state.inventory.mid.length; i++) {
                    if (state.inventory.mid[i].ads_time == state.lastTime) {
                        curAd = state.inventory.mid[i];
                        state.midroll = true;
                        state.preroll = false;
                        //console.log("midroll at :" + state.lastTime + ", curtime: " + currentTime);
                        playAd();
                    }
                }
            }
            //else
            //    //
            //    state.lastTime = Math.floor(currentTime);
            //if (state.midrollPlayed) {
            //    return;
            //}
            //if (opportunity) {
            //    state.midroll = true;
            //    state.preroll = false;
            //    playAd();
            //}
        });
    });


})(window, document, videojs);
