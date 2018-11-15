var templatingApp;
(
    function () {
        'use strict';
        templatingApp = angular.module('templating_app', ['ui.router']);
    }
)();

templatingApp.config(['$locationProvider', '$stateProvider', '$urlRouterProvider', '$urlMatcherFactoryProvider', '$compileProvider',
    function ($locationProvider, $stateProvider, $urlRouterProvider, $urlMatcherFactoryProvider, $compileProvider) {

        //console.log('Appt.Main is now running')
        if (window.history && window.history.pushState) {
            $locationProvider.html5Mode({
                enabled: true,
                requireBase: true
            }).hashPrefix('!');
        };
        $urlMatcherFactoryProvider.strictMode(false);
        $compileProvider.debugInfoEnabled(false);

        $stateProvider
            .state('home', {
                url: '/',
                templateUrl: './views/home/home.html',
                controller: 'HomeController'
            })
            .state('about', {
                url: '/about',
                templateUrl: './views/about/about.html',
                controller: 'AboutController'
            });

        $urlRouterProvider.otherwise('/home');
    }]); 


templatingApp.controller('HomeController', ['$scope', '$http', function ($scope, $http) {

    $scope.dbId = 0; $scope.dbname = null;
    $scope.tableInfo = null;
    $scope.collist = [];
    $scope.isCheckAll = 0;
    $scope.colmaplist = [];
    $scope.copySuccess = function () {
        console.log('Copied!');
    };

    $scope.copyFail = function (err) {
        console.error('Error!', err);
    };

    $("#checkAll").click(function () {
        $('input:checkbox').not(this).prop('checked', this.checked);
        var elm = $('#checkboxes input:checked[name="coList[]"]').map(function () { return $(this).val(); }).get();
        var totalelm = elm.length;
        if (totalelm > 0) {
            for (var i = 1; i <= totalelm; i++) {
                var el = document.getElementById('chkc_' + i);
                angular.element(el).triggerHandler('click');
            };
        }
        else {
            $scope.collist = [];
        };
    });

    GetAllDb();
    
    function GetAllDb() {
        $http({
            method: 'GET',
            url: '/api/Codegen/GetDatabaseList'
        }).then(function successCallback(response) {
            $scope.dblist = response.data;
            $scope.getMapColumn();
        }, function errorCallback(response) {
            console.log(response);
        });
    };

    $scope.getAllTable = function (itm) {
        $scope.dbId = itm.databaseId; $scope.dbname = itm.databaseName;
        $scope.dbModel = {
            DatabaseId: itm.databaseId,
            DatabaseName: itm.databaseName
        };

        $http({
            method: 'POST',
            url: '/api/Codegen/GetDatabaseTableList',
            data: $scope.dbModel
        }).then(function successCallback(response) {
            //(TW)取得Table清單
            //(EN)get table list
            //(TW)註：response data的欄位名稱第一個字母會變小寫, why! ?
            $scope.tblist = response.data;
            
        }, function errorCallback(response) {
            console.log(response);
        });
    };

    $scope.getAllTableColumn = function (table) {

        $scope.dbModel = {
            DatabaseId: $scope.dbId,
            DatabaseName: $scope.dbname,
            TableId: table.tableId,
            TableName: table.tableName
        };

        $http({
            method: 'POST',
            url: '/api/Codegen/GetDatabaseTableColumnList',
            data: $scope.dbModel
        }).then(function successCallback(response) {
            $scope.tableInfo = table;
            $scope.colist = response.data;
            //sort or append to map column
            var tempcolist = $scope.colist.slice(0);
            $scope.sortOrAppendMapColumns(tempcolist);

        }, function errorCallback(response) {
            console.log(response);
        });
    };

    //欄位勾選時的event
    $scope.getColumn = function (itm, status) {
        if (status) {
            var result = checkValue(itm.columnId, $scope.collist);
            if (result == 'Not exist') {
                $scope.collist.push({
                    ColumnId: itm.columnId,
                    ColumnName: itm.columnName,
                    MapColumnName: '',
                    DataType: itm.dataType,
                    MaxLength: itm.maxLength,
                    IsNullable: itm.isNullable,
                    TableSchema: itm.tableSchema,
                    Tablename: itm.tablename,
                    ColumnDescription: itm.columnDescription
                });
            }
        }
        else {
            var index = $scope.collist.indexOf(itm.ColumnId);
            if (index > -1)
                $scope.collist.splice(index, 1);
        }
    };

    $scope.getMapColumn = function () {
        $http({
            method: 'POST',
            url: '/api/Codegen/GetMapColumns',
            data: {}
        }).then(function successCallback(response) {
            $scope.colmaplist =JSON.parse(response.data);

        }, function errorCallback(response) {
            console.log(response);
        });
    };
    $scope.saveMapColumn = function () {
        $http({
            method: 'POST',
            url: '/api/Codegen/SaveMapColumns',
            data: $scope.colmaplist
        }).then(function successCallback(response) {

        }, function errorCallback(response) {
            console.log(response);
        });
    };
    $scope.sortOrAppendMapColumns = function (colist) {
        //note:use key value pair to save mapcolumns
        //$scope.colmaplist
        var tempColList = colist.reverse();//反轉
        $(colist).each(function (i, col) {
            var colname = col.columnName;
            var coldesc = col.columnDescription;
            //find element
            var findElementIndex = 0;
            var findElement = $scope.colmaplist.find((m, index) => {
                if (m.columnName == colname) {
                    findElementIndex = index;
                }
                return m.columnName == colname;
            });
            if (findElement!=null) {
                //remove element
                $scope.colmaplist.splice(findElementIndex, 1);
                //add element
                $scope.colmaplist.splice(0, 0, findElement);
            }
            else {
                //add element to first
                $scope.colmaplist.splice(0, 0, { columnName: colname, mapColumnName: '', columnDescription: coldesc });
            }
        });
    };


    //產生程式碼
    $scope.generate = function () {

        $scope.saveMapColumn();

        $('.nav-tabs a[href="#model"]').tab('show');

        var rowGen = [];
        var elementIDSql = 'genCodeSql';
        var elementIDVm = 'genCodeVm';
        var elementIDVu = 'genCodeVu';
        var elementIDNg = 'genCodeAngular';
        var elementIDApi = 'genCodeAPI';
        var elementIDService = 'genCodeService';
        var elementIDInterface = 'genCodeInterface';
        var elementIDMarkdown = 'genCodeMarkdown';

        if ($scope.collist.length > 0) {
            //20181112-howard-change post data content.
            $http({
                method: 'POST',
                url: '/api/Codegen/GenerateCode',
                data: { table: $scope.tableInfo, columns: $scope.collist },
                dataType: "json",
                contentType: 'application/json; charset=utf-8'
            }).then(function (response) {

                $('#genCodeSql').text(''); $('#genCodeVm').text(''); $('#genCodeVu').text(''); $('#genCodeAngular').text(''); $('#genCodeAPI').text(''); $('#genCodeService').text(''); $('#genCodeInterface').text(''); $('#genCodeMarkdown').text('');

                //20181112-howard- change response data is dictionary fommat.
                rowGen = response.data;

                //Html
                if (rowGen.hasOwnProperty("View")) {
                    document.getElementById(elementIDVu).innerHTML += rowGen["View"] + "\r\n";
                }
                //Angular
                if (rowGen.hasOwnProperty("NG")) {
                    document.getElementById(elementIDNg).innerHTML += rowGen["NG"] + "\r\n";
                }
                //WebAPI
                if (rowGen.hasOwnProperty("APIGet")) {
                    document.getElementById(elementIDApi).innerHTML += rowGen["APIGet"] + "\r\n";
                }
                //Model
                if (rowGen.hasOwnProperty("DbModel")) {
                    document.getElementById(elementIDVm).innerHTML += rowGen["DbModel"] + "\r\n";
                }
                //SQL
                if (rowGen.hasOwnProperty("SP")) {
                    document.getElementById(elementIDSql).innerHTML += rowGen["SP"] + "\r\n" + "\r\n";
                }
                //Service
                if (rowGen.hasOwnProperty("Service")) {
                    document.getElementById(elementIDService).innerHTML += rowGen["Service"] + "\r\n" + "\r\n";
                }
                //Interface
                if (rowGen.hasOwnProperty("Interface")) {
                    document.getElementById(elementIDInterface).innerHTML += rowGen["Interface"] + "\r\n" + "\r\n";
                }
                //Markdown
                if (rowGen.hasOwnProperty("Markdown")) {
                    document.getElementById(elementIDMarkdown).innerHTML += rowGen["Markdown"] + "\r\n" + "\r\n";
                }
            }, function (error) {
                console.log(error);
            });
        }
        else {
            rowGen = []; $('#genCodeSql').text(''); $('#genCodeVm').text('');
            console.log("Please Choose a Column!!");
        };
    };


    $scope.generateAllTable = function () {
        $('.nav-tabs a[href="#markdown"]').tab('show');
        if ($scope.dbModel == null) {
            console.log("Please Choose a Database!!");
            return;
        }
        var rowGen = [];
        var elementIDMarkdown = 'genCodeMarkdown';
        $http({
            method: 'POST',
            url: '/api/Codegen/GenerateAllTable',
            data: { database: $scope.dbModel }
        }).then(function successCallback(response) {
            $("#genCodeMarkdown").text("");

            rowGen = response.data;

            //Markdown
            if (rowGen.hasOwnProperty("Markdown")) {
                document.getElementById(elementIDMarkdown).innerHTML += rowGen["Markdown"] + "\r\n";
            }
        }, function errorCallback(response) {
            console.log(response);
        });
    };


    $scope.reset = function () {
        $scope.collist = []; rowGen = [];
        $('#genCodeSql').text(''); $('#genCodeVm').text('');
    };

    var checkValue = function (value, arr) {
        var status = 'Not exist';
        for (var i = 0; i < arr.length; i++) {
            var columnId = arr[i].columnId;
            if (columnId == value) {
                status = 'Exist';
                break;
            }
        }
        return status;
    };

    var postMultipleModel = function (apiRoute, methodMode, model) {
        var models = "[" + JSON.stringify(model) + "]";
        var request = $http({
            method: methodMode,
            url: apiRoute,
            data: models,
            async: false,
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
        });

        return request;
    };
}]);


templatingApp.controller('AboutController', ['$scope', '$http', function ($scope, $http) {
    $scope.title = "About Page";
}]);


templatingApp.directive("topNavbarmenu", function () {
    return {
        restrict: 'EA',
        templateUrl: 'views/shared/navbar/nav.html'
    };
});

templatingApp.directive("fixedSidebarleft", function () {
    return {
        restrict: 'EA',
        templateUrl: 'views/shared/sidebar/menu.html'
    };
});