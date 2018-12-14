
templatingApp.controller('HomeController', ['$scope', '$http', function ($scope, $http) {

    $scope.dbId = 0; $scope.dbname = null;
    $scope.tableInfo = null;
    $scope.colselectedlist = [];
    $scope.isCheckAll = 0;
    $scope.colmaplist = [];
    $scope.copyClick = function () {
        alert('not implement');
    };
    $scope.copySuccess = function () {
        console.log('Copied!');
    };

    $scope.copyFail = function (err) {
        console.error('Error!', err);
        alert("Error!! to see console log");
    };



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
            alert("Error!! to see console log");
        });
    };

    $scope.getAllTable = function (itm) {
        $scope.dbId = itm.databaseId;
        $scope.dbname = itm.databaseName;
        $scope.dbModel = {
            DatabaseId: itm.databaseId,
            DatabaseName: itm.databaseName
        };
        //clear column list
        $scope.collist = [];
        $scope.colselectedlist = [];
        $('#checkAll').prop('checked', false);

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
            alert("Error!! to see console log");
        });
    };

    $scope.getAllTableColumn = function (table) {

        $scope.dbModel = {
            DatabaseId: $scope.dbId,
            DatabaseName: $scope.dbname,
            TableId: table.tableId,
            TableName: table.tableName
        };

        //clear column list
        $scope.collist = [];
        $scope.colselectedlist = [];
        $('#checkAll').prop('checked', false);

        $http({
            method: 'POST',
            url: '/api/Codegen/GetDatabaseTableColumnList',
            data: $scope.dbModel
        }).then(function successCallback(response) {
            $scope.tableInfo = table;
            $scope.collist = response.data;
            //sort or append to map column
            $scope.sortOrAppendMapColumns($scope.tableInfo, $scope.collist);

        }, function errorCallback(response) {
            console.log(response);
            alert("Error!! to see console log");
        });
    };

    $("#checkAll").click(function () {
        $('#checkboxes input:checkbox').not(this).prop('checked', this.checked);
        var elm = $('#checkboxes input:checked[name="colList[]"]').map(function () { return $(this).val(); }).get();
        var totalelm = elm.length;
        if (totalelm > 0) {
            for (var i = 1; i <= totalelm; i++) {
                var el = document.getElementById('chkc_' + i);
                angular.element(el).triggerHandler('click');
            };
        }
        else {
            $scope.colselectedlist = [];
        };
    });

    //each column when click, will trigger this event
    $scope.getColumn = function (itm, status) {
        if (status) {
            var result = checkValue(itm.columnId, $scope.colselectedlist);
            if (result == 'Not exist') {
                $scope.colselectedlist.push({
                    ColumnId: itm.columnId,
                    ColumnName: itm.columnName,
                    MapColumnName: itm.columnName,//set default
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
            var index = $scope.colselectedlist.indexOf(itm.ColumnId);
            if (index > -1)
                $scope.colselectedlist.splice(index, 1);
        }
    };

    $scope.getMapColumn = function () {
        $http({
            method: 'POST',
            url: '/api/Codegen/GetMapColumns',
            data: {}
        }).then(function successCallback(response) {
            $scope.colmaplist = JSON.parse(response.data);

        }, function errorCallback(response) {
            console.log(response);
            alert("Error!! to see console log");
        });
    };
    $scope.saveMapColumn = function () {
        $http({
            method: 'POST',
            url: '/api/Codegen/SaveMapColumns',
            data: $scope.colmaplist
        }).then(function successCallback(response) {
            console.log('Save map column data success!');
        }, function errorCallback(response) {
            console.log(response);
            alert("Error!! to see console log");
        });
    };
    $scope.sortOrAppendMapColumns = function (table, collist) {
        var tempTable = Object.assign({}, table);
        var tempColList = collist.slice(0).reverse();//clone array & reverse

        //add table info to tempColList (because need table rename mapping)
        tempColList.push({ columnName: tempTable.tableName, columnDescription: tempTable.tableDescription });

        $(tempColList).each(function (i, col) {
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
            if (findElement != null) {
                //remove element
                $scope.colmaplist.splice(findElementIndex, 1);
                //update element
                if (findElement.columnDescription == "") {
                    findElement.columnDescription = coldesc;
                }
                //add element to first
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
        var elementIDInterface = 'genCodeInterface';
        var elementIDService = 'genCodeService';
        var elementIDTest = 'genCodeTest';
        var elementIDCtrl = 'genCodeCtrl';
        var elementIDApi = 'genCodeAPI';
        var elementIDNg = 'genCodeAngular';
        var elementIDVu = 'genCodeVu';
        var elementIDMarkdown = 'genCodeMarkdown';

        if ($scope.colselectedlist.length == 0) {
            rowGen = []; $('#genCodeSql').text(''); $('#genCodeVm').text('');
            console.log("Please Choose a Column!!");
            alert("Please Choose a Column!!");
        }
        else {
            var enableMap = $("#enableMap").is(":checked");

            //Fill mapColumn data
            angular.forEach($scope.colselectedlist, function (item, i) {
                var colname = item.ColumnName;
                var coldesc = item.ColumnDescription;
                var orgdesc = item.OrgColumnDescription;
                var mapcolumn = $scope.colmaplist.find((elem) => elem.columnName == colname);
                //預設Map值為原Colname
                item.MapColumnName = colname;
                item.ColumnDescription = (orgdesc != null) ? orgdesc : coldesc;
                if (enableMap && mapcolumn != null) {
                    var mapColumnName = mapcolumn.mapColumnName;
                    var mapDesc = mapcolumn.columnDescription;
                    if (mapColumnName != "")
                        item.MapColumnName = mapColumnName;
                    item.ColumnDescription = mapDesc;
                    item.OrgColumnDescription = (orgdesc != null) ? orgdesc : coldesc;
                }
            });
            //Fill Table mapping data
            var table = $scope.tableInfo;
            var tablename = table.tableName;
            var tabledesc = table.tableDescription;
            var orgtabledesc = table.orgColumnDescription;
            var maptable = $scope.colmaplist.find((elem) => elem.columnName == tablename);
            //預設Map值為原Colname
            table.mapTableName = tablename;
            table.tableDescription = (orgtabledesc != null) ? orgtabledesc : tabledesc;
            if (enableMap && maptable != null) {
                var mapTableName = maptable.mapColumnName;
                var mapDesc = maptable.columnDescription;
                if (mapTableName != "")
                     table.mapTableName = mapTableName;
                table.tableDescription = mapDesc;
                table.orgColumnDescription = (orgtabledesc != null) ? orgtabledesc : tabledesc;
            }


            $http({
                method: 'POST',
                url: '/api/Codegen/GenerateCode',
                data: {
                    table: $scope.tableInfo,
                    columns: $scope.colselectedlist
                },
                dataType: "json",
                contentType: 'application/json; charset=utf-8'
            }).then(function (response) {

                $('#' + elementIDSql).text(''); $('#' + elementIDVm).text('');
                $('#' + elementIDInterface).text(''); $('#' + elementIDService).text('');
                $('#' + elementIDTest).text(''); $('#' + elementIDCtrl).text('');
                $('#' + elementIDApi).text(''); $('#' + elementIDNg).text('');
                $('#' + elementIDVu).text(''); $('#' + elementIDMarkdown).text('');

                //20181112-howard- change response data is dictionary fommat.
                rowGen = response.data;

                //Model
                if (rowGen.hasOwnProperty("DbModel")) {
                    document.getElementById(elementIDVm).innerHTML += rowGen["DbModel"] + "\r\n";
                }
                //SQL
                if (rowGen.hasOwnProperty("SP")) {
                    document.getElementById(elementIDSql).innerHTML += rowGen["SP"] + "\r\n" + "\r\n";
                }
                //Interface
                if (rowGen.hasOwnProperty("Interface")) {
                    document.getElementById(elementIDInterface).innerHTML += rowGen["Interface"] + "\r\n" + "\r\n";
                }
                //Service
                if (rowGen.hasOwnProperty("Service")) {
                    document.getElementById(elementIDService).innerHTML += rowGen["Service"] + "\r\n" + "\r\n";
                }
                //Test
                if (rowGen.hasOwnProperty("Test")) {
                    document.getElementById(elementIDTest).innerHTML += rowGen["Test"] + "\r\n" + "\r\n";
                }
                //Controller
                if (rowGen.hasOwnProperty("Controller")) {
                    document.getElementById(elementIDCtrl).innerHTML += rowGen["Controller"] + "\r\n" + "\r\n";
                }
                //WebAPI
                if (rowGen.hasOwnProperty("APIGet")) {
                    document.getElementById(elementIDApi).innerHTML += rowGen["APIGet"] + "\r\n";
                }
                //Angular
                if (rowGen.hasOwnProperty("NG")) {
                    document.getElementById(elementIDNg).innerHTML += rowGen["NG"] + "\r\n";
                }
                //Html
                if (rowGen.hasOwnProperty("View")) {
                    document.getElementById(elementIDVu).innerHTML += rowGen["View"] + "\r\n";
                }
                //Markdown
                if (rowGen.hasOwnProperty("Markdown")) {
                    document.getElementById(elementIDMarkdown).innerHTML += rowGen["Markdown"] + "\r\n" + "\r\n";
                }
            }, function (error) {
                console.log(error);
                alert("Error!! to see console log");
            });
        };
    };

    //when click gen Markdown file,will trigger this.
    $scope.generateAllTable = function () {
        $('.nav-tabs a[href="#markdown"]').tab('show');
        if ($scope.dbModel == null) {
            console.log("Please Choose a Database!!");
            alert("Please Choose a Database!!");
            return;
        }
        var rowGen = [];
        var elementIDMarkdown = 'genCodeMarkdown';
        var enableMap = $("#enableMap").is(":checked");
        $http({
            method: 'POST',
            url: '/api/Codegen/GenerateAllTable',
            data: {
                database: $scope.dbModel,
                enableMap: enableMap
            }
        }).then(function successCallback(response) {
            $("#genCodeMarkdown").text("");

            rowGen = response.data;

            //Markdown
            if (rowGen.hasOwnProperty("Markdown")) {
                document.getElementById(elementIDMarkdown).innerHTML += rowGen["Markdown"] + "\r\n";
            }
        }, function errorCallback(response) {
            console.log(response);
            alert("Error!! to see console log");
        });
    };


    $scope.reset = function () {
        $scope.colselectedlist = []; rowGen = [];
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
