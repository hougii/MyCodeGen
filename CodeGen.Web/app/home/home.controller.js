
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
            $scope.colmaplist = response.data;//暫
            
        }, function errorCallback(response) {
            console.log(response);
        });
    };

    //頁面上Keep欄位資訊
    $scope.getColumn = function (itm, status) {
        if (status) {
            var result = checkValue(itm.columnId, $scope.collist);
            if (result == 'Not exist') {
                $scope.collist.push({
                    ColumnId: itm.columnId,
                    ColumnName: itm.columnName,
                    MapColumnName:'',
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

    //產生程式碼
    $scope.generate = function () {
        $('.nav-tabs a[href="#model"]').tab('show');

        var rowGen = [];
        var elementIDSql = 'genCodeSql';
        var elementIDVm = 'genCodeVm';
        var elementIDVu = 'genCodeVu';
        var elementIDNg = 'genCodeAngular';
        var elementIDApi = 'genCodeAPI';
        var elementIDService = 'genCodeService';
        var elementIDInterface = 'genCodeInterface';

        if ($scope.collist.length > 0) {
            //20181112-howard-change post data content.
            $http({
                method: 'POST',
                url: '/api/Codegen/GenerateCode',
                data: {table:$scope.tableInfo,columns:$scope.collist},
                dataType: "json",
                contentType: 'application/json; charset=utf-8'
            }).then(function (response) {

                $('#genCodeSql').text(''); $('#genCodeVm').text(''); $('#genCodeVu').text(''); $('#genCodeAngular').text(''); $('#genCodeAPI').text(''); $('#genCodeService').text(''); $('#genCodeInterface').text('');

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
            }, function (error) {
                console.log(error);
            });
        }
        else {
            rowGen = []; $('#genCodeSql').text(''); $('#genCodeVm').text('');
            console.log("Please Choose a Column!!");
        };
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
