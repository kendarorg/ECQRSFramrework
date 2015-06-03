applications.service('applicationRolesPermissionsDataService', [
    function () {

        this.deleteConfirm = function (item) { return "You want to delete permission '" + item.Code + "'?"; }

        this.detailMessage = function (item) { return "permission '" + item.Code + "'" }
        this.createMessage = function () { return "Create permission"; }

        this.list = function (currentPage, pageSize, count, filter,scope) {
            var realFilter = {};
            var didSomething = false;
            for (var prop in filter) {
                var value = filter[prop];
                if (value != undefined && value !== null && value.length > 0) {
                    realFilter[prop] = value;
                    didSomething = true;
                }
            }

            var start = currentPage * pageSize;
            var end = start + count;
            var result = "/api/ApplicationRoles/permissions/" +
                encodeURI(scope.item.ApplicationId) + "/" +
                encodeURI(scope.item.Id) + "?range=[" + start + "," + end + "]";
            if (didSomething) {
                result += "&filter=" + encodeURI(JSON.stringify(realFilter));
            }
            return result;
        };


        this.add = function (item, scope) {
            return '/api/ApplicationRoles/permissions/' +
            encodeURI(scope.item.ApplicationId) + "/" +
            encodeURI(scope.item.Id) }
        this.delete = function (item, scope) {
            return '/api/ApplicationRoles/permissions/' +
            encodeURI(scope.item.ApplicationId) + "/" +
            encodeURI(scope.item.Id)+'/' + item.Id ; }

        //this.getListCount = function (data, headers) {
        //    var contentRange = headers()['content-range'];
        //    var length = contentRange.split('/');
        //    return parseInt(length[1]);
        //}
    }
]);

applications.controller('applicationRolesPermissionsListController', ['$scope', '$http', 'globalMessagesService',
		'$controller', 'applicationRolesPermissionsDataService',
	function ($scope, $http, globalMessagesService, $controller, applicationsDataService) {
	    $controller('ListController', {
	        $scope: $scope,
	        dataService: applicationsDataService
	    });
	    $scope.pageSize = 5;
	    $scope.maxPages = 5;

	    $scope.associate = function (rolePermission) {
	        $http.post(applicationsDataService.add(rolePermission, $scope), rolePermission).
				success(function (result, status, headers, config) {
				    $scope.loadData(0);
				}).
				error(function (data, status, headers, config) {
				    globalMessagesService.showMessage(data.message, status);
				});
	    }

	    $scope.dissociate = function (rolePermission) {
	        $scope.delete(rolePermission, $scope);
	    }
	}]);
