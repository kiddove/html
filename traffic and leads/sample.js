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

    // http://stackoverflow.com/questions/5566541/how-to-reload-the-datatablejquery-data
    function RefreshTable(tableId, urlData) {
        //Retrieve the new data with $.getJSON. You could use it ajax too
        $.getJSON(urlData, null, function (json) {
            table = $(tableId).dataTable();
            oSettings = table.fnSettings();

            table.fnClearTable(this);

            for (var i = 0; i < json.length; i++) {
                table.oApi._fnAddData(oSettings, json[i]);
            }

            oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();
            table.fnDraw();

            if ($("#spinner").length === 1) {
                $("#spinner").addClass('hide-element');
            }
        });
    }

    function canAccessGoogleVisualization() {
        if ((typeof google === 'undefined') || (typeof google.visualization === 'undefined')) {
            return false;
        }
        else {
            return true;
        }
    }

    var pageType = getParameterByName('t');
    var adUrl;
    if (pageType && pageType.toLowerCase() === 'as') {
        adUrl = getParameterByName('u');
    }
    //var distributor = getParameterByName('d');
    var distributor = "kectech";
    var distributor;
    if (typeof showname !== 'undefined') {
        distributor = showname;
    }

    var blogname;
    if (typeof blog !== 'undefined') {
        var result = blog.split('/');
        if (result && result.length > 4) {
            blogname = result[4];
        }
    }

    var startDate;
    startDate = getParameterByName('s');
    var endDate;
    endDate = getParameterByName('e');

    var spinner = '<div id="spinner" class="spinner hide-element" ><img id="img-spinner" src="./js/stat/ajax-loader.gif" alt="Loading" /></div>';

    if (!startDate)
        startDate = moment().subtract(29, 'days').format('YYYY-MM-DD');
    if (!endDate)
        endDate = moment().format('YYYY-MM-DD');

    $(document).ready(function () {
        $.fn.editable.defaults.mode = 'inline';
        function cb(start, end) {
            $('#reportrange span').html(start.format('MMM D, YYYY') + ' - ' + end.format('MMM D, YYYY'));
        }

        $('#reportrange').on('apply.daterangepicker', function (ev, picker) {
            startDate = picker.startDate.format('YYYY-MM-DD');
            endDate = picker.endDate.format('YYYY-MM-DD');
            if ($("#spinner").length === 1) {
                $("#spinner").removeClass('hide-element');
            }
            // update table
            if (pageType) {
                if (pageType.toLowerCase() === 'tot') {
                    InitTrafficOverTime(distributor, pageType, startDate, endDate);
                    $('#tot').addClass('type-selected');
                } else if (pageType.toLowerCase() === 'pv') {
                    InitPageView(distributor, pageType, startDate, endDate);
                    $('#pv').addClass('type-selected');
                } else if (pageType.toLowerCase() === 'to') {
                    InitTrafficOrigin(distributor, pageType, startDate, endDate);
                    $('#to').addClass('type-selected');
                } else if (pageType.toLowerCase() === 'as') {
                    InitAdsStat(distributor, pageType, startDate, endDate);
                    $('#as').addClass('type-selected');
                } else if (pageType.toLowerCase() === 'aqs') {
                    InitAdsQuickStat(distributor, pageType, startDate, endDate, adUrl);
                    $('#as').addClass('type-selected');
                } else {
                    // detail visitor
                    $('#dv').addClass('type-selected');
                    RefreshTable('#test', "http://206.190.131.92:6009/SampleAnalytics.ashx?s=" + startDate + "&e=" + endDate + "&d=" + distributor + "&t=" + pageType);
                }
            } else
                RefreshTable('#test', "http://206.190.131.92:6009/SampleAnalytics.ashx?s=" + startDate + "&e=" + endDate + "&d=" + distributor + "&t=" + pageType);

            RefreshNavBar(distributor, startDate, endDate);
        });

        function RefreshNavBar(usr, start, end) {
            //$('.navbar-brand').attr('href', '?d=' + usr);
            $('.navbar-brand').attr('href', '/TrafficLeads.aspx');
            $("#navbar > ul").children().each(function () {
                switch ($(this).index()) {
                    case 0:
                        //$(this).children().eq(0).attr("href", '?d=' + usr + '&s=' + start + '&e=' + end + '&t=dv');
                        $(this).children().eq(0).attr("href", '?s=' + start + '&e=' + end + '&t=dv');
                        break;
                    case 1:
                        //$(this).children().eq(0).attr("href", '?d=' + usr + '&s=' + start + '&e=' + end + '&t=tot');
                        $(this).children().eq(0).attr("href", '?s=' + start + '&e=' + end + '&t=tot');
                        break;
                    case 2:
                        //$(this).children().eq(0).attr("href", '?d=' + usr + '&s=' + start + '&e=' + end + '&t=pv');
                        $(this).children().eq(0).attr("href", '?s=' + start + '&e=' + end + '&t=pv');
                        break;
                    case 3:
                        //$(this).children().eq(0).attr("href", '?d=' + usr + '&s=' + start + '&e=' + end + '&t=to');
                        $(this).children().eq(0).attr("href", '?s=' + start + '&e=' + end + '&t=to');
                        break;
                    case 4:
                        //$(this).children().eq(0).attr("href", '?s=' + start + '&e=' + end + '&t=as');
                        $(this).children().eq(1).children().each(function () {
                            switch ($(this).index()) {
                                case 0:
                                    $(this).children().eq(0).attr("href", '?s=' + start + '&e=' + end + '&t=aqs');
                                    break;
                                case 1:
                                    $(this).children().eq(0).attr("href", '?s=' + start + '&e=' + end + '&t=as');
                                    break;
                            }
                        });
                        break;
                    default:
                        break;

                }
            });
        }
        function InitDefault() {
            // default
            var time_period = {
                'Yesterday': { start: moment().subtract(1, 'days').format('YYYY-MM-DD'), end: moment().format('YYYY-MM-DD') },
                'Last_7_Days': { start: moment().subtract(6, 'days').format('YYYY-MM-DD'), end: moment().format('YYYY-MM-DD') },
                'Last_30_Days': { start: moment().subtract(29, 'days').format('YYYY-MM-DD'), end: moment().format('YYYY-MM-DD') },
                'This_Month': { start: moment().startOf('month').format('YYYY-MM-DD'), end: moment().endOf('month').format('YYYY-MM-DD') },
                'Last_Month': { start: moment().subtract(1, 'month').startOf('month').format('YYYY-MM-DD'), end: moment().subtract(1, 'month').endOf('month').format('YYYY-MM-DD') }
            };

            var table = $('#test').DataTable({
                //"processing": true,
                //"serverSide": true,
                "destroy": true,
                "ajax": {
                    "url": "http://206.190.131.92:6009/SampleAnalytics.ashx?d=" + distributor + "&ti=" + JSON.stringify(time_period),
                    //"type": 'POST',
                    "dataSrc": ""
                },

                "columns": [
                    // data is case sensitive....
                    {
                        "title": "Distributor", "data": "period", "render": function (data, type, full, meta) {
                            return distributor;
                        }
                    },
                    { "title": "Period", "data": "period" },
                    { "title": "New Visitors", "data": "visit.n" },
                    { "title": "Return Visitors", "data": "visit.r" },
                    {
                        "title": "Total Visitors", "data": "visit", "render": function (data, type, full, meta) {
                            return data.r + data.n;
                        }
                    }
                ],

                //"deferRender": true,
                "ordering": false,
                "searching": false,
                "paging": false,
                //"pagingType": "simple_numbers",
                "info": false,
                //"order": [],
                //"fixedHeader": true,
                //"responsive": true,
                //"lengthMenu": [20, 50, 80],
                //"stateSave": true
                //"initComplete": function (settings, json) {
                //    $('#test tbody').on('click', 'td', function (e, data) {
                //        console.log('Data: ' + $(this).html().trim());
                //        console.log('Row: ' + $(this).parent().find('td').html().trim());
                //        console.log('Column:' + $('#test thead tr th').eq($(this).index()).html().trim());
                //    });
                //},
                "initComplete": function (settings, json) {
                    $("#test_wrapper").append(spinner);
                }
            });
        }
        function InitDetailVisitors(usr, type, start, end) {
            var edit_option = {
                validate: function (value) {
                    if ($.trim(value) == '') {
                        return 'This field is required';
                    }
                }
            };
            var saveFn = function (e, params) {
                var $td = $(e.target).closest('td');
                var newValue = params.newValue;
                var oldValue = $td.children('a').html();
                // send ajax post request, if alias exist, then prompt error.

                $.ajax({
                    type: "POST",
                    url: "http://206.190.131.92:6009/SampleAnalytics.ashx",
                    data: {
                        update: new Date().getTime(),
                        t: 'a',
                        o: oldValue,
                        n: newValue,
                        d: distributor
                    },
                    success: function (data, status) {
                        // todo what if failed, when alias already exists.
                        //console.log(data);
                    }
                });

                // change all match cells value
                table.column(0).nodes().each(function (node, index, dt) {
                    if (table.cell(node).data() == oldValue) {
                        table.cell(node).data(newValue);
                        var cell = table.cell(node).node();

                        $(cell).children('a').editable(edit_option);
                        $(cell).children('a').one('save', saveFn);
                    }
                });
            };

            $('#reportrange').removeClass('hide-element');
            var table = $('#test').DataTable({
                //"processing": true,
                //"serverSide": true,
                "autoWidth": true,
                "destroy": true,
                "ajax": {
                    "url": "http://206.190.131.92:6009/SampleAnalytics.ashx?d=" + usr + "&s=" + start + "&e=" + end + "&t=" + type + "&b=" + blogname,
                    //"type": 'POST',
                    "dataSrc": ""
                },

                "columns": [
                    {
                        "title": "Visitor", "data": "alias", "render": function (data, type, row, meta) {
                            return '<a href="#" class="username" data-type="text" data-pk="1" data-title="Enter an alias name" data-placeholder="Required">' + data + '</a>';
                        }
                    },
                    { "title": "Visit Time", "data": "time" },
                    {
                        "title": "Status", "data": "type", "createdCell": function (cell, cellData, rowData, rowIndex, colIndex) {
                            if (cellData) {
                                if (cellData.toLowerCase() === 'new')
                                    $(cell).addClass('new-visitor');
                                else
                                    $(cell).addClass('return-visitor');
                            }
                        }
                    },
                    { "title": "Page", "data": "page" },
                    {
                        "title": "Location", "data": "country", "render": function (data, type, full, meta) {
                            var city, province, country;
                            if (full.city)
                                city = full.city + ', ';
                            if (full.province)
                                province = full.province + ', ';
                            return city + province + full.country;
                        }
                    },
                    {
                        "title": "Referrer", "data": "refer", "render": function (data, type, full, meta) {
                            if (type === "display" || type === 'filter') {
                                if (!data)
                                    return 'Direct Access';
                                else
                                    return data;
                            }

                            return data;
                        }
                    },
                    //{
                    //    "title": "Country", "data": "country", "render": function (data, type, full, meta) {
                    //        if (type === "display" || type === "filter")
                    //            if (!data)
                    //                return "N/A";
                    //        return data;
                    //    }
                    //},
                    //{
                    //    "title": "Region", "data": "province", "render": function (data, type, full, meta) {
                    //        if (type === "display" || type === "filter")
                    //            if (!data)
                    //                return "N/A";
                    //        return data;
                    //    }
                    //},
                    //{
                    //    "title": "City", "data": "city", "render": function (data, type, full, meta) {
                    //        if (type === "display" || type === "filter")
                    //            if (!data)
                    //                return "N/A";
                    //        return data;
                    //    }
                    //},
                    { "title": "IP", "data": "ip" }
                ],

                //"deferRender": true,
                "ordering": true,
                "searching": true,
                "paging": true,
                "pagingType": "simple_numbers",
                "info": true,
                "order": [],
                //"fixedHeader": true,
                //"responsive": true,
                "lengthMenu": [20, 50, 80],
                "fnDrawCallback": function (oSettings) {
                    $('.username').editable(edit_option);
                    $('.username').one('save', saveFn);
                },
                "initComplete": function (settings, json) {
                    $("#test_wrapper").append(spinner);
                },
                "sScrollX": "100%",
                "bScrollCollapse": true,
            });

            table.order([1, 'desc']).draw();
        }
        function InitTrafficOverTime(usr, type, start, end) {
            $('#reportrange').removeClass('hide-element');
            //$('#todo').addClass('hide-element');
            $.ajax({
                type: "GET",
                url: "http://206.190.131.92:6009/SampleAnalytics.ashx?d=" + usr + "&s=" + start + "&e=" + end + "&t=" + type,
                success: function (data, status) {
                    // todo what if failed, when alias already exists.
                    var dataSet = JSON.parse(data);
                    if (dataSet != null) {
                        var table = $('#test').DataTable({
                            //"processing": true,
                            //"serverSide": true,
                            "destroy": true,
                            //"ajax": {
                            //    "url": "http://206.190.131.92:6009/SampleAnalytics.ashx?d=" + usr + "&s=" + start + "&e=" + end + "&t=" + type,
                            //    //"type": 'POST',
                            //    "dataSrc": "",
                            //    "success": function (data, status) {
                            //        console.log(data);
                            //    }
                            //},
                            "data": dataSet,

                            "columns": [
                                // data is case sensitive....
                                {
                                    "title": "Distributor", "data": "", "render": function (data, type, full, meta) {
                                        return distributor;
                                    }
                                },
                                {
                                    "title": "Date", "data": "period", "render": function (data, type, full, meta) {
                                        //return moment.unix(parseInt(data)).format("YYYY-MM-DD");
                                        if (type === 'display')
                                            return moment(data, "YYYY-MM-DD").format("YY-MMM-DD");
                                        return data;
                                    }
                                },
                                { "title": "New Visitors", "data": "visit.n" },
                                { "title": "Return Visitors", "data": "visit.r" },
                                {
                                    "title": "Total Visitors", "data": "visit", "render": function (data, type, full, meta) {
                                        return data.r + data.n;
                                    }
                                }
                            ],

                            //"deferRender": true,
                            "ordering": true,
                            "searching": true,
                            "paging": true,
                            "pagingType": "simple_numbers",
                            "info": true,
                            "order": [],
                            //"fixedHeader": true,
                            //"responsive": true,
                            "lengthMenu": [20, 50, 80],
                            //"stateSave": true
                            //"initComplete": function (settings, json) {
                            //    $('#test tbody').on('click', 'td', function (e, data) {
                            //        console.log('Data: ' + $(this).html().trim());
                            //        console.log('Row: ' + $(this).parent().find('td').html().trim());
                            //        console.log('Column:' + $('#test thead tr th').eq($(this).index()).html().trim());
                            //    });
                            //},
                            "initComplete": function (settings, json) {
                                $("#test_wrapper").append(spinner);
                            }
                        });
                        table.order([1, 'asc']).draw();

                        var chartData = [];
                        var style_normal = 'opacity: 1.0';
                        var style_weekend = 'opacity: 0.6';
                        chartData.push(['Period', 'New', { role: 'style' }, 'Return', { role: 'style' }, 'Total', { role: 'style' }]);
                        dataSet.forEach(function (entry) {
                            var d = [];
                            //var mDate = moment.unix(parseInt(entry.period))
                            var mDate = moment(entry.period, "YYYY-MM-DD");
                            d.push(mDate.format("YY-MMM-DD"));
                            d.push(entry.visit.n);
                            var wDay = mDate.day();
                            if (wDay === 6 || wDay === 0)
                                d.push(style_weekend);
                            else
                                d.push(style_normal);
                            d.push(entry.visit.r);
                            if (wDay === 6 || wDay === 0)
                                d.push(style_weekend);
                            else
                                d.push(style_normal);
                            d.push(entry.visit.n + entry.visit.r);
                            if (wDay === 6 || wDay === 0)
                                d.push(style_weekend);
                            else
                                d.push(style_normal);
                            chartData.push(d);
                        });

                        function drawTotChart() {
                            if (dataSet.length > 0)
                                $('#chart-stat').removeClass('hide-element');
                            else
                                $('#chart-stat').addClass('hide-element');
                            var options = {
                                title: "Daily Page Visit",
                                width: '100%',
                                height: '100%',
                                axisTitlesPosition: 'out',
                                //'isStacked': true,
                                pieSliceText: 'percentage',
                                colors: ['#FF6161', '#1AA35F', '#1A87D1'],
                                //colors: ['#97C774', '#B63E98', '#D18E62', '#DB3E41', '#1BABA5'],
                                chartArea: {
                                    left: "5%",
                                    top: "10%",
                                    right: "20%",
                                    bottom: "30%",
                                    height: "85%",
                                    width: "80%"
                                },
                                vAxis: {
                                    //title: "Visit",
                                    format: ''
                                },
                                hAxis: {
                                    //title: "Date",
                                    //direction: -1,
                                    slantedText: true,
                                    slantedTextAngle: 75
                                }
                            };

                            // Create and populate the data table.
                            var data = google.visualization.arrayToDataTable(chartData);

                            // Create and draw the visualization.
                            new google.visualization.ColumnChart(document.getElementById('chart-stat')).draw(data, options);
                        };

                        function draw() {
                            if (!canAccessGoogleVisualization()) {
                                google.charts.load('current', { 'packages': ['corechart'] });
                                google.charts.setOnLoadCallback(function () {
                                    drawTotChart();
                                });
                            } else
                                drawTotChart();
                        }

                        $(window).resize(function () {
                            draw();
                        });

                        draw();
                    }
                }
            });
        }
        function InitPageView(usr, type, start, end) {
            $('#reportrange').removeClass('hide-element');

            $.ajax({
                type: "GET",
                url: "http://206.190.131.92:6009/SampleAnalytics.ashx?d=" + usr + "&s=" + start + "&e=" + end + "&t=" + type,
                success: function (data, status) {
                    var dataSet = JSON.parse(data);
                    if (dataSet != null) {
                        var table = $('#test').DataTable({
                            "destroy": true,
                            "data": dataSet,
                            "columns": [
                                { "title": "Page", "data": "page" },
                                { "title": "Visit", "data": "count" },
                                { "title": "Percentage", "data": "percentage" }
                            ],
                            "ordering": true,
                            "searching": true,
                            "paging": false,
                            //"pagingType": "simple_numbers",
                            "info": true,
                            //"order": [],
                            //"lengthMenu": [10, 30, 50],
                            "initComplete": function (settings, json) {
                                $("#test_wrapper").append(spinner);
                            }
                        });
                        table.order([1, 'desc']).draw();

                        var chartData = [];
                        chartData.push(['Page', 'Visit']);
                        dataSet.forEach(function (entry) {
                            var d = [];
                            d.push(entry.page);
                            d.push(entry.count);
                            chartData.push(d);
                        });

                        function drawPvChart() {
                            if (dataSet.length > 0)
                                $('#chart-stat').removeClass('hide-element');
                            else
                                $('#chart-stat').addClass('hide-element');
                            var options = {
                                title: "Page View Chart",
                                width: '100%',
                                height: '100%',
                                is3D: true,
                                colors: ['#FF6161', '#1AA35F', '#1A87D1', '#8381DA', '#675874'],
                                chartArea: {
                                    left: "5%",
                                    top: "5%",
                                    right: "20%",
                                    bottom: "5%",
                                    height: "100%",
                                    width: "100%"
                                }
                            };

                            // Create and populate the data table.
                            var data = google.visualization.arrayToDataTable(chartData);

                            // Create and draw the visualization.
                            new google.visualization.PieChart(document.getElementById('chart-stat')).draw(data, options);
                        };

                        function draw() {
                            if (!canAccessGoogleVisualization()) {
                                google.charts.load('current', { 'packages': ['corechart'] });
                                google.charts.setOnLoadCallback(function () {
                                    drawPvChart();
                                });
                            } else
                                drawPvChart();
                        }

                        $(window).resize(function () {
                            draw();
                        });

                        draw();
                    }
                }
            });
        }
        function InitTrafficOrigin(usr, type, start, end) {
            $('#reportrange').removeClass('hide-element');

            $.ajax({
                type: "GET",
                url: "http://206.190.131.92:6009/SampleAnalytics.ashx?d=" + usr + "&s=" + start + "&e=" + end + "&t=" + type,
                success: function (data, status) {
                    var dataSet = JSON.parse(data);
                    if (dataSet != null) {
                        var table = $('#test').DataTable({
                            "destroy": true,
                            "data": dataSet,
                            "columns": [
                               {
                                   "title": "Referrer", "data": "page", "render": function (data, type, full, meta) {
                                       if (type === "display" || type === 'filter') {
                                           if (!data)
                                               return 'Direct Access';
                                           else
                                               return data;
                                       }

                                       return data;
                                   }
                               },
                               { "title": "Visit", "data": "count" },
                               { "title": "Percentage", "data": "percentage" }
                            ],
                            "ordering": true,
                            "searching": true,
                            "paging": false,
                            //"pagingType": "simple_numbers",
                            "info": true,
                            //"order": [],
                            //"lengthMenu": [10, 30, 50],
                            "initComplete": function (settings, json) {
                                $("#test_wrapper").append(spinner);
                            }
                        });
                        table.order([1, 'desc']).draw();

                        var chartData = [];
                        chartData.push(['Referrer', 'Visit']);
                        // IE8 do NOT support foreach...., neither google chart.
                        dataSet.forEach(function (entry) {
                            var d = [];
                            if (entry.page)
                                d.push(entry.page);
                            else
                                d.push("Direct Access");
                            d.push(entry.count);
                            chartData.push(d);
                        });

                        function drawToChart() {
                            if (dataSet.length > 0)
                                $('#chart-stat').removeClass('hide-element');
                            else
                                $('#chart-stat').addClass('hide-element');
                            var options = {
                                title: "Referrer Chart",
                                width: '100%',
                                height: '100%',
                                is3D: true,
                                colors: ['#FF6161', '#1AA35F', '#1A87D1', '#8381DA', '#675874'],
                                //colors: ['#97C774', '#B63E98', '#D18E62', '#DB3E41', '#1BABA5'],
                                chartArea: {
                                    left: "5%",
                                    top: "5%",
                                    right: "20%",
                                    bottom: "5%",
                                    height: "100%",
                                    width: "100%"
                                }
                            };

                            // Create and populate the data table.
                            var data = google.visualization.arrayToDataTable(chartData);

                            // Create and draw the visualization.
                            new google.visualization.PieChart(document.getElementById('chart-stat')).draw(data, options);
                        };

                        function draw() {
                            if (!canAccessGoogleVisualization()) {
                                google.charts.load('current', { 'packages': ['corechart'] });
                                google.charts.setOnLoadCallback(function () {
                                    drawToChart();
                                });
                            } else
                                drawToChart();
                        }

                        $(window).resize(function () {
                            draw();
                        });

                        draw();
                    }
                }
            });
        }
        function InitAdsStat(usr, type, start, end, url) {
            $('#reportrange').removeClass('hide-element');
            var con = "";
            if (url) {
                con = "&u=" + url;
            }
            var table = $('#test').DataTable({
                //"processing": true,
                //"serverSide": true,
                "destroy": true,
                "ajax": {
                    "url": "http://206.190.131.92:6009/SampleAnalytics.ashx?d=" + usr + "&s=" + start + "&e=" + end + "&t=" + type + "&b=" + blogname + con,
                    //"type": 'POST',
                    "dataSrc": ""
                },

                "columns": [
                    {
                        "title": "Distributor", "data": "", render: function (data, type, full, meta) {
                            return distributor;
                        }
                    },
                    {
                        "title": "Banner", "data": "url", "render": function (data, type, full, meta) {
                            var wordArray = CryptoJS.enc.Utf8.parse(data);
                            var base64 = CryptoJS.enc.Base64.stringify(wordArray);
                            if (url)
                                return '<img class="img-thumbnail" src="' + data + '" alt="banner">';
                            else
                                return '<a href="?t=as' + '&s=' + start + '&e=' + end + '&u=' + base64 + '" target="_blank"><img class="img-thumbnail" src="' + data + '" alt="banner"></a>';
                            //return '<a href="' + data + '" target="_blank"><img src="' + data + '" alt="banner"></a>';
                        }
                    },
                    { "title": "Page", "data": "page" },
                    {
                        "title": "Time", "data": "time", "render": function (data, type, full, meta) {
                            if (type === "display" || type === "filter")
                                if (!data)
                                    return "N/A";
                            return data;
                        }
                    }
                ],

                //"deferRender": true,
                "ordering": true,
                "searching": true,
                "paging": true,
                "pagingType": "simple_numbers",
                "info": true,
                "order": [],
                //"fixedHeader": true,
                //"responsive": true,
                "lengthMenu": [20, 50, 80],
                "initComplete": function (settings, json) {
                    $("#test_wrapper").append(spinner);
                }
            });

            table.order([3, 'desc']).draw();
        }
        function InitAdsQuickStat(usr, type, start, end, url) {
            $('#reportrange').removeClass('hide-element');
            var con = "";
            if (url) {
                con = "&u=" + url;
            }
            var table = $('#test').DataTable({
                //"processing": true,
                //"serverSide": true,
                "destroy": true,
                "ajax": {
                    "url": "http://206.190.131.92:6009/SampleAnalytics.ashx?d=" + usr + "&s=" + start + "&e=" + end + "&t=" + type + "&b=" + blogname + con,
                    //"type": 'POST',
                    "dataSrc": ""
                },

                "columns": [
                    {
                        "title": "Distributor", "data": "", render: function (data, type, full, meta) {
                            return distributor;
                        }
                    },
                    {
                        "title": "Banner", "data": "url", "render": function (data, type, full, meta) {
                            var wordArray = CryptoJS.enc.Utf8.parse(data);
                            var base64 = CryptoJS.enc.Base64.stringify(wordArray);

                            return '<a href="?t=as' + '&s=' + start + '&e=' + end + '&u=' + base64 + '" target="_blank"><img class="img-thumbnail" src="' + data + '" alt="banner"></a>';
                            //return '<img src="' + data + '" alt="banner">';
                        }
                    },
                    { "title": "Page", "data": "page" },
                    { "title": "Count", "data": "count" }
                ],

                //"deferRender": true,
                "ordering": true,
                "searching": true,
                "paging": true,
                "pagingType": "simple_numbers",
                "info": true,
                "order": [],
                //"fixedHeader": true,
                //"responsive": true,
                "lengthMenu": [20, 50, 80],
                "initComplete": function (settings, json) {
                    $("#test_wrapper").append(spinner);
                }
            });

            table.order([3, 'desc']).draw();
        }

        cb(moment(startDate, "YYYY-MM-DD"), moment(endDate, "YYYY-MM-DD"));

        $('#reportrange').daterangepicker({
            //locale: {
            //    format: 'YYYY-MM-DD'
            //},
            ranges: {
                'Today': [moment(), moment()],
                'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
                'Last 7 Days': [moment().subtract(6, 'days'), moment()],
                'Last 30 Days': [moment().subtract(29, 'days'), moment()],
                'This Month': [moment().startOf('month'), moment().endOf('month')],
                'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
            },
            maxDate: moment(),
            startDate: moment(startDate, "YYYY-MM-DD"),
            endDate: moment(endDate, "YYYY-MM-DD")
        }, cb);

        $('#todo').html('<table id="test" class="hover order-column table-bordered"></table>');
        $('#reportrange').addClass('hide-element');
        if (distributor) {
            RefreshNavBar(distributor, startDate, endDate);
            if (pageType) {
                if (pageType.toLowerCase() === 'dv') {
                    // detail visitors
                    $("#title").html('Detail Visitors');
                    InitDetailVisitors(distributor, pageType, startDate, endDate);
                    $('#dv').addClass('type-selected');
                }
                else if (pageType.toLowerCase() === 'tot') {
                    // traffic over time
                    $("#title").html('Traffic Over Time');
                    InitTrafficOverTime(distributor, pageType, startDate, endDate);
                    $('#tot').addClass('type-selected');
                } else if (pageType.toLowerCase() === 'pv') {
                    // page view
                    $("#title").html('Page Views(Top 5)');
                    InitPageView(distributor, pageType, startDate, endDate);
                    $('#pv').addClass('type-selected');
                } else if (pageType.toLowerCase() === 'to') {
                    // traffic origin -- refer
                    $("#title").html('Traffic Origin(Top 5)');
                    InitTrafficOrigin(distributor, pageType, startDate, endDate);
                    $('#to').addClass('type-selected');
                } else if (pageType.toLowerCase() === 'as') {
                    $("#title").html('Ads Details');
                    InitAdsStat(distributor, pageType, startDate, endDate, adUrl);
                    $('#as').addClass('type-selected');
                } else if (pageType.toLowerCase() === 'aqs') {
                    $("#title").html('Ads Quick Stats');
                    InitAdsQuickStat(distributor, pageType, startDate, endDate, adUrl);
                    $('#as').addClass('type-selected');
                } else {
                    $("#title").html('Quick Stats');
                    InitDefault();
                }
            }
            else {
                $("#title").html('Quick Stats');
                InitDefault();
            }
        }
    });
})(window, document);