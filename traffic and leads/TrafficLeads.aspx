﻿<%@ Page Language="C#" MasterPageFile="~/WebTemplate.Master" AutoEventWireup="true" CodeBehind="TrafficLeads.aspx.cs" Inherits="WysLink.TrafficLeads" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContentHolder" runat="server">
<!--<meta charset="utf-8">-->
<!--<meta http-equiv="X-UA-Compatible" content="IE=edge">-->
<!--<meta name="viewport" content="width=device-width, initial-scale=1">-->
    <!-- The above 3 meta tags *must* come first in the head; any other head content must come *after* these tags -->
    <!--<link rel="icon" href="../favicon.ico">-->
    <title>Traffic And Leads</title>
    <!--jquery and bootstrap-->
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.12.2/jquery.min.js"></script>
    <!--Cross-Domain AJAX for IE8 and IE9 https://github.com/MoonScript/jQuery-ajaxTransport-XDomainRequest -->
    <!--[if lte IE 9]>
    <script src="http://cdnjs.cloudflare.com/ajax/libs/jquery-ajaxtransport-xdomainrequest/1.0.3/jquery.xdomainrequest.min.js"></script>
    <![endif]-->

    <!--<script src="https://code.jquery.com/jquery-2.2.3.min.js" integrity="sha256-a23g1Nt4dtEYOj7bR+vTu7+T8VP13humZFBJNIYoEJo=" crossorigin="anonymous"></script>-->
    <link rel="stylesheet" type="text/css" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/css/bootstrap.min.css">
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/js/bootstrap.min.js"></script>

    <!--originnal jquery datatable-->
    <script src="https://cdn.datatables.net/1.10.11/js/jquery.dataTables.min.js"></script>
    <link rel="stylesheet" type="text/css" href="https://cdn.datatables.net/1.10.11/css/jquery.dataTables.min.css" />

    <!--elipsis-->
    <script type="text/javascript" src="https://cdn.datatables.net/plug-ins/1.10.12/dataRender/ellipsis.js"></script>

    <!--Parse, validate, manipulate, and display dates in JavaScript. http://momentjs.com/-->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.13.0/moment.min.js"></script>

    <!-- Include Date Range Picker -->
    <script type="text/javascript" src="https://cdn.jsdelivr.net/bootstrap.daterangepicker/2/daterangepicker.js"></script>
    <link rel="stylesheet" type="text/css" href="https://cdn.jsdelivr.net/bootstrap.daterangepicker/2/daterangepicker.css" />

    <!--x-editable-->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/x-editable/1.5.0/bootstrap3-editable/css/bootstrap-editable.css" rel="stylesheet" />
    <script src="https://cdnjs.cloudflare.com/ajax/libs/x-editable/1.5.0/bootstrap3-editable/js/bootstrap-editable.min.js"></script>

    <!--google chart-->
    <script type="text/javascript" src="https://www.gstatic.com/charts/loader.js"></script>

    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/crypto-js/3.1.2/components/core-min.js"></script>
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/crypto-js/3.1.2/components/enc-base64-min.js"></script>

    <script type="text/javascript">
        var showname = '<% =showname %>';
        var blog = '<% =blog %>';
    </script>

    <script src="./js/stat/bootbox.min.js"></script>
    <script src="./js/stat/sample.js"></script>
    <link rel="stylesheet" type="text/css" href="./js/stat/sample.css">

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContentHolder" Runat="Server">
    <div class="myContent">
        <nav class="navbar navbar-inverse">
            <div class="container-fluid">
                <div class="navbar-header">
                    <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#navbar" aria-expanded="false" aria-controls="navbar">
                        <span class="sr-only">Toggle navigation</span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                    </button>
                    <a class="navbar-brand">Traffic And Leads</a>
                </div>
                <div id="navbar" class="navbar-collapse collapse" aria-expanded="false" style="height: 1px;">
                    <ul class="nav navbar-nav navbar-right">
                        <li><a href="#" id="dv">Visit Detail</a></li>
                        <li class="dropdown">
                            <a class="dropdown-toggle" data-toggle="dropdown" href="#" id="ts">Traffic Stat
                            <span class="caret"></span></a>
                            <ul class="dropdown-menu">
                                <li><a href="#" id="tot">Traffic Over Time</a></li>
                                <li><a href="#" id="tfr">Traffic From Region</a></li>
                                <li><a href="#" id="pv">Page View</a></li>
                                <li><a href="#" id="to">Traffic Origin</a></li>
                            </ul>
                        </li>
                        <!--<li><a href="#" id="as">Ads Stat</a></li>-->
                        <li class="dropdown">
                            <a class="dropdown-toggle" data-toggle="dropdown" href="#" id="as">Ads Stat
                            <span class="caret"></span></a>
                            <ul class="dropdown-menu">
                                <li><a href="#" id="aqs">Quick Stat</a></li>
                                <li><a href="#" id="acd">Click Detail</a></li>
                            </ul>
                        </li>
                        <!--<li><a href="#" id="ls">Leads Stat</a></li>-->
                    </ul>
                </div>
            </div>
        </nav>

        <div class="container-fluid">
            <h3 id="title" style="float: left">Quick Stats</h3>
            <div id="reportrange" class="range-picker pull-left">
                <i class="glyphicon glyphicon-calendar fa fa-calendar"></i>
                <span></span><b class="caret"></b>
            </div>
            <div class="table-responsive" id="todo">
                <div id="spinner" class="spinner hide-element">
                    <img id="img-spinner" src="./js/stat/ajax-loader.gif" alt="Loading" />
                </div>
            </div>
            <div id="chart-stat" class="chart-stat hide-element"></div>
        </div>
    </div>

    <script>
        (function (i, s, o, g, r, a, m) {
            i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () {
                (i[r].q = i[r].q || []).push(arguments)
            }, i[r].l = 1 * new Date(); a = s.createElement(o),
            m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)
        })(window, document, 'script', 'https://www.google-analytics.com/analytics.js', 'ga');

        ga('create', 'UA-78065765-1', 'auto');
        ga('send', 'pageview');
    </script>
</asp:Content>  