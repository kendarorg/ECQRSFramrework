organizations.service('organizationGroupsRolesDataService', [
    function () {

        this.deleteConfirm = function (item) { return "You want to delete role '" + item.Code + "'?"; }

        this.detailMessage = function (item) { return "role '" + item.Code + "'" }
        this.createMessage = function () { return "Create role"; }

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
            var result = "/api/OrganizationGroups/roles/" +
                encodeURI(scope.item.OrganizationId) + "/" +
                encodeURI(scope.item.Id) + "?range=[" + start + "," + end + "]";
            if (didSomething) {
                result += "&filter=" + encodeURI(JSON.stringify(realFilter));
            }
            return result;
        };


        this.add = function (item, scope) {
            return '/api/OrganizationGroups/roles/' +
            encodeURI(scope.item.OrganizationId) + "/" +
            encodeURI(scope.item.Id) }
        this.delete = function (item, scope) {
            return '/api/OrganizationGroups/roles/' +
            encodeURI(scope.item.OrganizationId) + "/" +
            encodeURI(scope.item.Id)+'/' + item.Id ; }

        //this.getListCount = function (data, headers) {
        //    var contentRange = headers()['content-range'];
        //    var length = contentRange.split('/');
        //    return parseInt(length[1]);
        //}
    }
]);

organizations.controller('organizationGroupsRolesListController', ['$scope', '$http', 'globalMessagesService',
		'$controller', 'organizationGroupsRolesDataService',
	function ($scope, $http, globalMessagesService, $controller, organizationsDataService) {
	    $controller('ListController', {
	        $scope: $scope,
	        dataService: organizationsDataService
	    });
	    $scope.pageSize = 5;
	    $scope.maxPages = 5;

	    $scope.associate = function (groupRole) {
	        $http.post(organizationsDataService.add(groupRole, $scope), groupRole).
				success(function (result, status, headers, config) {
				    $scope.loadData(0);
				}).
				error(function (data, status, headers, config) {
				    globalMessagesService.showMessage(data.message, status);
				});
	    }

	    $scope.dissociate = function (groupRole) {
	        $scope.delete(groupRole, $scope);
	    }
	}]);
