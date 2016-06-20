(function (window, document, undefined) {
    var getUrlParameter = function getUrlParameter(sParam) {
        var sPageURL = decodeURIComponent(window.location.search.substring(1)),
            sURLVariables = sPageURL.split('&'),
            sParameterName,
            i;

        for (i = 0; i < sURLVariables.length; i++) {
            sParameterName = sURLVariables[i].split('=');

            if (sParameterName[0] === sParam) {
                return sParameterName[1] === undefined ? true : sParameterName[1];
            }
        }
    };

    var current_dt;
    var timer;
    var channel = getUrlParameter('c');
    var w = getUrlParameter('w');
    var h = getUrlParameter('h');
    var interval = 30 * 1000;
    if (channel) {
        $(document).ready(function () {
            // click event
            $("#btn-live").click(function (event) {
                //if ($("#btn-live").is(":disabled"))
                //    return;
                disableButton(0);
                disableButton(1);
                disableButton(10);

                requestLatest();
                timer = setInterval(requestLatest, interval);
            });
            $("#btn-10-forward").click(function (event) {
                clear();
                // disable itself when callback will enable again
                disableButton(10);
                var request_dt = moment(current_dt);
                request_dt.add(10, 'minute');
                //console.log("10 : " + request_dt.format("YYYY-MM-DD HH:mm:ss"));
                requestForDateTime(request_dt, 10, 1);
            });
            $("#btn-1-forward").click(function (event) {
                clear();
                // disable itself when callback will enable again
                disableButton(1);
                var request_dt = moment(current_dt);
                request_dt.add(1, 'minute');
                //console.log("1 : " + request_dt.format("YYYY-MM-DD HH:mm:ss"));
                requestForDateTime(request_dt, 1, 1);
            });
            $("#btn-10-backward").click(function (event) {
                clear();
                // disable itself when callback will enable again
                disableButton(-10);
                var request_dt = moment(current_dt);
                request_dt.subtract(10, 'minute');
                //console.log("-10 : " + request_dt.format("YYYY-MM-DD HH:mm:ss"));
                requestForDateTime(request_dt, -10, -1);
            });
            $("#btn-1-backward").click(function (event) {
                clear();
                disableButton(-1);
                var request_dt = moment(current_dt);
                request_dt.subtract(1, 'minute');
                //console.log("-1 : " + request_dt.format("YYYY-MM-DD HH:mm:ss"));
                requestForDateTime(request_dt, -1, -1);
            });

            // set disabled for live and forward button
            disableButton(0);
            disableButton(1);
            disableButton(10);


            function clear() {
                clearInterval(timer);
                enableButton(0);
                enableButton(1);
                enableButton(10);
                enableButton(-1);
                enableButton(-10);
            }
            // set timer
            function requestLatest() {
                // request the latest image
                $.ajax({
                    type: "GET",
                    timeout: 30 * 1000,
                    url: "http://206.190.133.140/VideoImage/RequestImage.php?c=" + channel,
                    success: function (data, status) {
                        try {
                            var o = JSON.parse(data);
                            $("#video-image").attr('src', o.path);
                            current_dt = moment(o.dt, "YYYYMMDDHHmmss");
                            //console.log("requestLatest: " + current_dt.format("YYYY-MMM-DD HH:mm:ss"));
                            $("#image-dt-label").text(current_dt.format("YYYY-MMM-DD HH:mm:ss"));
                        } catch (e) {
                            console.log(data + " --- " + e.message);
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        if (textStatus === "timeout") {
                        }
                    }
                });
            }

            function requestForDateTime(request_dt, request_type, gt) {
                if (!request_dt)
                    return;
                $.ajax({
                    type: "GET",
                    timeout: 30 * 1000,
                    url: "http://206.190.133.140/VideoImage/RequestImage.php?c=" + channel + "&d=" + request_dt.format("YYYYMMDD") + "&t=" + request_dt.format("HHmmss") + "&gt=" + gt,
                    success: function (data, status) {
                        enableButton(request_type);
                        try {
                            var o = JSON.parse(data);
                            if (o.length === 0) {
                                // no data
                            } else {
                                $("#video-image").attr('src', o.path);
                                current_dt = moment(o.dt, "YYYYMMDDHHmmss");
                                //console.log("requestForDateTime: " + current_dt.format("YYYY-MMM-DD HH:mm:ss"));
                                $("#image-dt-label").text(current_dt.format("YYYY-MMM-DD HH:mm:ss"));
                            }
                        } catch (e) {
                            console.log(data + " --- " + e.message);
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        //if (textStatus === "timeout") {
                            enableButton(request_type);
                        //}
                    }
                });
            }

            function enableButton(request_type) {
                switch (request_type) {
                    case 0:
                        $("#btn-live").removeClass("disabled");
                        $("#btn-live").prop('disabled', false);
                        break;
                    case 10:
                        $("#btn-10-forward").removeClass("disabled");
                        $("#btn-10-forward").prop('disabled', false);
                        break;
                    case 1:
                        $("#btn-1-forward").removeClass("disabled");
                        $("#btn-1-forward").prop('disabled', false);
                        break;
                    case -10:
                        $("#btn-10-backward").removeClass("disabled");
                        $("#btn-10-backward").prop('disabled', false);
                        break;
                    case -1:
                        $("#btn-1-backward").removeClass("disabled");
                        $("#btn-1-backward").prop('disabled', false);
                        break;
                }
            }

            function disableButton(request_type) {
                switch (request_type) {
                    case 0:
                        $("#btn-live").addClass("disabled");
                        $("#btn-live").prop('disabled', true);
                        break;
                    case 10:
                        $("#btn-10-forward").addClass("disabled");
                        $("#btn-10-forward").prop('disabled', true);
                        break;
                    case 1:
                        $("#btn-1-forward").addClass("disabled");
                        $("#btn-1-forward").prop('disabled', true);
                        break;
                    case -10:
                        $("#btn-10-backward").addClass("disabled");
                        $("#btn-10-backward").prop('disabled', true);
                        break;
                    case -1:
                        $("#btn-1-backward").addClass("disabled");
                        $("#btn-1-backward").prop('disabled', true);
                        break;
                }
            }

            requestLatest();
            timer = setInterval(requestLatest, interval);
            // clearInterval
        });
    }
})(window, document);