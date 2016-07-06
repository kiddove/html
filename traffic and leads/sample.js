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
    var table;
    var pageType = getParameterByName('t');
    var adUrl;
    if (pageType && pageType.toLowerCase() === 'as') {
        adUrl = getParameterByName('u');
    }
    var alias;
    if (pageType && pageType.toLowerCase() === 'si') {
        alias = getParameterByName('a');
    }

    var distributor = "kectech";
    //var distributor = "brendacook";
    //var distributor;
    if (typeof showname !== 'undefined') {
        distributor = showname;
    }

    var blogname = "healthylife";
    //var blogname = "brendacook";
    //var blogname;
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

    var ad_click_time;
    ad_click_time = getParameterByName('ac');

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
                } else if (pageType.toLowerCase() === 'tfr') {
                    InitTrafficFromRegion(distributor, pageType, startDate, endDate);
                    $('#tfr').addClass('type-selected');
                } else if (pageType.toLowerCase() === 'si') {
                    InitSingleVisitor(distributor, pageType, startDate, endDate, alias, ad_click_time);
                    $('#dv').addClass('type-selected');
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
            $('.navbar-brand').attr('href', window.location.origin + window.location.pathname);
            $("#navbar > ul").children().each(function () {
                switch ($(this).index()) {
                    case 0:
                        $(this).children().eq(0).attr("href", '?s=' + start + '&e=' + end + '&t=dv');
                        break;
                    case 1:
                        $(this).children().eq(1).children().each(function () {
                            switch ($(this).index()) {
                                case 0:
                                    $(this).children().eq(0).attr("href", '?s=' + start + '&e=' + end + '&t=tot');
                                    break;
                                case 1:
                                    $(this).children().eq(0).attr("href", '?s=' + start + '&e=' + end + '&t=tfr');
                                    break;
                                case 2:
                                    $(this).children().eq(0).attr("href", '?s=' + start + '&e=' + end + '&t=pv');
                                    break;
                                case 3:
                                    $(this).children().eq(0).attr("href", '?s=' + start + '&e=' + end + '&t=to');
                                    break;
                            }
                        });
                        break;
                    case 2:
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

        function HilightRow() {
            $('#test tbody').on('click', 'tr', function () {
                if ($(this).hasClass('selected')) {
                    $(this).removeClass('selected');
                }
                else {
                    table.$('tr.selected').removeClass('selected');
                    $(this).addClass('selected');
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

            table = $('#test').DataTable({
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
                    { "title": "New Visitors", "data": "visitor.n" },
                    { "title": "Return Visitors", "data": "visitor.r" },
                    {
                        "title": "Total Visitors", "data": "visitor", "render": function (data, type, full, meta) {
                            return data.r + data.n;
                        }
                    },
                    {
                        "title": "Total PageView", "data": "visit", "render": function (data, type, full, meta) {
                            return data;
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
                    HilightRow();
                    //$(window).trigger('resize');
                }
            });
        }
        function InitDetailVisitors(usr, type, start, end) {

            var edit_option = {
                validate: function (value) {
                    if ($.trim(value) == '') {
                        return 'This field is required';
                    } else if (value.indexOf('#') > -1) {
                        return 'Cannot contain special charactors';
                    }
                },
                url: "http://206.190.131.92:6009/SampleAnalytics.ashx",
                send: "always",
                params: function (params) {
                    //The params already have the default pk, name and value.
                    var data = {};
                    data['t'] = 'a';
                    data['n'] = params.value;
                    data['o'] = $(this).text();
                    data['d'] = usr;
                    data['update'] = new Date().getTime();
                    return data;
                },
                success: function (response, newValue) {
                    // data.success
                    // data.message
                    // data.key
                    var dataObj = JSON.parse(response);
                    if (dataObj.success != 1) {
                        bootbox.dialog({
                            message: dataObj.message,
                            //title: "Custom title",
                            //buttons: {
                            //    success: {
                            //        label: "Success!",
                            //        className: "btn-success",
                            //        callback: function () {
                            //            Example.show("great success");
                            //        }
                            //    },
                            //    danger: {
                            //        label: "Danger!",
                            //        className: "btn-danger",
                            //        callback: function () {
                            //            Example.show("uh oh, look out!");
                            //        }
                            //    },
                            //    main: {
                            //        label: "Click ME!",
                            //        className: "btn-primary",
                            //        callback: function () {
                            //            Example.show("Primary button");
                            //        }
                            //    }
                            //}
                        });
                        return { newValue: dataObj.key };
                    }
                    else {
                        // change all match cells value
                        table.column(0).nodes().each(function (node, index, dt) {
                            if (table.cell(node).data() == dataObj.key) {
                                table.cell(node).data(newValue);
                                var cell = table.cell(node).node();

                                $(cell).children('a').editable(edit_option);
                                //$(cell).children('a').one('save', saveFn);
                            }
                        });
                    }
                },
                error: function (response, newValue) {
                    if (response.status === 500) {
                        return 'Service unavailable. Please try later.';
                    } else {
                        return response.responseText;
                    }
                }
            };
            $('#reportrange').removeClass('hide-element');
            table = $('#test').DataTable({
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
                    { "title": "Page", "data": "page", "render": $.fn.dataTable.render.ellipsis(30) },
                    {
                        "title": "Video", "data": "video", "width": "6em", "render": function (data, type, full, meta) {
                            if (!data)
                                return "N/A";
                            else
                                return '<img class="img-thumbnail" src="' + data + '" alt="banner">';
                        }
                    },
                    {
                        "title": "Location", "data": "", "render": function (data, type, full, meta) {
                            var city = province = country = "";
                            if (full.city)
                                city = full.city + ', ';
                            if (full.province)
                                province = full.province + ', ';
                            //return city + province + full.country;

                            if (type === "display") {
                                return $.fn.dataTable.render.ellipsis(30)(city + province + full.country, type, full);
                            } else
                                return city + province + full.country;
                        }
                    },
                    {
                        "title": "Referrer", "data": "refer", "render": function (data, type, full, meta) {
                            //if (type === "display" || type === 'filter') {
                            //    if (!data)
                            //        return 'Direct Access';
                            //    else
                            //        return data;
                            //}

                            //return data;

                            if (type === "filter") {
                                if (!data)
                                    return 'Direct Access';
                                else
                                    return data;
                            } else if (type === "display") {
                                if (!data)
                                    return 'Direct Access';
                                else
                                    return $.fn.dataTable.render.ellipsis(50)(data, type, full);
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
                    { "title": "IP", "data": "ip" },

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
                    //$('.username').one('save', saveFn);
                },
                "initComplete": function (settings, json) {
                    $("#test_wrapper").append(spinner);
                    HilightRow();
                    //table.columns.adjust().draw();
                },
                //"sScrollX": "100%",
                "bScrollCollapse": true,
                scrollX: true,
                //"autoWidth": false,
                //"columnDefs": [
                //    {
                //        "targets": [5, 6],
                //        render: $.fn.dataTable.render.ellipsis(30)
                //    }
                //]
                //if editable can not be fixed
                //fixedColumns: {
                //    leftColumns:1
                //}

                //"createdRow": function (row, data, index) {
                //    if (data.video) {
                //        //$('td', row).parent().eq(0).addClass('selected');
                //        $(row).eq(0).addClass('highlighted');
                //    }
                //}
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
                        table = $('#test').DataTable({
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
                                { "title": "New Visitors", "data": "visitor.n" },
                                { "title": "Return Visitors", "data": "visitor.r" },
                                {
                                    "title": "Total Visitors", "data": "visitor", "render": function (data, type, full, meta) {
                                        return data.r + data.n;
                                    }
                                },
                                {
                                    "title": "Total PageView", "data": "visit", "render": function (data, type, full, meta) {
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
                                HilightRow();
                                //$(window).trigger('resize');
                            },
                            "sScrollX": "100%",
                            "bScrollCollapse": true,
                        });
                        table.order([1, 'desc']).draw();

                        var chartData = [];
                        var style_normal = 'opacity: 1.0';
                        var style_weekend = 'opacity: 0.6';
                        chartData.push(['Period', 'New', { role: 'style' }, 'Return', { role: 'style' }, 'Total', { role: 'style' }]);
                        dataSet.forEach(function (entry) {
                            var d = [];
                            //var mDate = moment.unix(parseInt(entry.period))
                            var mDate = moment(entry.period, "YYYY-MM-DD");
                            d.push(mDate.format("YY-MMM-DD"));
                            d.push(entry.visitor.n);
                            var wDay = mDate.day();
                            if (wDay === 6 || wDay === 0)
                                d.push(style_weekend);
                            else
                                d.push(style_normal);
                            d.push(entry.visitor.r);
                            if (wDay === 6 || wDay === 0)
                                d.push(style_weekend);
                            else
                                d.push(style_normal);
                            d.push(entry.visitor.n + entry.visitor.r);
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
                                title: "Daily Visitors",
                                width: '100%',
                                height: '100%',
                                axisTitlesPosition: 'out',
                                //'isStacked': true,
                                pieSliceText: 'percentage',
                                colors: ['#FF6161', '#1AA35F', '#1A87D1'],
                                //colors: ['#FF6161', '#97C774', '#B63E98', '#D18E62', '#DB3E41', '#1BABA5', '#CE7676'],
                                chartArea: {
                                    left: "8%",
                                    top: "10%",
                                    right: "14%",
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
                        table = $('#test').DataTable({
                            "destroy": true,
                            "data": dataSet.list,
                            "columns": [
                                { "title": "Page", "data": "page", "render": $.fn.dataTable.render.ellipsis(90) },
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
                                HilightRow();
                                //$(window).trigger('resize');
                            },
                            "sScrollX": "100%",
                            "bScrollCollapse": true,
                        });
                        table.order([1, 'desc']).draw();

                        var chartData = [];
                        var rest = 0;
                        chartData.push(['Page', 'Visit']);
                        dataSet.list.forEach(function (entry) {
                            var d = [];
                            d.push(entry.page);
                            d.push(entry.count);
                            chartData.push(d);
                            rest += entry.count;
                        });

                        if (dataSet.total > rest)
                            chartData.push(["Other Pages", dataSet.total - rest]);


                        function drawPvChart() {

                            var pieTitle = "Page View Chart";
                            if (dataSet.list.length > 0)
                                $('#chart-stat').removeClass('hide-element');
                            else
                                $('#chart-stat').addClass('hide-element');
                            var options = {
                                title: pieTitle,
                                titlePosition: "out",
                                width: '100%',
                                height: '100%',
                                is3D: true,
                                //colors: ['#FF6161', '#1AA35F', '#1A87D1', '#C39BD3', '#675874', '#97C774', '#B63E98', '#D18E62', '#8381DA', '#581845', '#0C6B65'],
                                //colors: ['#C0392B', '#E74C3C', '#9B59B6', '#8E44AD', '#2980B9', '#3498DB', '#1ABC9C', '#16A085', '#27AE60', '#2ECC71', '#34495E', '#2C3E50', '#D35400', '#E67E22'],
                                colors: [
                                    '#FF6161', '#1AA35F', '#1A87D1',
                                    '#9B59B6', '#1ABC9C', '#566573',
                                    '#D6428A', '#586E36', '#44B2AF',
                                    '#A2393B', '#2ECC71', '#2C3E50',
                                ],
                                //colors: ['#97C774', '#B63E98', '#D18E62', '#DB3E41', '#1BABA5'],8381DA
                                chartArea: {
                                    left: "3%",
                                    top: "5%",
                                    right: "5%",
                                    bottom: "5%",
                                    height: "100%",
                                    width: "100%"
                                },
                                sliceVisibilityThreshold: 0
                            };

                            // Create and populate the data table.
                            var data = google.visualization.arrayToDataTable(chartData);

                            // Create and draw the visualization.
                            new google.visualization.PieChart(document.getElementById('chart-stat')).draw(data, options);
                            //$("text:contains(" + pieTitle + ")").attr({ 'x': '1' });
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
                        table = $('#test').DataTable({
                            "destroy": true,
                            "data": dataSet.list,
                            "columns": [
                               {
                                   "title": "Referrer", "data": "page", "render": function (data, type, full, meta) {
                                       //if (type === "display" || type === 'filter') {
                                       //    if (!data)
                                       //        return 'Direct Access';
                                       //    else
                                       //        return data;
                                       //}

                                       //return data;
                                       if (type === "filter") {
                                           if (!data)
                                               return 'Direct Access';
                                           else
                                               return data;
                                       } else if (type === "display") {
                                           if (!data)
                                               return 'Direct Access';
                                           else
                                               return $.fn.dataTable.render.ellipsis(90)(data, type, full);
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
                                HilightRow();
                                //$(window).trigger('resize');
                            },
                            "sScrollX": "100%",
                            "bScrollCollapse": true,
                        });
                        table.order([1, 'desc']).draw();

                        var chartData = [];
                        chartData.push(['Referrer', 'Visit']);
                        // IE8 do NOT support foreach...., neither google chart.
                        var rest = 0;
                        dataSet.list.forEach(function (entry) {
                            var d = [];
                            if (entry.page)
                                d.push(entry.page);
                            else
                                d.push("Direct Access");
                            d.push(entry.count);
                            chartData.push(d);
                            rest += entry.count;
                        });

                        chartData.push(["Other Referres", dataSet.total - rest]);

                        function drawToChart() {
                            var pieTitle = "Referrer Chart"
                            if (dataSet.list.length > 0)
                                $('#chart-stat').removeClass('hide-element');
                            else
                                $('#chart-stat').addClass('hide-element');
                            var options = {
                                title: pieTitle,
                                titlePosition: "out",
                                width: '100%',
                                height: '100%',
                                is3D: true,
                                colors: [
                                    '#FF6161', '#1AA35F', '#1A87D1',
                                    '#9B59B6', '#1ABC9C', '#566573',
                                    '#D6428A', '#586E36', '#44B2AF',
                                    '#A2393B', '#2ECC71', '#2C3E50',
                                ],
                                //colors: ['#97C774', '#B63E98', '#D18E62', '#DB3E41', '#1BABA5'],
                                chartArea: {
                                    left: "3%",
                                    top: "5%",
                                    right: "5%",
                                    bottom: "5%",
                                    height: "100%",
                                    width: "100%"
                                }
                            };

                            // Create and populate the data table.
                            var data = google.visualization.arrayToDataTable(chartData);

                            // Create and draw the visualization.
                            new google.visualization.PieChart(document.getElementById('chart-stat')).draw(data, options);

                            //$("text:contains(" + pieTitle + ")").attr({ 'x': '1' });
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
            table = $('#test').DataTable({
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
                        "title": "Visitor", "data": "alias", render: function (data, type, full, meta) {
                            //return data;
                            //return '<a href="#" class="username" data-type="text" data-pk="1" data-title="Enter an alias name" data-placeholder="Required">' + data + '</a>';
                            return '<a href="?t=si' + '&s=' + start + '&e=' + end + '&a=' + data + '&d=' + distributor + '">' + data + '</a>';
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
                    { "title": "Page", "data": "page", "render": $.fn.dataTable.render.ellipsis(75) },
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
                    HilightRow();
                    //$(window).trigger('resize');
                },
                "sScrollX": "100%",
                "bScrollCollapse": true,
            });

            table.order([3, 'desc']).draw();
        }
        function InitAdsQuickStat(usr, type, start, end, url) {
            $('#reportrange').removeClass('hide-element');
            var con = "";
            if (url) {
                con = "&u=" + url;
            }
            table = $('#test').DataTable({
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
                    //{ "title": "Page", "data": "page" },
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
                    HilightRow();
                    //$(window).trigger('resize');
                },
                //"sScrollX": "100%",
                scrollX: true,
                "bScrollCollapse": true,
            });

            table.order([2, 'desc']).draw();
        }
        function InitSingleVisitor(usr, type, start, end, alias, time) {
            var edit_option = {
                validate: function (value) {
                    if ($.trim(value) == '') {
                        return 'This field is required';
                    } else if (value.indexOf('#') > -1) {
                        return 'Cannot contain special charactors';
                    }
                },
                url: "http://206.190.131.92:6009/SampleAnalytics.ashx",
                send: "always",
                params: function (params) {
                    //The params already have the default pk, name and value.
                    var data = {};
                    data['t'] = 'a';
                    data['n'] = params.value;
                    data['o'] = $(this).text();
                    data['d'] = usr;
                    data['update'] = new Date().getTime();
                    return data;
                },
                success: function (response, newValue) {
                    // data.success
                    // data.message
                    // data.key
                    var dataObj = JSON.parse(response);
                    if (dataObj.success != 1) {
                        bootbox.dialog({
                            message: "I am a custom dialog",
                            title: "Custom title",
                            buttons: {
                                success: {
                                    label: "Success!",
                                    className: "btn-success",
                                    callback: function () {
                                        Example.show("great success");
                                    }
                                },
                                danger: {
                                    label: "Danger!",
                                    className: "btn-danger",
                                    callback: function () {
                                        Example.show("uh oh, look out!");
                                    }
                                },
                                main: {
                                    label: "Click ME!",
                                    className: "btn-primary",
                                    callback: function () {
                                        Example.show("Primary button");
                                    }
                                }
                            }
                        });
                        return { newValue: dataObj.key };
                    }
                    else {
                        // change all match cells value
                        table.column(0).nodes().each(function (node, index, dt) {
                            if (table.cell(node).data() == dataObj.key) {
                                table.cell(node).data(newValue);
                                var cell = table.cell(node).node();

                                $(cell).children('a').editable(edit_option);
                                //$(cell).children('a').one('save', saveFn);
                            }
                        });
                    }
                },
                error: function (response, newValue) {
                    if (response.status === 500) {
                        return 'Service unavailable. Please try later.';
                    } else {
                        return response.responseText;
                    }
                }
            };
            //var saveFn = function (e, params) {
            //    var $td = $(e.target).closest('td');
            //    var newValue = params.newValue;
            //    var oldValue = $td.children('a').html();
            //    // send ajax post request, if alias exist, then prompt error.

            //    $.ajax({
            //        type: "POST",
            //        url: "http://206.190.131.92:6009/SampleAnalytics.ashx",
            //        data: {
            //            update: new Date().getTime(),
            //            t: 'a',
            //            o: oldValue,
            //            n: newValue,
            //            d: distributor
            //        },
            //        success: function (data, status) {
            //            // todo what if failed, when alias already exists.
            //            //console.log(data);
            //        }
            //    });

            //    // change all match cells value
            //    table.column(0).nodes().each(function (node, index, dt) {
            //        if (table.cell(node).data() == oldValue) {
            //            table.cell(node).data(newValue);
            //            var cell = table.cell(node).node();

            //            $(cell).children('a').editable(edit_option);
            //            $(cell).children('a').one('save', saveFn);
            //        }
            //    });
            //};

            $('#reportrange').removeClass('hide-element');
            table = $('#test').DataTable({
                //"processing": true,
                //"serverSide": true,
                "autoWidth": true,
                "destroy": true,
                "ajax": {
                    "url": "http://206.190.131.92:6009/SampleAnalytics.ashx?d=" + usr + "&s=" + start + "&e=" + end + "&t=" + type + "&b=" + blogname + "&a=" + alias,
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
                    { "title": "Page", "data": "page", "render": $.fn.dataTable.render.ellipsis(30) },
                    {
                        "title": "Video", "data": "video", "width": "6em", "render": function (data, type, full, meta) {
                            if (!data)
                                return "N/A";
                            else
                                return '<img class="img-thumbnail" src="' + data + '" alt="banner">';
                        }
                    },
                    {
                        "title": "Location", "data": "", "render": function (data, type, full, meta) {
                            var city = province = country = "";
                            if (full.city)
                                city = full.city + ', ';
                            if (full.province)
                                province = full.province + ', ';
                            //return city + province + full.country;
                            if (type === "display") {
                                return $.fn.dataTable.render.ellipsis(30)(city + province + full.country, type, full);
                            } else
                                return city + province + full.country;
                        }
                    },
                    {
                        "title": "Referrer", "data": "refer", "render": function (data, type, full, meta) {
                            //if (type === "display" || type === 'filter') {
                            //    if (!data)
                            //        return 'Direct Access';
                            //    else
                            //        return data;
                            //}

                            //return data;
                            if (type === "filter") {
                                if (!data)
                                    return 'Direct Access';
                                else
                                    return data;
                            } else if (type === "display") {
                                if (!data)
                                    return 'Direct Access';
                                else
                                    return $.fn.dataTable.render.ellipsis(50)(data, type, full);
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
                    //$('.username').one('save', saveFn);
                },
                "initComplete": function (settings, json) {
                    $("#test_wrapper").append(spinner);
                    HilightRow();
                    //$(window).trigger('resize');
                },
                //"sScrollX": "100%",
                scrollX: true,
                "bScrollCollapse": true,
                autoWidth: false,
                //"createdRow": function (row, data, index) {
                //    if (data.video) {
                //        //$('td', row).parent().eq(0).addClass('selected');
                //        $(row).eq(0).addClass('highlighted');
                //    }
                //}
            });

            table.order([1, 'desc']).draw();
        }
        function InitTrafficFromRegion(usr, type, start, end) {
            $('#reportrange').removeClass('hide-element');

            $.ajax({
                type: "GET",
                url: "http://206.190.131.92:6009/SampleAnalytics.ashx?d=" + usr + "&s=" + start + "&e=" + end + "&t=" + type + "&b=" + blogname,
                success: function (data, status) {
                    var dataSet = JSON.parse(data);
                    if (dataSet != null) {
                        table = $('#test').DataTable({
                            "destroy": true,
                            "data": dataSet.list,
                            "columns": [
                                {
                                    "title": "Region", "data": "", "render": function (data, type, full, meta) {
                                        var city, province, country;
                                        if (full.page)
                                            city = full.page + ', ';
                                        if (full.province)
                                            province = full.province + ', ';
                                        return city + province + full.country;
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
                                HilightRow();
                                //$(window).trigger('resize');
                            },
                            "sScrollX": "100%",
                            "bScrollCollapse": true,
                        });
                        table.order([1, 'desc']).draw();

                        var chartData = [];
                        var rest = 0;
                        chartData.push(['Region', 'Visit']);
                        dataSet.list.forEach(function (entry) {
                            var d = [];
                            d.push(entry.page);
                            d.push(entry.count);
                            chartData.push(d);
                            rest += entry.count;
                        });
                        if (dataSet.total > rest)
                            chartData.push(["Other Region", dataSet.total - rest]);


                        function drawPvChart() {

                            var pieTitle = "Traffic Region Chart";
                            if (dataSet.list.length > 0)
                                $('#chart-stat').removeClass('hide-element');
                            else
                                $('#chart-stat').addClass('hide-element');
                            var options = {
                                title: pieTitle,
                                titlePosition: "out",
                                width: '100%',
                                height: '100%',
                                is3D: true,
                                //colors: ['#FF6161', '#1AA35F', '#1A87D1', '#C39BD3', '#675874', '#97C774', '#B63E98', '#D18E62', '#8381DA', '#581845', '#0C6B65'],
                                //colors: ['#C0392B', '#E74C3C', '#9B59B6', '#8E44AD', '#2980B9', '#3498DB', '#1ABC9C', '#16A085', '#27AE60', '#2ECC71', '#34495E', '#2C3E50', '#D35400', '#E67E22'],
                                colors: [
                                    '#FF6161', '#1AA35F', '#1A87D1',
                                    '#9B59B6', '#1ABC9C', '#566573',
                                    '#D6428A', '#586E36', '#44B2AF',
                                    '#A2393B', '#2ECC71', '#2C3E50',
                                ],
                                //colors: ['#97C774', '#B63E98', '#D18E62', '#DB3E41', '#1BABA5'],8381DA
                                chartArea: {
                                    left: "3%",
                                    top: "5%",
                                    right: "5%",
                                    bottom: "5%",
                                    height: "100%",
                                    width: "100%"
                                },
                                sliceVisibilityThreshold: 0
                            };

                            // Create and populate the data table.
                            var data = google.visualization.arrayToDataTable(chartData);

                            // Create and draw the visualization.
                            new google.visualization.PieChart(document.getElementById('chart-stat')).draw(data, options);
                            //$("text:contains(" + pieTitle + ")").attr({ 'x': '1' });
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

        $('#todo').html('<table id="test" class="hover order-column table-bordered table" cellspacing="0" width="100%"></table>');
        $('#reportrange').addClass('hide-element');
        if (distributor) {
            RefreshNavBar(distributor, startDate, endDate);
            if (pageType) {
                if (pageType.toLowerCase() === 'dv') {
                    // detail visitors
                    $("#title").html('Visit Detail');
                    InitDetailVisitors(distributor, pageType, startDate, endDate);
                    $('#dv').addClass('type-selected');
                }
                else if (pageType.toLowerCase() === 'tot') {
                    // traffic over time
                    $("#title").html('Traffic Over Time');
                    InitTrafficOverTime(distributor, pageType, startDate, endDate);
                    //$('#tot').addClass('type-selected');
                    $('#ts').addClass('type-selected');
                } else if (pageType.toLowerCase() === 'pv') {
                    // page view
                    $("#title").html('Page Views(Top 10)');
                    InitPageView(distributor, pageType, startDate, endDate);
                    //$('#pv').addClass('type-selected');
                    $('#ts').addClass('type-selected');
                } else if (pageType.toLowerCase() === 'to') {
                    // traffic origin -- refer
                    $("#title").html('Traffic Origin(Top 5)');
                    InitTrafficOrigin(distributor, pageType, startDate, endDate);
                    //$('#to').addClass('type-selected');
                    $('#ts').addClass('type-selected');
                } else if (pageType.toLowerCase() === 'as') {
                    $("#title").html('Ads Details');
                    InitAdsStat(distributor, pageType, startDate, endDate, adUrl);
                    $('#as').addClass('type-selected');
                    //$('#acd').addClass('type-selected');
                } else if (pageType.toLowerCase() === 'aqs') {
                    $("#title").html('Ads Quick Stats');
                    InitAdsQuickStat(distributor, pageType, startDate, endDate, adUrl);
                    $('#as').addClass('type-selected');
                    //$('#aqs').addClass('type-selected');
                } else if (pageType.toLowerCase() === 'si') {
                    // single visitors
                    $("#title").html('Visit Detail');
                    InitSingleVisitor(distributor, pageType, startDate, endDate, alias);
                    $('#dv').addClass('type-selected');
                } else if (pageType.toLowerCase() === 'tfr') {
                    // single visitors
                    $("#title").html('Traffic From Region(Top 10)');
                    InitTrafficFromRegion(distributor, pageType, startDate, endDate, blogname);
                    //$('#tfr').addClass('type-selected');
                    $('#ts').addClass('type-selected');
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